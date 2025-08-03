using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Calendar;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for managing calendar integrations (Google Calendar, Outlook)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarIntegrationController : BaseController
{
    private readonly ICalendarIntegrationService _calendarService;

    public CalendarIntegrationController(ICalendarIntegrationService calendarService)
    {
        _calendarService = calendarService;
    }

    /// <summary>
    /// Connect Google Calendar for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="authorizationCode">Google OAuth authorization code</param>
    /// <returns>Integration result</returns>
    [HttpPost("google/connect")]
    [Authorize(Policy = "Permission:Calendar.Connect")]
    public async Task<IActionResult> ConnectGoogleCalendar([FromQuery] int employeeId, [FromBody] string authorizationCode)
    {
        try
        {
            var result = await _calendarService.ConnectGoogleCalendarAsync(employeeId, authorizationCode);
            if (result.Success)
                return Ok(new { success = true, data = result });
            else
                return BadRequest(new { success = false, message = result.Message, errorCode = result.ErrorCode });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Disconnect Google Calendar for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Integration result</returns>
    [HttpPost("google/disconnect")]
    [Authorize(Policy = "Permission:Calendar.Disconnect")]
    public async Task<IActionResult> DisconnectGoogleCalendar([FromQuery] int employeeId)
    {
        try
        {
            var result = await _calendarService.DisconnectGoogleCalendarAsync(employeeId);
            if (result.Success)
                return Ok(new { success = true, message = result.Message });
            else
                return BadRequest(new { success = false, message = result.Message, errorCode = result.ErrorCode });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get Google Calendar events for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of calendar events</returns>
    [HttpGet("google/events")]
    [Authorize(Policy = "Permission:Calendar.View")]
    public async Task<IActionResult> GetGoogleCalendarEvents([FromQuery] int employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var events = await _calendarService.GetGoogleCalendarEventsAsync(employeeId, startDate, endDate);
            return Ok(new { success = true, data = events });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Create Google Calendar event
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="eventDto">Event details</param>
    /// <returns>Created calendar event</returns>
    [HttpPost("google/events")]
    [Authorize(Policy = "Permission:Calendar.Create")]
    public async Task<IActionResult> CreateGoogleCalendarEvent([FromQuery] int employeeId, [FromBody] CreateCalendarEventDto eventDto)
    {
        try
        {
            var calendarEvent = await _calendarService.CreateGoogleCalendarEventAsync(employeeId, eventDto);
            return Ok(new { success = true, data = calendarEvent });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update Google Calendar event
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="eventDto">Updated event details</param>
    /// <returns>Updated calendar event</returns>
    [HttpPut("google/events/{eventId}")]
    [Authorize(Policy = "Permission:Calendar.Update")]
    public async Task<IActionResult> UpdateGoogleCalendarEvent([FromQuery] int employeeId, string eventId, [FromBody] UpdateCalendarEventDto eventDto)
    {
        try
        {
            var calendarEvent = await _calendarService.UpdateGoogleCalendarEventAsync(employeeId, eventId, eventDto);
            return Ok(new { success = true, data = calendarEvent });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete Google Calendar event
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="eventId">Event ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("google/events/{eventId}")]
    [Authorize(Policy = "Permission:Calendar.Delete")]
    public async Task<IActionResult> DeleteGoogleCalendarEvent([FromQuery] int employeeId, string eventId)
    {
        try
        {
            var result = await _calendarService.DeleteGoogleCalendarEventAsync(employeeId, eventId);
            if (result)
                return Ok(new { success = true, message = "Event deleted successfully" });
            else
                return BadRequest(new { success = false, message = "Failed to delete event" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Connect Outlook Calendar for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="authorizationCode">Microsoft OAuth authorization code</param>
    /// <returns>Integration result</returns>
    [HttpPost("outlook/connect")]
    [Authorize(Policy = "Permission:Calendar.Connect")]
    public async Task<IActionResult> ConnectOutlookCalendar([FromQuery] int employeeId, [FromBody] string authorizationCode)
    {
        try
        {
            var result = await _calendarService.ConnectOutlookCalendarAsync(employeeId, authorizationCode);
            if (result.Success)
                return Ok(new { success = true, data = result });
            else
                return BadRequest(new { success = false, message = result.Message, errorCode = result.ErrorCode });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Outlook Calendar integration is not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Disconnect Outlook Calendar for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Integration result</returns>
    [HttpPost("outlook/disconnect")]
    [Authorize(Policy = "Permission:Calendar.Disconnect")]
    public async Task<IActionResult> DisconnectOutlookCalendar([FromQuery] int employeeId)
    {
        try
        {
            var result = await _calendarService.DisconnectOutlookCalendarAsync(employeeId);
            if (result.Success)
                return Ok(new { success = true, message = result.Message });
            else
                return BadRequest(new { success = false, message = result.Message, errorCode = result.ErrorCode });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Outlook Calendar integration is not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get employee calendar integrations
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>List of calendar integrations</returns>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Policy = "Permission:Calendar.View")]
    public async Task<IActionResult> GetEmployeeCalendarIntegrations(int employeeId)
    {
        try
        {
            var integrations = await _calendarService.GetEmployeeCalendarIntegrationsAsync(employeeId);
            return Ok(new { success = true, data = integrations });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get all calendar events for an employee from all connected calendars
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of calendar events from all providers</returns>
    [HttpGet("employee/{employeeId}/events")]
    [Authorize(Policy = "Permission:Calendar.View")]
    public async Task<IActionResult> GetAllCalendarEvents(int employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var events = await _calendarService.GetAllCalendarEventsAsync(employeeId, startDate, endDate);
            return Ok(new { success = true, data = events });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Sync calendar events for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="provider">Calendar provider</param>
    /// <returns>Sync result</returns>
    [HttpPost("employee/{employeeId}/sync")]
    [Authorize(Policy = "Permission:Calendar.Sync")]
    public async Task<IActionResult> SyncCalendarEvents(int employeeId, [FromQuery] CalendarProvider provider)
    {
        try
        {
            var result = await _calendarService.SyncCalendarEventsAsync(employeeId, provider);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Validate calendar connection for an employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="provider">Calendar provider</param>
    /// <returns>Validation result</returns>
    [HttpPost("employee/{employeeId}/validate")]
    [Authorize(Policy = "Permission:Calendar.Validate")]
    public async Task<IActionResult> ValidateCalendarConnection(int employeeId, [FromQuery] CalendarProvider provider)
    {
        try
        {
            var isValid = await _calendarService.ValidateCalendarConnectionAsync(employeeId, provider);
            return Ok(new { success = true, valid = isValid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Create leave event in calendar
    /// </summary>
    /// <param name="leaveRequestId">Leave request ID</param>
    /// <returns>Created calendar event</returns>
    [HttpPost("leave/{leaveRequestId}/event")]
    [Authorize(Policy = "Permission:Calendar.Create")]
    public async Task<IActionResult> CreateLeaveEvent(int leaveRequestId)
    {
        try
        {
            var calendarEvent = await _calendarService.CreateLeaveEventAsync(leaveRequestId);
            return Ok(new { success = true, data = calendarEvent });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Leave event creation is not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Create meeting event in calendar
    /// </summary>
    /// <param name="meetingId">Meeting ID</param>
    /// <returns>Created calendar event</returns>
    [HttpPost("meeting/{meetingId}/event")]
    [Authorize(Policy = "Permission:Calendar.Create")]
    public async Task<IActionResult> CreateMeetingEvent(int meetingId)
    {
        try
        {
            var calendarEvent = await _calendarService.CreateMeetingEventAsync(meetingId);
            return Ok(new { success = true, data = calendarEvent });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Meeting event creation is not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update leave event status in calendar
    /// </summary>
    /// <param name="leaveRequestId">Leave request ID</param>
    /// <param name="status">New status</param>
    /// <returns>Success status</returns>
    [HttpPut("leave/{leaveRequestId}/event")]
    [Authorize(Policy = "Permission:Calendar.Update")]
    public async Task<IActionResult> UpdateLeaveEvent(int leaveRequestId, [FromBody] string status)
    {
        try
        {
            var result = await _calendarService.UpdateLeaveEventAsync(leaveRequestId, status);
            if (result)
                return Ok(new { success = true, message = "Leave event updated successfully" });
            else
                return BadRequest(new { success = false, message = "Failed to update leave event" });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Leave event updates are not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete leave event from calendar
    /// </summary>
    /// <param name="leaveRequestId">Leave request ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("leave/{leaveRequestId}/event")]
    [Authorize(Policy = "Permission:Calendar.Delete")]
    public async Task<IActionResult> DeleteLeaveEvent(int leaveRequestId)
    {
        try
        {
            var result = await _calendarService.DeleteLeaveEventAsync(leaveRequestId);
            if (result)
                return Ok(new { success = true, message = "Leave event deleted successfully" });
            else
                return BadRequest(new { success = false, message = "Failed to delete leave event" });
        }
        catch (NotImplementedException)
        {
            return BadRequest(new { success = false, message = "Leave event deletion is not yet implemented" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}