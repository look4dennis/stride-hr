using StrideHR.Core.Models.Calendar;

namespace StrideHR.Core.Entities;

public class CalendarIntegration : BaseEntity
{
    public int EmployeeId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public string CalendarId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
}

public class CalendarEvent : BaseEntity
{
    public int CalendarIntegrationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public CalendarEventType EventType { get; set; }
    public int? RelatedEntityId { get; set; }
    public string Attendees { get; set; } = string.Empty; // JSON array of attendees
    
    // Navigation properties
    public virtual CalendarIntegration CalendarIntegration { get; set; } = null!;
}