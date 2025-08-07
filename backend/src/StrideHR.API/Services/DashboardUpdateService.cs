using Microsoft.AspNetCore.SignalR;
using StrideHR.API.Hubs;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Interfaces.Repositories;

namespace StrideHR.API.Services;

public class DashboardUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(1); // Update every minute

    public DashboardUpdateService(
        IServiceProvider serviceProvider,
        ILogger<DashboardUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dashboard Update Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await UpdateDashboardStatistics(scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during dashboard statistics update");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }

        _logger.LogInformation("Dashboard Update Service stopped");
    }

    private async Task UpdateDashboardStatistics(IServiceProvider serviceProvider)
    {
        try
        {
            var hubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            var attendanceService = serviceProvider.GetRequiredService<IAttendanceService>();
            var employeeService = serviceProvider.GetRequiredService<IEmployeeService>();
            var branchRepository = serviceProvider.GetRequiredService<IBranchRepository>();

            // Get all active branches
            var branches = await branchRepository.GetAllAsync();
            var activeBranches = branches.Where(b => b.IsActive).ToList();

            foreach (var branch in activeBranches)
            {
                try
                {
                    // Get attendance statistics for the branch
                    var attendanceStats = await attendanceService.GetTodayAttendanceOverviewAsync(branch.Id);
                    
                    // Get employee statistics for the branch
                    var employeeStats = await employeeService.GetEmployeeStatisticsAsync(branch.Id);

                    // Calculate productivity metrics
                    var productivityMetrics = await CalculateProductivityMetrics(branch.Id, serviceProvider);

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
                        BranchId = branch.Id,
                        OrganizationId = branch.OrganizationId
                    };

                    // Broadcast to branch members
                    await hubContext.Clients.Group($"Branch_{branch.Id}")
                        .SendAsync("DashboardStatisticsUpdate", statistics);

                    // Also broadcast to organization
                    await hubContext.Clients.Group($"Organization_{branch.OrganizationId}")
                        .SendAsync("DashboardStatisticsUpdate", statistics);

                    _logger.LogDebug("Dashboard statistics updated for branch {BranchId}", branch.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating dashboard statistics for branch {BranchId}", branch.Id);
                }
            }

            // Update global online users count
            await UpdateOnlineUsersCount(hubContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dashboard statistics update process");
        }
    }

    private async Task<ProductivityMetrics> CalculateProductivityMetrics(int branchId, IServiceProvider serviceProvider)
    {
        try
        {
            // This is a placeholder implementation
            // In a real application, you would calculate these metrics based on:
            // - Project completion rates
            // - Task completion times
            // - Employee performance ratings
            // - Time tracking data
            // - Goal achievement rates

            var projectRepository = serviceProvider.GetService<IProjectRepository>();
            var taskRepository = serviceProvider.GetService<IProjectTaskRepository>();

            var metrics = new ProductivityMetrics
            {
                AverageProductivity = 85.0m, // Default value
                TopPerformers = 0,
                LowPerformers = 0,
                TasksCompleted = 0,
                ProjectsOnTrack = 0,
                OverdueTasks = 0
            };

            if (projectRepository != null && taskRepository != null)
            {
                // Get projects for the branch
                var projects = await projectRepository.GetProjectsByBranchAsync(branchId);
                var activeProjects = projects.Where(p => p.Status == "Active").ToList();

                metrics.ProjectsOnTrack = activeProjects.Count(p => p.Progress >= 80);

                // Get tasks for the branch
                var tasks = await taskRepository.GetTasksByBranchAsync(branchId);
                var todayTasks = tasks.Where(t => t.CompletedAt?.Date == DateTime.Today).ToList();
                var overdueTasks = tasks.Where(t => t.DueDate < DateTime.Now && t.Status != "Completed").ToList();

                metrics.TasksCompleted = todayTasks.Count;
                metrics.OverdueTasks = overdueTasks.Count;

                // Calculate productivity based on task completion rate
                var totalTasks = tasks.Count();
                var completedTasks = tasks.Count(t => t.Status == "Completed");
                
                if (totalTasks > 0)
                {
                    metrics.AverageProductivity = (decimal)(completedTasks * 100) / totalTasks;
                }

                // Calculate top and low performers (simplified)
                var employeePerformance = tasks
                    .Where(t => t.AssignedToId.HasValue)
                    .GroupBy(t => t.AssignedToId.Value)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        CompletionRate = g.Count(t => t.Status == "Completed") * 100.0 / g.Count()
                    })
                    .ToList();

                metrics.TopPerformers = employeePerformance.Count(e => e.CompletionRate >= 90);
                metrics.LowPerformers = employeePerformance.Count(e => e.CompletionRate < 60);
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating productivity metrics for branch {BranchId}", branchId);
            
            // Return default metrics on error
            return new ProductivityMetrics
            {
                AverageProductivity = 75.0m,
                TopPerformers = 0,
                LowPerformers = 0,
                TasksCompleted = 0,
                ProjectsOnTrack = 0,
                OverdueTasks = 0
            };
        }
    }

    private async Task UpdateOnlineUsersCount(IHubContext<NotificationHub> hubContext)
    {
        try
        {
            var connections = NotificationHub.GetCurrentConnections();
            var onlineUsers = connections.Values
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            var onlineUpdate = new
            {
                OnlineCount = onlineUsers.Count,
                OnlineUserIds = onlineUsers,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to all connected clients
            await hubContext.Clients.All.SendAsync("OnlineUsersUpdate", onlineUsers);

            _logger.LogDebug("Online users count updated: {Count}", onlineUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating online users count");
        }
    }
}

public class ProductivityMetrics
{
    public decimal AverageProductivity { get; set; }
    public int TopPerformers { get; set; }
    public int LowPerformers { get; set; }
    public int TasksCompleted { get; set; }
    public int ProjectsOnTrack { get; set; }
    public int OverdueTasks { get; set; }
}