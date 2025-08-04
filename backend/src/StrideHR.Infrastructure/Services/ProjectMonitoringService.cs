using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.Infrastructure.Services;

public class ProjectMonitoringService : IProjectMonitoringService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAlertRepository _alertRepository;
    private readonly IDSRRepository _dsrRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectMonitoringService> _logger;

    public ProjectMonitoringService(
        IProjectRepository projectRepository,
        IProjectAlertRepository alertRepository,
        IDSRRepository dsrRepository,
        IProjectAssignmentRepository assignmentRepository,
        IMapper mapper,
        ILogger<ProjectMonitoringService> logger)
    {
        _projectRepository = projectRepository;
        _alertRepository = alertRepository;
        _dsrRepository = dsrRepository;
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectHoursReportDto> GetProjectHoursTrackingAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, start, end);
            var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);

            var report = new ProjectHoursReportDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                EstimatedHours = project.EstimatedHours,
                TotalHoursWorked = dsrRecords.Sum(d => d.HoursWorked),
                StartDate = start,
                EndDate = end,
                DailyHours = dsrRecords
                    .GroupBy(d => d.Date.Date)
                    .Select(g => new DailyHoursDto
                    {
                        Date = g.Key,
                        HoursWorked = g.Sum(d => d.HoursWorked),
                        TasksWorked = g.Select(d => d.TaskId).Distinct().Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            report.HoursVariance = report.TotalHoursWorked - report.EstimatedHours;

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours tracking for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectHoursReportDto>> GetTeamHoursTrackingAsync(int teamLeaderId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var reports = new List<ProjectHoursReportDto>();

            foreach (var project in projects)
            {
                var report = await GetProjectHoursTrackingAsync(project.Id, startDate, endDate);
                reports.Add(report);
            }

            return reports;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team hours tracking for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectDashboardDto> GetTeamLeaderDashboardAsync(int teamLeaderId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var projectIds = projects.Select(p => p.Id).ToList();

            var analytics = new List<ProjectAnalyticsDto>();
            foreach (var project in projects)
            {
                var analytic = await GetProjectAnalyticsAsync(project.Id);
                analytics.Add(analytic);
            }

            var criticalAlerts = await _alertRepository.GetCriticalAlertsAsync();
            // var highRisks = await _riskRepository.GetHighRisksAsync(projectIds);

            var teamOverview = new TeamOverviewDto
            {
                TotalProjects = projects.Count(),
                ActiveProjects = projects.Count(p => p.Status == Core.Enums.ProjectStatus.Active),
                CompletedProjects = projects.Count(p => p.Status == Core.Enums.ProjectStatus.Completed),
                DelayedProjects = projects.Count(p => p.EndDate < DateTime.Today && p.Status != Core.Enums.ProjectStatus.Completed),
                TotalBudget = projects.Sum(p => p.Budget),
                BudgetUtilized = analytics.Sum(a => a.Metrics.BudgetUtilized),
                TotalTeamMembers = analytics.Sum(a => a.Metrics.TeamMembersCount),
                OverallProductivity = analytics.Any() ? analytics.Average(a => a.Performance.OverallEfficiency) : 0,
                AverageProjectHealth = analytics.Any() ? analytics.Average(a => CalculateHealthScore(a.Metrics)) : 0
            };

            var employee = await _projectRepository.GetEmployeeAsync(teamLeaderId);

            return new ProjectDashboardDto
            {
                TeamLeaderId = teamLeaderId,
                TeamLeaderName = employee?.FirstName + " " + employee?.LastName ?? "Unknown",
                ProjectAnalytics = analytics,
                TeamOverview = teamOverview,
                CriticalAlerts = _mapper.Map<List<ProjectAlertDto>>(criticalAlerts),
                // HighRisks = _mapper.Map<List<ProjectRiskDto>>(highRisks)
                HighRisks = new List<ProjectRiskDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader dashboard for {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectAnalyticsDto> GetProjectAnalyticsAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, null, null);
            var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);
            // TODO: Implement risk repository when available
            var risks = new List<ProjectRisk>();

            var metrics = new ProjectMetricsDto
            {
                TotalHoursWorked = dsrRecords.Sum(d => d.HoursWorked),
                EstimatedHours = project.EstimatedHours,
                BudgetUtilized = CalculateBudgetUtilized(project, dsrRecords.ToList()),
                CompletionPercentage = CalculateCompletionPercentage(project),
                TotalTasks = project.Tasks.Count,
                CompletedTasks = project.Tasks.Count(t => t.Status == Core.Enums.ProjectTaskStatus.Done),
                OverdueTasks = project.Tasks.Count(t => t.DueDate < DateTime.Today && t.Status != Core.Enums.ProjectTaskStatus.Done),
                TeamMembersCount = teamMembers.Count(),
                AverageTaskCompletionTime = CalculateAverageTaskCompletionTime(project.Tasks)
            };

            metrics.HoursVariance = metrics.TotalHoursWorked - metrics.EstimatedHours;
            metrics.BudgetVariance = metrics.BudgetUtilized - project.Budget;

            var trends = await GetProjectTrendsInternalAsync(projectId, 30);
            var performance = CalculateProjectPerformance(metrics, project);

            return new ProjectAnalyticsDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Metrics = metrics,
                Trends = trends.FirstOrDefault() ?? new ProjectTrendsDto(),
                Performance = performance,
                Risks = _mapper.Map<List<ProjectRiskDto>>(risks),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project analytics for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectAnalyticsDto>> GetTeamProjectAnalyticsAsync(int teamLeaderId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var analytics = new List<ProjectAnalyticsDto>();

            foreach (var project in projects)
            {
                var analytic = await GetProjectAnalyticsAsync(project.Id);
                analytics.Add(analytic);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team project analytics for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectPerformanceDto> GetProjectPerformanceAsync(int projectId)
    {
        try
        {
            var analytics = await GetProjectAnalyticsAsync(projectId);
            return analytics.Performance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project performance for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectTrendsDto>> GetProjectTrendsAsync(int projectId, int days = 30)
    {
        try
        {
            return await GetProjectTrendsInternalAsync(projectId, days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project trends for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectAlertDto>> GetProjectAlertsAsync(int projectId)
    {
        try
        {
            var alerts = await _alertRepository.GetAlertsByProjectAsync(projectId);
            return _mapper.Map<List<ProjectAlertDto>>(alerts.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project alerts for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectAlertDto>> GetTeamAlertsAsync(int teamLeaderId)
    {
        try
        {
            var alerts = await _alertRepository.GetAlertsByTeamLeadAsync(teamLeaderId);
            return _mapper.Map<List<ProjectAlertDto>>(alerts.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team alerts for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectAlertDto> CreateProjectAlertAsync(int projectId, string alertType, string message, string severity)
    {
        try
        {
            // Parse the enum values
            if (!Enum.TryParse<ProjectAlertType>(alertType, out var alertTypeEnum))
                alertTypeEnum = ProjectAlertType.QualityIssue;
            
            if (!Enum.TryParse<AlertSeverity>(severity, out var severityEnum))
                severityEnum = AlertSeverity.Medium;

            var alert = new ProjectAlert
            {
                ProjectId = projectId,
                AlertType = alertTypeEnum,
                Message = message,
                Severity = severityEnum,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert);
            await _alertRepository.SaveChangesAsync();

            _logger.LogInformation("Project alert created for project {ProjectId}: {AlertType}", projectId, alertType);

            return _mapper.Map<ProjectAlertDto>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project alert for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> ResolveProjectAlertAsync(int alertId, int resolvedBy)
    {
        try
        {
            var result = await _alertRepository.ResolveAlertAsync(alertId, resolvedBy);
            
            if (result)
            {
                _logger.LogInformation("Project alert {AlertId} resolved by employee {EmployeeId}", alertId, resolvedBy);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving project alert {AlertId}", alertId);
            throw;
        }
    }

    public async Task<List<ProjectAlertDto>> GetCriticalAlertsAsync(int teamLeaderId)
    {
        try
        {
            var criticalAlerts = await _alertRepository.GetAlertsBySeverityAsync(AlertSeverity.Critical);
            var highAlerts = await _alertRepository.GetAlertsBySeverityAsync(AlertSeverity.High);
            var allAlerts = criticalAlerts.Concat(highAlerts).Where(a => !a.IsResolved);
            return _mapper.Map<List<ProjectAlertDto>>(allAlerts.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical alerts for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<List<ProjectRiskDto>> GetProjectRisksAsync(int projectId)
    {
        try
        {
            // TODO: Implement risk repository when available
            return new List<ProjectRiskDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project risks for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<ProjectRiskDto> CreateProjectRiskAsync(int projectId, string riskType, string description, string severity, decimal probability, decimal impact)
    {
        try
        {
            // TODO: Implement risk repository when available
            var risk = new ProjectRiskDto
            {
                ProjectId = projectId,
                RiskType = riskType,
                Description = description,
                Severity = severity,
                Probability = probability,
                Impact = impact,
                Status = "Identified",
                IdentifiedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Project risk created for project {ProjectId}: {RiskType}", projectId, riskType);

            return risk;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project risk for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> UpdateProjectRiskAsync(int riskId, string mitigationPlan, string status, int? assignedTo)
    {
        try
        {
            // TODO: Implement risk repository when available
            _logger.LogInformation("Project risk {RiskId} updated with status {Status}", riskId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project risk {RiskId}", riskId);
            throw;
        }
    }

    public async Task<List<ProjectRiskDto>> GetHighRisksAsync(int teamLeaderId)
    {
        try
        {
            // TODO: Implement risk repository when available
            return new List<ProjectRiskDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high risks for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task CheckProjectHealthAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                return;

            var healthScore = await CalculateProjectHealthScoreAsync(projectId);
            var isAtRisk = await IsProjectAtRiskAsync(projectId);

            if (isAtRisk)
            {
                await CreateProjectAlertAsync(projectId, "LowProductivity", 
                    $"Project health score is low ({healthScore:F1}/10). Immediate attention required.", 
                    healthScore < 3 ? "Critical" : "High");
            }

            // Check for overdue tasks
            var overdueTasks = project.Tasks.Count(t => t.DueDate < DateTime.Today && t.Status != Core.Enums.ProjectTaskStatus.Done);
            if (overdueTasks > 0)
            {
                await CreateProjectAlertAsync(projectId, "TaskOverdue", 
                    $"{overdueTasks} task(s) are overdue and need immediate attention.", 
                    overdueTasks > 5 ? "Critical" : "High");
            }

            // Check budget variance
            var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, null, null);
            var budgetUtilized = CalculateBudgetUtilized(project, dsrRecords.ToList());
            if (budgetUtilized > project.Budget * 0.9m)
            {
                await CreateProjectAlertAsync(projectId, "BudgetOverrun", 
                    $"Project has utilized {(budgetUtilized / project.Budget * 100):F1}% of budget.", 
                    budgetUtilized > project.Budget ? "Critical" : "High");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking project health for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task GenerateAutomaticAlertsAsync()
    {
        try
        {
            var activeProjects = await _projectRepository.GetActiveProjectsAsync();

            foreach (var project in activeProjects)
            {
                await CheckProjectHealthAsync(project.Id);
            }

            _logger.LogInformation("Automatic alerts generated for {ProjectCount} active projects", activeProjects.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating automatic alerts");
            throw;
        }
    }

    public async Task<bool> IsProjectAtRiskAsync(int projectId)
    {
        try
        {
            var healthScore = await CalculateProjectHealthScoreAsync(projectId);
            return healthScore < 5; // Projects with health score below 5 are considered at risk
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is at risk {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<decimal> CalculateProjectHealthScoreAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                return 0;

            var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, null, null);
            
            // Calculate various health factors (0-10 scale)
            var timelineScore = CalculateTimelineScore(project);
            var budgetScore = CalculateBudgetScore(project, dsrRecords.ToList());
            var progressScore = CalculateProgressScore(project);
            var teamScore = CalculateTeamScore(project);

            // Weighted average
            var healthScore = (timelineScore * 0.3m + budgetScore * 0.3m + progressScore * 0.3m + teamScore * 0.1m);

            return Math.Max(0, Math.Min(10, healthScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project health score for project {ProjectId}", projectId);
            throw;
        }
    }

    // Private helper methods
    private async Task<List<ProjectTrendsDto>> GetProjectTrendsInternalAsync(int projectId, int days)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, startDate, endDate);
        var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);

        var dailyProgress = dsrRecords
            .GroupBy(d => d.Date.Date)
            .Select(g => new DailyProgressDto
            {
                Date = g.Key,
                HoursWorked = g.Sum(d => d.HoursWorked),
                TasksCompleted = g.Count(d => d.Task?.Status == Core.Enums.ProjectTaskStatus.Done),
                CompletionPercentage = CalculateCompletionPercentage(project)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var trends = new ProjectTrendsDto
        {
            DailyProgress = dailyProgress,
            WeeklyHours = CalculateWeeklyHours(dsrRecords.ToList()),
            TeamProductivity = await CalculateTeamProductivity(projectId),
            TaskStatusTrends = CalculateTaskStatusTrends(project, days)
        };

        return new List<ProjectTrendsDto> { trends };
    }

    private decimal CalculateBudgetUtilized(Project project, List<DSR> dsrRecords)
    {
        // Simplified calculation - in real scenario, this would include actual costs
        var totalHours = dsrRecords.Sum(d => d.HoursWorked);
        var averageHourlyRate = project.Budget / Math.Max(project.EstimatedHours, 1);
        return totalHours * averageHourlyRate;
    }

    private decimal CalculateCompletionPercentage(Project project)
    {
        if (project.Tasks.Count == 0)
            return 0;

        var completedTasks = project.Tasks.Count(t => t.Status == Core.Enums.ProjectTaskStatus.Done);
        return (decimal)completedTasks / project.Tasks.Count * 100;
    }

    private decimal CalculateAverageTaskCompletionTime(ICollection<ProjectTask> tasks)
    {
        var completedTasks = tasks.Where(t => t.Status == Core.Enums.ProjectTaskStatus.Done && t.UpdatedAt.HasValue).ToList();
        
        if (completedTasks.Count == 0)
            return 0;

        var totalDays = completedTasks.Sum(t => (t.UpdatedAt!.Value - t.CreatedAt).TotalDays);
        return (decimal)(totalDays / completedTasks.Count);
    }

    private ProjectPerformanceDto CalculateProjectPerformance(ProjectMetricsDto metrics, Project project)
    {
        var efficiency = metrics.EstimatedHours > 0 ? (metrics.EstimatedHours / Math.Max(metrics.TotalHoursWorked, 1)) * 100 : 100;
        var timelineAdherence = CalculateTimelineAdherence(project);
        var budgetAdherence = project.Budget > 0 ? Math.Max(0, (project.Budget - metrics.BudgetUtilized) / project.Budget * 100) : 100;

        return new ProjectPerformanceDto
        {
            OverallEfficiency = Math.Min(100, efficiency),
            QualityScore = CalculateQualityScore(metrics),
            TimelineAdherence = timelineAdherence,
            BudgetAdherence = budgetAdherence,
            TeamSatisfaction = 85, // This would come from surveys in real implementation
            PerformanceGrade = CalculatePerformanceGrade(efficiency, timelineAdherence, budgetAdherence),
            StrengthAreas = IdentifyStrengthAreas(metrics),
            ImprovementAreas = IdentifyImprovementAreas(metrics)
        };
    }

    private decimal CalculateHealthScore(ProjectMetricsDto metrics)
    {
        var completionScore = metrics.CompletionPercentage / 10;
        var efficiencyScore = metrics.EstimatedHours > 0 ? (metrics.EstimatedHours / Math.Max(metrics.TotalHoursWorked, 1)) * 10 : 10;
        var taskScore = metrics.TotalTasks > 0 ? (decimal)metrics.CompletedTasks / metrics.TotalTasks * 10 : 10;
        
        return (completionScore + efficiencyScore + taskScore) / 3;
    }

    private decimal CalculateTimelineScore(Project project)
    {
        var totalDays = (project.EndDate - project.StartDate).TotalDays;
        var elapsedDays = (DateTime.Today - project.StartDate).TotalDays;
        var progressRatio = (decimal)(elapsedDays / totalDays);
        var completionRatio = CalculateCompletionPercentage(project) / 100;

        if (progressRatio <= 0) return 10;
        
        var timelineScore = (completionRatio / progressRatio) * 10;
        return Math.Max(0, Math.Min(10, timelineScore));
    }

    private decimal CalculateBudgetScore(Project project, List<DSR> dsrRecords)
    {
        var budgetUtilized = CalculateBudgetUtilized(project, dsrRecords);
        var utilizationRatio = project.Budget > 0 ? budgetUtilized / project.Budget : 0;

        if (utilizationRatio <= 0.8m) return 10;
        if (utilizationRatio <= 1.0m) return 7;
        if (utilizationRatio <= 1.2m) return 4;
        return 1;
    }

    private decimal CalculateProgressScore(Project project)
    {
        var completionPercentage = CalculateCompletionPercentage(project);
        return completionPercentage / 10;
    }

    private decimal CalculateTeamScore(Project project)
    {
        // Simplified team score based on task distribution
        var teamMembers = project.ProjectAssignments.Count;
        if (teamMembers == 0) return 5;

        var tasksPerMember = (decimal)project.Tasks.Count / teamMembers;
        if (tasksPerMember >= 3 && tasksPerMember <= 8) return 10;
        if (tasksPerMember >= 1 && tasksPerMember <= 10) return 7;
        return 4;
    }

    private List<WeeklyHoursDto> CalculateWeeklyHours(List<DSR> dsrRecords)
    {
        return dsrRecords
            .GroupBy(d => GetWeekStartDate(d.Date))
            .Select(g => new WeeklyHoursDto
            {
                WeekStartDate = g.Key,
                ActualHours = g.Sum(d => d.HoursWorked),
                PlannedHours = 40, // Simplified - would come from project planning
                Variance = g.Sum(d => d.HoursWorked) - 40
            })
            .OrderBy(w => w.WeekStartDate)
            .ToList();
    }

    private async Task<List<TeamMemberProductivityDto>> CalculateTeamProductivity(int projectId)
    {
        var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);
        var dsrRecords = await _dsrRepository.GetProjectDSRsAsync(projectId, null, null);

        return teamMembers.Select(tm => new TeamMemberProductivityDto
        {
            EmployeeId = tm.Id, // Use Employee.Id instead of EmployeeId
            EmployeeName = tm.FullName ?? $"Employee {tm.Id}",
            HoursWorked = dsrRecords.Where(d => d.EmployeeId == tm.Id).Sum(d => d.HoursWorked),
            TasksCompleted = dsrRecords.Where(d => d.EmployeeId == tm.Id).Count(),
            ProductivityScore = 85, // Placeholder - implement actual calculation
            EfficiencyRating = 85.0m // Placeholder - implement actual calculation
        }).ToList();
    }

    private List<TaskStatusTrendDto> CalculateTaskStatusTrends(Project project, int days)
    {
        var trends = new List<TaskStatusTrendDto>();
        var endDate = DateTime.Today;

        for (int i = 0; i < days; i++)
        {
            var date = endDate.AddDays(-i);
            trends.Add(new TaskStatusTrendDto
            {
                Date = date,
                TodoTasks = project.Tasks.Count(t => t.Status == Core.Enums.ProjectTaskStatus.ToDo),
                InProgressTasks = project.Tasks.Count(t => t.Status == Core.Enums.ProjectTaskStatus.InProgress),
                CompletedTasks = project.Tasks.Count(t => t.Status == Core.Enums.ProjectTaskStatus.Done),
                OverdueTasks = project.Tasks.Count(t => t.DueDate < date && t.Status != Core.Enums.ProjectTaskStatus.Done)
            });
        }

        return trends.OrderBy(t => t.Date).ToList();
    }

    private decimal CalculateTimelineAdherence(Project project)
    {
        var totalDays = (project.EndDate - project.StartDate).TotalDays;
        var elapsedDays = (DateTime.Today - project.StartDate).TotalDays;
        var completionPercentage = CalculateCompletionPercentage(project);

        if (totalDays <= 0) return 100;

        var expectedProgress = (decimal)(elapsedDays / totalDays) * 100;
        var adherence = (completionPercentage / Math.Max(expectedProgress, 1)) * 100;

        return Math.Max(0, Math.Min(100, adherence));
    }

    private decimal CalculateQualityScore(ProjectMetricsDto metrics)
    {
        // Simplified quality score based on task completion ratio and overdue tasks
        var completionRatio = metrics.TotalTasks > 0 ? (decimal)metrics.CompletedTasks / metrics.TotalTasks : 1;
        var overdueRatio = metrics.TotalTasks > 0 ? (decimal)metrics.OverdueTasks / metrics.TotalTasks : 0;

        var qualityScore = (completionRatio * 100) - (overdueRatio * 50);
        return Math.Max(0, Math.Min(100, qualityScore));
    }

    private string CalculatePerformanceGrade(decimal efficiency, decimal timelineAdherence, decimal budgetAdherence)
    {
        var averageScore = (efficiency + timelineAdherence + budgetAdherence) / 3;

        return averageScore switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B+",
            >= 60 => "B",
            >= 50 => "C+",
            >= 40 => "C",
            _ => "D"
        };
    }

    private List<string> IdentifyStrengthAreas(ProjectMetricsDto metrics)
    {
        var strengths = new List<string>();

        if (metrics.CompletionPercentage >= 80)
            strengths.Add("High task completion rate");

        if (metrics.HoursVariance <= 0)
            strengths.Add("Efficient time management");

        if (metrics.OverdueTasks == 0)
            strengths.Add("Excellent deadline adherence");

        if (metrics.TeamMembersCount > 0 && metrics.CompletedTasks / metrics.TeamMembersCount >= 5)
            strengths.Add("Strong team productivity");

        return strengths;
    }

    private List<string> IdentifyImprovementAreas(ProjectMetricsDto metrics)
    {
        var improvements = new List<string>();

        if (metrics.CompletionPercentage < 50)
            improvements.Add("Task completion rate needs improvement");

        if (metrics.HoursVariance > metrics.EstimatedHours * 0.2m)
            improvements.Add("Time estimation and management");

        if (metrics.OverdueTasks > 0)
            improvements.Add("Deadline management and planning");

        if (metrics.BudgetVariance > 0)
            improvements.Add("Budget control and monitoring");

        return improvements;
    }

    private DateTime GetWeekStartDate(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private decimal CalculateProductivityScore(int employeeId, List<DSR> dsrRecords)
    {
        var employeeDsrs = dsrRecords.Where(d => d.EmployeeId == employeeId).ToList();
        if (employeeDsrs.Count == 0) return 0;

        var averageHoursPerDay = employeeDsrs.Average(d => d.HoursWorked);
        return Math.Min(100, (averageHoursPerDay / 8) * 100); // Assuming 8 hours as full productivity
    }

    private decimal CalculateEfficiencyRating(int employeeId, List<DSR> dsrRecords)
    {
        var employeeDsrs = dsrRecords.Where(d => d.EmployeeId == employeeId).ToList();
        if (employeeDsrs.Count == 0) return 0;

        var totalHours = employeeDsrs.Sum(d => d.HoursWorked);
        var totalTasks = employeeDsrs.Count;

        if (totalTasks == 0) return 0;

        var hoursPerTask = totalHours / totalTasks;
        return Math.Min(100, (8 / Math.Max(hoursPerTask, 1)) * 100); // Assuming 8 hours per task as baseline
    }
}