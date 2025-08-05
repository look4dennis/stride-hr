using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Hubs;
using StrideHR.API.Services;
using StrideHR.Core.Interfaces.Services;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Integration;

public class SignalRConnectionRecoveryTests : IDisposable
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<IRealTimeNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<SignalRConnectionRecoveryService>> _mockLogger;
    private readonly SignalRConnectionRecoveryService _recoveryService;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public SignalRConnectionRecoveryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockNotificationService = new Mock<IRealTimeNotificationService>();
        _mockLogger = new Mock<ILogger<SignalRConnectionRecoveryService>>();
        _cancellationTokenSource = new CancellationTokenSource();

        // Setup service provider chain
        _mockServiceProvider.Setup(sp => sp.GetRequiredService<IServiceScopeFactory>())
            .Returns(_mockServiceScopeFactory.Object);
        _mockServiceScopeFactory.Setup(sf => sf.CreateScope())
            .Returns(_mockServiceScope.Object);
        _mockServiceScope.Setup(s => s.ServiceProvider)
            .Returns(_mockServiceProvider.Object);

        // Setup service resolution
        _mockServiceProvider.Setup(sp => sp.GetRequiredService<IHubContext<NotificationHub>>())
            .Returns(_mockHubContext.Object);
        _mockServiceProvider.Setup(sp => sp.GetRequiredService<IRealTimeNotificationService>())
            .Returns(_mockNotificationService.Object);

        _recoveryService = new SignalRConnectionRecoveryService(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetConnectionHealth_WithExistingConnection_ShouldReturnHealthInfo()
    {
        // Arrange
        var connectionId = "test-connection-123";
        
        // Simulate a connection being tracked (this would normally happen through the hub)
        // For testing, we'll use reflection to access the private static field
        var connectionHealthField = typeof(SignalRConnectionRecoveryService)
            .GetField("_connectionHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (connectionHealthField != null)
        {
            var connectionHealth = connectionHealthField.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, ConnectionHealthInfo>;
            connectionHealth?.TryAdd(connectionId, new ConnectionHealthInfo
            {
                ConnectionId = connectionId,
                UserId = "123",
                IsHealthy = true,
                LastSeen = DateTime.UtcNow
            });
        }

        // Act
        var healthInfo = SignalRConnectionRecoveryService.GetConnectionHealth(connectionId);

        // Assert
        healthInfo.Should().NotBeNull();
        healthInfo!.ConnectionId.Should().Be(connectionId);
        healthInfo.UserId.Should().Be("123");
        healthInfo.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public void GetConnectionHealth_WithNonExistentConnection_ShouldReturnNull()
    {
        // Arrange
        var connectionId = "non-existent-connection";

        // Act
        var healthInfo = SignalRConnectionRecoveryService.GetConnectionHealth(connectionId);

        // Assert
        healthInfo.Should().BeNull();
    }

    [Fact]
    public void RecordHeartbeatResponse_ShouldUpdateConnectionHealth()
    {
        // Arrange
        var connectionId = "test-connection-456";
        
        // Add initial connection health
        var connectionHealthField = typeof(SignalRConnectionRecoveryService)
            .GetField("_connectionHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (connectionHealthField != null)
        {
            var connectionHealth = connectionHealthField.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, ConnectionHealthInfo>;
            connectionHealth?.TryAdd(connectionId, new ConnectionHealthInfo
            {
                ConnectionId = connectionId,
                UserId = "456",
                IsHealthy = false,
                ConsecutiveFailures = 2,
                LastSeen = DateTime.UtcNow.AddMinutes(-5)
            });
        }

        // Act
        SignalRConnectionRecoveryService.RecordHeartbeatResponse(connectionId);

        // Assert
        var healthInfo = SignalRConnectionRecoveryService.GetConnectionHealth(connectionId);
        healthInfo.Should().NotBeNull();
        healthInfo!.IsHealthy.Should().BeTrue();
        healthInfo.ConsecutiveFailures.Should().Be(0);
        healthInfo.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetAllConnectionHealth_ShouldReturnAllConnections()
    {
        // Arrange
        var connectionId1 = "connection-1";
        var connectionId2 = "connection-2";
        
        var connectionHealthField = typeof(SignalRConnectionRecoveryService)
            .GetField("_connectionHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (connectionHealthField != null)
        {
            var connectionHealth = connectionHealthField.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, ConnectionHealthInfo>;
            connectionHealth?.Clear(); // Clear any existing data
            
            connectionHealth?.TryAdd(connectionId1, new ConnectionHealthInfo
            {
                ConnectionId = connectionId1,
                UserId = "123",
                IsHealthy = true
            });
            
            connectionHealth?.TryAdd(connectionId2, new ConnectionHealthInfo
            {
                ConnectionId = connectionId2,
                UserId = "456",
                IsHealthy = false
            });
        }

        // Act
        var allHealth = SignalRConnectionRecoveryService.GetAllConnectionHealth();

        // Assert
        allHealth.Should().HaveCount(2);
        allHealth.Should().ContainKey(connectionId1);
        allHealth.Should().ContainKey(connectionId2);
        allHealth[connectionId1].IsHealthy.Should().BeTrue();
        allHealth[connectionId2].IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessRecoveryPeriodically()
    {
        // Arrange
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

        // Setup notification service to return some online users
        _mockNotificationService.Setup(ns => ns.GetOnlineUserIdsAsync())
            .ReturnsAsync(new List<int> { 123, 456 });

        // Cancel after a short delay to prevent infinite execution
        _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert - Should not throw and should call the required services
        var executeTask = _recoveryService.StartAsync(_cancellationTokenSource.Token);
        
        // Wait a bit for the service to start
        await Task.Delay(50);
        
        // Stop the service
        await _recoveryService.StopAsync(_cancellationTokenSource.Token);

        // Verify that the hub context was accessed (indicating the service ran)
        _mockServiceProvider.Verify(sp => sp.GetRequiredService<IHubContext<NotificationHub>>(), Times.AtLeastOnce);
    }

    [Fact]
    public void ConnectionHealthInfo_ShouldInitializeWithDefaults()
    {
        // Act
        var healthInfo = new ConnectionHealthInfo();

        // Assert
        healthInfo.ConnectionId.Should().BeEmpty();
        healthInfo.UserId.Should().BeEmpty();
        healthInfo.IsHealthy.Should().BeFalse();
        healthInfo.ConsecutiveFailures.Should().Be(0);
        healthInfo.RecoveryAttempts.Should().Be(0);
        healthInfo.LastRecoveryAttempt.Should().BeNull();
        healthInfo.ConnectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConnectionHealthInfo_ShouldAllowPropertyUpdates()
    {
        // Arrange
        var healthInfo = new ConnectionHealthInfo();
        var testTime = DateTime.UtcNow;

        // Act
        healthInfo.ConnectionId = "test-123";
        healthInfo.UserId = "user-456";
        healthInfo.IsHealthy = true;
        healthInfo.ConsecutiveFailures = 3;
        healthInfo.RecoveryAttempts = 2;
        healthInfo.LastRecoveryAttempt = testTime;
        healthInfo.LastSeen = testTime;

        // Assert
        healthInfo.ConnectionId.Should().Be("test-123");
        healthInfo.UserId.Should().Be("user-456");
        healthInfo.IsHealthy.Should().BeTrue();
        healthInfo.ConsecutiveFailures.Should().Be(3);
        healthInfo.RecoveryAttempts.Should().Be(2);
        healthInfo.LastRecoveryAttempt.Should().Be(testTime);
        healthInfo.LastSeen.Should().Be(testTime);
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _recoveryService?.Dispose();
    }
}