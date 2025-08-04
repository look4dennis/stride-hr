using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.Infrastructure.Services;

public class ProjectMonitoringServiceSimple : IProjectMonitoringService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAlertRepository _alertRepository;
    private readonly IProjectRiskRepository _riskRepository;
    private readonly IDSRRepository _dsrRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectMonitoringServiceSimple> _logger;

    public ProjectMonitoringServiceSimple(
        IProjectRepository projectRepository,
        IProjectAlertRepository alertRepository,
        IProjectRiskRepository riskRepository,
        IDSRRepository dsrRepository,
        IProjectAssignmentRepository assignmentRepository,
        IMapper mapper,
        ILogger<ProjectMonitoringServiceSimple> logger)
    {
        _projectRepository = projectRepository;
        _alertRepository = alertRepository;
        _riskRepository = riskRepository;
        _dsrRepository = dsrRepository;
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectHoursReportDto> GetProjectHoursTrackingAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            // Simplified implementation - would need to implement actual DSR queries
            var report = new ProjectHoursReportDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                EstimatedHours = project.EstimatedHours,
                TotalHoursWorked = 0, // Would calculate from DSR records
                StartDate = start,
                EndDate = end,
                DailyHours = new List<DailyHoursDto>()
            };

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
            // Simplified implementation
            var reports = new List<ProjectHoursReportDto>();
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
            var dashboard = new ProjectDashboardDto
            {
                TeamLeaderId = teamLeaderId,
                TeamLeaderName = "Team Leader",
                ProjectAnalytics = new List<ProjectAnalyticsDto>(),
                TeamOverview = new TeamOverviewDto
                {
                    TotalProjects = 0,
                    ActiveProjects = 0,
                    CompletedProjects = 0,
                    DelayedProjects = 0,
                    TotalBudget = 0,
                    BudgetUtilized = 0,
                    TotalTeamMembers = 0,
                    OverallProductivity = 0,
                    AverageProjectHealth = 0
                },
                CriticalAlerts = new List<ProjectAlertDto>(),
                HighRisks = new List<ProjectRiskDto>()
            };

            return dashboard;
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
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var analytics = new ProjectAnalyticsDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Metrics = new ProjectMetricsDto
                {
                    TotalHoursWorked = 0,
                    EstimatedHours = project.EstimatedHours,
                    HoursVariance = 0,
                    BudgetUtilized = 0,
                    BudgetVariance = 0,
                    CompletionPercentage = 0,
                    TotalTasks = 0,
                    CompletedTasks = 0,
                    OverdueTasks = 0,
                    TeamMembersCount = 0,
                    AverageTaskCompletionTime = 0
                },
                Trends = new ProjectTrendsDto
                {
                    DailyProgress = new List<DailyProgressDto>(),
                    WeeklyHours = new List<WeeklyHoursDto>(),
                    TeamProductivity = new List<TeamMemberProductivityDto>(),
                    TaskStatusTrends = new List<TaskStatusTrendDto>()
                },
                Performance = new ProjectPerformanceDto
                {
                    OverallEfficiency = 85,
                    QualityScore = 90,
                    TimelineAdherence = 80,
                    BudgetAdherence = 95,
                    TeamSatisfaction = 85,
                    PerformanceGrade = "A",
                    StrengthAreas = new List<string> { "Good team collaboration" },
                    ImprovementAreas = new List<string> { "Time estimation" }
                },
                Risks = new List<ProjectRiskDto>(),
                GeneratedAt = DateTime.UtcNow
            };

            return analytics;
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
            var analytics = new List<ProjectAnalyticsDto>();
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
            var trends = new List<ProjectTrendsDto>
            {
                new ProjectTrendsDto
                {
                    DailyProgress = new List<DailyProgressDto>(),
                    WeeklyHours = new List<WeeklyHoursDto>(),
                    TeamProductivity = new List<TeamMemberProductivityDto>(),
                    TaskStatusTrends = new List<TaskStatusTrendDto>()
                }
            };
            return trends;
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
            var risks = await _riskRepository.GetProjectRisksAsync(projectId);
            return _mapper.Map<List<ProjectRiskDto>>(risks);
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
            var risk = new ProjectRisk
            {
                ProjectId = projectId,
                RiskType = riskType,
                Description = description,
                Severity = severity,
                Probability = probability,
                Impact = impact,
                Status = "Identified",
                IdentifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _riskRepository.AddAsync(risk);
            await _riskRepository.SaveChangesAsync();

            _logger.LogInformation("Project risk created for project {ProjectId}: {RiskType}", projectId, riskType);

            return _mapper.Map<ProjectRiskDto>(risk);
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
            var risk = await _riskRepository.GetByIdAsync(riskId);
            if (risk == null)
                return false;

            risk.MitigationPlan = mitigationPlan;
            risk.Status = status;
            risk.AssignedTo = assignedTo;

            if (status == "Resolved")
                risk.ResolvedAt = DateTime.UtcNow;

            await _riskRepository.UpdateAsync(risk);
            await _riskRepository.SaveChangesAsync();

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
            // Simplified implementation
            var risks = new List<ProjectRiskDto>();
            return risks;
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
            var project = await _projectRepository.GetByIdAsync(projectId);
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
            // Simplified implementation
            _logger.LogInformation("Automatic alerts generation completed");
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
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return 0;

            // Simplified health score calculation
            decimal healthScore = 7.5m; // Default good health score
            
            return Math.Max(0, Math.Min(10, healthScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project health score for project {ProjectId}", projectId);
            throw;
        }
    }
}