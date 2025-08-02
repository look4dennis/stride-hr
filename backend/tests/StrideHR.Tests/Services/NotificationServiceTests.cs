using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Employee;
using StrideHR.Core.Models.Notification;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _mockNotificationRepository;
    private readonly Mock<INotificationTemplateRepository> _mockTemplateRepository;
    private readonly Mock<IUserNotificationPreferenceRepository> _mockPreferenceRepository;
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IRealTimeNotificationService> _mockRealTimeService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockTemplateRepository = new Mock<INotificationTemplateRepository>();
        _mockPreferenceRepository = new Mock<IUserNotificationPreferenceRepository>();
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockRealTimeService = new Mock<IRealTimeNotificationService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<NotificationService>>();

        _notificationService = new NotificationService(
            _mockNotificationRepository.Object,
            _mockTemplateRepository.Object,
            _mockPreferenceRepository.Object,
            _mockEmployeeService.Object,
            _mockRealTimeService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidDto_ReturnsNotificationDto()
    {
        // Arrange
        var dto = new CreateNotificationDto
        {
            UserId = 1,
            Title = "Test Notification",
            Message = "Test Message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        };

        var notification = new Notification
        {
            Id = 1,
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Priority = dto.Priority,
            Channel = dto.Channel,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        var notificationDto = new NotificationDto
        {
            Id = 1,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Priority = dto.Priority,
            Channel = dto.Channel
        };

        _mockNotificationRepository.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync(notification);
        _mockNotificationRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<NotificationDto>(It.IsAny<Notification>()))
            .Returns(notificationDto);
        _mockRealTimeService.Setup(r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.CreateNotificationAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Message, result.Message);
        Assert.Equal(dto.Type, result.Type);
        _mockNotificationRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _mockNotificationRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ValidUserId_ReturnsNotifications()
    {
        // Arrange
        var userId = 1;
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = 1,
                UserId = userId,
                Title = "Test 1",
                Message = "Message 1",
                Type = NotificationType.Announcement,
                Priority = NotificationPriority.Normal,
                Channel = NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            },
            new Notification
            {
                Id = 2,
                UserId = userId,
                Title = "Test 2",
                Message = "Message 2",
                Type = NotificationType.Reminder,
                Priority = NotificationPriority.High,
                Channel = NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            }
        };

        var notificationDtos = new List<NotificationDto>
        {
            new NotificationDto { Id = 1, Title = "Test 1", Message = "Message 1" },
            new NotificationDto { Id = 2, Title = "Test 2", Message = "Message 2" }
        };

        _mockNotificationRepository.Setup(r => r.GetUserNotificationsAsync(userId, false, 0, 50))
            .ReturnsAsync(notifications);
        _mockMapper.Setup(m => m.Map<List<NotificationDto>>(notifications))
            .Returns(notificationDtos);

        // Act
        var result = await _notificationService.GetUserNotificationsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Test 1", result[0].Title);
        Assert.Equal("Test 2", result[1].Title);
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidNotificationId_ReturnsTrue()
    {
        // Arrange
        var notificationId = 1;
        var userId = 1;

        _mockNotificationRepository.Setup(r => r.MarkAsReadAsync(notificationId, userId))
            .ReturnsAsync(true);
        _mockRealTimeService.Setup(r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result);
        _mockNotificationRepository.Verify(r => r.MarkAsReadAsync(notificationId, userId), Times.Once);
    }

    [Fact]
    public async Task SendBirthdayWishAsync_ValidUsers_SendsNotification()
    {
        // Arrange
        var fromUserId = 1;
        var toUserId = 2;
        var message = "Happy Birthday!";

        var fromEmployee = new Employee
        {
            Id = fromUserId,
            FirstName = "John",
            LastName = "Doe",
            ProfilePhoto = "photo1.jpg"
        };

        var toEmployee = new Employee
        {
            Id = toUserId,
            FirstName = "Jane",
            LastName = "Smith"
        };

        var notification = new Notification
        {
            Id = 1,
            UserId = toUserId,
            Title = $"Birthday Wish from {fromEmployee.FirstName} {fromEmployee.LastName}",
            Message = message,
            Type = NotificationType.BirthdayWishes
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(fromUserId))
            .ReturnsAsync(fromEmployee);
        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(toUserId))
            .ReturnsAsync(toEmployee);

        _mockNotificationRepository.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync(notification);
        _mockNotificationRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var notificationDto = new NotificationDto
        {
            Id = 1,
            Title = $"Birthday Wish from {fromEmployee.FirstName} {fromEmployee.LastName}",
            Message = message,
            Type = NotificationType.BirthdayWishes
        };

        _mockMapper.Setup(m => m.Map<NotificationDto>(It.IsAny<Notification>()))
            .Returns(notificationDto);

        _mockRealTimeService.Setup(r => r.SendBirthdayWishAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _notificationService.SendBirthdayWishAsync(fromUserId, toUserId, message);

        // Assert
        _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(fromUserId), Times.Once);
        _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(toUserId), Times.Once);
        _mockNotificationRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task SendAttendanceAlertAsync_ValidEmployee_CreatesNotification()
    {
        // Arrange
        var employeeId = 1;
        var alertType = NotificationType.AttendanceCheckIn;
        var metadata = new Dictionary<string, object> { ["Location"] = "Office" };

        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe"
        };

        var notification = new Notification
        {
            Id = 1,
            UserId = employeeId,
            Title = "Check-in Successful",
            Message = $"Welcome back, {employee.FirstName}! You have successfully checked in.",
            Type = alertType
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId))
            .ReturnsAsync(employee);

        _mockNotificationRepository.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync(notification);
        _mockNotificationRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var notificationDto = new NotificationDto
        {
            Id = 1,
            Title = "Check-in Successful",
            Message = $"Welcome back, {employee.FirstName}! You have successfully checked in.",
            Type = alertType
        };

        _mockMapper.Setup(m => m.Map<NotificationDto>(It.IsAny<Notification>()))
            .Returns(notificationDto);

        _mockRealTimeService.Setup(r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        // Act
        await _notificationService.SendAttendanceAlertAsync(employeeId, alertType, metadata);

        // Assert
        _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(employeeId), Times.Once);
        _mockNotificationRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ValidUserId_ReturnsCount()
    {
        // Arrange
        var userId = 1;
        var expectedCount = 5;

        _mockNotificationRepository.Setup(r => r.GetUnreadCountAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _notificationService.GetUnreadCountAsync(userId);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockNotificationRepository.Verify(r => r.GetUnreadCountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CreateBulkNotificationAsync_ValidDto_CreatesMultipleNotifications()
    {
        // Arrange
        var dto = new BulkNotificationDto
        {
            UserIds = new List<int> { 1, 2, 3 },
            Title = "Bulk Notification",
            Message = "This is a bulk message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        };

        var notifications = new List<Notification>
        {
            new Notification { Id = 1, Title = dto.Title, Message = dto.Message },
            new Notification { Id = 2, Title = dto.Title, Message = dto.Message },
            new Notification { Id = 3, Title = dto.Title, Message = dto.Message }
        };

        _mockNotificationRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()))
            .ReturnsAsync(notifications);
        _mockNotificationRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var notificationDtos = new List<NotificationDto>
        {
            new NotificationDto { Id = 1, Title = dto.Title, Message = dto.Message },
            new NotificationDto { Id = 2, Title = dto.Title, Message = dto.Message },
            new NotificationDto { Id = 3, Title = dto.Title, Message = dto.Message }
        };

        _mockMapper.Setup(m => m.Map<List<NotificationDto>>(It.IsAny<List<Notification>>()))
            .Returns(notificationDtos);

        _mockRealTimeService.Setup(r => r.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<NotificationDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _notificationService.CreateBulkNotificationAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockNotificationRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()), Times.Once);
        _mockNotificationRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTodaysBirthdaysAsync_ValidBranch_ReturnsBirthdayEmployees()
    {
        // Arrange
        var branchId = 1;
        var today = DateTime.Today;
        var employees = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, today.Month, today.Day),
                BranchId = branchId,
                Department = "IT",
                Designation = "Developer",
                ProfilePhoto = "photo1.jpg"
            },
            new EmployeeDto
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, today.Month, today.Day),
                BranchId = branchId,
                Department = "HR",
                Designation = "Manager"
            }
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeDtosAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _notificationService.GetTodaysBirthdaysAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("John Doe", result[0].EmployeeName);
        Assert.Equal("Jane Smith", result[1].EmployeeName);
        Assert.Equal(today.Year - 1990, result[0].Age);
        Assert.Equal(today.Year - 1985, result[1].Age);
    }
}