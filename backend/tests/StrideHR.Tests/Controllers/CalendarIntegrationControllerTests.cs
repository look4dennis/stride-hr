using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Calendar;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class CalendarIntegrationControllerTests
{
    private readonly Mock<ICalendarIntegrationService> _mockCalendarService;
    private readonly CalendarIntegrationController _controller;

    public CalendarIntegrationControllerTests()
    {
        _mockCalendarService = new Mock<ICalendarIntegrationService>();
        _controller = new CalendarIntegrationController(_mockCalendarService.Object);
    }

    [Fact]
    public async Task ConnectGoogleCalendar_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var authorizationCode = "auth_code_123";
        var result = new CalendarIntegrationResult
        {
            Success = true,
            Message = "Google Calendar connected successfully",
            Integration = new CalendarIntegration
            {
                Id = 1,
                EmployeeId = employeeId,
                Provider = CalendarProvider.GoogleCalendar,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockCalendarService.Setup(s => s.ConnectGoogleCalendarAsync(employeeId, authorizationCode))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ConnectGoogleCalendar(employeeId, authorizationCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task ConnectGoogleCalendar_FailedConnection_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = 1;
        var authorizationCode = "invalid_code";
        var result = new CalendarIntegrationResult
        {
            Success = false,
            Message = "Failed to exchange authorization code for access token",
            ErrorCode = "TOKEN_EXCHANGE_FAILED"
        };

        _mockCalendarService.Setup(s => s.ConnectGoogleCalendarAsync(employeeId, authorizationCode))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ConnectGoogleCalendar(employeeId, authorizationCode);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as dynamic;
        Assert.False(responseData?.success);
        Assert.Equal("Failed to exchange authorization code for access token", responseData?.message?.ToString());
        Assert.Equal("TOKEN_EXCHANGE_FAILED", responseData?.errorCode?.ToString());
    }

    [Fact]
    public async Task DisconnectGoogleCalendar_ValidEmployeeId_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var result = new CalendarIntegrationResult
        {
            Success = true,
            Message = "Google Calendar disconnected successfully"
        };

        _mockCalendarService.Setup(s => s.DisconnectGoogleCalendarAsync(employeeId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DisconnectGoogleCalendar(employeeId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.Equal("Google Calendar disconnected successfully", responseData?.message?.ToString());
    }

    [Fact]
    public async Task GetGoogleCalendarEvents_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date.AddDays(7);
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = "event1",
                Title = "Meeting 1",
                StartTime = startDate.AddHours(9),
                EndTime = startDate.AddHours(10),
                Provider = CalendarProvider.GoogleCalendar
            },
            new CalendarEvent
            {
                Id = "event2",
                Title = "Meeting 2",
                StartTime = startDate.AddHours(14),
                EndTime = startDate.AddHours(15),
                Provider = CalendarProvider.GoogleCalendar
            }
        };

        _mockCalendarService.Setup(s => s.GetGoogleCalendarEventsAsync(employeeId, startDate, endDate))
            .ReturnsAsync(events);

        // Act
        var response = await _controller.GetGoogleCalendarEvents(employeeId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task CreateGoogleCalendarEvent_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var eventDto = new CreateCalendarEventDto
        {
            Title = "New Meeting",
            Description = "Team meeting",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            Location = "Conference Room A",
            AttendeeEmails = new List<string> { "john@example.com", "jane@example.com" },
            EventType = CalendarEventType.Meeting
        };

        var createdEvent = new CalendarEvent
        {
            Id = "new_event_id",
            Title = eventDto.Title,
            Description = eventDto.Description,
            StartTime = eventDto.StartTime,
            EndTime = eventDto.EndTime,
            Location = eventDto.Location,
            Provider = CalendarProvider.GoogleCalendar,
            EventType = eventDto.EventType
        };

        _mockCalendarService.Setup(s => s.CreateGoogleCalendarEventAsync(employeeId, eventDto))
            .ReturnsAsync(createdEvent);

        // Act
        var response = await _controller.CreateGoogleCalendarEvent(employeeId, eventDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task CreateGoogleCalendarEvent_IntegrationNotFound_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = 1;
        var eventDto = new CreateCalendarEventDto
        {
            Title = "New Meeting",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        };

        _mockCalendarService.Setup(s => s.CreateGoogleCalendarEventAsync(employeeId, eventDto))
            .ThrowsAsync(new InvalidOperationException("Google Calendar integration not found or inactive"));

        // Act
        var response = await _controller.CreateGoogleCalendarEvent(employeeId, eventDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as dynamic;
        Assert.False(responseData?.success);
        Assert.Equal("Google Calendar integration not found or inactive", responseData?.message?.ToString());
    }

    [Fact]
    public async Task UpdateGoogleCalendarEvent_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var eventId = "event123";
        var eventDto = new UpdateCalendarEventDto
        {
            Title = "Updated Meeting",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(3)
        };

        var updatedEvent = new CalendarEvent
        {
            Id = eventId,
            Title = eventDto.Title,
            StartTime = eventDto.StartTime.Value,
            EndTime = eventDto.EndTime.Value,
            Provider = CalendarProvider.GoogleCalendar
        };

        _mockCalendarService.Setup(s => s.UpdateGoogleCalendarEventAsync(employeeId, eventId, eventDto))
            .ReturnsAsync(updatedEvent);

        // Act
        var response = await _controller.UpdateGoogleCalendarEvent(employeeId, eventId, eventDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task DeleteGoogleCalendarEvent_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var eventId = "event123";

        _mockCalendarService.Setup(s => s.DeleteGoogleCalendarEventAsync(employeeId, eventId))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.DeleteGoogleCalendarEvent(employeeId, eventId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.Equal("Event deleted successfully", responseData?.message?.ToString());
    }

    [Fact]
    public async Task DeleteGoogleCalendarEvent_FailedDeletion_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = 1;
        var eventId = "event123";

        _mockCalendarService.Setup(s => s.DeleteGoogleCalendarEventAsync(employeeId, eventId))
            .ReturnsAsync(false);

        // Act
        var response = await _controller.DeleteGoogleCalendarEvent(employeeId, eventId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as dynamic;
        Assert.False(responseData?.success);
        Assert.Equal("Failed to delete event", responseData?.message?.ToString());
    }

    [Fact]
    public async Task GetEmployeeCalendarIntegrations_ValidEmployeeId_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var integrations = new List<CalendarIntegration>
        {
            new CalendarIntegration
            {
                Id = 1,
                EmployeeId = employeeId,
                Provider = CalendarProvider.GoogleCalendar,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockCalendarService.Setup(s => s.GetEmployeeCalendarIntegrationsAsync(employeeId))
            .ReturnsAsync(integrations);

        // Act
        var response = await _controller.GetEmployeeCalendarIntegrations(employeeId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task GetAllCalendarEvents_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date.AddDays(7);
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = "google_event",
                Title = "Google Meeting",
                Provider = CalendarProvider.GoogleCalendar,
                StartTime = startDate.AddHours(9),
                EndTime = startDate.AddHours(10)
            }
        };

        _mockCalendarService.Setup(s => s.GetAllCalendarEventsAsync(employeeId, startDate, endDate))
            .ReturnsAsync(events);

        // Act
        var response = await _controller.GetAllCalendarEvents(employeeId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task SyncCalendarEvents_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var provider = CalendarProvider.GoogleCalendar;
        var syncResult = new CalendarSyncResult
        {
            Success = true,
            Message = "Successfully synced 5 events from GoogleCalendar",
            EventsSynced = 5,
            EventsCreated = 2,
            EventsUpdated = 2,
            EventsDeleted = 1,
            SyncedAt = DateTime.UtcNow
        };

        _mockCalendarService.Setup(s => s.SyncCalendarEventsAsync(employeeId, provider))
            .ReturnsAsync(syncResult);

        // Act
        var response = await _controller.SyncCalendarEvents(employeeId, provider);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.NotNull(responseData?.data);
    }

    [Fact]
    public async Task ValidateCalendarConnection_ValidData_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var provider = CalendarProvider.GoogleCalendar;

        _mockCalendarService.Setup(s => s.ValidateCalendarConnectionAsync(employeeId, provider))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.ValidateCalendarConnection(employeeId, provider);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as dynamic;
        Assert.True(responseData?.success);
        Assert.True(responseData?.valid);
    }

    [Fact]
    public async Task ConnectOutlookCalendar_NotImplemented_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = 1;
        var authorizationCode = "auth_code_123";

        _mockCalendarService.Setup(s => s.ConnectOutlookCalendarAsync(employeeId, authorizationCode))
            .ThrowsAsync(new NotImplementedException("Outlook Calendar integration will be implemented in a future update"));

        // Act
        var response = await _controller.ConnectOutlookCalendar(employeeId, authorizationCode);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as dynamic;
        Assert.False(responseData?.success);
        Assert.Equal("Outlook Calendar integration is not yet implemented", responseData?.message?.ToString());
    }

    [Fact]
    public async Task CreateLeaveEvent_NotImplemented_ReturnsBadRequest()
    {
        // Arrange
        var leaveRequestId = 1;

        _mockCalendarService.Setup(s => s.CreateLeaveEventAsync(leaveRequestId))
            .ThrowsAsync(new NotImplementedException("Leave event creation will be implemented when integrating with leave management"));

        // Act
        var response = await _controller.CreateLeaveEvent(leaveRequestId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as dynamic;
        Assert.False(responseData?.success);
        Assert.Equal("Leave event creation is not yet implemented", responseData?.message?.ToString());
    }
}