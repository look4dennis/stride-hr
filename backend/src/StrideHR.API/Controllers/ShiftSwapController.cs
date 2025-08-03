using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftSwapController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftSwapController> _logger;

    public ShiftSwapController(IShiftService shiftService, ILogger<ShiftSwapController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    [HttpPost("requests")]
    public async Task<ActionResult<ShiftSwapRequestDto>> CreateShiftSwapRequest([FromBody] CreateShiftSwapRequestDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.CreateShiftSwapRequestAsync(userId, createDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift swap request");
            return StatusCode(500, "An error occurred while creating the shift swap request");
        }
    }

    [HttpPost("requests/{requestId}/responses")]
    public async Task<ActionResult<ShiftSwapRequestDto>> RespondToShiftSwapRequest(int requestId, [FromBody] CreateShiftSwapResponseDto responseDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            responseDto.ShiftSwapRequestId = requestId;
            var result = await _shiftService.RespondToShiftSwapRequestAsync(userId, responseDto);
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
            _logger.LogError(ex, "Error responding to shift swap request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while responding to the shift swap request");
        }
    }

    [HttpPost("requests/{requestId}/approve")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ShiftSwapRequestDto>> ApproveShiftSwapRequest(int requestId, [FromBody] ApproveShiftSwapDto approvalDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.ApproveShiftSwapRequestAsync(requestId, userId, approvalDto);
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
            _logger.LogError(ex, "Error approving shift swap request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while approving the shift swap request");
        }
    }

    [HttpDelete("requests/{requestId}")]
    public async Task<ActionResult> CancelShiftSwapRequest(int requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.CancelShiftSwapRequestAsync(requestId, userId);
            
            if (!result)
            {
                return NotFound("Shift swap request not found");
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
            _logger.LogError(ex, "Error cancelling shift swap request {RequestId}", requestId);
            return StatusCode(500, "An error occurred while cancelling the shift swap request");
        }
    }

    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<ShiftSwapRequestDto>>> GetShiftSwapRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.GetShiftSwapRequestsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift swap requests");
            return StatusCode(500, "An error occurred while retrieving shift swap requests");
        }
    }

    [HttpGet("requests/pending-approvals")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<IEnumerable<ShiftSwapRequestDto>>> GetPendingShiftSwapApprovals()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _shiftService.GetPendingShiftSwapApprovalsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending shift swap approvals");
            return StatusCode(500, "An error occurred while retrieving pending shift swap approvals");
        }
    }

    [HttpPost("requests/search")]
    public async Task<ActionResult<object>> SearchShiftSwapRequests([FromBody] ShiftSwapSearchCriteria criteria)
    {
        try
        {
            var (requests, totalCount) = await _shiftService.SearchShiftSwapRequestsAsync(criteria);
            return Ok(new { requests, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching shift swap requests");
            return StatusCode(500, "An error occurred while searching shift swap requests");
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