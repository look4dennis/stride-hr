using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.API.Models;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly NotificationController _controller;

    public NotificationControllerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _controller = new NotificationController(_mockNotificationService.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("BranchId", "456")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetNotifications_ValidRequest_ReturnsNotifications()
    {
        // Arrange
        var userId = 123;
        var notifications = new List<NotificationDto>
        {
            new NotificationDto
            {
                Id = 1,
                Title = "Test Notification",
                Message = "Test Message",
                Type = NotificationType.Announcement,
                Priority = NotificationPriority.Normal,
                Channel = NotificationChannel.InApp
            }
        };

        _mockNotificationService.Setup(s => s.GetUserNotificationsAsync(userId, false))
            .ReturnsAsync(notifications);

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<NotificationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data);
        Assert.Equal("Test Notification", response.Data[0].Title);
    }

    [Fact]
    public async Task GetNotification_ExistingId_ReturnsNotification()
    {
        // Arrange
        var notificationId = 1;
        var notification = new NotificationDto
        {
            Id = notificationId,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        };

        _mockNotificationService.Setup(s => s.GetNotificationByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.GetNotification(notificationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<NotificationDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Test Notification", response.Data.Title);
    }

    [Fact]
    public async Task GetNotification_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var notificationId = 999;

        _mockNotificationService.Setup(s => s.GetNotificationByIdAsync(notificationId))
            .ReturnsAsync((NotificationDto?)null);

        // Act
        var result = await _controller.GetNotification(notificationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<NotificationDto>>(notFoundResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Notification not found", response.Message);
    }

    [Fact]
    public async Task CreateNotification_ValidDto_ReturnsCreatedNotification()
    {
        // Arrange
        var dto = new CreateNotificationDto
        {
            UserId = 1,
            Title = "New Notification",
            Message = "New Message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        };

        var createdNotification = new NotificationDto
        {
            Id = 1,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Priority = dto.Priority,
            Channel = dto.Channel
        };

        _mockNotificationService.Setup(s => s.CreateNotificationAsync(dto))
            .ReturnsAsync(createdNotification);

        // Act
        var result = await _controller.CreateNotification(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<NotificationDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.Equal("New Notification", response.Data.Title);
        Assert.Equal("Notification created successfully", response.Message);
    }

    [Fact]
    public async Task MarkAsRead_ValidId_ReturnsSuccess()
    {
        // Arrange
        var notificationId = 1;
        var userId = 123;

        _mockNotificationService.Setup(s => s.MarkAsReadAsync(notificationId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Equal("Notification marked as read", response.Message);
    }

    [Fact]
    public async Task MarkAsRead_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var notificationId = 999;
        var userId = 123;

        _mockNotificationService.Setup(s => s.MarkAsReadAsync(notificationId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(notFoundResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Notification not found or access denied", response.Message);
    }

    [Fact]
    public async Task GetUnreadCount_ValidUser_ReturnsCount()
    {
        // Arrange
        var userId = 123;
        var expectedCount = 5;

        _mockNotificationService.Setup(s => s.GetUnreadCountAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _controller.GetUnreadCount();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<int>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(expectedCount, response.Data);
        Assert.Equal("Unread count retrieved successfully", response.Message);
    }

    [Fact]
    public async Task GetTodaysBirthdays_ValidRequest_ReturnsBirthdays()
    {
        // Arrange
        var branchId = 456;
        var birthdays = new List<BirthdayNotificationDto>
        {
            new BirthdayNotificationDto
            {
                EmployeeId = 1,
                EmployeeName = "John Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Age = 34,
                Department = "IT",
                Designation = "Developer"
            }
        };

        _mockNotificationService.Setup(s => s.GetTodaysBirthdaysAsync(branchId))
            .ReturnsAsync(birthdays);

        // Act
        var result = await _controller.GetTodaysBirthdays();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<BirthdayNotificationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data);
        Assert.Equal("John Doe", response.Data[0].EmployeeName);
    }

    [Fact]
    public async Task SendBirthdayWish_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new SendBirthdayWishDto
        {
            ToUserId = 2,
            Message = "Happy Birthday!"
        };

        var fromUserId = 123;

        _mockNotificationService.Setup(s => s.SendBirthdayWishAsync(fromUserId, dto.ToUserId, dto.Message))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SendBirthdayWish(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Equal("Birthday wish sent successfully", response.Message);
    }

    [Fact]
    public async Task CreateBulkNotifications_ValidDto_ReturnsCreatedNotifications()
    {
        // Arrange
        var dto = new BulkNotificationDto
        {
            UserIds = new List<int> { 1, 2, 3 },
            Title = "Bulk Notification",
            Message = "Bulk Message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        };

        var createdNotifications = new List<NotificationDto>
        {
            new NotificationDto { Id = 1, Title = dto.Title, Message = dto.Message },
            new NotificationDto { Id = 2, Title = dto.Title, Message = dto.Message },
            new NotificationDto { Id = 3, Title = dto.Title, Message = dto.Message }
        };

        _mockNotificationService.Setup(s => s.CreateBulkNotificationAsync(dto))
            .ReturnsAsync(createdNotifications);

        // Act
        var result = await _controller.CreateBulkNotifications(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<NotificationDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(3, response.Data.Count);
        Assert.Equal("3 notifications created successfully", response.Message);
    }

    [Fact]
    public async Task SendProductivityAlert_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new ProductivityAlertDto
        {
            EmployeeId = 1,
            EmployeeName = "John Doe",
            IdlePercentage = 75.5m,
            IdleDuration = TimeSpan.FromHours(2),
            LastActivity = DateTime.UtcNow.AddHours(-2),
            AlertType = "High Idle Time"
        };

        _mockNotificationService.Setup(s => s.SendProductivityAlertAsync(dto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SendProductivityAlert(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Equal("Productivity alert sent successfully", response.Message);
    }
}