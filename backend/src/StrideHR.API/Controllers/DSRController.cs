using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DSR;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[Authorize]
public class DSRController : BaseController
{
    private readonly IDSRService _dsrService;

    public DSRController(IDSRService dsrService)
    {
        _dsrService = dsrService;
    }

    /// <summary>
    /// Create a new DSR
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDSR([FromBody] CreateDSRRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (request.EmployeeId != currentUserId && !await IsManagerOrHR())
            {
                return Forbid("You can only create DSRs for yourself");
            }

            var dsr = await _dsrService.CreateDSRAsync(request);
            return Success(dsr, "DSR created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing DSR
    /// </summary>
    [HttpPut("{dsrId}")]
    public async Task<IActionResult> UpdateDSR(int dsrId, [FromBody] UpdateDSRRequest request)
    {
        try
        {
            var dsr = await _dsrService.UpdateDSRAsync(dsrId, request);
            return Success(dsr, "DSR updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Submit a DSR for review
    /// </summary>
    [HttpPost("{dsrId}/submit")]
    public async Task<IActionResult> SubmitDSR(int dsrId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var dsr = await _dsrService.SubmitDSRAsync(dsrId, currentUserId);
            return Success(dsr, "DSR submitted successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Delete a DSR
    /// </summary>
    [HttpDelete("{dsrId}")]
    public async Task<IActionResult> DeleteDSR(int dsrId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _dsrService.DeleteDSRAsync(dsrId, currentUserId);
            
            if (result)
                return Success("DSR deleted successfully");
            else
                return Error("DSR not found or cannot be deleted");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get DSR by ID
    /// </summary>
    [HttpGet("{dsrId}")]
    public async Task<IActionResult> GetDSR(int dsrId)
    {
        var dsr = await _dsrService.GetDSRByIdAsync(dsrId);
        if (dsr == null)
            return NotFound("DSR not found");

        return Success(dsr);
    }

    /// <summary>
    /// Get DSR by employee and date
    /// </summary>
    [HttpGet("employee/{employeeId}/date/{date}")]
    public async Task<IActionResult> GetDSRByEmployeeAndDate(int employeeId, DateTime date)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own DSRs");
        }

        var dsr = await _dsrService.GetDSRByEmployeeAndDateAsync(employeeId, date);
        if (dsr == null)
            return NotFound("DSR not found for the specified date");

        return Success(dsr);
    }

    /// <summary>
    /// Get DSRs by employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetDSRsByEmployee(int employeeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own DSRs");
        }

        var dsrs = await _dsrService.GetDSRsByEmployeeAsync(employeeId, startDate, endDate);
        return Success(dsrs);
    }

    /// <summary>
    /// Get DSRs by project
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetDSRsByProject(int projectId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var dsrs = await _dsrService.GetDSRsByProjectAsync(projectId, startDate, endDate);
        return Success(dsrs);
    }

    /// <summary>
    /// Review a DSR (Approve/Reject)
    /// </summary>
    [HttpPost("{dsrId}/review")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> ReviewDSR(int dsrId, [FromBody] ReviewDSRRequest request)
    {
        try
        {
            request.ReviewerId = GetCurrentUserId();
            var dsr = await _dsrService.ReviewDSRAsync(dsrId, request);
            return Success(dsr, "DSR reviewed successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get pending DSRs for review
    /// </summary>
    [HttpGet("pending-review")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> GetPendingDSRsForReview()
    {
        var currentUserId = GetCurrentUserId();
        var dsrs = await _dsrService.GetPendingDSRsForReviewAsync(currentUserId);
        return Success(dsrs);
    }

    /// <summary>
    /// Get DSRs by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> GetDSRsByStatus(DSRStatus status)
    {
        var dsrs = await _dsrService.GetDSRsByStatusAsync(status);
        return Success(dsrs);
    }

    /// <summary>
    /// Get employee productivity report
    /// </summary>
    [HttpGet("productivity/employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeProductivity(int employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own productivity report");
        }

        try
        {
            var report = await _dsrService.GetEmployeeProductivityAsync(employeeId, startDate, endDate);
            return Success(report);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get idle employees for a specific date
    /// </summary>
    [HttpGet("idle-employees")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> GetIdleEmployees([FromQuery] DateTime date)
    {
        var idleEmployees = await _dsrService.GetIdleEmployeesAsync(date);
        return Success(idleEmployees);
    }

    /// <summary>
    /// Get team productivity summary
    /// </summary>
    [HttpGet("productivity/team")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> GetTeamProductivity([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var summary = await _dsrService.GetTeamProductivityAsync(currentUserId, startDate, endDate);
            return Success(summary);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get project hours report
    /// </summary>
    [HttpGet("project/{projectId}/hours-report")]
    public async Task<IActionResult> GetProjectHoursReport(int projectId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _dsrService.GetProjectHoursReportAsync(projectId, startDate, endDate);
            return Success(report);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get project hours vs estimates comparison
    /// </summary>
    [HttpGet("project-hours-comparison")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> GetProjectHoursVsEstimates()
    {
        var currentUserId = GetCurrentUserId();
        var comparisons = await _dsrService.GetProjectHoursVsEstimatesAsync(currentUserId);
        return Success(comparisons);
    }

    /// <summary>
    /// Get total hours by project
    /// </summary>
    [HttpGet("project/{projectId}/total-hours")]
    public async Task<IActionResult> GetTotalHoursByProject(int projectId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var totalHours = await _dsrService.GetTotalHoursByProjectAsync(projectId, startDate, endDate);
        return Success(totalHours);
    }

    /// <summary>
    /// Get total hours by employee
    /// </summary>
    [HttpGet("employee/{employeeId}/total-hours")]
    public async Task<IActionResult> GetTotalHoursByEmployee(int employeeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own hours");
        }

        var totalHours = await _dsrService.GetTotalHoursByEmployeeAsync(employeeId, startDate, endDate);
        return Success(totalHours);
    }

    /// <summary>
    /// Check if employee can submit DSR for a date
    /// </summary>
    [HttpGet("can-submit/{employeeId}/{date}")]
    public async Task<IActionResult> CanEmployeeSubmitDSR(int employeeId, DateTime date)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only check your own DSR submission status");
        }

        var canSubmit = await _dsrService.CanEmployeeSubmitDSRAsync(employeeId, date);
        return Success(canSubmit);
    }

    /// <summary>
    /// Get assigned projects for an employee
    /// </summary>
    [HttpGet("employee/{employeeId}/assigned-projects")]
    public async Task<IActionResult> GetAssignedProjects(int employeeId)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own assigned projects");
        }

        var projects = await _dsrService.GetAssignedProjectsAsync(employeeId);
        return Success(projects);
    }

    /// <summary>
    /// Get assigned tasks for an employee
    /// </summary>
    [HttpGet("employee/{employeeId}/assigned-tasks")]
    public async Task<IActionResult> GetAssignedTasks(int employeeId, [FromQuery] int? projectId = null)
    {
        var currentUserId = GetCurrentUserId();
        if (employeeId != currentUserId && !await IsManagerOrHR())
        {
            return Forbid("You can only view your own assigned tasks");
        }

        var tasks = await _dsrService.GetAssignedTasksAsync(employeeId, projectId);
        return Success(tasks);
    }

    // Helper methods
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<bool> IsManagerOrHR()
    {
        // This is a simplified check - in a real application, you would check the user's roles
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        return roles.Any(r => r == "Manager" || r == "HR" || r == "Admin");
    }
}