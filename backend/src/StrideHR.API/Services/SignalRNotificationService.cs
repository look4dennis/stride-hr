using Microsoft.AspNetCore.SignalR;
using StrideHR.API.Hubs;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;
using StrideHR.Core.Enums;
using System.Collections.Concurrent;

namespace StrideHR.API.Services;

public class SignalRNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;
    
    // In-memory storage for queued notifications and delivery tracking
    private static readonly ConcurrentDictionary<int, ConcurrentQueue<QueuedNotificationDto>> _queuedNotifications = new();
    private static readonly ConcurrentDictionary<string, NotificationDeliveryStatus> _deliveryStatus = new();
    private static readonly ConcurrentQueue<FailedNotificationDto> _failedNotifications = new();

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(int userId, NotificationDto notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("Attempted to send null notification to user {UserId}", userId);
                return;
            }

            var enhancedNotification = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Priority,
                notification.CreatedAt,
                notification.Metadata,
                DeliveredAt = DateTime.UtcNow,
                TargetType = "User",
                TargetId = userId.ToString()
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationReceived", enhancedNotification);
            
            _logger.LogInformation("Notification sent to user {UserId}: {Title} (Type: {Type}, Priority: {Priority})", 
                userId, notification.Title, notification.Type, notification.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}: {Title}", userId, notification?.Title);
            throw;
        }
    }

    public async Task SendNotificationToGroupAsync(string groupName, NotificationDto notification)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                _logger.LogWarning("Attempted to send notification to empty group name");
                return;
            }

            if (notification == null)
            {
                _logger.LogWarning("Attempted to send null notification to group {GroupName}", groupName);
                return;
            }

            var enhancedNotification = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Priority,
                notification.CreatedAt,
                notification.Metadata,
                DeliveredAt = DateTime.UtcNow,
                TargetType = "Group",
                TargetId = groupName
            };

            await _hubContext.Clients.Group(groupName)
                .SendAsync("NotificationReceived", enhancedNotification);
            
            _logger.LogInformation("Notification sent to group {GroupName}: {Title} (Type: {Type}, Priority: {Priority})", 
                groupName, notification.Title, notification.Type, notification.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to group {GroupName}: {Title}", groupName, notification?.Title);
            throw;
        }
    }

    public async Task SendNotificationToBranchAsync(int branchId, NotificationDto notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("Attempted to send null notification to branch {BranchId}", branchId);
                return;
            }

            var enhancedNotification = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Priority,
                notification.CreatedAt,
                notification.Metadata,
                DeliveredAt = DateTime.UtcNow,
                TargetType = "Branch",
                TargetId = branchId.ToString()
            };

            await _hubContext.Clients.Group($"Branch_{branchId}")
                .SendAsync("NotificationReceived", enhancedNotification);
            
            _logger.LogInformation("Notification sent to branch {BranchId}: {Title} (Type: {Type}, Priority: {Priority})", 
                branchId, notification.Title, notification.Type, notification.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to branch {BranchId}: {Title}", branchId, notification?.Title);
            throw;
        }
    }

    public async Task SendNotificationToRoleAsync(string role, NotificationDto notification)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                _logger.LogWarning("Attempted to send notification to empty role");
                return;
            }

            if (notification == null)
            {
                _logger.LogWarning("Attempted to send null notification to role {Role}", role);
                return;
            }

            var enhancedNotification = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Priority,
                notification.CreatedAt,
                notification.Metadata,
                DeliveredAt = DateTime.UtcNow,
                TargetType = "Role",
                TargetId = role
            };

            await _hubContext.Clients.Group($"Role_{role}")
                .SendAsync("NotificationReceived", enhancedNotification);
            
            _logger.LogInformation("Notification sent to role {Role}: {Title} (Type: {Type}, Priority: {Priority})", 
                role, notification.Title, notification.Type, notification.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to role {Role}: {Title}", role, notification?.Title);
            throw;
        }
    }

    public async Task SendNotificationToAllAsync(NotificationDto notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("Attempted to send null global notification");
                return;
            }

            var enhancedNotification = new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.Priority,
                notification.CreatedAt,
                notification.Metadata,
                DeliveredAt = DateTime.UtcNow,
                TargetType = "All",
                TargetId = "Global"
            };

            await _hubContext.Clients.All
                .SendAsync("NotificationReceived", enhancedNotification);
            
            _logger.LogInformation("Global notification sent: {Title} (Type: {Type}, Priority: {Priority})", 
                notification.Title, notification.Type, notification.Priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send global notification: {Title}", notification?.Title);
            throw;
        }
    }

    public async Task SendBirthdayWishAsync(int fromUserId, int toUserId, string fromUserName, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromUserName) || string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Invalid birthday wish data from user {FromUserId} to {ToUserId}", fromUserId, toUserId);
                return;
            }

            var wishData = new
            {
                FromUserId = fromUserId,
                FromUserName = fromUserName.Trim(),
                Message = message.Trim(),
                Timestamp = DateTime.UtcNow,
                Type = "BirthdayWish"
            };

            await _hubContext.Clients.Group($"User_{toUserId}")
                .SendAsync("BirthdayWishReceived", wishData);
            
            _logger.LogInformation("Birthday wish sent from {FromUserId} ({FromUserName}) to {ToUserId}", 
                fromUserId, fromUserName, toUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send birthday wish from {FromUserId} to {ToUserId}", fromUserId, toUserId);
            throw;
        }
    }

    public async Task SendAttendanceStatusUpdateAsync(int branchId, int userId, string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                _logger.LogWarning("Invalid attendance status for user {UserId} in branch {BranchId}", userId, branchId);
                return;
            }

            var statusData = new
            {
                UserId = userId,
                Status = status.Trim(),
                Timestamp = DateTime.UtcNow,
                BranchId = branchId,
                Type = "AttendanceUpdate"
            };

            await _hubContext.Clients.Group($"Branch_{branchId}")
                .SendAsync("AttendanceStatusUpdated", statusData);
            
            _logger.LogInformation("Attendance status update sent for user {UserId} in branch {BranchId}: {Status}", 
                userId, branchId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send attendance status update for user {UserId}: {Status}", userId, status);
            throw;
        }
    }

    public async Task SendProductivityAlertAcknowledgmentAsync(int userId, int alertId)
    {
        try
        {
            if (alertId <= 0)
            {
                _logger.LogWarning("Invalid alert ID {AlertId} for user {UserId}", alertId, userId);
                return;
            }

            var acknowledgmentData = new
            {
                AlertId = alertId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Type = "ProductivityAlertAcknowledgment"
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ProductivityAlertAcknowledged", acknowledgmentData);
            
            _logger.LogInformation("Productivity alert acknowledgment sent to user {UserId} for alert {AlertId}", 
                userId, alertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send productivity alert acknowledgment to user {UserId} for alert {AlertId}", 
                userId, alertId);
            throw;
        }
    }

    // Additional methods for enhanced functionality
    public async Task SendConnectionHealthCheckAsync()
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            var healthData = new
            {
                TotalConnections = connections.Count,
                Timestamp = DateTime.UtcNow,
                Type = "HealthCheck"
            };

            await _hubContext.Clients.All.SendAsync("HealthCheck", healthData);
            _logger.LogDebug("Connection health check sent to all clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send connection health check");
            throw;
        }
    }

    public async Task SendSystemMaintenanceNotificationAsync(string message, DateTime scheduledTime)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Attempted to send empty system maintenance notification");
                return;
            }

            var maintenanceData = new
            {
                Message = message.Trim(),
                ScheduledTime = scheduledTime,
                Timestamp = DateTime.UtcNow,
                Type = "SystemMaintenance",
                Priority = "High"
            };

            await _hubContext.Clients.All.SendAsync("SystemMaintenanceNotification", maintenanceData);
            _logger.LogInformation("System maintenance notification sent: {Message} (Scheduled: {ScheduledTime})", 
                message, scheduledTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system maintenance notification: {Message}", message);
            throw;
        }
    }

    // Enhanced notification delivery with confirmation
    public async Task<NotificationDeliveryResult> SendNotificationWithConfirmationAsync(int userId, NotificationDto notification)
    {
        var notificationId = Guid.NewGuid().ToString();
        var deliveryResult = new NotificationDeliveryResult
        {
            NotificationId = notificationId,
            UserId = userId,
            DeliveryMethod = NotificationDeliveryMethod.SignalR
        };

        try
        {
            // Track delivery status
            var deliveryStatus = new NotificationDeliveryStatus
            {
                NotificationId = notificationId,
                UserId = userId,
                State = NotificationDeliveryState.Delivering,
                CreatedAt = DateTime.UtcNow,
                DeliveryMethod = NotificationDeliveryMethod.SignalR
            };
            _deliveryStatus.TryAdd(notificationId, deliveryStatus);

            // Check if user is online
            if (await IsUserOnlineAsync(userId))
            {
                var enhancedNotification = new
                {
                    Id = notificationId,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.Priority,
                    notification.CreatedAt,
                    notification.Metadata,
                    DeliveredAt = DateTime.UtcNow,
                    TargetType = "User",
                    TargetId = userId.ToString(),
                    RequiresConfirmation = true
                };

                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("NotificationReceived", enhancedNotification);

                deliveryResult.IsDelivered = true;
                deliveryResult.DeliveredAt = DateTime.UtcNow;
                
                // Update delivery status
                deliveryStatus.State = NotificationDeliveryState.Delivered;
                deliveryStatus.DeliveredAt = DateTime.UtcNow;

                _logger.LogInformation("Notification with confirmation sent to user {UserId}: {Title} (ID: {NotificationId})", 
                    userId, notification.Title, notificationId);
            }
            else
            {
                // Queue for offline user
                await QueueNotificationForOfflineUserAsync(userId, notification);
                deliveryResult.IsDelivered = false;
                deliveryResult.ErrorMessage = "User is offline, notification queued";
                
                deliveryStatus.State = NotificationDeliveryState.Queued;
                
                _logger.LogInformation("User {UserId} is offline, notification queued: {Title} (ID: {NotificationId})", 
                    userId, notification.Title, notificationId);
            }
        }
        catch (Exception ex)
        {
            deliveryResult.IsDelivered = false;
            deliveryResult.ErrorMessage = ex.Message;
            deliveryResult.RetryCount++;

            // Update delivery status
            if (_deliveryStatus.TryGetValue(notificationId, out var status))
            {
                status.State = NotificationDeliveryState.Failed;
                status.ErrorMessage = ex.Message;
                status.RetryCount++;
            }

            _logger.LogError(ex, "Failed to send notification with confirmation to user {UserId}: {Title}", 
                userId, notification.Title);
        }

        return deliveryResult;
    }

    public async Task<List<NotificationDeliveryResult>> SendNotificationToGroupWithConfirmationAsync(string groupName, NotificationDto notification)
    {
        var results = new List<NotificationDeliveryResult>();
        var connections = NotificationHub.GetCurrentConnections();
        
        // Find users in the group
        var groupUsers = connections.Values
            .Where(c => IsUserInGroup(MapToCore(c), groupName))
            .Select(c => int.Parse(c.UserId))
            .Distinct()
            .ToList();

        foreach (var userId in groupUsers)
        {
            var result = await SendNotificationWithConfirmationAsync(userId, notification);
            results.Add(result);
        }

        _logger.LogInformation("Notification sent to group {GroupName} with {UserCount} users: {Title}", 
            groupName, groupUsers.Count, notification.Title);

        return results;
    }

    public async Task<List<NotificationDeliveryResult>> SendNotificationToBranchWithConfirmationAsync(int branchId, NotificationDto notification)
    {
        return await SendNotificationToGroupWithConfirmationAsync($"Branch_{branchId}", notification);
    }

    // Message queuing for offline users
    public async Task QueueNotificationForOfflineUserAsync(int userId, NotificationDto notification)
    {
        try
        {
            var queuedNotification = new QueuedNotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Notification = notification,
                QueuedAt = DateTime.UtcNow,
                Priority = notification.Priority,
                RetryCount = 0
            };

            _queuedNotifications.AddOrUpdate(userId, 
                new ConcurrentQueue<QueuedNotificationDto>(new[] { queuedNotification }),
                (key, existingQueue) =>
                {
                    existingQueue.Enqueue(queuedNotification);
                    return existingQueue;
                });

            _logger.LogInformation("Notification queued for offline user {UserId}: {Title} (Queue ID: {QueueId})", 
                userId, notification.Title, queuedNotification.Id);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue notification for user {UserId}: {Title}", userId, notification.Title);
            throw;
        }
    }

    public async Task ProcessQueuedNotificationsAsync(int userId)
    {
        try
        {
            if (!_queuedNotifications.TryGetValue(userId, out var queue) || queue.IsEmpty)
            {
                return;
            }

            var processedCount = 0;
            var maxProcessCount = 10; // Limit to prevent overwhelming the user

            while (queue.TryDequeue(out var queuedNotification) && processedCount < maxProcessCount)
            {
                try
                {
                    await SendNotificationToUserAsync(userId, queuedNotification.Notification);
                    processedCount++;
                    
                    _logger.LogDebug("Processed queued notification for user {UserId}: {Title}", 
                        userId, queuedNotification.Notification.Title);
                }
                catch (Exception ex)
                {
                    // Re-queue with retry logic
                    queuedNotification.RetryCount++;
                    if (queuedNotification.RetryCount < 3)
                    {
                        queuedNotification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, queuedNotification.RetryCount));
                        queue.Enqueue(queuedNotification);
                    }
                    else
                    {
                        // Move to failed notifications
                        var failedNotification = new FailedNotificationDto
                        {
                            NotificationId = queuedNotification.Id,
                            UserId = userId,
                            Notification = queuedNotification.Notification,
                            ErrorMessage = ex.Message,
                            FailedAt = DateTime.UtcNow,
                            RetryCount = queuedNotification.RetryCount
                        };
                        _failedNotifications.Enqueue(failedNotification);
                    }

                    _logger.LogWarning(ex, "Failed to process queued notification for user {UserId}: {Title} (Retry: {RetryCount})", 
                        userId, queuedNotification.Notification.Title, queuedNotification.RetryCount);
                }
            }

            if (processedCount > 0)
            {
                _logger.LogInformation("Processed {ProcessedCount} queued notifications for user {UserId}", 
                    processedCount, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process queued notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<QueuedNotificationDto>> GetQueuedNotificationsAsync(int userId)
    {
        try
        {
            if (_queuedNotifications.TryGetValue(userId, out var queue))
            {
                return queue.ToList();
            }
            return new List<QueuedNotificationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queued notifications for user {UserId}", userId);
            return new List<QueuedNotificationDto>();
        }
    }

    public async Task ClearQueuedNotificationsAsync(int userId)
    {
        try
        {
            if (_queuedNotifications.TryRemove(userId, out var queue))
            {
                var count = queue.Count;
                _logger.LogInformation("Cleared {Count} queued notifications for user {UserId}", count, userId);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear queued notifications for user {UserId}", userId);
            throw;
        }
    }

    // Delivery status tracking
    public async Task<NotificationDeliveryStatus> GetDeliveryStatusAsync(string notificationId)
    {
        try
        {
            if (_deliveryStatus.TryGetValue(notificationId, out var status))
            {
                return status;
            }
            
            return new NotificationDeliveryStatus
            {
                NotificationId = notificationId,
                State = NotificationDeliveryState.Pending
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery status for notification {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<List<NotificationDeliveryStatus>> GetUserDeliveryHistoryAsync(int userId, int limit = 50)
    {
        try
        {
            var userDeliveries = _deliveryStatus.Values
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .Take(limit)
                .ToList();

            return userDeliveries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery history for user {UserId}", userId);
            return new List<NotificationDeliveryStatus>();
        }
    }

    public async Task ConfirmNotificationDeliveryAsync(string notificationId, int userId)
    {
        try
        {
            if (_deliveryStatus.TryGetValue(notificationId, out var status) && status.UserId == userId)
            {
                status.State = NotificationDeliveryState.Confirmed;
                status.ConfirmedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Notification delivery confirmed by user {UserId}: {NotificationId}", 
                    userId, notificationId);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm notification delivery for {NotificationId} by user {UserId}", 
                notificationId, userId);
            throw;
        }
    }

    public async Task ConfirmNotificationReadAsync(string notificationId, int userId)
    {
        try
        {
            if (_deliveryStatus.TryGetValue(notificationId, out var status) && status.UserId == userId)
            {
                status.State = NotificationDeliveryState.Read;
                status.ReadAt = DateTime.UtcNow;
                
                _logger.LogInformation("Notification read confirmed by user {UserId}: {NotificationId}", 
                    userId, notificationId);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm notification read for {NotificationId} by user {UserId}", 
                notificationId, userId);
            throw;
        }
    }

    // Connection management
    public async Task<List<StrideHR.Core.Models.Notification.UserConnectionInfo>> GetActiveConnectionsAsync()
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            return connections.Values.Select(MapToCore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active connections");
            return new List<StrideHR.Core.Models.Notification.UserConnectionInfo>();
        }
    }

    public async Task<List<StrideHR.Core.Models.Notification.UserConnectionInfo>> GetUserConnectionsAsync(int userId)
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            return connections.Values
                .Where(c => c.UserId == userId.ToString())
                .Select(MapToCore)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connections for user {UserId}", userId);
            return new List<StrideHR.Core.Models.Notification.UserConnectionInfo>();
        }
    }

    public async Task<bool> IsUserOnlineAsync(int userId)
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            return connections.Values.Any(c => c.UserId == userId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} is online", userId);
            return false;
        }
    }

    public async Task<int> GetOnlineUsersCountAsync()
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            return connections.Values.Select(c => c.UserId).Distinct().Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online users count");
            return 0;
        }
    }

    public async Task<List<int>> GetOnlineUserIdsAsync()
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            return connections.Values
                .Select(c => int.Parse(c.UserId))
                .Distinct()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online user IDs");
            return new List<int>();
        }
    }

    // Bulk operations
    public async Task<List<NotificationDeliveryResult>> SendBulkNotificationAsync(List<int> userIds, NotificationDto notification)
    {
        var results = new List<NotificationDeliveryResult>();
        
        foreach (var userId in userIds)
        {
            var result = await SendNotificationWithConfirmationAsync(userId, notification);
            results.Add(result);
        }

        _logger.LogInformation("Bulk notification sent to {UserCount} users: {Title}", 
            userIds.Count, notification.Title);

        return results;
    }

    public async Task<List<NotificationDeliveryResult>> SendNotificationToMultipleGroupsAsync(List<string> groupNames, NotificationDto notification)
    {
        var allResults = new List<NotificationDeliveryResult>();
        
        foreach (var groupName in groupNames)
        {
            var results = await SendNotificationToGroupWithConfirmationAsync(groupName, notification);
            allResults.AddRange(results);
        }

        _logger.LogInformation("Notification sent to {GroupCount} groups: {Title}", 
            groupNames.Count, notification.Title);

        return allResults;
    }

    // Priority and retry mechanisms
    public async Task SendHighPriorityNotificationAsync(int userId, NotificationDto notification)
    {
        try
        {
            // Set high priority
            notification.Priority = NotificationPriority.Critical;
            
            var result = await SendNotificationWithConfirmationAsync(userId, notification);
            
            if (!result.IsDelivered)
            {
                // For high priority notifications, try alternative delivery methods
                await QueueNotificationForOfflineUserAsync(userId, notification);
                
                // Could also trigger email/SMS backup here
                _logger.LogWarning("High priority notification failed to deliver immediately to user {UserId}, queued for retry: {Title}", 
                    userId, notification.Title);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send high priority notification to user {UserId}: {Title}", 
                userId, notification.Title);
            throw;
        }
    }

    public async Task RetryFailedNotificationsAsync()
    {
        try
        {
            var retryCount = 0;
            var maxRetries = 50; // Limit batch size

            while (_failedNotifications.TryDequeue(out var failedNotification) && retryCount < maxRetries)
            {
                try
                {
                    if (failedNotification.NextRetryAt.HasValue && failedNotification.NextRetryAt > DateTime.UtcNow)
                    {
                        // Not ready for retry yet, re-queue
                        _failedNotifications.Enqueue(failedNotification);
                        continue;
                    }

                    var result = await SendNotificationWithConfirmationAsync(failedNotification.UserId, failedNotification.Notification);
                    
                    if (result.IsDelivered)
                    {
                        retryCount++;
                        _logger.LogInformation("Successfully retried failed notification for user {UserId}: {Title}", 
                            failedNotification.UserId, failedNotification.Notification.Title);
                    }
                    else
                    {
                        // Still failed, update retry info
                        failedNotification.RetryCount++;
                        failedNotification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, failedNotification.RetryCount));
                        
                        if (failedNotification.RetryCount < 5) // Max 5 retries
                        {
                            _failedNotifications.Enqueue(failedNotification);
                        }
                        else
                        {
                            _logger.LogWarning("Notification permanently failed after {RetryCount} retries for user {UserId}: {Title}", 
                                failedNotification.RetryCount, failedNotification.UserId, failedNotification.Notification.Title);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during notification retry for user {UserId}: {Title}", 
                        failedNotification.UserId, failedNotification.Notification.Title);
                    
                    // Re-queue for later retry
                    failedNotification.RetryCount++;
                    failedNotification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, failedNotification.RetryCount));
                    
                    if (failedNotification.RetryCount < 5)
                    {
                        _failedNotifications.Enqueue(failedNotification);
                    }
                }
            }

            if (retryCount > 0)
            {
                _logger.LogInformation("Retried {RetryCount} failed notifications", retryCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry failed notifications");
            throw;
        }
    }

    public async Task<List<FailedNotificationDto>> GetFailedNotificationsAsync(int limit = 100)
    {
        try
        {
            return _failedNotifications.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failed notifications");
            return new List<FailedNotificationDto>();
        }
    }

    // Helper methods
    private bool IsUserInGroup(StrideHR.Core.Models.Notification.UserConnectionInfo connection, string groupName)
    {
        if (groupName.StartsWith("User_"))
        {
            var userId = groupName.Substring(5);
            return connection.UserId == userId;
        }
        
        if (groupName.StartsWith("Branch_"))
        {
            var branchId = groupName.Substring(7);
            return connection.BranchId == branchId;
        }
        
        if (groupName.StartsWith("Organization_"))
        {
            var organizationId = groupName.Substring(13);
            return connection.OrganizationId == organizationId;
        }
        
        if (groupName.StartsWith("Role_"))
        {
            var role = groupName.Substring(5);
            return connection.Roles.Contains(role);
        }
        
        return false;
    }

    private StrideHR.Core.Models.Notification.UserConnectionInfo MapToCore(StrideHR.API.Hubs.UserConnectionInfo hubConnection)
    {
        return new StrideHR.Core.Models.Notification.UserConnectionInfo
        {
            ConnectionId = hubConnection.ConnectionId,
            UserId = hubConnection.UserId,
            EmployeeId = hubConnection.EmployeeId,
            BranchId = hubConnection.BranchId,
            OrganizationId = hubConnection.OrganizationId,
            Roles = hubConnection.Roles,
            ConnectedAt = hubConnection.ConnectedAt,
            UserAgent = hubConnection.UserAgent,
            IpAddress = hubConnection.IpAddress
        };
    }
}