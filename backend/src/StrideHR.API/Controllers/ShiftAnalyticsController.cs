using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftAnalyticsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftAnalyticsController> _logger;

    public ShiftAnalyticsController(IShiftService shiftService, ILogger<ShiftAnalyticsController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    [HttpGet("basic/{branchId}")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<Dictionary<string, object>>> GetBasicShiftAnalytics(int branchId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _shiftService.GetShiftAnalyticsAsync(branchId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basic shift analytics for branch {BranchId}", branchId);
            return StatusCode(500, "An error occurred while retrieving shift analytics");
        }
    }

    [HttpPost("detailed")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ShiftAnalyticsDto>> GetDetailedShiftAnalytics([FromBody] ShiftAnalyticsSearchCriteria criteria)
    {
        try
        {
            var result = await _shiftService.GetDetailedShiftAnalyticsAsync(criteria);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detailed shift analytics");
            return StatusCode(500, "An error occurred while retrieving detailed shift analytics");
        }
    }
}