using Microsoft.AspNetCore.SignalR;
using StrideHR.API.Hubs;
using StrideHR.Core.Interfaces.Services;
using System.Collections.Concurrent;

namespace StrideHR.API.Services;

public class SignalRConnectionRecoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SignalRConnectionRecoveryService> _logger;
    private readonly TimeSpan _recoveryInterval = TimeSpan.FromSeconds(30);
    
    // Track connection health and recovery attempts
    private static readonly ConcurrentDictionary<string, ConnectionHealthInfo> _connectionHealth = new();
    private static readonly ConcurrentDictionary<string, DateTime> _lastHeartbeat = new();

    public SignalRConnectionRecoveryService(
        IServiceProvider serviceProvider,
        ILogger<SignalRConnectionRecoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SignalR Connection Recovery Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
                var notificationService = scope.ServiceProvider.GetRequiredService<IRealTimeNotificationService>();

                // Check connection health
                await CheckConnectionHealth(hubContext);

                // Process connection recovery
                await ProcessConnectionRecovery(notificationService);

                // Clean up stale connection data
                await CleanupStaleConnections();

                // Send periodic heartbeat to detect connection issues
                await SendHeartbeatToConnections(hubContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during connection recovery processing");
            }

            await Task.Delay(_recoveryInterval, stoppingToken);
        }

        _logger.LogInformation("SignalR Connection Recovery Service stopped");
    }

    private async Task CheckConnectionHealth(IHubContext<NotificationHub> hubContext)
    {
        try
        {
            var currentConnections = NotificationHub.GetCurrentConnections();
            var currentTime = DateTime.UtcNow;

            foreach (var connection in currentConnections.Values)
            {
                var connectionId = connection.ConnectionId;
                
                // Update or create health info
                _connectionHealth.AddOrUpdate(connectionId, 
                    new ConnectionHealthInfo
                    {
                        ConnectionId = connectionId,
                        UserId = connection.UserId,
                        LastSeen = currentTime,
                        IsHealthy = true,
                        ConsecutiveFailures = 0
                    },
                    (key, existing) =>
                    {
                        existing.LastSeen = currentTime;
                        return existing;
                    });
            }

            // Mark connections as unhealthy if they haven't been seen recently
            var unhealthyThreshold = currentTime.AddMinutes(-2);
            foreach (var healthInfo in _connectionHealth.Values)
            {
                if (healthInfo.LastSeen < unhealthyThreshold && healthInfo.IsHealthy)
                {
                    healthInfo.IsHealthy = false;
                    healthInfo.ConsecutiveFailures++;
                    
                    _logger.LogWarning("Connection {ConnectionId} for user {UserId} marked as unhealthy", 
                        healthInfo.ConnectionId, healthInfo.UserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connection health");
        }
    }

    private async Task ProcessConnectionRecovery(IRealTimeNotificationService notificationService)
    {
        try
        {
            var currentTime = DateTime.UtcNow;
            var recoveryThreshold = currentTime.AddMinutes(-5);

            foreach (var healthInfo in _connectionHealth.Values.Where(h => !h.IsHealthy))
            {
                if (healthInfo.LastRecoveryAttempt.HasValue && 
                    healthInfo.LastRecoveryAttempt.Value > recoveryThreshold)
                {
                    continue; // Too soon to retry
                }

                try
                {
                    // Attempt to recover connection by processing queued notifications
                    if (int.TryParse(healthInfo.UserId, out var userId))
                    {
                        await notificationService.ProcessQueuedNotificationsAsync(userId);
                        
                        healthInfo.LastRecoveryAttempt = currentTime;
                        healthInfo.RecoveryAttempts++;
                        
                        _logger.LogInformation("Attempted connection recovery for user {UserId} (Attempt: {Attempts})", 
                            userId, healthInfo.RecoveryAttempts);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to recover connection for user {UserId}", healthInfo.UserId);
                    healthInfo.ConsecutiveFailures++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing connection recovery");
        }
    }

    private async Task CleanupStaleConnections()
    {
        try
        {
            var currentTime = DateTime.UtcNow;
            var staleThreshold = currentTime.AddMinutes(-10);
            var currentConnections = NotificationHub.GetCurrentConnections();

            // Remove health info for connections that no longer exist
            var staleConnectionIds = _connectionHealth.Keys
                .Where(id => !currentConnections.ContainsKey(id))
                .ToList();

            foreach (var connectionId in staleConnectionIds)
            {
                if (_connectionHealth.TryRemove(connectionId, out var healthInfo))
                {
                    _logger.LogDebug("Cleaned up stale connection health info for {ConnectionId}", connectionId);
                }
            }

            // Remove old heartbeat data
            var staleHeartbeats = _lastHeartbeat.Keys
                .Where(id => !currentConnections.ContainsKey(id) || 
                           (_lastHeartbeat.TryGetValue(id, out var lastBeat) && lastBeat < staleThreshold))
                .ToList();

            foreach (var connectionId in staleHeartbeats)
            {
                _lastHeartbeat.TryRemove(connectionId, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stale connections");
        }
    }

    private async Task SendHeartbeatToConnections(IHubContext<NotificationHub> hubContext)
    {
        try
        {
            var currentConnections = NotificationHub.GetCurrentConnections();
            var currentTime = DateTime.UtcNow;

            // Send heartbeat to all connections
            await hubContext.Clients.All.SendAsync("Heartbeat", new
            {
                Timestamp = currentTime,
                ServerTime = currentTime.ToString("O")
            });

            // Update last heartbeat sent time
            foreach (var connectionId in currentConnections.Keys)
            {
                _lastHeartbeat.AddOrUpdate(connectionId, currentTime, (key, existing) => currentTime);
            }

            _logger.LogDebug("Sent heartbeat to {ConnectionCount} connections", currentConnections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending heartbeat to connections");
        }
    }

    // Static methods for external access
    public static ConnectionHealthInfo? GetConnectionHealth(string connectionId)
    {
        _connectionHealth.TryGetValue(connectionId, out var healthInfo);
        return healthInfo;
    }

    public static Dictionary<string, ConnectionHealthInfo> GetAllConnectionHealth()
    {
        return _connectionHealth.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static void RecordHeartbeatResponse(string connectionId)
    {
        var currentTime = DateTime.UtcNow;
        _lastHeartbeat.AddOrUpdate(connectionId, currentTime, (key, existing) => currentTime);

        if (_connectionHealth.TryGetValue(connectionId, out var healthInfo))
        {
            healthInfo.LastSeen = currentTime;
            healthInfo.IsHealthy = true;
            healthInfo.ConsecutiveFailures = 0;
        }
    }
}

public class ConnectionHealthInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public bool IsHealthy { get; set; }
    public int ConsecutiveFailures { get; set; }
    public int RecoveryAttempts { get; set; }
    public DateTime? LastRecoveryAttempt { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}