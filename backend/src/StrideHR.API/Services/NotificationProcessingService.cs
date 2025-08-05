using StrideHR.Core.Interfaces.Services;

namespace StrideHR.API.Services;

public class NotificationProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationProcessingService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1); // Process every minute

    public NotificationProcessingService(
        IServiceProvider serviceProvider,
        ILogger<NotificationProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processing Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IRealTimeNotificationService>();

                // Process queued notifications for online users
                await ProcessQueuedNotificationsForOnlineUsers(notificationService);

                // Retry failed notifications
                await notificationService.RetryFailedNotificationsAsync();

                // Log processing statistics
                await LogProcessingStatistics(notificationService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification processing");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Notification Processing Service stopped");
    }

    private async Task ProcessQueuedNotificationsForOnlineUsers(IRealTimeNotificationService notificationService)
    {
        try
        {
            var onlineUserIds = await notificationService.GetOnlineUserIdsAsync();
            
            foreach (var userId in onlineUserIds)
            {
                var queuedNotifications = await notificationService.GetQueuedNotificationsAsync(userId);
                
                if (queuedNotifications.Any())
                {
                    await notificationService.ProcessQueuedNotificationsAsync(userId);
                    _logger.LogDebug("Processed queued notifications for online user {UserId}", userId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queued notifications for online users");
        }
    }

    private async Task LogProcessingStatistics(IRealTimeNotificationService notificationService)
    {
        try
        {
            var onlineUsersCount = await notificationService.GetOnlineUsersCountAsync();
            var failedNotifications = await notificationService.GetFailedNotificationsAsync(10);
            
            _logger.LogDebug("Notification Processing Stats - Online Users: {OnlineUsers}, Failed Notifications: {FailedCount}", 
                onlineUsersCount, failedNotifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging processing statistics");
        }
    }
}