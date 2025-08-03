using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Calendar;

namespace StrideHR.Infrastructure.Services;

public class CalendarIntegrationService : ICalendarIntegrationService
{
    private readonly ICalendarIntegrationRepository _integrationRepository;
    private readonly ICalendarEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalendarIntegrationService> _logger;

    public CalendarIntegrationService(
        ICalendarIntegrationRepository integrationRepository,
        ICalendarEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CalendarIntegrationService> logger)
    {
        _integrationRepository = integrationRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    // Google Calendar Integration
    public async Task<CalendarIntegrationResult> ConnectGoogleCalendarAsync(int employeeId, string authorizationCode)
    {
        try
        {
            var tokenResponse = await ExchangeGoogleAuthCodeAsync(authorizationCode);
            if (tokenResponse == null)
            {
                return new CalendarIntegrationResult
                {
                    Success = false,
                    Message = "Failed to exchange authorization code for access token",
                    ErrorCode = "TOKEN_EXCHANGE_FAILED"
                };
            }

            var existingIntegration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
            
            CalendarIntegration integration;
            if (existingIntegration != null)
            {
                existingIntegration.AccessToken = tokenResponse.AccessToken;
                existingIntegration.RefreshToken = tokenResponse.RefreshToken;
                existingIntegration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                existingIntegration.IsActive = true;
                existingIntegration.UpdatedAt = DateTime.UtcNow;
                
                await _integrationRepository.UpdateAsync(existingIntegration);
                integration = existingIntegration;
            }
            else
            {
                integration = new CalendarIntegration
                {
                    EmployeeId = employeeId,
                    Provider = CalendarProvider.GoogleCalendar,
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                    CalendarId = "primary",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _integrationRepository.AddAsync(integration);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Google Calendar connected for employee {EmployeeId}", employeeId);
            return new CalendarIntegrationResult
            {
                Success = true,
                Message = "Google Calendar connected successfully",
                Integration = integration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting Google Calendar for employee {EmployeeId}", employeeId);
            return new CalendarIntegrationResult
            {
                Success = false,
                Message = "Failed to connect Google Calendar",
                ErrorCode = "CONNECTION_ERROR"
            };
        }
    }

    public async Task<CalendarIntegrationResult> DisconnectGoogleCalendarAsync(int employeeId)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
        if (integration == null)
        {
            return new CalendarIntegrationResult
            {
                Success = false,
                Message = "Google Calendar integration not found",
                ErrorCode = "INTEGRATION_NOT_FOUND"
            };
        }

        integration.IsActive = false;
        integration.UpdatedAt = DateTime.UtcNow;
        
        await _integrationRepository.UpdateAsync(integration);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Google Calendar disconnected for employee {EmployeeId}", employeeId);
        return new CalendarIntegrationResult
        {
            Success = true,
            Message = "Google Calendar disconnected successfully"
        };
    }

    public async Task<List<CalendarEvent>> GetGoogleCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
        if (integration == null || !integration.IsActive)
            return new List<CalendarEvent>();

        try
        {
            await EnsureValidTokenAsync(integration);

            var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events" +
                     $"?timeMin={startDate:yyyy-MM-ddTHH:mm:ssZ}&timeMax={endDate:yyyy-MM-ddTHH:mm:ssZ}";

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var googleEvents = JsonSerializer.Deserialize<GoogleCalendarEventsResponse>(content);
                
                return googleEvents?.Items?.Select(MapGoogleEventToCalendarEvent).ToList() ?? new List<CalendarEvent>();
            }

            _logger.LogWarning("Failed to fetch Google Calendar events for employee {EmployeeId}: {StatusCode}", 
                employeeId, response.StatusCode);
            return new List<CalendarEvent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Google Calendar events for employee {EmployeeId}", employeeId);
            return new List<CalendarEvent>();
        }
    }

    public async Task<CalendarEvent> CreateGoogleCalendarEventAsync(int employeeId, CreateCalendarEventDto eventDto)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
        if (integration == null || !integration.IsActive)
            throw new InvalidOperationException("Google Calendar integration not found or inactive");

        await EnsureValidTokenAsync(integration);

        var googleEvent = new
        {
            summary = eventDto.Title,
            description = eventDto.Description,
            location = eventDto.Location,
            start = new { dateTime = eventDto.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
            end = new { dateTime = eventDto.EndTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
            attendees = eventDto.AttendeeEmails.Select(email => new { email }).ToArray()
        };

        var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events";
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        var json = JsonSerializer.Serialize(googleEvent);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonSerializer.Deserialize<GoogleCalendarEvent>(responseContent);
            
            var calendarEvent = MapGoogleEventToCalendarEvent(createdEvent!);
            calendarEvent.EventType = eventDto.EventType;
            calendarEvent.RelatedEntityId = eventDto.RelatedEntityId;

            // Save to local database
            var eventEntity = new CalendarEvent
            {
                CalendarIntegrationId = integration.Id,
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                StartTime = calendarEvent.StartTime,
                EndTime = calendarEvent.EndTime,
                Location = calendarEvent.Location,
                IsAllDay = calendarEvent.IsAllDay,
                ProviderEventId = calendarEvent.ProviderEventId,
                EventType = calendarEvent.EventType,
                RelatedEntityId = calendarEvent.RelatedEntityId,
                Attendees = JsonSerializer.Serialize(calendarEvent.Attendees),
                CreatedAt = DateTime.UtcNow
            };

            await _eventRepository.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            return calendarEvent;
        }

        throw new InvalidOperationException($"Failed to create Google Calendar event: {response.StatusCode}");
    }

    public async Task<CalendarEvent> UpdateGoogleCalendarEventAsync(int employeeId, string eventId, UpdateCalendarEventDto eventDto)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
        if (integration == null || !integration.IsActive)
            throw new InvalidOperationException("Google Calendar integration not found or inactive");

        await EnsureValidTokenAsync(integration);

        var updateData = new Dictionary<string, object>();
        
        if (!string.IsNullOrEmpty(eventDto.Title))
            updateData["summary"] = eventDto.Title;
        
        if (!string.IsNullOrEmpty(eventDto.Description))
            updateData["description"] = eventDto.Description;
        
        if (!string.IsNullOrEmpty(eventDto.Location))
            updateData["location"] = eventDto.Location;
        
        if (eventDto.StartTime.HasValue)
            updateData["start"] = new { dateTime = eventDto.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") };
        
        if (eventDto.EndTime.HasValue)
            updateData["end"] = new { dateTime = eventDto.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") };

        if (eventDto.AttendeeEmails != null)
            updateData["attendees"] = eventDto.AttendeeEmails.Select(email => new { email }).ToArray();

        var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events/{eventId}";
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        var json = JsonSerializer.Serialize(updateData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedEvent = JsonSerializer.Deserialize<GoogleCalendarEvent>(responseContent);
            
            return MapGoogleEventToCalendarEvent(updatedEvent!);
        }

        throw new InvalidOperationException($"Failed to update Google Calendar event: {response.StatusCode}");
    }

    public async Task<bool> DeleteGoogleCalendarEventAsync(int employeeId, string eventId)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, CalendarProvider.GoogleCalendar);
        if (integration == null || !integration.IsActive)
            return false;

        await EnsureValidTokenAsync(integration);

        var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}/events/{eventId}";
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    // Outlook Integration (similar implementation)
    public async Task<CalendarIntegrationResult> ConnectOutlookCalendarAsync(int employeeId, string authorizationCode)
    {
        // Implementation similar to Google Calendar but using Microsoft Graph API
        // This would require Microsoft Graph SDK or direct HTTP calls
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    public async Task<CalendarIntegrationResult> DisconnectOutlookCalendarAsync(int employeeId)
    {
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    public async Task<List<CalendarEvent>> GetOutlookCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    public async Task<CalendarEvent> CreateOutlookCalendarEventAsync(int employeeId, CreateCalendarEventDto eventDto)
    {
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    public async Task<CalendarEvent> UpdateOutlookCalendarEventAsync(int employeeId, string eventId, UpdateCalendarEventDto eventDto)
    {
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    public async Task<bool> DeleteOutlookCalendarEventAsync(int employeeId, string eventId)
    {
        throw new NotImplementedException("Outlook Calendar integration will be implemented in a future update");
    }

    // Generic Calendar Operations
    public async Task<List<CalendarIntegration>> GetEmployeeCalendarIntegrationsAsync(int employeeId)
    {
        return await _integrationRepository.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<List<CalendarEvent>> GetAllCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var events = new List<CalendarEvent>();
        
        // Get Google Calendar events
        var googleEvents = await GetGoogleCalendarEventsAsync(employeeId, startDate, endDate);
        events.AddRange(googleEvents);
        
        // Get Outlook events (when implemented)
        // var outlookEvents = await GetOutlookCalendarEventsAsync(employeeId, startDate, endDate);
        // events.AddRange(outlookEvents);

        return events.OrderBy(e => e.StartTime).ToList();
    }

    public async Task<CalendarSyncResult> SyncCalendarEventsAsync(int employeeId, CalendarProvider provider)
    {
        var result = new CalendarSyncResult
        {
            SyncedAt = DateTime.UtcNow
        };

        try
        {
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow.AddDays(90);

            List<CalendarEvent> events;
            if (provider == CalendarProvider.GoogleCalendar)
            {
                events = await GetGoogleCalendarEventsAsync(employeeId, startDate, endDate);
            }
            else
            {
                events = await GetOutlookCalendarEventsAsync(employeeId, startDate, endDate);
            }

            result.EventsSynced = events.Count;
            result.Success = true;
            result.Message = $"Successfully synced {events.Count} events from {provider}";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Failed to sync calendar events: {ex.Message}";
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error syncing calendar events for employee {EmployeeId} with provider {Provider}", 
                employeeId, provider);
        }

        return result;
    }

    public async Task<bool> ValidateCalendarConnectionAsync(int employeeId, CalendarProvider provider)
    {
        var integration = await _integrationRepository.GetByEmployeeAndProviderAsync(employeeId, provider);
        if (integration == null || !integration.IsActive)
            return false;

        try
        {
            if (provider == CalendarProvider.GoogleCalendar)
            {
                await EnsureValidTokenAsync(integration);
                var url = $"https://www.googleapis.com/calendar/v3/calendars/{integration.CalendarId}";
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", integration.AccessToken);

                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            
            // Outlook validation would go here
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating calendar connection for employee {EmployeeId} with provider {Provider}", 
                employeeId, provider);
            return false;
        }
    }

    // Leave and Meeting Integration
    public async Task<CalendarEvent> CreateLeaveEventAsync(int leaveRequestId)
    {
        // This would integrate with the leave management system
        // Implementation would fetch leave details and create calendar events
        throw new NotImplementedException("Leave event creation will be implemented when integrating with leave management");
    }

    public async Task<CalendarEvent> CreateMeetingEventAsync(int meetingId)
    {
        // This would integrate with meeting management
        throw new NotImplementedException("Meeting event creation will be implemented when integrating with meeting management");
    }

    public async Task<bool> UpdateLeaveEventAsync(int leaveRequestId, string status)
    {
        throw new NotImplementedException("Leave event updates will be implemented when integrating with leave management");
    }

    public async Task<bool> DeleteLeaveEventAsync(int leaveRequestId)
    {
        throw new NotImplementedException("Leave event deletion will be implemented when integrating with leave management");
    }

    // Private helper methods
    private async Task EnsureValidTokenAsync(CalendarIntegration integration)
    {
        if (integration.TokenExpiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            // Token is expired or about to expire, refresh it
            await RefreshGoogleTokenAsync(integration);
        }
    }

    private async Task RefreshGoogleTokenAsync(CalendarIntegration integration)
    {
        var clientId = _configuration["GoogleCalendar:ClientId"];
        var clientSecret = _configuration["GoogleCalendar:ClientSecret"];

        var refreshData = new Dictionary<string, string>
        {
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["refresh_token"] = integration.RefreshToken,
            ["grant_type"] = "refresh_token"
        };

        var content = new FormUrlEncodedContent(refreshData);
        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);

            if (tokenResponse != null)
            {
                integration.AccessToken = tokenResponse.AccessToken;
                integration.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                integration.UpdatedAt = DateTime.UtcNow;

                await _integrationRepository.UpdateAsync(integration);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }

    private async Task<GoogleTokenResponse?> ExchangeGoogleAuthCodeAsync(string authorizationCode)
    {
        var clientId = _configuration["GoogleCalendar:ClientId"];
        var clientSecret = _configuration["GoogleCalendar:ClientSecret"];
        var redirectUri = _configuration["GoogleCalendar:RedirectUri"];

        var tokenData = new Dictionary<string, string>
        {
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["code"] = authorizationCode,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirectUri!
        };

        var content = new FormUrlEncodedContent(tokenData);
        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);
        }

        return null;
    }

    private static CalendarEvent MapGoogleEventToCalendarEvent(GoogleCalendarEvent googleEvent)
    {
        return new CalendarEvent
        {
            Id = googleEvent.Id,
            Title = googleEvent.Summary ?? string.Empty,
            Description = googleEvent.Description ?? string.Empty,
            StartTime = DateTime.Parse(googleEvent.Start.DateTime ?? googleEvent.Start.Date!),
            EndTime = DateTime.Parse(googleEvent.End.DateTime ?? googleEvent.End.Date!),
            Location = googleEvent.Location ?? string.Empty,
            IsAllDay = !string.IsNullOrEmpty(googleEvent.Start.Date),
            Provider = CalendarProvider.GoogleCalendar,
            ProviderEventId = googleEvent.Id,
            Attendees = googleEvent.Attendees?.Select(a => new CalendarAttendee
            {
                Email = a.Email,
                Name = a.DisplayName ?? a.Email,
                Status = MapGoogleAttendeeStatus(a.ResponseStatus)
            }).ToList() ?? new List<CalendarAttendee>()
        };
    }

    private static CalendarAttendeeStatus MapGoogleAttendeeStatus(string? status)
    {
        return status switch
        {
            "accepted" => CalendarAttendeeStatus.Accepted,
            "declined" => CalendarAttendeeStatus.Declined,
            "tentative" => CalendarAttendeeStatus.Tentative,
            _ => CalendarAttendeeStatus.Pending
        };
    }

    // Google Calendar API response models
    private class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    private class GoogleCalendarEventsResponse
    {
        public List<GoogleCalendarEvent>? Items { get; set; }
    }

    private class GoogleCalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public GoogleEventDateTime Start { get; set; } = new();
        public GoogleEventDateTime End { get; set; } = new();
        public List<GoogleAttendee>? Attendees { get; set; }
    }

    private class GoogleEventDateTime
    {
        public string? DateTime { get; set; }
        public string? Date { get; set; }
    }

    private class GoogleAttendee
    {
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? ResponseStatus { get; set; }
    }
}