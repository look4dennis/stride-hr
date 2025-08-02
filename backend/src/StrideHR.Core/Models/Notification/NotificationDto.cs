using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsGlobal { get; set; }
    public string? TargetRole { get; set; }
}

public class CreateNotificationDto
{
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string? ActionUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsGlobal { get; set; }
    public string? TargetRole { get; set; }
}

public class BulkNotificationDto
{
    public List<int>? UserIds { get; set; }
    public int? BranchId { get; set; }
    public string? TargetRole { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string? ActionUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class NotificationPreferenceDto
{
    public int Id { get; set; }
    public NotificationType NotificationType { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public bool WeekendNotifications { get; set; }
}

public class UpdateNotificationPreferenceDto
{
    public NotificationType NotificationType { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public bool WeekendNotifications { get; set; }
}

public class BirthdayNotificationDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}

public class ProductivityAlertDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal IdlePercentage { get; set; }
    public TimeSpan IdleDuration { get; set; }
    public DateTime LastActivity { get; set; }
    public string AlertType { get; set; } = string.Empty;
}