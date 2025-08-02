using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportBuilderService _reportBuilderService;
    private readonly IReportExportService _exportService;
    private readonly IReportSchedulingService _schedulingService;
    private readonly IReportDataVisualizationService _visualizationService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportBuilderService reportBuilderService,
        IReportExportService exportService,
        IReportSchedulingService schedulingService,
        IReportDataVisualizationService visualizationService,
        ILogger<ReportsController> logger)
    {
        _reportBuilderService = reportBuilderService;
        _exportService = exportService;
        _schedulingService = schedulingService;
        _visualizationService = visualizationService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private int? GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports()
    {
        try
        {
            var userId = GetCurrentUserId();
            var branchId = GetCurrentBranchId();

            var userReports = await _reportBuilderService.GetUserReportsAsync(userId, branchId);
            var publicReports = await _reportBuilderService.GetPublicReportsAsync(branchId);
            var sharedReports = await _reportBuilderService.GetSharedReportsAsync(userId);

            var result = new
            {
                UserReports = userReports,
                PublicReports = publicReports,
                SharedReports = sharedReports
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get reports for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving reports");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var report = await _reportBuilderService.GetReportAsync(id, userId);

            if (report == null)
                return NotFound("Report not found or access denied");

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving the report");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var branchId = GetCurrentBranchId();

            var report = await _reportBuilderService.CreateReportAsync(
                request.Name,
                request.Description,
                request.Type,
                request.Configuration,
                userId,
                branchId);

            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create report for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while creating the report");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var report = await _reportBuilderService.UpdateReportAsync(
                id,
                request.Name,
                request.Description,
                request.Configuration,
                userId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to update this report");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while updating the report");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _reportBuilderService.DeleteReportAsync(id, userId);

            if (!success)
                return NotFound("Report not found");

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to delete this report");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while deleting the report");
        }
    }

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> ExecuteReport(int id, [FromBody] Dictionary<string, object>? parameters = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _reportBuilderService.ExecuteReportAsync(id, userId, parameters);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to execute this report");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while executing the report");
        }
    }

    [HttpPost("preview")]
    public async Task<IActionResult> PreviewReport([FromBody] ReportBuilderConfiguration configuration)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _reportBuilderService.PreviewReportAsync(configuration, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview report for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while previewing the report");
        }
    }

    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareReport(int id, [FromBody] ShareReportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _reportBuilderService.ShareReportAsync(
                id,
                request.SharedWithUserId,
                request.Permission,
                userId,
                request.ExpiresAt);

            if (!success)
                return NotFound("Report not found or access denied");

            return Ok(new { Message = "Report shared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to share report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while sharing the report");
        }
    }

    [HttpDelete("{id}/share/{userId}")]
    public async Task<IActionResult> RevokeReportShare(int id, int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var success = await _reportBuilderService.RevokeReportShareAsync(id, userId, currentUserId);

            if (!success)
                return NotFound("Report share not found or access denied");

            return Ok(new { Message = "Report share revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke report share {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while revoking the report share");
        }
    }

    [HttpPost("{id}/export")]
    public async Task<IActionResult> ExportReport(int id, [FromBody] ReportExportRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var exportData = await _exportService.ExportReportAsync(id, request.Format, userId, request.Parameters);
            var mimeType = await _exportService.GetExportMimeTypeAsync(request.Format);
            var fileExtension = await _exportService.GetExportFileExtensionAsync(request.Format);
            
            var fileName = !string.IsNullOrEmpty(request.FileName) 
                ? request.FileName 
                : $"report_{id}_{DateTime.Now:yyyyMMdd_HHmmss}";

            return File(exportData, mimeType, $"{fileName}{fileExtension}");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to export this report");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export report {ReportId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while exporting the report");
        }
    }

    [HttpGet("data-sources")]
    public async Task<IActionResult> GetDataSources()
    {
        try
        {
            var userId = GetCurrentUserId();
            var dataSources = await _reportBuilderService.GetAvailableDataSourcesAsync(userId);

            return Ok(dataSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data sources for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving data sources");
        }
    }

    [HttpGet("data-sources/{name}/schema")]
    public async Task<IActionResult> GetDataSourceSchema(string name)
    {
        try
        {
            var userId = GetCurrentUserId();
            var schema = await _reportBuilderService.GetDataSourceSchemaAsync(name, userId);

            return Ok(schema);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get data source schema {DataSource} for user {UserId}", name, GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving data source schema");
        }
    }

    [HttpPost("chart-data")]
    public async Task<IActionResult> GenerateChartData([FromBody] GenerateChartDataRequest request)
    {
        try
        {
            var chartData = await _visualizationService.GenerateChartDataAsync(request.Data, request.ChartConfiguration);
            return Ok(chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate chart data for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while generating chart data");
        }
    }

    [HttpGet("chart-types")]
    public async Task<IActionResult> GetSupportedChartTypes()
    {
        try
        {
            var chartTypes = await _visualizationService.GetSupportedChartTypesAsync();
            return Ok(chartTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get supported chart types");
            return StatusCode(500, "An error occurred while retrieving chart types");
        }
    }

    [HttpPost("suggest-chart")]
    public async Task<IActionResult> SuggestChartConfiguration([FromBody] SuggestChartRequest request)
    {
        try
        {
            var suggestion = await _visualizationService.SuggestChartConfigurationAsync(request.Columns, request.SampleData);
            return Ok(suggestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suggest chart configuration for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while suggesting chart configuration");
        }
    }
}

// Request/Response DTOs
public class CreateReportRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportBuilderConfiguration Configuration { get; set; } = new();
}

public class UpdateReportRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportBuilderConfiguration Configuration { get; set; } = new();
}

public class ShareReportRequest
{
    public int SharedWithUserId { get; set; }
    public ReportPermission Permission { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class GenerateChartDataRequest
{
    public ReportExecutionResult Data { get; set; } = new();
    public ReportChartConfiguration ChartConfiguration { get; set; } = new();
}

public class SuggestChartRequest
{
    public List<ReportColumn> Columns { get; set; } = new();
    public List<Dictionary<string, object>> SampleData { get; set; } = new();
}