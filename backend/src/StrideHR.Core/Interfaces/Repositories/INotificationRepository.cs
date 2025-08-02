using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, int skip = 0, int take = 50);
    Task<List<Notification>> GetBranchNotificationsAsync(int branchId, bool unreadOnly = false);
    Task<List<Notification>> GetGlobalNotificationsAsync(bool unreadOnly = false);
    Task<List<Notification>> GetRoleNotificationsAsync(string role, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<int> MarkAllAsReadAsync(int userId);
    Task<List<Notification>> GetExpiredNotificationsAsync();
    Task<int> DeleteExpiredNotificationsAsync();
    Task<Dictionary<string, int>> GetNotificationStatsAsync(int userId);
    Task<Dictionary<string, int>> GetBranchNotificationStatsAsync(int branchId);
}

public interface INotificationTemplateRepository : IRepository<NotificationTemplate>
{
    Task<NotificationTemplate?> GetByNameAsync(string name);
    Task<List<NotificationTemplate>> GetByTypeAsync(NotificationType type);
    Task<List<NotificationTemplate>> GetActiveTemplatesAsync();
}

public interface IUserNotificationPreferenceRepository : IRepository<UserNotificationPreference>
{
    Task<List<UserNotificationPreference>> GetUserPreferencesAsync(int userId);
    Task<UserNotificationPreference?> GetPreferenceAsync(int userId, NotificationType type, NotificationChannel channel);
    Task<List<UserNotificationPreference>> GetUsersWithPreferenceAsync(NotificationType type, NotificationChannel channel, bool enabled = true);
}