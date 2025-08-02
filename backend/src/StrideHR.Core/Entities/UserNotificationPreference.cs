using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class UserNotificationPreference : BaseEntity
{
    public int UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public bool WeekendNotifications { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}