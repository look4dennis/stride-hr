using Microsoft.AspNetCore.SignalR;
using StrideHR.API.Hubs;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;

namespace StrideHR.API.Services;

public class SignalRNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(int userId, NotificationDto notification)
    {
        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("NotificationReceived", notification);
        
        _logger.LogDebug("Notification sent to user {UserId}: {Title}", userId, notification.Title);
    }

    public async Task SendNotificationToGroupAsync(string groupName, NotificationDto notification)
    {
        await _hubContext.Clients.Group(groupName)
            .SendAsync("NotificationReceived", notification);
        
        _logger.LogDebug("Notification sent to group {GroupName}: {Title}", groupName, notification.Title);
    }

    public async Task SendNotificationToBranchAsync(int branchId, NotificationDto notification)
    {
        await _hubContext.Clients.Group($"Branch_{branchId}")
            .SendAsync("NotificationReceived", notification);
        
        _logger.LogDebug("Notification sent to branch {BranchId}: {Title}", branchId, notification.Title);
    }

    public async Task SendNotificationToRoleAsync(string role, NotificationDto notification)
    {
        await _hubContext.Clients.Group($"Role_{role}")
            .SendAsync("NotificationReceived", notification);
        
        _logger.LogDebug("Notification sent to role {Role}: {Title}", role, notification.Title);
    }

    public async Task SendNotificationToAllAsync(NotificationDto notification)
    {
        await _hubContext.Clients.All
            .SendAsync("NotificationReceived", notification);
        
        _logger.LogDebug("Global notification sent: {Title}", notification.Title);
    }

    public async Task SendBirthdayWishAsync(int fromUserId, int toUserId, string fromUserName, string message)
    {
        await _hubContext.Clients.Group($"User_{toUserId}")
            .SendAsync("BirthdayWishReceived", new
            {
                FromUserId = fromUserId,
                FromUserName = fromUserName,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        
        _logger.LogDebug("Birthday wish sent from {FromUserId} to {ToUserId}", fromUserId, toUserId);
    }

    public async Task SendAttendanceStatusUpdateAsync(int branchId, int userId, string status)
    {
        await _hubContext.Clients.Group($"Branch_{branchId}")
            .SendAsync("AttendanceStatusUpdated", new
            {
                UserId = userId,
                Status = status,
                Timestamp = DateTime.UtcNow
            });
        
        _logger.LogDebug("Attendance status update sent for user {UserId}: {Status}", userId, status);
    }

    public async Task SendProductivityAlertAcknowledgmentAsync(int userId, int alertId)
    {
        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("ProductivityAlertAcknowledged", new
            {
                AlertId = alertId,
                Timestamp = DateTime.UtcNow
            });
        
        _logger.LogDebug("Productivity alert acknowledgment sent to user {UserId} for alert {AlertId}", userId, alertId);
    }
}