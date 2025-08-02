using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Leave;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveEncashmentController : ControllerBase
{
    private readonly ILeaveManagementService _leaveManagementService;
    private readonly ILogger<LeaveEncashmentController> _logger;

    public LeaveEncashmentController(
        ILeaveManagementService leaveManagementService,
        ILogger<LeaveEncashmentController> logger)
    {
        _leaveManagementService = leaveManagementService;
        _logger = logger;
    }

    [HttpGet("employee/{employeeId}/year/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveEncashmentDto>>> GetEmployeeEncashments(int employeeId, int year)
    {
        try
        {
            var encashments = await _leaveManagementService.GetEmployeeEncashmentsAsync(employeeId, year);
            return Ok(encashments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting encashments for employee {EmployeeId}, year {Year}", 
                employeeId, year);
            return StatusCode(500, "An error occurred while retrieving leave encashments");
        }
    }

    [HttpPost]
    public async Task<ActionResult<LeaveEncashmentDto>> CreateEncashmentRequest([FromBody] CreateLeaveEncashmentDto requestDto)
    {
        try
        {
            var encashment = await _leaveManagementService.CreateEncashmentRequestAsync(requestDto);
            return CreatedAtAction(nameof(GetEmployeeEncashments), 
                new { employeeId = encashment.EmployeeId, year = encashment.Year }, encashment);
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
            _logger.LogError(ex, "Error creating encashment request");
            return StatusCode(500, "An error occurred while creating encashment request");
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<LeaveEncashmentDto>> ApproveEncashment(
        int id, 
        [FromBody] ApproveLeaveEncashmentDto approval,
        [FromQuery] int approverId)
    {
        try
        {
            var encashment = await _leaveManagementService.ApproveEncashmentAsync(id, approval, approverId);
            return Ok(encashment);
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
            _logger.LogError(ex, "Error approving encashment {EncashmentId}", id);
            return StatusCode(500, "An error occurred while approving encashment");
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<LeaveEncashmentDto>> RejectEncashment(
        int id, 
        [FromBody] string reason,
        [FromQuery] int approverId)
    {
        try
        {
            var encashment = await _leaveManagementService.RejectEncashmentAsync(id, reason, approverId);
            return Ok(encashment);
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
            _logger.LogError(ex, "Error rejecting encashment {EncashmentId}", id);
            return StatusCode(500, "An error occurred while rejecting encashment");
        }
    }

    [HttpGet("pending/branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<LeaveEncashmentDto>>> GetPendingEncashments(int branchId)
    {
        try
        {
            var encashments = await _leaveManagementService.GetPendingEncashmentsAsync(branchId);
            return Ok(encashments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending encashments for branch {BranchId}", branchId);
            return StatusCode(500, "An error occurred while retrieving pending encashments");
        }
    }

    [HttpGet("calculate-amount")]
    public async Task<ActionResult<decimal>> CalculateEncashmentAmount(
        [FromQuery] int employeeId,
        [FromQuery] int leavePolicyId,
        [FromQuery] decimal days)
    {
        try
        {
            var amount = await _leaveManagementService.CalculateEncashmentAmountAsync(employeeId, leavePolicyId, days);
            return Ok(amount);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating encashment amount for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while calculating encashment amount");
        }
    }
}