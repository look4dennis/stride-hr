using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportSchedulesController : ControllerBase
{
    private readonly IReportSchedulingService _schedulingService;
    private readonly ILogger<ReportSchedulesController> _logger;

    public ReportSchedulesController(
        IReportSchedulingService schedulingService,
        ILogger<ReportSchedulesController> logger)
    {
        _schedulingService = schedulingService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserSchedules()
    {
        try
        {
            var userId = GetCurrentUserId();
            var schedules = await _schedulingService.GetUserSchedulesAsync(userId);

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schedules for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving schedules");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var schedule = await _schedulingService.GetScheduleAsync(id, userId);

            if (schedule == null)
                return NotFound("Schedule not found or access denied");

            return Ok(schedule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schedule {ScheduleId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving the schedule");
        }
    }

    [HttpGet("report/{reportId}")]
    public async Task<IActionResult> GetReportSchedules(int reportId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var schedules = await _schedulingService.GetReportSchedulesAsync(reportId, userId);

            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get schedules for report {ReportId} and user {UserId}", reportId, GetCurrentUserId());
            return StatusCode(500, "An error occurred while retrieving report schedules");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] ReportScheduleRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var schedule = await _schedulingService.CreateScheduleAsync(request, userId);

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to schedule this report");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schedule for user {UserId}", GetCurrentUserId());
            return StatusCode(500, "An error occurred while creating the schedule");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ReportScheduleRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var schedule = await _schedulingService.UpdateScheduleAsync(id, request, userId);

            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to update this schedule");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update schedule {ScheduleId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while updating the schedule");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _schedulingService.DeleteScheduleAsync(id, userId);

            if (!success)
                return NotFound("Schedule not found");

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("You don't have permission to delete this schedule");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete schedule {ScheduleId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while deleting the schedule");
        }
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateSchedule(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _schedulingService.ActivateScheduleAsync(id, userId);

            if (!success)
                return NotFound("Schedule not found or access denied");

            return Ok(new { Message = "Schedule activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate schedule {ScheduleId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while activating the schedule");
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateSchedule(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _schedulingService.DeactivateScheduleAsync(id, userId);

            if (!success)
                return NotFound("Schedule not found or access denied");

            return Ok(new { Message = "Schedule deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate schedule {ScheduleId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, "An error occurred while deactivating the schedule");
        }
    }

    [HttpPost("validate-cron")]
    public async Task<IActionResult> ValidateCronExpression([FromBody] ValidateCronRequest request)
    {
        try
        {
            var isValid = await _schedulingService.ValidateCronExpressionAsync(request.CronExpression);
            var nextRunTime = isValid ? await _schedulingService.GetNextRunTimeAsync(request.CronExpression) : null;

            return Ok(new
            {
                IsValid = isValid,
                NextRunTime = nextRunTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate cron expression: {CronExpression}", request.CronExpression);
            return StatusCode(500, "An error occurred while validating the cron expression");
        }
    }
}

public class ValidateCronRequest
{
    public string CronExpression { get; set; } = string.Empty;
}