namespace StrideHR.Core.Models.Calendar;

public class CalendarIntegration
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public string CalendarId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
}

public class CalendarEvent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public CalendarProvider Provider { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public List<CalendarAttendee> Attendees { get; set; } = new();
    public CalendarEventType EventType { get; set; }
    public int? RelatedEntityId { get; set; } // For leave requests, meetings, etc.
}

public class CalendarAttendee
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CalendarAttendeeStatus Status { get; set; }
}

public class CreateCalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public List<string> AttendeeEmails { get; set; } = new();
    public CalendarEventType EventType { get; set; }
    public int? RelatedEntityId { get; set; }
}

public class UpdateCalendarEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Location { get; set; }
    public bool? IsAllDay { get; set; }
    public List<string>? AttendeeEmails { get; set; }
}

public class CalendarIntegrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CalendarIntegration? Integration { get; set; }
    public string? ErrorCode { get; set; }
}

public class CalendarSyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EventsSynced { get; set; }
    public int EventsCreated { get; set; }
    public int EventsUpdated { get; set; }
    public int EventsDeleted { get; set; }
    public DateTime SyncedAt { get; set; }
    public List<string> Errors { get; set; } = new();
}

public enum CalendarProvider
{
    GoogleCalendar,
    OutlookCalendar
}

public enum CalendarEventType
{
    Meeting,
    Leave,
    Training,
    Interview,
    Performance,
    Other
}

public enum CalendarAttendeeStatus
{
    Pending,
    Accepted,
    Declined,
    Tentative
}