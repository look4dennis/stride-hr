using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get user notifications
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        
        return Ok(new ApiResponse<List<NotificationDto>>
        {
            Success = true,
            Data = notifications,
            Message = "Notifications retrieved successfully"
        });
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetNotification(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        
        if (notification == null)
        {
            return NotFound(new ApiResponse<NotificationDto>
            {
                Success = false,
                Message = "Notification not found"
            });
        }

        return Ok(new ApiResponse<NotificationDto>
        {
            Success = true,
            Data = notification,
            Message = "Notification retrieved successfully"
        });
    }

    /// <summary>
    /// Create a new notification
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotificationAsync(dto);
        
        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, 
            new ApiResponse<NotificationDto>
            {
                Success = true,
                Data = notification,
                Message = "Notification created successfully"
            });
    }

    /// <summary>
    /// Create bulk notifications
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> CreateBulkNotifications([FromBody] BulkNotificationDto dto)
    {
        var notifications = await _notificationService.CreateBulkNotificationAsync(dto);
        
        return Ok(new ApiResponse<List<NotificationDto>>
        {
            Success = true,
            Data = notifications,
            Message = $"{notifications.Count} notifications created successfully"
        });
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);
        
        if (!result)
        {
            return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Message = "Notification not found or access denied"
            });
        }

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Notification marked as read"
        });
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.MarkAllAsReadAsync(userId);
        
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = result,
            Message = result ? "All notifications marked as read" : "No unread notifications found"
        });
    }

    /// <summary>
    /// Delete notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(int id)
    {
        var result = await _notificationService.DeleteNotificationAsync(id);
        
        if (!result)
        {
            return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Message = "Notification not found"
            });
        }

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Notification deleted successfully"
        });
    }

    /// <summary>
    /// Get unread notification count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        
        return Ok(new ApiResponse<int>
        {
            Success = true,
            Data = count,
            Message = "Unread count retrieved successfully"
        });
    }

    /// <summary>
    /// Get today's birthdays
    /// </summary>
    [HttpGet("birthdays/today")]
    public async Task<ActionResult<ApiResponse<List<BirthdayNotificationDto>>>> GetTodaysBirthdays()
    {
        var branchId = GetCurrentUserBranchId();
        var birthdays = await _notificationService.GetTodaysBirthdaysAsync(branchId);
        
        return Ok(new ApiResponse<List<BirthdayNotificationDto>>
        {
            Success = true,
            Data = birthdays,
            Message = "Today's birthdays retrieved successfully"
        });
    }

    /// <summary>
    /// Send birthday wish
    /// </summary>
    [HttpPost("birthdays/wish")]
    public async Task<ActionResult<ApiResponse<bool>>> SendBirthdayWish([FromBody] SendBirthdayWishDto dto)
    {
        var fromUserId = GetCurrentUserId();
        await _notificationService.SendBirthdayWishAsync(fromUserId, dto.ToUserId, dto.Message);
        
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Birthday wish sent successfully"
        });
    }

    /// <summary>
    /// Get user notification preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<List<NotificationPreferenceDto>>>> GetPreferences()
    {
        var userId = GetCurrentUserId();
        var preferences = await _notificationService.GetUserPreferencesAsync(userId);
        
        return Ok(new ApiResponse<List<NotificationPreferenceDto>>
        {
            Success = true,
            Data = preferences,
            Message = "Notification preferences retrieved successfully"
        });
    }

    /// <summary>
    /// Update notification preference
    /// </summary>
    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdatePreference([FromBody] UpdateNotificationPreferenceDto dto)
    {
        var userId = GetCurrentUserId();
        var preference = await _notificationService.UpdatePreferenceAsync(userId, dto);
        
        return Ok(new ApiResponse<NotificationPreferenceDto>
        {
            Success = true,
            Data = preference,
            Message = "Notification preference updated successfully"
        });
    }

    /// <summary>
    /// Get notification statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetStats()
    {
        var userId = GetCurrentUserId();
        var stats = await _notificationService.GetNotificationStatsAsync(userId);
        
        return Ok(new ApiResponse<Dictionary<string, int>>
        {
            Success = true,
            Data = stats,
            Message = "Notification statistics retrieved successfully"
        });
    }

    /// <summary>
    /// Send productivity alert (for testing purposes)
    /// </summary>
    [HttpPost("productivity-alert")]
    public async Task<ActionResult<ApiResponse<bool>>> SendProductivityAlert([FromBody] ProductivityAlertDto dto)
    {
        await _notificationService.SendProductivityAlertAsync(dto);
        
        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Productivity alert sent successfully"
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private int? GetCurrentUserBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : null;
    }
}

public class SendBirthdayWishDto
{
    public int ToUserId { get; set; }
    public string Message { get; set; } = string.Empty;
}