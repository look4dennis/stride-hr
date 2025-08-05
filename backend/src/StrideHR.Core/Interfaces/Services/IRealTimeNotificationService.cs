using StrideHR.Core.Models.Notification;

namespace StrideHR.Core.Interfaces.Services;

public interface IRealTimeNotificationService
{
    // Basic notification delivery
    Task SendNotificationToUserAsync(int userId, NotificationDto notification);
    Task SendNotificationToGroupAsync(string groupName, NotificationDto notification);
    Task SendNotificationToBranchAsync(int branchId, NotificationDto notification);
    Task SendNotificationToRoleAsync(string role, NotificationDto notification);
    Task SendNotificationToAllAsync(NotificationDto notification);
    
    // Enhanced notification delivery with confirmation
    Task<NotificationDeliveryResult> SendNotificationWithConfirmationAsync(int userId, NotificationDto notification);
    Task<List<NotificationDeliveryResult>> SendNotificationToGroupWithConfirmationAsync(string groupName, NotificationDto notification);
    Task<List<NotificationDeliveryResult>> SendNotificationToBranchWithConfirmationAsync(int branchId, NotificationDto notification);
    
    // Message queuing for offline users
    Task QueueNotificationForOfflineUserAsync(int userId, NotificationDto notification);
    Task ProcessQueuedNotificationsAsync(int userId);
    Task<List<QueuedNotificationDto>> GetQueuedNotificationsAsync(int userId);
    Task ClearQueuedNotificationsAsync(int userId);
    
    // Delivery status tracking
    Task<NotificationDeliveryStatus> GetDeliveryStatusAsync(string notificationId);
    Task<List<NotificationDeliveryStatus>> GetUserDeliveryHistoryAsync(int userId, int limit = 50);
    Task ConfirmNotificationDeliveryAsync(string notificationId, int userId);
    Task ConfirmNotificationReadAsync(string notificationId, int userId);
    
    // Connection management
    Task<List<UserConnectionInfo>> GetActiveConnectionsAsync();
    Task<List<UserConnectionInfo>> GetUserConnectionsAsync(int userId);
    Task<bool> IsUserOnlineAsync(int userId);
    Task<int> GetOnlineUsersCountAsync();
    Task<List<int>> GetOnlineUserIdsAsync();
    
    // Specialized notifications
    Task SendBirthdayWishAsync(int fromUserId, int toUserId, string fromUserName, string message);
    Task SendAttendanceStatusUpdateAsync(int branchId, int userId, string status);
    Task SendProductivityAlertAcknowledgmentAsync(int userId, int alertId);
    Task SendSystemMaintenanceNotificationAsync(string message, DateTime scheduledTime);
    Task SendConnectionHealthCheckAsync();
    
    // Bulk operations
    Task<List<NotificationDeliveryResult>> SendBulkNotificationAsync(List<int> userIds, NotificationDto notification);
    Task<List<NotificationDeliveryResult>> SendNotificationToMultipleGroupsAsync(List<string> groupNames, NotificationDto notification);
    
    // Priority and retry mechanisms
    Task SendHighPriorityNotificationAsync(int userId, NotificationDto notification);
    Task RetryFailedNotificationsAsync();
    Task<List<FailedNotificationDto>> GetFailedNotificationsAsync(int limit = 100);
}