using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Notification : BaseEntity
{
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsGlobal { get; set; }
    public string? TargetRole { get; set; }
    
    // Navigation Properties
    public virtual User? User { get; set; }
    public virtual Branch? Branch { get; set; }
}