using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.API.Controllers;

[Authorize]
public class ProjectMonitoringController : BaseController
{
    private readonly IProjectMonitoringService _monitoringService;
    private readonly ILogger<ProjectMonitoringController> _logger;

    public ProjectMonitoringController(
        IProjectMonitoringService monitoringService,
        ILogger<ProjectMonitoringController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : 0;
    }

    // Project Hours Tracking Dashboard
    [HttpGet("projects/{projectId}/hours-tracking")]
    public async Task<IActionResult> GetProjectHoursTracking(int projectId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _monitoringService.GetProjectHoursTrackingAsync(projectId, startDate, endDate);
            return Success(report);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours tracking for project {ProjectId}", projectId);
            return Error("Failed to get project hours tracking");
        }
    }

    [HttpGet("team-hours-tracking")]
    public async Task<IActionResult> GetTeamHoursTracking([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var reports = await _monitoringService.GetTeamHoursTrackingAsync(currentEmployeeId, startDate, endDate);
            return Success(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team hours tracking");
            return Error("Failed to get team hours tracking");
        }
    }

    [HttpGet("team-leader-dashboard")]
    public async Task<IActionResult> GetTeamLeaderDashboard()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var dashboard = await _monitoringService.GetTeamLeaderDashboardAsync(currentEmployeeId);
            return Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader dashboard");
            return Error("Failed to get team leader dashboard");
        }
    }

    // Project Analytics and Reporting
    [HttpGet("projects/{projectId}/analytics")]
    public async Task<IActionResult> GetProjectAnalytics(int projectId)
    {
        try
        {
            var analytics = await _monitoringService.GetProjectAnalyticsAsync(projectId);
            return Success(analytics);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project analytics for project {ProjectId}", projectId);
            return Error("Failed to get project analytics");
        }
    }

    [HttpGet("team-project-analytics")]
    public async Task<IActionResult> GetTeamProjectAnalytics()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var analytics = await _monitoringService.GetTeamProjectAnalyticsAsync(currentEmployeeId);
            return Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team project analytics");
            return Error("Failed to get team project analytics");
        }
    }

    [HttpGet("projects/{projectId}/performance")]
    public async Task<IActionResult> GetProjectPerformance(int projectId)
    {
        try
        {
            var performance = await _monitoringService.GetProjectPerformanceAsync(projectId);
            return Success(performance);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project performance for project {ProjectId}", projectId);
            return Error("Failed to get project performance");
        }
    }

    [HttpGet("projects/{projectId}/trends")]
    public async Task<IActionResult> GetProjectTrends(int projectId, [FromQuery] int days = 30)
    {
        try
        {
            var trends = await _monitoringService.GetProjectTrendsAsync(projectId, days);
            return Success(trends);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project trends for project {ProjectId}", projectId);
            return Error("Failed to get project trends");
        }
    }

    // Project Alerts and Notifications
    [HttpGet("projects/{projectId}/alerts")]
    public async Task<IActionResult> GetProjectAlerts(int projectId)
    {
        try
        {
            var alerts = await _monitoringService.GetProjectAlertsAsync(projectId);
            return Success(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project alerts for project {ProjectId}", projectId);
            return Error("Failed to get project alerts");
        }
    }

    [HttpGet("team-alerts")]
    public async Task<IActionResult> GetTeamAlerts()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var alerts = await _monitoringService.GetTeamAlertsAsync(currentEmployeeId);
            return Success(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team alerts");
            return Error("Failed to get team alerts");
        }
    }

    [HttpPost("projects/{projectId}/alerts")]
    public async Task<IActionResult> CreateProjectAlert(int projectId, [FromBody] CreateProjectAlertRequest request)
    {
        try
        {
            var alert = await _monitoringService.CreateProjectAlertAsync(projectId, request.AlertType, request.Message, request.Severity);
            return Success(alert, "Project alert created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project alert for project {ProjectId}", projectId);
            return Error("Failed to create project alert");
        }
    }

    [HttpPut("alerts/{alertId}/resolve")]
    public async Task<IActionResult> ResolveProjectAlert(int alertId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _monitoringService.ResolveProjectAlertAsync(alertId, currentEmployeeId);
            if (!result)
                return Error("Alert not found or already resolved");

            return Success("Alert resolved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving project alert {AlertId}", alertId);
            return Error("Failed to resolve project alert");
        }
    }

    [HttpGet("critical-alerts")]
    public async Task<IActionResult> GetCriticalAlerts()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var alerts = await _monitoringService.GetCriticalAlertsAsync(currentEmployeeId);
            return Success(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical alerts");
            return Error("Failed to get critical alerts");
        }
    }

    // Risk Management
    [HttpGet("projects/{projectId}/risks")]
    public async Task<IActionResult> GetProjectRisks(int projectId)
    {
        try
        {
            var risks = await _monitoringService.GetProjectRisksAsync(projectId);
            return Success(risks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project risks for project {ProjectId}", projectId);
            return Error("Failed to get project risks");
        }
    }

    [HttpPost("projects/{projectId}/risks")]
    public async Task<IActionResult> CreateProjectRisk(int projectId, [FromBody] CreateProjectRiskRequest request)
    {
        try
        {
            var risk = await _monitoringService.CreateProjectRiskAsync(projectId, request.RiskType, request.Description, request.Severity, request.Probability, request.Impact);
            return Success(risk, "Project risk created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project risk for project {ProjectId}", projectId);
            return Error("Failed to create project risk");
        }
    }

    [HttpPut("risks/{riskId}")]
    public async Task<IActionResult> UpdateProjectRisk(int riskId, [FromBody] UpdateProjectRiskRequest request)
    {
        try
        {
            var result = await _monitoringService.UpdateProjectRiskAsync(riskId, request.MitigationPlan, request.Status, request.AssignedTo);
            if (!result)
                return Error("Risk not found");

            return Success("Project risk updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project risk {RiskId}", riskId);
            return Error("Failed to update project risk");
        }
    }

    [HttpGet("high-risks")]
    public async Task<IActionResult> GetHighRisks()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var risks = await _monitoringService.GetHighRisksAsync(currentEmployeeId);
            return Success(risks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high risks");
            return Error("Failed to get high risks");
        }
    }

    // Automated Monitoring
    [HttpPost("projects/{projectId}/check-health")]
    public async Task<IActionResult> CheckProjectHealth(int projectId)
    {
        try
        {
            await _monitoringService.CheckProjectHealthAsync(projectId);
            return Success("Project health check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking project health for project {ProjectId}", projectId);
            return Error("Failed to check project health");
        }
    }

    [HttpPost("generate-automatic-alerts")]
    public async Task<IActionResult> GenerateAutomaticAlerts()
    {
        try
        {
            await _monitoringService.GenerateAutomaticAlertsAsync();
            return Success("Automatic alerts generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating automatic alerts");
            return Error("Failed to generate automatic alerts");
        }
    }

    [HttpGet("projects/{projectId}/is-at-risk")]
    public async Task<IActionResult> IsProjectAtRisk(int projectId)
    {
        try
        {
            var isAtRisk = await _monitoringService.IsProjectAtRiskAsync(projectId);
            return Success(new { ProjectId = projectId, IsAtRisk = isAtRisk });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is at risk {ProjectId}", projectId);
            return Error("Failed to check project risk status");
        }
    }

    [HttpGet("projects/{projectId}/health-score")]
    public async Task<IActionResult> GetProjectHealthScore(int projectId)
    {
        try
        {
            var healthScore = await _monitoringService.CalculateProjectHealthScoreAsync(projectId);
            return Success(new { ProjectId = projectId, HealthScore = healthScore });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project health score for project {ProjectId}", projectId);
            return Error("Failed to get project health score");
        }
    }
}

// Request DTOs
public class CreateProjectAlertRequest
{
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class CreateProjectRiskRequest
{
    public string RiskType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
}

public class UpdateProjectRiskRequest
{
    public string MitigationPlan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedTo { get; set; }
}