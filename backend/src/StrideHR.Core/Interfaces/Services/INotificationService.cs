using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Notification;

namespace StrideHR.Core.Interfaces.Services;

public interface INotificationService
{
    // Basic notification operations
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);
    Task<List<NotificationDto>> CreateBulkNotificationAsync(BulkNotificationDto dto);
    Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task<NotificationDto?> GetNotificationByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<bool> DeleteNotificationAsync(int id);
    
    // Real-time notification delivery
    Task SendRealTimeNotificationAsync(int userId, NotificationDto notification);
    Task SendRealTimeNotificationToGroupAsync(string groupName, NotificationDto notification);
    Task SendRealTimeNotificationToBranchAsync(int branchId, NotificationDto notification);
    Task SendRealTimeNotificationToRoleAsync(string role, NotificationDto notification);
    
    // Birthday notifications
    Task<List<BirthdayNotificationDto>> GetTodaysBirthdaysAsync(int? branchId = null);
    Task SendBirthdayNotificationsAsync();
    Task SendBirthdayWishAsync(int fromUserId, int toUserId, string message);
    
    // Attendance and productivity alerts
    Task SendAttendanceAlertAsync(int employeeId, NotificationType alertType, Dictionary<string, object>? metadata = null);
    Task SendProductivityAlertAsync(ProductivityAlertDto alertDto);
    Task<List<ProductivityAlertDto>> GetIdleEmployeesAsync(int? branchId = null, TimeSpan? idleThreshold = null);
    
    // Notification preferences
    Task<List<NotificationPreferenceDto>> GetUserPreferencesAsync(int userId);
    Task<NotificationPreferenceDto> UpdatePreferenceAsync(int userId, UpdateNotificationPreferenceDto dto);
    Task<List<NotificationPreferenceDto>> CreateDefaultPreferencesAsync(int userId);
    
    // Template-based notifications
    Task<NotificationDto> CreateFromTemplateAsync(string templateName, int? userId, Dictionary<string, object> parameters);
    Task<List<NotificationDto>> CreateBulkFromTemplateAsync(string templateName, List<int> userIds, Dictionary<string, object> parameters);
    
    // Notification statistics
    Task<Dictionary<string, int>> GetNotificationStatsAsync(int userId);
    Task<Dictionary<string, int>> GetBranchNotificationStatsAsync(int branchId);
    
    // Cleanup operations
    Task CleanupExpiredNotificationsAsync();
    Task<int> GetUnreadCountAsync(int userId);
}