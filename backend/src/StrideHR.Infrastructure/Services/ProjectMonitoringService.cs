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
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IDSRRepository _dsrRepository;
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectMonitoringService> _logger;

    public ProjectMonitoringService(
        IProjectRepository projectRepository,
        IProjectAlertRepository alertRepository,
        IProjectAssignmentRepository assignmentRepository,
        IProjectTaskRepository taskRepository,
        IDSRRepository dsrRepository,
        IProjectService projectService,
        IMapper mapper,
        ILogger<ProjectMonitoringService> logger)
    {
        _projectRepository = projectRepository;
        _alertRepository = alertRepository;
        _assignmentRepository = assignmentRepository;
        _taskRepository = taskRepository;
        _dsrRepository = dsrRepository;
        _projectService = projectService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectMonitoringDto> GetProjectMonitoringDataAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            var progress = await _projectService.GetProjectProgressAsync(projectId);
            var variance = await CalculateProjectVarianceAsync(projectId);
            var alerts = await GetProjectAlertsAsync(projectId);
            var teamMembers = await _projectService.GetProjectTeamMembersAsync(projectId);

            return new ProjectMonitoringDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Progress = progress,
                Variance = variance,
                Alerts = alerts,
                TeamMembers = teamMembers,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project monitoring data for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectMonitoringDto>> GetProjectsMonitoringDataAsync(List<int> projectIds)
    {
        try
        {
            var monitoringData = new List<ProjectMonitoringDto>();

            foreach (var projectId in projectIds)
            {
                var data = await GetProjectMonitoringDataAsync(projectId);
                monitoringData.Add(data);
            }

            return monitoringData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring data for multiple projects");
            throw;
        }
    }

    public async Task<ProjectVarianceDto> CalculateProjectVarianceAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            var actualHours = await _dsrRepository.GetTotalHoursByProjectAsync(projectId);
            var progress = await _projectService.GetProjectProgressAsync(projectId);

            var hoursVariance = actualHours - project.EstimatedHours;
            var budgetVariance = (actualHours * 50) - project.Budget; // Assuming $50/hour rate
            var scheduleVarianceDays = (DateTime.Today - project.EndDate).Days;
            var performanceIndex = project.EstimatedHours > 0 ? actualHours / project.EstimatedHours : 0;

            return new ProjectVarianceDto
            {
                HoursVariance = hoursVariance,
                BudgetVariance = budgetVariance,
                ScheduleVarianceDays = scheduleVarianceDays,
                PerformanceIndex = performanceIndex,
                IsOverBudget = hoursVariance > 0,
                IsBehindSchedule = scheduleVarianceDays > 0 && progress.CompletionPercentage < 100,
                VarianceReason = GetVarianceReason(hoursVariance, scheduleVarianceDays, progress.CompletionPercentage)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project variance for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<TeamLeaderDashboardDto> GetTeamLeaderDashboardAsync(int teamLeaderId)
    {
        try
        {
            var projects = await GetProjectsForTeamLeaderAsync(teamLeaderId);
            var criticalAlerts = await GetTeamLeaderAlertsAsync(teamLeaderId);
            
            var summary = new ProjectSummaryDto
            {
                TotalProjects = projects.Count,
                OnTrackProjects = projects.Count(p => p.Progress.IsOnTrack),
                DelayedProjects = projects.Count(p => p.Variance.IsBehindSchedule),
                OverBudgetProjects = projects.Count(p => p.Variance.IsOverBudget),
                TotalEstimatedHours = projects.Sum(p => p.Progress.TotalEstimatedHours),
                TotalActualHours = projects.Sum(p => p.Progress.ActualHoursWorked),
                TotalTeamMembers = projects.SelectMany(p => p.TeamMembers).Select(tm => tm.EmployeeId).Distinct().Count()
            };

            summary.OverallEfficiency = summary.TotalEstimatedHours > 0 
                ? (summary.TotalEstimatedHours / summary.TotalActualHours) * 100 
                : 0;

            var teamLeader = await _projectRepository.GetEmployeeAsync(teamLeaderId);
            var teamLeaderName = teamLeader != null ? $"{teamLeader.FirstName} {teamLeader.LastName}" : "Unknown";

            return new TeamLeaderDashboardDto
            {
                TeamLeaderId = teamLeaderId,
                TeamLeaderName = teamLeaderName,
                Projects = projects,
                Summary = summary,
                CriticalAlerts = criticalAlerts.Where(a => a.Severity == "Critical" || a.Severity == "High").ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader dashboard for: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<List<ProjectMonitoringDto>> GetProjectsForTeamLeaderAsync(int teamLeaderId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var monitoringData = new List<ProjectMonitoringDto>();

            foreach (var project in projects)
            {
                var data = await GetProjectMonitoringDataAsync(project.Id);
                monitoringData.Add(data);
            }

            return monitoringData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for team leader: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<List<ProjectHoursReportDto>> GetProjectHoursAnalysisAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            return (await _projectService.GetProjectHoursReportAsync(projectId, startDate, endDate)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours analysis for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectHoursReportDto>> GetTeamHoursAnalysisAsync(int teamLeaderId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            return (await _projectService.GetHoursTrackingReportAsync(teamLeaderId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team hours analysis for team leader: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectVarianceDto> GetProjectVarianceReportAsync(int projectId)
    {
        try
        {
            return await CalculateProjectVarianceAsync(projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project variance report for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectAlertDto>> GetProjectAlertsAsync(int projectId)
    {
        try
        {
            var alerts = await _alertRepository.GetAlertsByProjectAsync(projectId);
            return _mapper.Map<List<ProjectAlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project alerts for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectAlertDto>> GetTeamLeaderAlertsAsync(int teamLeaderId)
    {
        try
        {
            var alerts = await _alertRepository.GetAlertsByTeamLeadAsync(teamLeaderId);
            return _mapper.Map<List<ProjectAlertDto>>(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader alerts for: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectAlertDto> CreateProjectAlertAsync(int projectId, ProjectAlertType alertType, string message, AlertSeverity severity)
    {
        try
        {
            var alert = new ProjectAlert
            {
                ProjectId = projectId,
                AlertType = alertType,
                Message = message,
                Severity = severity,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _alertRepository.AddAsync(alert);
            await _alertRepository.SaveChangesAsync();

            _logger.LogInformation("Project alert created: {ProjectId}, Type: {AlertType}, Severity: {Severity}", 
                projectId, alertType, severity);

            return _mapper.Map<ProjectAlertDto>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project alert for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> ResolveProjectAlertAsync(int alertId, int resolvedByEmployeeId, string? resolutionNotes = null)
    {
        try
        {
            var result = await _alertRepository.ResolveAlertAsync(alertId, resolvedByEmployeeId, resolutionNotes);
            
            if (result)
            {
                _logger.LogInformation("Project alert resolved: {AlertId} by employee: {EmployeeId}", 
                    alertId, resolvedByEmployeeId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving project alert: {AlertId}", alertId);
            throw;
        }
    }

    public async Task CheckAndCreateAutomatedAlertsAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null) return;

            var progress = await _projectService.GetProjectProgressAsync(projectId);
            var variance = await CalculateProjectVarianceAsync(projectId);

            // Check for schedule delays
            if (variance.IsBehindSchedule && variance.ScheduleVarianceDays > 3)
            {
                await CreateProjectAlertAsync(projectId, ProjectAlertType.ScheduleDelay,
                    $"Project is {variance.ScheduleVarianceDays} days behind schedule",
                    variance.ScheduleVarianceDays > 7 ? AlertSeverity.High : AlertSeverity.Medium);
            }

            // Check for budget overruns
            if (variance.IsOverBudget && variance.HoursVariance > project.EstimatedHours * 0.1m)
            {
                await CreateProjectAlertAsync(projectId, ProjectAlertType.BudgetOverrun,
                    $"Project is over budget by {variance.HoursVariance:F1} hours",
                    variance.HoursVariance > project.EstimatedHours * 0.2m ? AlertSeverity.High : AlertSeverity.Medium);
            }

            // Check for overdue tasks
            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);
            var overdueTasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.Today && 
                                               t.Status != ProjectTaskStatus.Done).ToList();

            if (overdueTasks.Any())
            {
                await CreateProjectAlertAsync(projectId, ProjectAlertType.TaskOverdue,
                    $"{overdueTasks.Count} tasks are overdue",
                    overdueTasks.Count > 3 ? AlertSeverity.High : AlertSeverity.Medium);
            }

            // Check for low productivity
            if (progress.CompletionPercentage < 50 && variance.PerformanceIndex < 0.8m)
            {
                await CreateProjectAlertAsync(projectId, ProjectAlertType.LowProductivity,
                    "Project productivity is below expected levels",
                    AlertSeverity.Medium);
            }

            _logger.LogInformation("Automated alerts check completed for project: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking automated alerts for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task CheckAndCreateAutomatedAlertsForAllProjectsAsync()
    {
        try
        {
            var activeProjects = await _projectRepository.GetActiveProjectsAsync();
            
            foreach (var project in activeProjects)
            {
                await CheckAndCreateAutomatedAlertsAsync(project.Id);
            }

            _logger.LogInformation("Automated alerts check completed for all active projects");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking automated alerts for all projects");
            throw;
        }
    }

    public async Task<decimal> CalculateProjectEfficiencyAsync(int projectId)
    {
        try
        {
            var progress = await _projectService.GetProjectProgressAsync(projectId);
            
            if (progress.ActualHoursWorked == 0)
                return 0;

            return (progress.TotalEstimatedHours / progress.ActualHoursWorked) * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project efficiency for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<decimal> CalculateTeamEfficiencyAsync(int teamLeaderId)
    {
        try
        {
            var projects = await GetProjectsForTeamLeaderAsync(teamLeaderId);
            
            if (!projects.Any())
                return 0;

            var totalEstimated = projects.Sum(p => p.Progress.TotalEstimatedHours);
            var totalActual = projects.Sum(p => p.Progress.ActualHoursWorked);

            if (totalActual == 0)
                return 0;

            return (totalEstimated / totalActual) * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating team efficiency for team leader: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<bool> IsProjectAtRiskAsync(int projectId)
    {
        try
        {
            var variance = await CalculateProjectVarianceAsync(projectId);
            var alertCount = await _alertRepository.GetUnresolvedAlertCountAsync(projectId);

            return variance.IsOverBudget || variance.IsBehindSchedule || alertCount > 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is at risk: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<int>> GetAtRiskProjectsAsync(int teamLeaderId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var atRiskProjects = new List<int>();

            foreach (var project in projects)
            {
                if (await IsProjectAtRiskAsync(project.Id))
                {
                    atRiskProjects.Add(project.Id);
                }
            }

            return atRiskProjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting at-risk projects for team leader: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    private string GetVarianceReason(decimal hoursVariance, int scheduleVarianceDays, decimal completionPercentage)
    {
        if (hoursVariance > 0 && scheduleVarianceDays > 0)
            return "Project is both over budget and behind schedule";
        
        if (hoursVariance > 0)
            return "Project is over the estimated hours";
        
        if (scheduleVarianceDays > 0)
            return "Project is behind the planned schedule";
        
        if (completionPercentage < 50)
            return "Project progress is slower than expected";
        
        return "Project is on track";
    }
}