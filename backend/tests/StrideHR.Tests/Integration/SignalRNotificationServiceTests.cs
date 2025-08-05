using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Hubs;
using StrideHR.API.Services;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Notification;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Integration;

public class SignalRNotificationServiceTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<SignalRNotificationService>> _mockLogger;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly SignalRNotificationService _service;

    public SignalRNotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<SignalRNotificationService>>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);

        _service = new SignalRNotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendNotificationToUserAsync_ShouldSendNotificationSuccessfully()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _service.SendNotificationToUserAsync(userId, notification);

        // Assert
        _mockClients.Verify(c => c.Group($"User_{userId}"), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("NotificationReceived", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithConfirmationAsync_ShouldReturnDeliveryResult()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.SendNotificationWithConfirmationAsync(userId, notification);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.DeliveryMethod.Should().Be(NotificationDeliveryMethod.SignalR);
        result.NotificationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task QueueNotificationForOfflineUserAsync_ShouldQueueNotification()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _service.QueueNotificationForOfflineUserAsync(userId, notification);

        // Assert
        var queuedNotifications = await _service.GetQueuedNotificationsAsync(userId);
        queuedNotifications.Should().HaveCount(1);
        queuedNotifications.First().Notification.Title.Should().Be(notification.Title);
    }

    [Fact]
    public async Task SendBulkNotificationAsync_ShouldSendToAllUsers()
    {
        // Arrange
        var userIds = new List<int> { 123, 456, 789 };
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Bulk Notification",
            Message = "This is a bulk message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var results = await _service.SendBulkNotificationAsync(userIds, notification);

        // Assert
        results.Should().HaveCount(3);
        results.All(r => r.NotificationId != null).Should().BeTrue();
        results.Select(r => r.UserId).Should().BeEquivalentTo(userIds);
    }

    [Fact]
    public async Task SendHighPriorityNotificationAsync_ShouldSetCriticalPriority()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "High Priority Notification",
            Message = "This is urgent",
            Type = NotificationType.SecurityAlert,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _service.SendHighPriorityNotificationAsync(userId, notification);

        // Assert
        notification.Priority.Should().Be(NotificationPriority.Critical);
        _mockClients.Verify(c => c.Group($"User_{userId}"), Times.Once);
    }

    [Fact]
    public async Task GetActiveConnectionsAsync_ShouldReturnEmptyListWhenNoConnections()
    {
        // Act
        var connections = await _service.GetActiveConnectionsAsync();

        // Assert
        connections.Should().NotBeNull();
        connections.Should().BeEmpty();
    }

    [Fact]
    public async Task IsUserOnlineAsync_ShouldReturnFalseWhenUserNotConnected()
    {
        // Arrange
        var userId = 123;

        // Act
        var isOnline = await _service.IsUserOnlineAsync(userId);

        // Assert
        isOnline.Should().BeFalse();
    }

    [Fact]
    public async Task GetOnlineUsersCountAsync_ShouldReturnZeroWhenNoUsers()
    {
        // Act
        var count = await _service.GetOnlineUsersCountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task ConfirmNotificationDeliveryAsync_ShouldUpdateDeliveryStatus()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Send notification first to create delivery status
        var result = await _service.SendNotificationWithConfirmationAsync(userId, notification);

        // Act
        await _service.ConfirmNotificationDeliveryAsync(result.NotificationId, userId);

        // Assert
        var status = await _service.GetDeliveryStatusAsync(result.NotificationId);
        status.State.Should().Be(NotificationDeliveryState.Confirmed);
        status.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmNotificationReadAsync_ShouldUpdateReadStatus()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Send notification first to create delivery status
        var result = await _service.SendNotificationWithConfirmationAsync(userId, notification);

        // Act
        await _service.ConfirmNotificationReadAsync(result.NotificationId, userId);

        // Assert
        var status = await _service.GetDeliveryStatusAsync(result.NotificationId);
        status.State.Should().Be(NotificationDeliveryState.Read);
        status.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ClearQueuedNotificationsAsync_ShouldRemoveAllQueuedNotifications()
    {
        // Arrange
        var userId = 123;
        var notification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        await _service.QueueNotificationForOfflineUserAsync(userId, notification);

        // Act
        await _service.ClearQueuedNotificationsAsync(userId);

        // Assert
        var queuedNotifications = await _service.GetQueuedNotificationsAsync(userId);
        queuedNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task SendSystemMaintenanceNotificationAsync_ShouldSendToAllClients()
    {
        // Arrange
        var message = "System maintenance scheduled";
        var scheduledTime = DateTime.UtcNow.AddHours(2);

        // Act
        await _service.SendSystemMaintenanceNotificationAsync(message, scheduledTime);

        // Assert
        _mockClients.Verify(c => c.All, Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("SystemMaintenanceNotification", It.IsAny<object[]>(), default),
            Times.Once);
    }
}