using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public NotificationChannel DefaultChannel { get; set; }
    public NotificationPriority DefaultPriority { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object>? DefaultMetadata { get; set; }
    public TimeSpan? DefaultExpiryDuration { get; set; }
}