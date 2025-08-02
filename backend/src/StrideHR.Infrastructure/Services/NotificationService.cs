using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;

namespace StrideHR.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly IUserNotificationPreferenceRepository _preferenceRepository;
    private readonly IEmployeeService _employeeService;
    private readonly IRealTimeNotificationService _realTimeService;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationTemplateRepository templateRepository,
        IUserNotificationPreferenceRepository preferenceRepository,
        IEmployeeService employeeService,
        IRealTimeNotificationService realTimeService,
        IMapper mapper,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _templateRepository = templateRepository;
        _preferenceRepository = preferenceRepository;
        _employeeService = employeeService;
        _realTimeService = realTimeService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            BranchId = dto.BranchId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Priority = dto.Priority,
            Channel = dto.Channel,
            ActionUrl = dto.ActionUrl,
            Metadata = dto.Metadata,
            ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(30),
            IsGlobal = dto.IsGlobal,
            TargetRole = dto.TargetRole,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        var notificationDto = _mapper.Map<NotificationDto>(notification);

        // Send real-time notification
        if (dto.UserId.HasValue)
        {
            await SendRealTimeNotificationAsync(dto.UserId.Value, notificationDto);
        }
        else if (dto.IsGlobal)
        {
            await _realTimeService.SendNotificationToAllAsync(notificationDto);
        }
        else if (dto.BranchId.HasValue)
        {
            await SendRealTimeNotificationToBranchAsync(dto.BranchId.Value, notificationDto);
        }
        else if (!string.IsNullOrEmpty(dto.TargetRole))
        {
            await SendRealTimeNotificationToRoleAsync(dto.TargetRole, notificationDto);
        }

        _logger.LogInformation("Notification created: {NotificationId} for user {UserId}", notification.Id, dto.UserId);
        return notificationDto;
    }

    public async Task<List<NotificationDto>> CreateBulkNotificationAsync(BulkNotificationDto dto)
    {
        var notifications = new List<Notification>();
        var userIds = dto.UserIds ?? new List<int>();

        // If targeting by role, get users with that role
        if (!string.IsNullOrEmpty(dto.TargetRole) && !userIds.Any())
        {
            // This would need to be implemented based on your user/role system
            // For now, we'll use the provided userIds
        }

        // If targeting by branch, get users in that branch
        if (dto.BranchId.HasValue && !userIds.Any())
        {
            // This would need to be implemented to get users by branch
            // For now, we'll use the provided userIds
        }

        foreach (var userId in userIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                BranchId = dto.BranchId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                Channel = dto.Channel,
                ActionUrl = dto.ActionUrl,
                Metadata = dto.Metadata,
                ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(30),
                IsGlobal = false,
                TargetRole = dto.TargetRole,
                CreatedAt = DateTime.UtcNow
            };

            notifications.Add(notification);
        }

        await _notificationRepository.AddRangeAsync(notifications);
        await _notificationRepository.SaveChangesAsync();

        var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

        // Send real-time notifications
        foreach (var notificationDto in notificationDtos)
        {
            if (notificationDto.Id > 0)
            {
                var notification = notifications.First(n => n.Title == notificationDto.Title);
                if (notification.UserId.HasValue)
                {
                    await SendRealTimeNotificationAsync(notification.UserId.Value, notificationDto);
                }
            }
        }

        _logger.LogInformation("Bulk notifications created: {Count} notifications", notifications.Count);
        return notificationDtos;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, unreadOnly);
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        return notification != null ? _mapper.Map<NotificationDto>(notification) : null;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var result = await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        
        if (result)
        {
            // Send real-time update about read status
            var readNotification = new NotificationDto
            {
                Id = notificationId,
                Title = "Notification Read",
                Message = "Notification marked as read",
                Type = NotificationType.Reminder
            };
            await _realTimeService.SendNotificationToUserAsync(userId, readNotification);
        }

        return result;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var result = await _notificationRepository.MarkAllAsReadAsync(userId);
        
        if (result > 0)
        {
            // Send real-time update about all notifications being read
            var readAllNotification = new NotificationDto
            {
                Title = "All Notifications Read",
                Message = $"{result} notifications marked as read",
                Type = NotificationType.Reminder
            };
            await _realTimeService.SendNotificationToUserAsync(userId, readAllNotification);
        }

        return result > 0;
    }

    public async Task<bool> DeleteNotificationAsync(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
            return false;

        await _notificationRepository.DeleteAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return true;
    }

    public async Task SendRealTimeNotificationAsync(int userId, NotificationDto notification)
    {
        await _realTimeService.SendNotificationToUserAsync(userId, notification);
    }

    public async Task SendRealTimeNotificationToGroupAsync(string groupName, NotificationDto notification)
    {
        await _realTimeService.SendNotificationToGroupAsync(groupName, notification);
    }

    public async Task SendRealTimeNotificationToBranchAsync(int branchId, NotificationDto notification)
    {
        await _realTimeService.SendNotificationToBranchAsync(branchId, notification);
    }

    public async Task SendRealTimeNotificationToRoleAsync(string role, NotificationDto notification)
    {
        await _realTimeService.SendNotificationToRoleAsync(role, notification);
    }

    public async Task<List<BirthdayNotificationDto>> GetTodaysBirthdaysAsync(int? branchId = null)
    {
        var today = DateTime.Today;
        var employees = await _employeeService.GetEmployeeDtosAsync();

        var birthdayEmployees = employees.Where(e => 
            e.DateOfBirth.Month == today.Month && 
            e.DateOfBirth.Day == today.Day &&
            (branchId == null || e.BranchId == branchId))
            .ToList();

        var birthdayNotifications = birthdayEmployees.Select(e => new BirthdayNotificationDto
        {
            EmployeeId = e.Id,
            EmployeeName = $"{e.FirstName} {e.LastName}",
            ProfilePhoto = e.ProfilePhoto,
            DateOfBirth = e.DateOfBirth,
            Age = today.Year - e.DateOfBirth.Year,
            Department = e.Department,
            Designation = e.Designation
        }).ToList();

        return birthdayNotifications.ToList();
    }

    public async Task SendBirthdayNotificationsAsync()
    {
        var birthdayEmployees = await GetTodaysBirthdaysAsync();

        foreach (var birthdayEmployee in birthdayEmployees)
        {
            // Send birthday notification to the birthday person
            var personalNotification = new CreateNotificationDto
            {
                UserId = birthdayEmployee.EmployeeId,
                Title = $"Happy Birthday, {birthdayEmployee.EmployeeName.Split(' ')[0]}! 🎉",
                Message = $"Wishing you a wonderful birthday and a fantastic year ahead!",
                Type = NotificationType.BirthdayToday,
                Priority = NotificationPriority.High,
                Channel = NotificationChannel.InApp | NotificationChannel.Email,
                Metadata = new Dictionary<string, object>
                {
                    ["Age"] = birthdayEmployee.Age,
                    ["Department"] = birthdayEmployee.Department
                }
            };

            await CreateNotificationAsync(personalNotification);

            // Send notification to all branch members about the birthday
            var branchNotification = new CreateNotificationDto
            {
                BranchId = birthdayEmployee.EmployeeId, // This should be the actual branch ID
                Title = $"🎂 Today's Birthday",
                Message = $"It's {birthdayEmployee.EmployeeName}'s birthday today! Send your wishes!",
                Type = NotificationType.BirthdayToday,
                Priority = NotificationPriority.Normal,
                Channel = NotificationChannel.InApp,
                ActionUrl = $"/employee/{birthdayEmployee.EmployeeId}",
                Metadata = new Dictionary<string, object>
                {
                    ["BirthdayEmployeeId"] = birthdayEmployee.EmployeeId,
                    ["BirthdayEmployeeName"] = birthdayEmployee.EmployeeName,
                    ["ProfilePhoto"] = birthdayEmployee.ProfilePhoto ?? "",
                    ["Department"] = birthdayEmployee.Department
                }
            };

            await CreateNotificationAsync(branchNotification);
        }

        _logger.LogInformation("Birthday notifications sent for {Count} employees", birthdayEmployees.Count);
    }

    public async Task SendBirthdayWishAsync(int fromUserId, int toUserId, string message)
    {
        var fromEmployee = await _employeeService.GetEmployeeByIdAsync(fromUserId);
        var toEmployee = await _employeeService.GetEmployeeByIdAsync(toUserId);

        if (fromEmployee == null || toEmployee == null)
            return;

        var notification = new CreateNotificationDto
        {
            UserId = toUserId,
            Title = $"Birthday Wish from {fromEmployee.FirstName} {fromEmployee.LastName}",
            Message = message,
            Type = NotificationType.BirthdayWishes,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp,
            Metadata = new Dictionary<string, object>
            {
                ["FromUserId"] = fromUserId,
                ["FromUserName"] = $"{fromEmployee.FirstName} {fromEmployee.LastName}",
                ["FromUserPhoto"] = fromEmployee.ProfilePhoto ?? ""
            }
        };

        await CreateNotificationAsync(notification);

        // Also send via SignalR for immediate delivery
        await _realTimeService.SendBirthdayWishAsync(
            fromUserId, 
            toUserId, 
            $"{fromEmployee.FirstName} {fromEmployee.LastName}", 
            message);

        _logger.LogInformation("Birthday wish sent from {FromUserId} to {ToUserId}", fromUserId, toUserId);
    }

    public async Task SendAttendanceAlertAsync(int employeeId, NotificationType alertType, Dictionary<string, object>? metadata = null)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
            return;

        string title = alertType switch
        {
            NotificationType.AttendanceCheckIn => "Check-in Successful",
            NotificationType.AttendanceCheckOut => "Check-out Successful",
            NotificationType.AttendanceLate => "Late Arrival Alert",
            NotificationType.AttendanceMissed => "Missed Attendance Alert",
            NotificationType.BreakStarted => "Break Started",
            NotificationType.BreakEnded => "Break Ended",
            NotificationType.OvertimeAlert => "Overtime Alert",
            _ => "Attendance Alert"
        };

        string message = alertType switch
        {
            NotificationType.AttendanceCheckIn => $"Welcome back, {employee.FirstName}! You have successfully checked in.",
            NotificationType.AttendanceCheckOut => $"Have a great day, {employee.FirstName}! You have successfully checked out.",
            NotificationType.AttendanceLate => $"You arrived late today. Please ensure punctuality.",
            NotificationType.AttendanceMissed => $"You missed attendance today. Please contact HR if needed.",
            NotificationType.BreakStarted => $"Break started. Enjoy your break!",
            NotificationType.BreakEnded => $"Break ended. Welcome back to work!",
            NotificationType.OvertimeAlert => $"You are working overtime. Please ensure proper rest.",
            _ => "Attendance status updated"
        };

        var notification = new CreateNotificationDto
        {
            UserId = employeeId,
            Title = title,
            Message = message,
            Type = alertType,
            Priority = alertType == NotificationType.AttendanceLate ? NotificationPriority.High : NotificationPriority.Normal,
            Channel = NotificationChannel.InApp,
            Metadata = metadata
        };

        await CreateNotificationAsync(notification);
    }

    public async Task SendProductivityAlertAsync(ProductivityAlertDto alertDto)
    {
        var notification = new CreateNotificationDto
        {
            UserId = alertDto.EmployeeId,
            Title = $"Productivity Alert - {alertDto.AlertType}",
            Message = $"Your idle time is {alertDto.IdlePercentage:F1}% ({alertDto.IdleDuration:hh\\:mm}). Please ensure you're staying productive.",
            Type = NotificationType.Reminder,
            Priority = NotificationPriority.High,
            Channel = NotificationChannel.InApp,
            Metadata = new Dictionary<string, object>
            {
                ["IdlePercentage"] = alertDto.IdlePercentage,
                ["IdleDuration"] = alertDto.IdleDuration.ToString(),
                ["LastActivity"] = alertDto.LastActivity,
                ["AlertType"] = alertDto.AlertType
            }
        };

        await CreateNotificationAsync(notification);

        // Also send to managers/supervisors
        var employee = await _employeeService.GetEmployeeByIdAsync(alertDto.EmployeeId);
        if (employee?.ReportingManagerId.HasValue == true)
        {
            var managerNotification = new CreateNotificationDto
            {
                UserId = employee.ReportingManagerId.Value,
                Title = $"Team Productivity Alert - {alertDto.EmployeeName}",
                Message = $"{alertDto.EmployeeName} has been idle for {alertDto.IdleDuration:hh\\:mm} ({alertDto.IdlePercentage:F1}%).",
                Type = NotificationType.Reminder,
                Priority = NotificationPriority.Normal,
                Channel = NotificationChannel.InApp,
                ActionUrl = $"/employee/{alertDto.EmployeeId}/productivity",
                Metadata = new Dictionary<string, object>
                {
                    ["EmployeeId"] = alertDto.EmployeeId,
                    ["EmployeeName"] = alertDto.EmployeeName,
                    ["IdlePercentage"] = alertDto.IdlePercentage,
                    ["IdleDuration"] = alertDto.IdleDuration.ToString()
                }
            };

            await CreateNotificationAsync(managerNotification);
        }
    }

    public async Task<List<ProductivityAlertDto>> GetIdleEmployeesAsync(int? branchId = null, TimeSpan? idleThreshold = null)
    {
        // This would need to be implemented based on your attendance/activity tracking system
        // For now, returning empty list as placeholder
        return new List<ProductivityAlertDto>();
    }

    public async Task<List<NotificationPreferenceDto>> GetUserPreferencesAsync(int userId)
    {
        var preferences = await _preferenceRepository.GetUserPreferencesAsync(userId);
        return _mapper.Map<List<NotificationPreferenceDto>>(preferences);
    }

    public async Task<NotificationPreferenceDto> UpdatePreferenceAsync(int userId, UpdateNotificationPreferenceDto dto)
    {
        var preference = await _preferenceRepository.GetPreferenceAsync(userId, dto.NotificationType, dto.Channel);
        
        if (preference == null)
        {
            preference = new UserNotificationPreference
            {
                UserId = userId,
                NotificationType = dto.NotificationType,
                Channel = dto.Channel,
                IsEnabled = dto.IsEnabled,
                QuietHoursStart = dto.QuietHoursStart,
                QuietHoursEnd = dto.QuietHoursEnd,
                WeekendNotifications = dto.WeekendNotifications,
                CreatedAt = DateTime.UtcNow
            };

            await _preferenceRepository.AddAsync(preference);
        }
        else
        {
            preference.IsEnabled = dto.IsEnabled;
            preference.QuietHoursStart = dto.QuietHoursStart;
            preference.QuietHoursEnd = dto.QuietHoursEnd;
            preference.WeekendNotifications = dto.WeekendNotifications;
            preference.UpdatedAt = DateTime.UtcNow;

            await _preferenceRepository.UpdateAsync(preference);
        }

        await _preferenceRepository.SaveChangesAsync();
        return _mapper.Map<NotificationPreferenceDto>(preference);
    }

    public async Task<List<NotificationPreferenceDto>> CreateDefaultPreferencesAsync(int userId)
    {
        var defaultPreferences = new List<UserNotificationPreference>();
        var notificationTypes = Enum.GetValues<NotificationType>();
        var channels = new[] { NotificationChannel.InApp, NotificationChannel.Email };

        foreach (var type in notificationTypes)
        {
            foreach (var channel in channels)
            {
                var preference = new UserNotificationPreference
                {
                    UserId = userId,
                    NotificationType = type,
                    Channel = channel,
                    IsEnabled = true, // Default to enabled
                    WeekendNotifications = type == NotificationType.BirthdayToday || type == NotificationType.BirthdayWishes,
                    CreatedAt = DateTime.UtcNow
                };

                defaultPreferences.Add(preference);
            }
        }

        await _preferenceRepository.AddRangeAsync(defaultPreferences);
        await _preferenceRepository.SaveChangesAsync();

        return _mapper.Map<List<NotificationPreferenceDto>>(defaultPreferences);
    }

    public async Task<NotificationDto> CreateFromTemplateAsync(string templateName, int? userId, Dictionary<string, object> parameters)
    {
        var template = await _templateRepository.GetByNameAsync(templateName);
        if (template == null)
            throw new ArgumentException($"Template '{templateName}' not found");

        var title = ProcessTemplate(template.TitleTemplate, parameters);
        var message = ProcessTemplate(template.MessageTemplate, parameters);

        var dto = new CreateNotificationDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = template.Type,
            Priority = template.DefaultPriority,
            Channel = template.DefaultChannel,
            ExpiresAt = template.DefaultExpiryDuration.HasValue 
                ? DateTime.UtcNow.Add(template.DefaultExpiryDuration.Value)
                : null,
            Metadata = template.DefaultMetadata
        };

        return await CreateNotificationAsync(dto);
    }

    public async Task<List<NotificationDto>> CreateBulkFromTemplateAsync(string templateName, List<int> userIds, Dictionary<string, object> parameters)
    {
        var template = await _templateRepository.GetByNameAsync(templateName);
        if (template == null)
            throw new ArgumentException($"Template '{templateName}' not found");

        var notifications = new List<NotificationDto>();

        foreach (var userId in userIds)
        {
            var notification = await CreateFromTemplateAsync(templateName, userId, parameters);
            notifications.Add(notification);
        }

        return notifications;
    }

    public async Task<Dictionary<string, int>> GetNotificationStatsAsync(int userId)
    {
        return await _notificationRepository.GetNotificationStatsAsync(userId);
    }

    public async Task<Dictionary<string, int>> GetBranchNotificationStatsAsync(int branchId)
    {
        return await _notificationRepository.GetBranchNotificationStatsAsync(branchId);
    }

    public async Task CleanupExpiredNotificationsAsync()
    {
        var deletedCount = await _notificationRepository.DeleteExpiredNotificationsAsync();
        _logger.LogInformation("Cleaned up {Count} expired notifications", deletedCount);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    private static string ProcessTemplate(string template, Dictionary<string, object> parameters)
    {
        var result = template;
        foreach (var parameter in parameters)
        {
            result = result.Replace($"{{{parameter.Key}}}", parameter.Value?.ToString() ?? "");
        }
        return result;
    }
}