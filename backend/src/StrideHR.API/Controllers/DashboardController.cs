using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Controllers;
using StrideHR.Core.Interfaces.Services;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IAttendanceService _attendanceService;
    private readonly IProjectService _projectService;
    private readonly ILeaveService _leaveService;
    private readonly IPayrollService _payrollService;
    private readonly IBranchService _branchService;
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IEmployeeService employeeService,
        IAttendanceService attendanceService,
        IProjectService projectService,
        ILeaveService leaveService,
        IPayrollService payrollService,
        IBranchService branchService,
        IOrganizationService organizationService,
        ILogger<DashboardController> logger)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
        _projectService = projectService;
        _leaveService = leaveService;
        _payrollService = payrollService;
        _branchService = branchService;
        _organizationService = organizationService;
        _logger = logger;
    }

    /// <summary>
    /// Get employee statistics for dashboard
    /// </summary>
    [HttpGet("employee/{employeeId}/stats")]
    public async Task<IActionResult> GetEmployeeStats(int employeeId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            
            // Employees can only view their own stats unless they have elevated permissions
            if (employeeId != currentEmployeeId && !HasPermission("Dashboard", "ViewAll"))
            {
                return Forbid("You can only view your own statistics");
            }

            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Get today's attendance
            var todayAttendance = await _attendanceService.GetEmployeeAttendanceForDateAsync(employeeId, DateTime.Today);
            
            // Get active tasks count (if project service supports it)
            var activeTasks = 0;
            try
            {
                var projects = await _projectService.GetEmployeeProjectsAsync(employeeId);
                activeTasks = projects?.SelectMany(p => p.Tasks ?? new List<dynamic>())
                    .Count(t => t.Status == "Active" || t.Status == "InProgress") ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load active tasks for employee {EmployeeId}", employeeId);
            }

            // Get leave balance
            var leaveBalance = 0;
            try
            {
                var balance = await _leaveService.GetEmployeeLeaveBalanceAsync(employeeId);
                leaveBalance = balance?.TotalBalance ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load leave balance for employee {EmployeeId}", employeeId);
            }

            // Calculate working hours
            var todayHours = "0.0";
            var currentStatus = "Not Checked In";
            string? checkInTime = null;
            string? checkOutTime = null;

            if (todayAttendance != null)
            {
                currentStatus = todayAttendance.Status?.ToString() ?? "Unknown";
                checkInTime = todayAttendance.CheckInTime?.ToString("HH:mm");
                checkOutTime = todayAttendance.CheckOutTime?.ToString("HH:mm");

                if (todayAttendance.CheckInTime.HasValue)
                {
                    var endTime = todayAttendance.CheckOutTime ?? DateTime.Now;
                    var workingHours = (endTime - todayAttendance.CheckInTime.Value).TotalHours;
                    todayHours = Math.Max(0, workingHours).ToString("F1");
                }
            }

            var stats = new
            {
                TodayHours = todayHours,
                ActiveTasks = activeTasks,
                LeaveBalance = leaveBalance,
                Productivity = 85, // Mock productivity score - implement actual calculation
                CurrentStatus = currentStatus,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime
            };

            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee stats for {EmployeeId}", employeeId);
            return Error("Failed to retrieve employee statistics");
        }
    }

    /// <summary>
    /// Get manager statistics for dashboard
    /// </summary>
    [HttpGet("manager/{managerId}/stats")]
    [Authorize(Policy = "CanViewTeamData")]
    public async Task<IActionResult> GetManagerStats(int managerId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            
            // Managers can only view their own stats unless they have elevated permissions
            if (managerId != currentEmployeeId && !HasPermission("Dashboard", "ViewAll"))
            {
                return Forbid("You can only view your own team statistics");
            }

            // Get team members
            var teamMembers = await _employeeService.GetTeamMembersAsync(managerId);
            var teamSize = teamMembers?.Count() ?? 0;

            // Get today's attendance for team
            var presentToday = 0;
            if (teamMembers != null)
            {
                foreach (var member in teamMembers)
                {
                    var attendance = await _attendanceService.GetEmployeeAttendanceForDateAsync(member.Id, DateTime.Today);
                    if (attendance?.Status?.ToString() == "Present")
                    {
                        presentToday++;
                    }
                }
            }

            // Get active projects
            var activeProjects = 0;
            try
            {
                var projects = await _projectService.GetManagerProjectsAsync(managerId);
                activeProjects = projects?.Count(p => p.Status == "Active" || p.Status == "InProgress") ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load active projects for manager {ManagerId}", managerId);
            }

            // Get pending approvals (mock for now)
            var pendingApprovals = 4; // Implement actual approval counting

            var stats = new
            {
                TeamSize = teamSize,
                PresentToday = presentToday,
                ActiveProjects = activeProjects,
                PendingApprovals = pendingApprovals,
                TeamProductivity = 82, // Mock team productivity
                OverdueTasksCount = 2 // Mock overdue tasks
            };

            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting manager stats for {ManagerId}", managerId);
            return Error("Failed to retrieve manager statistics");
        }
    }

    /// <summary>
    /// Get HR statistics for dashboard
    /// </summary>
    [HttpGet("hr/branch/{branchId}/stats")]
    [Authorize(Policy = "CanViewHRData")]
    public async Task<IActionResult> GetHRStats(int branchId)
    {
        try
        {
            var currentBranchId = GetCurrentBranchId();
            
            // HR can only view their branch stats unless they have elevated permissions
            if (branchId != currentBranchId && !HasPermission("Dashboard", "ViewAll"))
            {
                return Forbid("You can only view your branch statistics");
            }

            // Get total employees in branch
            var employees = await _employeeService.GetByBranchIdAsync(branchId);
            var totalEmployees = employees?.Count() ?? 0;

            // Get today's attendance
            var presentToday = 0;
            if (employees != null)
            {
                foreach (var employee in employees)
                {
                    var attendance = await _attendanceService.GetEmployeeAttendanceForDateAsync(employee.Id, DateTime.Today);
                    if (attendance?.Status?.ToString() == "Present")
                    {
                        presentToday++;
                    }
                }
            }

            // Get pending leaves
            var pendingLeaves = 0;
            try
            {
                var leaves = await _leaveService.GetPendingLeavesByBranchAsync(branchId);
                pendingLeaves = leaves?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load pending leaves for branch {BranchId}", branchId);
            }

            // Get payroll status
            var payrollStatus = "Pending";
            try
            {
                var payroll = await _payrollService.GetCurrentPayrollStatusAsync(branchId);
                payrollStatus = payroll?.Status ?? "Pending";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load payroll status for branch {BranchId}", branchId);
            }

            // Get new hires this month
            var newHiresThisMonth = 0;
            if (employees != null)
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                newHiresThisMonth = employees.Count(e => e.JoiningDate >= startOfMonth);
            }

            // Get upcoming birthdays (next 7 days)
            var upcomingBirthdays = 0;
            if (employees != null)
            {
                var today = DateTime.Today;
                var nextWeek = today.AddDays(7);
                
                upcomingBirthdays = employees.Count(e => 
                {
                    if (!e.DateOfBirth.HasValue) return false;
                    
                    var birthday = new DateTime(today.Year, e.DateOfBirth.Value.Month, e.DateOfBirth.Value.Day);
                    if (birthday < today)
                        birthday = birthday.AddYears(1);
                    
                    return birthday <= nextWeek;
                });
            }

            var stats = new
            {
                TotalEmployees = totalEmployees,
                PresentToday = presentToday,
                PendingLeaves = pendingLeaves,
                PayrollStatus = payrollStatus,
                NewHiresThisMonth = newHiresThisMonth,
                UpcomingBirthdays = upcomingBirthdays
            };

            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HR stats for branch {BranchId}", branchId);
            return Error("Failed to retrieve HR statistics");
        }
    }

    /// <summary>
    /// Get admin statistics for dashboard
    /// </summary>
    [HttpGet("admin/organization/{organizationId}/stats")]
    [Authorize(Policy = "CanViewAdminData")]
    public async Task<IActionResult> GetAdminStats(int organizationId)
    {
        try
        {
            var currentOrganizationId = GetCurrentOrganizationId();
            
            // Admin can only view their organization stats unless they are SuperAdmin
            if (organizationId != currentOrganizationId && !HasRole("SuperAdmin"))
            {
                return Forbid("You can only view your organization statistics");
            }

            // Get total branches
            var branches = await _branchService.GetByOrganizationIdAsync(organizationId);
            var totalBranches = branches?.Count() ?? 0;

            // Get total employees
            var totalEmployees = 0;
            if (branches != null)
            {
                foreach (var branch in branches)
                {
                    var employees = await _employeeService.GetByBranchIdAsync(branch.Id);
                    totalEmployees += employees?.Count() ?? 0;
                }
            }

            // System health (mock for now)
            var systemHealth = "Excellent";

            // Active users (mock for now)
            var activeUsers = 98;

            // System uptime (mock for now)
            var systemUptime = "99.9%";

            // Critical alerts (mock for now)
            var criticalAlerts = 0;

            var stats = new
            {
                TotalBranches = totalBranches,
                TotalEmployees = totalEmployees,
                SystemHealth = systemHealth,
                ActiveUsers = activeUsers,
                SystemUptime = systemUptime,
                CriticalAlerts = criticalAlerts
            };

            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin stats for organization {OrganizationId}", organizationId);
            return Error("Failed to retrieve admin statistics");
        }
    }

    /// <summary>
    /// Get super admin statistics for dashboard
    /// </summary>
    [HttpGet("superadmin/stats")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<IActionResult> GetSuperAdminStats()
    {
        try
        {
            // Get total organizations
            var organizations = await _organizationService.GetAllAsync();
            var totalOrganizations = organizations?.Count() ?? 0;

            // Get total branches across all organizations
            var totalBranches = 0;
            var totalEmployees = 0;

            if (organizations != null)
            {
                foreach (var org in organizations)
                {
                    var branches = await _branchService.GetByOrganizationIdAsync(org.Id);
                    totalBranches += branches?.Count() ?? 0;

                    if (branches != null)
                    {
                        foreach (var branch in branches)
                        {
                            var employees = await _employeeService.GetByBranchIdAsync(branch.Id);
                            totalEmployees += employees?.Count() ?? 0;
                        }
                    }
                }
            }

            // System metrics (mock for now - implement actual system monitoring)
            var systemHealth = "Excellent";
            var activeUsers = 98;
            var systemUptime = "99.9%";
            var criticalAlerts = 0;
            var databaseHealth = "Good";
            var serverLoad = 25;

            var stats = new
            {
                TotalOrganizations = totalOrganizations,
                TotalBranches = totalBranches,
                TotalEmployees = totalEmployees,
                SystemHealth = systemHealth,
                ActiveUsers = activeUsers,
                SystemUptime = systemUptime,
                CriticalAlerts = criticalAlerts,
                DatabaseHealth = databaseHealth,
                ServerLoad = serverLoad
            };

            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting super admin stats");
            return Error("Failed to retrieve super admin statistics");
        }
    }

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    [HttpGet("activities/recent")]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
    {
        try
        {
            var currentBranchId = GetCurrentBranchId();
            var currentOrganizationId = GetCurrentOrganizationId();

            // Mock recent activities - implement actual activity tracking
            var activities = new[]
            {
                new
                {
                    Id = "1",
                    Type = "employee_joined",
                    Message = "<strong>John Doe</strong> joined the Development team",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    UserId = "user1",
                    EmployeeId = "emp1",
                    BranchId = currentBranchId.ToString()
                },
                new
                {
                    Id = "2",
                    Type = "project_created",
                    Message = "New project <strong>\"Mobile App Redesign\"</strong> created",
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    UserId = "user2",
                    EmployeeId = "emp2",
                    BranchId = currentBranchId.ToString()
                },
                new
                {
                    Id = "3",
                    Type = "leave_requested",
                    Message = "<strong>Jane Smith</strong> requested leave for next week",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    UserId = "user3",
                    EmployeeId = "emp3",
                    BranchId = currentBranchId.ToString()
                },
                new
                {
                    Id = "4",
                    Type = "attendance_checkin",
                    Message = "<strong>Mike Johnson</strong> checked in at 9:15 AM",
                    CreatedAt = DateTime.UtcNow.AddHours(-8),
                    UserId = "user4",
                    EmployeeId = "emp4",
                    BranchId = currentBranchId.ToString()
                }
            };

            return Success(activities.Take(limit));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities");
            return Error("Failed to retrieve recent activities");
        }
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    [HttpGet("system/health")]
    [Authorize(Policy = "CanViewSystemHealth")]
    public async Task<IActionResult> GetSystemHealth()
    {
        try
        {
            // Mock system health - implement actual health checks
            var health = new
            {
                Status = "Excellent",
                DatabaseConnection = "Connected",
                ApiResponseTime = "120ms",
                MemoryUsage = "45%",
                CpuUsage = "25%",
                LastChecked = DateTime.UtcNow
            };

            return Success(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health");
            return Error("Failed to retrieve system health");
        }
    }

    /// <summary>
    /// Get dashboard summary for current user
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            var currentBranchId = GetCurrentBranchId();
            var currentOrganizationId = GetCurrentOrganizationId();
            var userRoles = GetCurrentUserRoles();

            var summary = new
            {
                EmployeeId = currentEmployeeId,
                BranchId = currentBranchId,
                OrganizationId = currentOrganizationId,
                Roles = userRoles,
                LastUpdated = DateTime.UtcNow,
                AvailableWidgets = GetAvailableWidgets(userRoles)
            };

            return Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return Error("Failed to retrieve dashboard summary");
        }
    }

    private string[] GetAvailableWidgets(IEnumerable<string> roles)
    {
        var widgets = new List<string> { "attendance", "personal_stats" };

        if (roles.Contains("Manager"))
        {
            widgets.AddRange(new[] { "team_overview", "project_stats" });
        }

        if (roles.Contains("HR"))
        {
            widgets.AddRange(new[] { "hr_stats", "employee_overview" });
        }

        if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
        {
            widgets.AddRange(new[] { "system_health", "organization_stats" });
        }

        return widgets.ToArray();
    }

    private bool HasPermission(string resource, string action)
    {
        // Implement actual permission checking logic
        var roles = GetCurrentUserRoles();
        return roles.Contains("SuperAdmin") || roles.Contains("Admin");
    }

    private bool HasRole(string role)
    {
        var roles = GetCurrentUserRoles();
        return roles.Contains(role);
    }

    private IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : 0;
    }

    private int GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }

    private int GetCurrentOrganizationId()
    {
        var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
        return int.TryParse(orgIdClaim, out var orgId) ? orgId : 0;
    }
}