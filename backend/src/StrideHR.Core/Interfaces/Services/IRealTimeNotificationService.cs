using StrideHR.Core.Models.Notification;

namespace StrideHR.Core.Interfaces.Services;

public interface IRealTimeNotificationService
{
    Task SendNotificationToUserAsync(int userId, NotificationDto notification);
    Task SendNotificationToGroupAsync(string groupName, NotificationDto notification);
    Task SendNotificationToBranchAsync(int branchId, NotificationDto notification);
    Task SendNotificationToRoleAsync(string role, NotificationDto notification);
    Task SendNotificationToAllAsync(NotificationDto notification);
    Task SendBirthdayWishAsync(int fromUserId, int toUserId, string fromUserName, string message);
    Task SendAttendanceStatusUpdateAsync(int branchId, int userId, string status);
    Task SendProductivityAlertAcknowledgmentAsync(int userId, int alertId);
}