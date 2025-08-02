using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;
using StrideHR.Core.Enums;
using StrideHR.API.Models;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectMonitoringController : ControllerBase
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

    /// <summary>
    /// Get real-time monitoring data for a specific project
    /// </summary>
    [HttpGet("{projectId}/monitoring")]
    public async Task<ActionResult<ApiResponse<ProjectMonitoringDto>>> GetProjectMonitoringData(int projectId)
    {
        try
        {
            var monitoringData = await _monitoringService.GetProjectMonitoringDataAsync(projectId);
            return Ok(new ApiResponse<ProjectMonitoringDto>
            {
                Success = true,
                Data = monitoringData,
                Message = "Project monitoring data retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<ProjectMonitoringDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project monitoring data for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<ProjectMonitoringDto>
            {
                Success = false,
                Message = "An error occurred while retrieving project monitoring data"
            });
        }
    }

    /// <summary>
    /// Get monitoring data for multiple projects
    /// </summary>
    [HttpPost("monitoring/batch")]
    public async Task<ActionResult<ApiResponse<List<ProjectMonitoringDto>>>> GetProjectsMonitoringData([FromBody] List<int> projectIds)
    {
        try
        {
            var monitoringData = await _monitoringService.GetProjectsMonitoringDataAsync(projectIds);
            return Ok(new ApiResponse<List<ProjectMonitoringDto>>
            {
                Success = true,
                Data = monitoringData,
                Message = "Projects monitoring data retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring data for multiple projects");
            return StatusCode(500, new ApiResponse<List<ProjectMonitoringDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving projects monitoring data"
            });
        }
    }

    /// <summary>
    /// Get team leader dashboard with all projects and summary
    /// </summary>
    [HttpGet("team-leader/{teamLeaderId}/dashboard")]
    public async Task<ActionResult<ApiResponse<TeamLeaderDashboardDto>>> GetTeamLeaderDashboard(int teamLeaderId)
    {
        try
        {
            var dashboard = await _monitoringService.GetTeamLeaderDashboardAsync(teamLeaderId);
            return Ok(new ApiResponse<TeamLeaderDashboardDto>
            {
                Success = true,
                Data = dashboard,
                Message = "Team leader dashboard retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader dashboard for: {TeamLeaderId}", teamLeaderId);
            return StatusCode(500, new ApiResponse<TeamLeaderDashboardDto>
            {
                Success = false,
                Message = "An error occurred while retrieving team leader dashboard"
            });
        }
    }

    /// <summary>
    /// Get project hours analysis and variance reporting
    /// </summary>
    [HttpGet("{projectId}/hours-analysis")]
    public async Task<ActionResult<ApiResponse<List<ProjectHoursReportDto>>>> GetProjectHoursAnalysis(
        int projectId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analysis = await _monitoringService.GetProjectHoursAnalysisAsync(projectId, startDate, endDate);
            return Ok(new ApiResponse<List<ProjectHoursReportDto>>
            {
                Success = true,
                Data = analysis,
                Message = "Project hours analysis retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours analysis for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<List<ProjectHoursReportDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving project hours analysis"
            });
        }
    }

    /// <summary>
    /// Get team hours analysis for a team leader
    /// </summary>
    [HttpGet("team-leader/{teamLeaderId}/hours-analysis")]
    public async Task<ActionResult<ApiResponse<List<ProjectHoursReportDto>>>> GetTeamHoursAnalysis(
        int teamLeaderId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analysis = await _monitoringService.GetTeamHoursAnalysisAsync(teamLeaderId, startDate, endDate);
            return Ok(new ApiResponse<List<ProjectHoursReportDto>>
            {
                Success = true,
                Data = analysis,
                Message = "Team hours analysis retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team hours analysis for team leader: {TeamLeaderId}", teamLeaderId);
            return StatusCode(500, new ApiResponse<List<ProjectHoursReportDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving team hours analysis"
            });
        }
    }

    /// <summary>
    /// Get project variance report
    /// </summary>
    [HttpGet("{projectId}/variance")]
    public async Task<ActionResult<ApiResponse<ProjectVarianceDto>>> GetProjectVarianceReport(int projectId)
    {
        try
        {
            var variance = await _monitoringService.GetProjectVarianceReportAsync(projectId);
            return Ok(new ApiResponse<ProjectVarianceDto>
            {
                Success = true,
                Data = variance,
                Message = "Project variance report retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<ProjectVarianceDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project variance report for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<ProjectVarianceDto>
            {
                Success = false,
                Message = "An error occurred while retrieving project variance report"
            });
        }
    }

    /// <summary>
    /// Get project alerts
    /// </summary>
    [HttpGet("{projectId}/alerts")]
    public async Task<ActionResult<ApiResponse<List<ProjectAlertDto>>>> GetProjectAlerts(int projectId)
    {
        try
        {
            var alerts = await _monitoringService.GetProjectAlertsAsync(projectId);
            return Ok(new ApiResponse<List<ProjectAlertDto>>
            {
                Success = true,
                Data = alerts,
                Message = "Project alerts retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project alerts for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<List<ProjectAlertDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving project alerts"
            });
        }
    }

    /// <summary>
    /// Get team leader alerts
    /// </summary>
    [HttpGet("team-leader/{teamLeaderId}/alerts")]
    public async Task<ActionResult<ApiResponse<List<ProjectAlertDto>>>> GetTeamLeaderAlerts(int teamLeaderId)
    {
        try
        {
            var alerts = await _monitoringService.GetTeamLeaderAlertsAsync(teamLeaderId);
            return Ok(new ApiResponse<List<ProjectAlertDto>>
            {
                Success = true,
                Data = alerts,
                Message = "Team leader alerts retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team leader alerts for: {TeamLeaderId}", teamLeaderId);
            return StatusCode(500, new ApiResponse<List<ProjectAlertDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving team leader alerts"
            });
        }
    }

    /// <summary>
    /// Create a project alert
    /// </summary>
    [HttpPost("{projectId}/alerts")]
    public async Task<ActionResult<ApiResponse<ProjectAlertDto>>> CreateProjectAlert(
        int projectId, 
        [FromBody] CreateProjectAlertRequest request)
    {
        try
        {
            var alert = await _monitoringService.CreateProjectAlertAsync(
                projectId, 
                request.AlertType, 
                request.Message, 
                request.Severity);

            return CreatedAtAction(
                nameof(GetProjectAlerts), 
                new { projectId }, 
                new ApiResponse<ProjectAlertDto>
                {
                    Success = true,
                    Data = alert,
                    Message = "Project alert created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project alert for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<ProjectAlertDto>
            {
                Success = false,
                Message = "An error occurred while creating project alert"
            });
        }
    }

    /// <summary>
    /// Resolve a project alert
    /// </summary>
    [HttpPut("alerts/{alertId}/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ResolveProjectAlert(
        int alertId, 
        [FromBody] ResolveAlertRequest request)
    {
        try
        {
            var result = await _monitoringService.ResolveProjectAlertAsync(
                alertId, 
                request.ResolvedByEmployeeId, 
                request.ResolutionNotes);

            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Alert not found or already resolved"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = result,
                Message = "Project alert resolved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving project alert: {AlertId}", alertId);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while resolving project alert"
            });
        }
    }

    /// <summary>
    /// Check and create automated alerts for a project
    /// </summary>
    [HttpPost("{projectId}/check-alerts")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckAutomatedAlerts(int projectId)
    {
        try
        {
            await _monitoringService.CheckAndCreateAutomatedAlertsAsync(projectId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Automated alerts check completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking automated alerts for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking automated alerts"
            });
        }
    }

    /// <summary>
    /// Get project efficiency metrics
    /// </summary>
    [HttpGet("{projectId}/efficiency")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetProjectEfficiency(int projectId)
    {
        try
        {
            var efficiency = await _monitoringService.CalculateProjectEfficiencyAsync(projectId);
            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Data = efficiency,
                Message = "Project efficiency calculated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project efficiency for project: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = "An error occurred while calculating project efficiency"
            });
        }
    }

    /// <summary>
    /// Get team efficiency metrics
    /// </summary>
    [HttpGet("team-leader/{teamLeaderId}/efficiency")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTeamEfficiency(int teamLeaderId)
    {
        try
        {
            var efficiency = await _monitoringService.CalculateTeamEfficiencyAsync(teamLeaderId);
            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Data = efficiency,
                Message = "Team efficiency calculated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating team efficiency for team leader: {TeamLeaderId}", teamLeaderId);
            return StatusCode(500, new ApiResponse<decimal>
            {
                Success = false,
                Message = "An error occurred while calculating team efficiency"
            });
        }
    }

    /// <summary>
    /// Check if project is at risk
    /// </summary>
    [HttpGet("{projectId}/at-risk")]
    public async Task<ActionResult<ApiResponse<bool>>> IsProjectAtRisk(int projectId)
    {
        try
        {
            var isAtRisk = await _monitoringService.IsProjectAtRiskAsync(projectId);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isAtRisk,
                Message = "Project risk status retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is at risk: {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while checking project risk status"
            });
        }
    }

    /// <summary>
    /// Get at-risk projects for a team leader
    /// </summary>
    [HttpGet("team-leader/{teamLeaderId}/at-risk-projects")]
    public async Task<ActionResult<ApiResponse<List<int>>>> GetAtRiskProjects(int teamLeaderId)
    {
        try
        {
            var atRiskProjects = await _monitoringService.GetAtRiskProjectsAsync(teamLeaderId);
            return Ok(new ApiResponse<List<int>>
            {
                Success = true,
                Data = atRiskProjects,
                Message = "At-risk projects retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting at-risk projects for team leader: {TeamLeaderId}", teamLeaderId);
            return StatusCode(500, new ApiResponse<List<int>>
            {
                Success = false,
                Message = "An error occurred while retrieving at-risk projects"
            });
        }
    }
}

public class CreateProjectAlertRequest
{
    public ProjectAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
}

public class ResolveAlertRequest
{
    public int ResolvedByEmployeeId { get; set; }
    public string? ResolutionNotes { get; set; }
}