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

public class NotificationDeliveryResult
{
    public string NotificationId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public NotificationDeliveryMethod DeliveryMethod { get; set; }
}

public class QueuedNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public NotificationDto Notification { get; set; } = new();
    public DateTime QueuedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public NotificationPriority Priority { get; set; }
}

public class NotificationDeliveryStatus
{
    public string NotificationId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public NotificationDeliveryState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public NotificationDeliveryMethod DeliveryMethod { get; set; }
}

public class FailedNotificationDto
{
    public string NotificationId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public NotificationDto Notification { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
}

public class UserConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? BranchId { get; set; }
    public string? OrganizationId { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime ConnectedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}