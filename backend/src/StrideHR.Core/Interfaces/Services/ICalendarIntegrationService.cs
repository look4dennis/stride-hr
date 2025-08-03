using StrideHR.Core.Models.Calendar;

namespace StrideHR.Core.Interfaces.Services;

public interface ICalendarIntegrationService
{
    // Google Calendar Integration
    Task<CalendarIntegrationResult> ConnectGoogleCalendarAsync(int employeeId, string authorizationCode);
    Task<CalendarIntegrationResult> DisconnectGoogleCalendarAsync(int employeeId);
    Task<List<CalendarEvent>> GetGoogleCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<CalendarEvent> CreateGoogleCalendarEventAsync(int employeeId, CreateCalendarEventDto eventDto);
    Task<CalendarEvent> UpdateGoogleCalendarEventAsync(int employeeId, string eventId, UpdateCalendarEventDto eventDto);
    Task<bool> DeleteGoogleCalendarEventAsync(int employeeId, string eventId);
    
    // Outlook Integration
    Task<CalendarIntegrationResult> ConnectOutlookCalendarAsync(int employeeId, string authorizationCode);
    Task<CalendarIntegrationResult> DisconnectOutlookCalendarAsync(int employeeId);
    Task<List<CalendarEvent>> GetOutlookCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<CalendarEvent> CreateOutlookCalendarEventAsync(int employeeId, CreateCalendarEventDto eventDto);
    Task<CalendarEvent> UpdateOutlookCalendarEventAsync(int employeeId, string eventId, UpdateCalendarEventDto eventDto);
    Task<bool> DeleteOutlookCalendarEventAsync(int employeeId, string eventId);
    
    // Generic Calendar Operations
    Task<List<CalendarIntegration>> GetEmployeeCalendarIntegrationsAsync(int employeeId);
    Task<List<CalendarEvent>> GetAllCalendarEventsAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<CalendarSyncResult> SyncCalendarEventsAsync(int employeeId, CalendarProvider provider);
    Task<bool> ValidateCalendarConnectionAsync(int employeeId, CalendarProvider provider);
    
    // Leave and Meeting Integration
    Task<CalendarEvent> CreateLeaveEventAsync(int leaveRequestId);
    Task<CalendarEvent> CreateMeetingEventAsync(int meetingId);
    Task<bool> UpdateLeaveEventAsync(int leaveRequestId, string status);
    Task<bool> DeleteLeaveEventAsync(int leaveRequestId);
}