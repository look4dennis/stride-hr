using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StrideHR.API.Hubs;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Notification;
using StrideHR.Core.Enums;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IRealTimeNotificationService _realTimeNotificationService;
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IHubContext<NotificationHub> hubContext,
        IRealTimeNotificationService realTimeNotificationService,
        IAttendanceService attendanceService,
        IEmployeeService employeeService,
        ILogger<DashboardController> logger)
    {
        _hubContext = hubContext;
        _realTimeNotificationService = realTimeNotificationService;
        _attendanceService = attendanceService;
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Get current dashboard statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var branchId = User.FindFirst("BranchId")?.Value;
            var organizationId = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(branchId))
            {
                return BadRequest("User context is invalid");
            }

            var branchIdInt = int.Parse(branchId);
            var organizationIdInt = int.Parse(organizationId ?? "0");

            // Get attendance statistics
            var attendanceStats = await _attendanceService.GetTodayAttendanceOverviewAsync(branchIdInt);
            
            // Get employee statistics
            var employeeStats = await _employeeService.GetEmployeeStatisticsAsync(branchIdInt);

            // Calculate productivity metrics (placeholder - implement based on your business logic)
            var productivityMetrics = await CalculateProductivityMetrics(branchIdInt);

            var statistics = new
            {
                TotalEmployees = employeeStats.TotalActive,
                PresentToday = attendanceStats.PresentCount,
                AbsentToday = attendanceStats.AbsentCount,
                OnBreak = attendanceStats.OnBreakCount,
                LateArrivals = attendanceStats.LateCount,
                EarlyDepartures = attendanceStats.EarlyDepartureCount,
                Overtime = attendanceStats.OvertimeCount,
                Productivity = productivityMetrics.AverageProductivity,
                LastUpdated = DateTime.UtcNow,
                BranchId = branchIdInt,
                OrganizationId = organizationIdInt
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics for user {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, "An error occurred while retrieving dashboard statistics");
        }
    }

    /// <summary>
    /// Broadcast dashboard statistics update to all connected clients
    /// </summary>
    [HttpPost("broadcast-statistics")]
    [Authorize(Policy = "HRManager")]
    public async Task<IActionResult> BroadcastDashboardStatistics()
    {
        try
        {
            var branchId = User.FindFirst("BranchId")?.Value;
            var organizationId = User.FindFirst("OrganizationId")?.Value;

            if (string.IsNullOrEmpty(branchId))
            {
                return BadRequest("Branch context is required");
            }

            var branchIdInt = int.Parse(branchId);
            var organizationIdInt = int.Parse(organizationId ?? "0");

            // Get current statistics
            var attendanceStats = await _attendanceService.GetTodayAttendanceOverviewAsync(branchIdInt);
            var employeeStats = await _employeeService.GetEmployeeStatisticsAsync(branchIdInt);
            var productivityMetrics = await CalculateProductivityMetrics(branchIdInt);

            var statistics = new
            {
                TotalEmployees = employeeStats.TotalActive,
                PresentToday = attendanceStats.PresentCount,
                AbsentToday = attendanceStats.AbsentCount,
                OnBreak = attendanceStats.OnBreakCount,
                LateArrivals = attendanceStats.LateCount,
                EarlyDepartures = attendanceStats.EarlyDepartureCount,
                Overtime = attendanceStats.OvertimeCount,
                Productivity = productivityMetrics.AverageProductivity,
                LastUpdated = DateTime.UtcNow,
                BranchId = branchIdInt,
                OrganizationId = organizationIdInt
            };

            // Broadcast to branch members
            await _hubContext.Clients.Group($"Branch_{branchId}")
                .SendAsync("DashboardStatisticsUpdate", statistics);

            // Also broadcast to organization if different from branch
            if (organizationIdInt > 0 && organizationIdInt.ToString() != branchId)
            {
                await _hubContext.Clients.Group($"Organization_{organizationId}")
                    .SendAsync("DashboardStatisticsUpdate", statistics);
            }

            _logger.LogInformation("Dashboard statistics broadcasted for branch {BranchId}", branchIdInt);

            return Ok(new { Message = "Dashboard statistics broadcasted successfully", Statistics = statistics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting dashboard statistics for user {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, "An error occurred while broadcasting dashboard statistics");
        }
    }

    /// <summary>
    /// Send real-time attendance update
    /// </summary>
    [HttpPost("attendance-update")]
    public async Task<IActionResult> SendAttendanceUpdate([FromBody] AttendanceUpdateRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeId = User.FindFirst("EmployeeId")?.Value;
            var branchId = User.FindFirst("BranchId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(branchId))
            {
                return BadRequest("User context is invalid");
            }

            var branchIdInt = int.Parse(branchId);
            var employeeIdInt = int.Parse(employeeId);

            // Get employee details
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeIdInt);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var attendanceUpdate = new
            {
                EmployeeId = employeeIdInt,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                ProfilePhoto = employee.ProfilePhoto,
                Action = request.Action,
                Timestamp = DateTime.UtcNow,
                Location = request.Location,
                Department = employee.Department,
                BranchId = branchIdInt
            };

            // Broadcast to branch members
            await _hubContext.Clients.Group($"Branch_{branchId}")
                .SendAsync("AttendanceStatusUpdated", attendanceUpdate);

            // Send notification to HR managers
            var notification = new NotificationDto
            {
                Title = "Attendance Update",
                Message = $"{employee.FirstName} {employee.LastName} has {GetActionText(request.Action)}",
                Type = NotificationType.AttendanceCheckIn,
                Priority = NotificationPriority.Low,
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["employeeId"] = employeeIdInt,
                    ["action"] = request.Action,
                    ["location"] = request.Location ?? ""
                }
            };

            await _realTimeNotificationService.SendNotificationToRoleAsync("HRManager", notification);

            _logger.LogInformation("Attendance update sent for employee {EmployeeId}: {Action}", 
                employeeIdInt, request.Action);

            return Ok(new { Message = "Attendance update sent successfully", Update = attendanceUpdate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending attendance update for user {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, "An error occurred while sending attendance update");
        }
    }

    /// <summary>
    /// Send system alert to dashboard
    /// </summary>
    [HttpPost("system-alert")]
    [Authorize(Policy = "HRManager")]
    public async Task<IActionResult> SendSystemAlert([FromBody] SystemAlertRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var branchId = User.FindFirst("BranchId")?.Value;
            var organizationId = User.FindFirst("OrganizationId")?.Value;

            var alert = new
            {
                Id = Guid.NewGuid().ToString(),
                Type = request.Type,
                Severity = request.Severity,
                Title = request.Title,
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                ActionRequired = !string.IsNullOrEmpty(request.ActionUrl),
                ActionUrl = request.ActionUrl,
                Metadata = request.Metadata,
                BranchId = branchId,
                OrganizationId = organizationId
            };

            // Determine target audience
            string targetGroup = request.Scope switch
            {
                "branch" => $"Branch_{branchId}",
                "organization" => $"Organization_{organizationId}",
                "role" => $"Role_{request.TargetRole}",
                _ => $"Branch_{branchId}"
            };

            await _hubContext.Clients.Group(targetGroup).SendAsync("SystemAlert", alert);

            // Also send as notification for high/critical alerts
            if (request.Severity == "high" || request.Severity == "critical")
            {
                var notification = new NotificationDto
                {
                    Title = request.Title,
                    Message = request.Message,
                    Type = NotificationType.SystemMaintenance,
                    Priority = request.Severity == "critical" ? NotificationPriority.Critical : NotificationPriority.High,
                    CreatedAt = DateTime.UtcNow,
                    ActionUrl = request.ActionUrl,
                    Metadata = request.Metadata
                };

                switch (request.Scope)
                {
                    case "branch":
                        await _realTimeNotificationService.SendNotificationToBranchAsync(int.Parse(branchId!), notification);
                        break;
                    case "role":
                        await _realTimeNotificationService.SendNotificationToRoleAsync(request.TargetRole!, notification);
                        break;
                    default:
                        await _realTimeNotificationService.SendNotificationToBranchAsync(int.Parse(branchId!), notification);
                        break;
                }
            }

            _logger.LogInformation("System alert sent: {Title} (Severity: {Severity}, Scope: {Scope})", 
                request.Title, request.Severity, request.Scope);

            return Ok(new { Message = "System alert sent successfully", Alert = alert });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system alert for user {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, "An error occurred while sending system alert");
        }
    }

    /// <summary>
    /// Get online users count
    /// </summary>
    [HttpGet("online-users")]
    public async Task<IActionResult> GetOnlineUsers()
    {
        try
        {
            var onlineCount = await _realTimeNotificationService.GetOnlineUsersCountAsync();
            var onlineUserIds = await _realTimeNotificationService.GetOnlineUserIdsAsync();

            return Ok(new 
            { 
                OnlineCount = onlineCount,
                OnlineUserIds = onlineUserIds,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving online users count");
            return StatusCode(500, "An error occurred while retrieving online users");
        }
    }

    /// <summary>
    /// Trigger dashboard refresh for all connected clients
    /// </summary>
    [HttpPost("refresh")]
    [Authorize(Policy = "HRManager")]
    public async Task<IActionResult> RefreshDashboard()
    {
        try
        {
            var branchId = User.FindFirst("BranchId")?.Value;
            var organizationId = User.FindFirst("OrganizationId")?.Value;

            // Send refresh signal to branch members
            await _hubContext.Clients.Group($"Branch_{branchId}")
                .SendAsync("DashboardRefreshRequested", new { Timestamp = DateTime.UtcNow });

            // Also send to organization if different
            if (!string.IsNullOrEmpty(organizationId) && organizationId != branchId)
            {
                await _hubContext.Clients.Group($"Organization_{organizationId}")
                    .SendAsync("DashboardRefreshRequested", new { Timestamp = DateTime.UtcNow });
            }

            // Broadcast updated statistics
            await BroadcastDashboardStatistics();

            _logger.LogInformation("Dashboard refresh triggered for branch {BranchId}", branchId);

            return Ok(new { Message = "Dashboard refresh triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering dashboard refresh for user {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, "An error occurred while refreshing dashboard");
        }
    }

    // Private helper methods

    private async Task<ProductivityMetrics> CalculateProductivityMetrics(int branchId)
    {
        // Placeholder implementation - replace with actual business logic
        // This could involve calculating metrics from project management, task completion, etc.
        
        return new ProductivityMetrics
        {
            AverageProductivity = 85.5m, // Placeholder value
            TopPerformers = 5,
            LowPerformers = 2,
            TasksCompleted = 45,
            ProjectsOnTrack = 8,
            OverdueTasks = 3
        };
    }

    private string GetActionText(string action)
    {
        return action.ToLower() switch
        {
            "checkin" => "checked in",
            "checkout" => "checked out",
            "break_start" => "started break",
            "break_end" => "ended break",
            _ => action
        };
    }
}

// Request DTOs
public class AttendanceUpdateRequest
{
    public string Action { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public class SystemAlertRequest
{
    public string Type { get; set; } = "system";
    public string Severity { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string Scope { get; set; } = "branch"; // branch, organization, role
    public string? TargetRole { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

// Response DTOs
public class ProductivityMetrics
{
    public decimal AverageProductivity { get; set; }
    public int TopPerformers { get; set; }
    public int LowPerformers { get; set; }
    public int TasksCompleted { get; set; }
    public int ProjectsOnTrack { get; set; }
    public int OverdueTasks { get; set; }
}