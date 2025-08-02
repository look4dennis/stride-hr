using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace StrideHR.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var branchId = Context.User?.FindFirst("BranchId")?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation("User {UserId} connected to notification hub", userId);
        }

        if (!string.IsNullOrEmpty(branchId))
        {
            // Add user to their branch group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
            _logger.LogInformation("User {UserId} added to branch {BranchId} group", userId, branchId);
        }

        if (!string.IsNullOrEmpty(role))
        {
            // Add user to their role group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            _logger.LogInformation("User {UserId} added to role {Role} group", userId, role);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client can join specific groups
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} joined group {GroupName}", userId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} left group {GroupName}", userId, groupName);
    }

    // Handle birthday wishes
    public async Task SendBirthdayWish(int toUserId, string message)
    {
        var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var fromUserName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (!string.IsNullOrEmpty(fromUserId) && !string.IsNullOrEmpty(fromUserName))
        {
            var wishData = new
            {
                FromUserId = fromUserId,
                FromUserName = fromUserName,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Send to the birthday person
            await Clients.Group($"User_{toUserId}").SendAsync("BirthdayWishReceived", wishData);
            
            _logger.LogInformation("Birthday wish sent from {FromUserId} to {ToUserId}", fromUserId, toUserId);
        }
    }

    // Handle attendance status updates
    public async Task UpdateAttendanceStatus(string status)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var branchId = Context.User?.FindFirst("BranchId")?.Value;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(branchId))
        {
            var statusData = new
            {
                UserId = userId,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to branch members (for real-time attendance dashboard)
            await Clients.Group($"Branch_{branchId}").SendAsync("AttendanceStatusUpdated", statusData);
            
            _logger.LogInformation("Attendance status updated for user {UserId}: {Status}", userId, status);
        }
    }

    // Handle productivity alerts acknowledgment
    public async Task AcknowledgeProductivityAlert(int alertId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("ProductivityAlertAcknowledged", new { AlertId = alertId, Timestamp = DateTime.UtcNow });
            _logger.LogInformation("Productivity alert {AlertId} acknowledged by user {UserId}", alertId, userId);
        }
    }
}