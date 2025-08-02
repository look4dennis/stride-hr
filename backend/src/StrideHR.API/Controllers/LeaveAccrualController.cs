using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Leave;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveAccrualController : ControllerBase
{
    private readonly ILeaveManagementService _leaveManagementService;
    private readonly ILogger<LeaveAccrualController> _logger;

    public LeaveAccrualController(
        ILeaveManagementService leaveManagementService,
        ILogger<LeaveAccrualController> logger)
    {
        _leaveManagementService = leaveManagementService;
        _logger = logger;
    }

    [HttpGet("employee/{employeeId}/year/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveAccrualDto>>> GetEmployeeAccruals(int employeeId, int year)
    {
        try
        {
            var accruals = await _leaveManagementService.GetEmployeeAccrualsAsync(employeeId, year);
            return Ok(accruals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accruals for employee {EmployeeId}, year {Year}", 
                employeeId, year);
            return StatusCode(500, "An error occurred while retrieving leave accruals");
        }
    }

    [HttpPost]
    public async Task<ActionResult<LeaveAccrualDto>> CreateAccrual([FromBody] CreateLeaveAccrualDto accrualDto)
    {
        try
        {
            var accrual = await _leaveManagementService.CreateAccrualAsync(accrualDto);
            return CreatedAtAction(nameof(GetEmployeeAccruals), 
                new { employeeId = accrual.EmployeeId, year = accrual.Year }, accrual);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave accrual");
            return StatusCode(500, "An error occurred while creating leave accrual");
        }
    }

    [HttpPost("process-monthly/{year}/{month}")]
    public async Task<ActionResult<IEnumerable<LeaveAccrualDto>>> ProcessMonthlyAccruals(int year, int month)
    {
        try
        {
            var accruals = await _leaveManagementService.ProcessMonthlyAccrualsAsync(year, month);
            return Ok(accruals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing monthly accruals for {Year}-{Month}", year, month);
            return StatusCode(500, "An error occurred while processing monthly accruals");
        }
    }

    [HttpPost("process-employee/{employeeId}/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveAccrualDto>>> ProcessEmployeeAccruals(int employeeId, int year)
    {
        try
        {
            var accruals = await _leaveManagementService.ProcessEmployeeAccrualsAsync(employeeId, year);
            return Ok(accruals);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing accruals for employee {EmployeeId}, year {Year}", 
                employeeId, year);
            return StatusCode(500, "An error occurred while processing employee accruals");
        }
    }

    [HttpGet("rules/{leavePolicyId}")]
    public async Task<ActionResult<IEnumerable<LeaveAccrualRuleDto>>> GetAccrualRules(int leavePolicyId)
    {
        try
        {
            var rules = await _leaveManagementService.GetAccrualRulesAsync(leavePolicyId);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accrual rules for policy {PolicyId}", leavePolicyId);
            return StatusCode(500, "An error occurred while retrieving accrual rules");
        }
    }

    [HttpPost("rules")]
    public async Task<ActionResult<LeaveAccrualRuleDto>> CreateAccrualRule([FromBody] CreateLeaveAccrualRuleDto ruleDto)
    {
        try
        {
            var rule = await _leaveManagementService.CreateAccrualRuleAsync(ruleDto);
            return CreatedAtAction(nameof(GetAccrualRules), 
                new { leavePolicyId = rule.LeavePolicyId }, rule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating accrual rule");
            return StatusCode(500, "An error occurred while creating accrual rule");
        }
    }

    [HttpPut("rules/{id}")]
    public async Task<ActionResult<LeaveAccrualRuleDto>> UpdateAccrualRule(int id, [FromBody] CreateLeaveAccrualRuleDto ruleDto)
    {
        try
        {
            var rule = await _leaveManagementService.UpdateAccrualRuleAsync(id, ruleDto);
            return Ok(rule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating accrual rule {RuleId}", id);
            return StatusCode(500, "An error occurred while updating accrual rule");
        }
    }

    [HttpDelete("rules/{id}")]
    public async Task<ActionResult> DeleteAccrualRule(int id)
    {
        try
        {
            var result = await _leaveManagementService.DeleteAccrualRuleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting accrual rule {RuleId}", id);
            return StatusCode(500, "An error occurred while deleting accrual rule");
        }
    }
}