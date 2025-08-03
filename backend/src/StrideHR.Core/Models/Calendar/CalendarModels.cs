namespace StrideHR.Core.Models.Calendar;

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
    public Entities.CalendarIntegration? Integration { get; set; }
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