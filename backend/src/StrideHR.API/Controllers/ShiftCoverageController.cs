using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftCoverageController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftCoverageController> _logger;

    public ShiftCoverageController(IShiftService shiftService, ILogger<ShiftCoverageController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ShiftCoverageRequestDto>> CreateShiftCoverageRequest([FromBody] CreateShiftCoverageRequestDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.CreateShiftCoverageRequestAsync(userId, createDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift coverage request");
            return StatusCode(500, "An error occurred while creating the shift coverage request");
        }
    }

    [HttpPost("requests/{requestId}/responses")]
    public async Task<ActionResult<ShiftCoverageRequestDto>> RespondToShiftCoverageRequest(int requestId, [FromBody] CreateShiftCoverageResponseDto responseDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            responseDto.ShiftCoverageRequestId = requestId;
            var result = await _shiftService.RespondToShiftCoverageRequestAsync(userId, responseDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to shift coverage request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while responding to the shift coverage request");
        }
    }

    [HttpPost("requests/{requestId}/approve")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ShiftCoverageRequestDto>> ApproveShiftCoverageRequest(int requestId, [FromBody] ApproveShiftCoverageDto approvalDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.ApproveShiftCoverageRequestAsync(requestId, userId, approvalDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving shift coverage request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while approving the shift coverage request");
        }
    }

    [HttpDelete("requests/{requestId}")]
    public async Task<ActionResult> CancelShiftCoverageRequest(int requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.CancelShiftCoverageRequestAsync(requestId, userId);
            
            if (!result)
            {
                return NotFound("Shift coverage request not found");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling shift coverage request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while cancelling the shift coverage request");
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<ShiftCoverageRequestDto>>> GetShiftCoverageRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.GetShiftCoverageRequestsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift coverage requests");
            return StatusCode(500, "An error occurred while retrieving shift coverage requests");
        }
    }

    [HttpGet("requests/pending-approvals")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<IEnumerable<ShiftCoverageRequestDto>>> GetPendingShiftCoverageApprovals()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.GetPendingShiftCoverageApprovalsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending shift coverage approvals");
            return StatusCode(500, "An error occurred while retrieving pending shift coverage approvals");
        }
    }

    [HttpPost("requests/search")]
    public async Task<ActionResult<object>> SearchShiftCoverageRequests([FromBody] ShiftCoverageSearchCriteria criteria)
    {
        try
        {
            var (requests, totalCount) = await _shiftService.SearchShiftCoverageRequestsAsync(criteria);
            return Ok(new { requests, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching shift coverage requests");
            return StatusCode(500, "An error occurred while searching shift coverage requests");
        }
    }

    [HttpPost("emergency-broadcast")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<List<ShiftCoverageRequestDto>>> BroadcastEmergencyShiftCoverage([FromBody] EmergencyShiftCoverageBroadcastDto broadcastDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.BroadcastEmergencyShiftCoverageAsync(userId, broadcastDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting emergency shift coverage");
            return StatusCode(500, "An error occurred while broadcasting emergency shift coverage");
        }
    }

    [HttpGet("emergency-requests/{branchId}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<IEnumerable<ShiftCoverageRequestDto>>> GetEmergencyShiftCoverageRequests(int branchId)
    {
        try
        {
            var result = await _shiftService.GetEmergencyShiftCoverageRequestsAsync(branchId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency shift coverage requests for branch {BranchId}", branchId);
            return StatusCode(500, "An error occurred while retrieving emergency shift coverage requests");
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Invalid user ID");
    }
}