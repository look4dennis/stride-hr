using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, int skip = 0, int take = 50)
    {
        var query = _context.Set<Notification>()
            .Where(n => (n.UserId == userId || n.IsGlobal) && n.ExpiresAt > DateTime.UtcNow)
            .Include(n => n.User)
            .Include(n => n.Branch)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetBranchNotificationsAsync(int branchId, bool unreadOnly = false)
    {
        var query = _context.Set<Notification>()
            .Where(n => n.BranchId == branchId && n.ExpiresAt > DateTime.UtcNow)
            .Include(n => n.User)
            .Include(n => n.Branch)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetGlobalNotificationsAsync(bool unreadOnly = false)
    {
        var query = _context.Set<Notification>()
            .Where(n => n.IsGlobal && n.ExpiresAt > DateTime.UtcNow)
            .Include(n => n.User)
            .Include(n => n.Branch)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetRoleNotificationsAsync(string role, bool unreadOnly = false)
    {
        var query = _context.Set<Notification>()
            .Where(n => n.TargetRole == role && n.ExpiresAt > DateTime.UtcNow)
            .Include(n => n.User)
            .Include(n => n.Branch)
            .AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => (n.UserId == userId || n.IsGlobal) && 
                           !n.IsRead && 
                           n.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && 
                                    (n.UserId == userId || n.IsGlobal));

        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Set<Notification>()
            .Where(n => (n.UserId == userId || n.IsGlobal) && 
                       !n.IsRead && 
                       n.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetExpiredNotificationsAsync()
    {
        return await _context.Set<Notification>()
            .Where(n => n.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredNotificationsAsync()
    {
        var expiredNotifications = await GetExpiredNotificationsAsync();
        _context.Set<Notification>().RemoveRange(expiredNotifications);
        return await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, int>> GetNotificationStatsAsync(int userId)
    {
        var stats = await _context.Set<Notification>()
            .Where(n => (n.UserId == userId || n.IsGlobal) && n.ExpiresAt > DateTime.UtcNow)
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return stats;
    }

    public async Task<Dictionary<string, int>> GetBranchNotificationStatsAsync(int branchId)
    {
        var stats = await _context.Set<Notification>()
            .Where(n => n.BranchId == branchId && n.ExpiresAt > DateTime.UtcNow)
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return stats;
    }
}

public class NotificationTemplateRepository : Repository<NotificationTemplate>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<NotificationTemplate?> GetByNameAsync(string name)
    {
        return await _context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
    }

    public async Task<List<NotificationTemplate>> GetByTypeAsync(NotificationType type)
    {
        return await _context.Set<NotificationTemplate>()
            .Where(t => t.Type == type && t.IsActive)
            .ToListAsync();
    }

    public async Task<List<NotificationTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.Set<NotificationTemplate>()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}

public class UserNotificationPreferenceRepository : Repository<UserNotificationPreference>, IUserNotificationPreferenceRepository
{
    public UserNotificationPreferenceRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<UserNotificationPreference>> GetUserPreferencesAsync(int userId)
    {
        return await _context.Set<UserNotificationPreference>()
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<UserNotificationPreference?> GetPreferenceAsync(int userId, NotificationType type, NotificationChannel channel)
    {
        return await _context.Set<UserNotificationPreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                    p.NotificationType == type && 
                                    p.Channel == channel);
    }

    public async Task<List<UserNotificationPreference>> GetUsersWithPreferenceAsync(NotificationType type, NotificationChannel channel, bool enabled = true)
    {
        return await _context.Set<UserNotificationPreference>()
            .Where(p => p.NotificationType == type && 
                       p.Channel == channel && 
                       p.IsEnabled == enabled)
            .Include(p => p.User)
            .ToListAsync();
    }
}