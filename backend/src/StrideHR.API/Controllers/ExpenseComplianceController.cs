using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseComplianceController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpenseComplianceController> _logger;

    public ExpenseComplianceController(IExpenseService expenseService, ILogger<ExpenseComplianceController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get compliance report for an organization
    /// </summary>
    [HttpGet("organization/{organizationId}/report")]
    public async Task<ActionResult<ExpenseComplianceReportDto>> GetComplianceReport(
        int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var report = await _expenseService.GetComplianceReportAsync(organizationId, startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance report for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving the compliance report");
        }
    }

    /// <summary>
    /// Get compliance violations for an organization
    /// </summary>
    [HttpGet("organization/{organizationId}/violations")]
    public async Task<ActionResult<IEnumerable<ExpenseComplianceViolationDto>>> GetComplianceViolations(
        int organizationId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var violations = await _expenseService.GetComplianceViolationsAsync(organizationId, startDate, endDate);
            return Ok(violations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance violations for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving compliance violations");
        }
    }

    /// <summary>
    /// Validate expense compliance for a claim
    /// </summary>
    [HttpGet("claims/{expenseClaimId}/validate")]
    public async Task<ActionResult<bool>> ValidateExpenseCompliance(int expenseClaimId)
    {
        try
        {
            var isCompliant = await _expenseService.ValidateExpenseComplianceAsync(expenseClaimId);
            return Ok(isCompliant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating expense compliance for claim {ExpenseClaimId}", expenseClaimId);
            return StatusCode(500, "An error occurred while validating expense compliance");
        }
    }

    /// <summary>
    /// Resolve a compliance violation
    /// </summary>
    [HttpPut("violations/{violationId}/resolve")]
    public async Task<ActionResult> ResolveComplianceViolation(
        int violationId,
        [FromBody] ResolveViolationDto dto)
    {
        try
        {
            var result = await _expenseService.ResolveComplianceViolationAsync(violationId, dto.ResolvedBy, dto.ResolutionNotes);
            if (!result)
                return NotFound("Violation not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving compliance violation {ViolationId}", violationId);
            return StatusCode(500, "An error occurred while resolving the compliance violation");
        }
    }

    /// <summary>
    /// Waive a compliance violation
    /// </summary>
    [HttpPut("violations/{violationId}/waive")]
    public async Task<ActionResult> WaiveComplianceViolation(
        int violationId,
        [FromBody] WaiveViolationDto dto)
    {
        try
        {
            var result = await _expenseService.WaiveComplianceViolationAsync(violationId, dto.WaivedBy, dto.WaiverReason);
            if (!result)
                return NotFound("Violation not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiving compliance violation {ViolationId}", violationId);
            return StatusCode(500, "An error occurred while waiving the compliance violation");
        }
    }
}

public class ResolveViolationDto
{
    public int ResolvedBy { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class WaiveViolationDto
{
    public int WaivedBy { get; set; }
    public string WaiverReason { get; set; } = string.Empty;
}