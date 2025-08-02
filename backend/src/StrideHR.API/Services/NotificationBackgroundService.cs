using StrideHR.Core.Interfaces.Services;

namespace StrideHR.API.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _birthdayCheckInterval = TimeSpan.FromHours(1); // Check every hour
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Cleanup every 6 hours

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service started");

        var birthdayTimer = new PeriodicTimer(_birthdayCheckInterval);
        var cleanupTimer = new PeriodicTimer(_cleanupInterval);

        var birthdayTask = RunBirthdayNotifications(birthdayTimer, stoppingToken);
        var cleanupTask = RunNotificationCleanup(cleanupTimer, stoppingToken);

        await Task.WhenAll(birthdayTask, cleanupTask);
    }

    private async Task RunBirthdayNotifications(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        var lastBirthdayCheck = DateTime.MinValue;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var now = DateTime.Now;
                
                // Only send birthday notifications once per day at 9 AM
                if (now.Hour == 9 && lastBirthdayCheck.Date != now.Date)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    await notificationService.SendBirthdayNotificationsAsync();
                    lastBirthdayCheck = now;

                    _logger.LogInformation("Birthday notifications sent at {Time}", now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending birthday notifications");
            }
        }
    }

    private async Task RunNotificationCleanup(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                await notificationService.CleanupExpiredNotificationsAsync();

                _logger.LogInformation("Notification cleanup completed at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification cleanup");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}