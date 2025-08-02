using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Leave;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[Authorize]
public class LeaveController : BaseController
{
    private readonly ILeaveManagementService _leaveManagementService;
    private readonly ILogger<LeaveController> _logger;

    public LeaveController(
        ILeaveManagementService leaveManagementService,
        ILogger<LeaveController> logger)
    {
        _leaveManagementService = leaveManagementService;
        _logger = logger;
    }

    #region Leave Requests

    [HttpPost("requests")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.CreateLeaveRequestAsync(employeeId, request);
            return Success(result, "Leave request created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            return Error("An error occurred while creating the leave request");
        }
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetLeaveRequest(int id)
    {
        try
        {
            var result = await _leaveManagementService.GetLeaveRequestAsync(id);
            return Success(result);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave request {Id}", id);
            return Error("An error occurred while retrieving the leave request");
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetMyLeaveRequests()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.GetEmployeeLeaveRequestsAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave requests");
            return Error("An error occurred while retrieving leave requests");
        }
    }

    [HttpGet("requests/employee/{employeeId}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetEmployeeLeaveRequests(int employeeId)
    {
        try
        {
            var result = await _leaveManagementService.GetEmployeeLeaveRequestsAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave requests for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving leave requests");
        }
    }

    [HttpGet("requests/pending")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var branchId = GetCurrentBranchId();
            var result = await _leaveManagementService.GetPendingRequestsAsync(branchId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending leave requests");
            return Error("An error occurred while retrieving pending leave requests");
        }
    }

    [HttpGet("requests/for-approval")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetRequestsForApproval()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.GetRequestsForApprovalAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving requests for approval");
            return Error("An error occurred while retrieving requests for approval");
        }
    }

    [HttpPut("requests/{id}")]
    public async Task<IActionResult> UpdateLeaveRequest(int id, [FromBody] CreateLeaveRequestDto request)
    {
        try
        {
            var result = await _leaveManagementService.UpdateLeaveRequestAsync(id, request);
            return Success(result, "Leave request updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave request {Id}", id);
            return Error("An error occurred while updating the leave request");
        }
    }

    [HttpDelete("requests/{id}")]
    public async Task<IActionResult> CancelLeaveRequest(int id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.CancelLeaveRequestAsync(id, employeeId);
            
            if (result)
                return Success("Leave request cancelled successfully");
            else
                return Error("Leave request not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling leave request {Id}", id);
            return Error("An error occurred while cancelling the leave request");
        }
    }

    #endregion

    #region Leave Approvals

    [HttpPost("requests/{id}/approve")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] LeaveApprovalDto approval)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.ApproveLeaveRequestAsync(id, approval, approverId);
            return Success(result, "Leave request approved successfully");
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
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving leave request {Id}", id);
            return Error("An error occurred while approving the leave request");
        }
    }

    [HttpPost("requests/{id}/reject")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> RejectLeaveRequest(int id, [FromBody] LeaveApprovalDto rejection)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.RejectLeaveRequestAsync(id, rejection, approverId);
            return Success(result, "Leave request rejected successfully");
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
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting leave request {Id}", id);
            return Error("An error occurred while rejecting the leave request");
        }
    }

    [HttpPost("requests/{id}/escalate")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> EscalateLeaveRequest(int id, [FromBody] EscalateLeaveRequestDto escalation)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.EscalateLeaveRequestAsync(
                id, escalation.EscalateToId, approverId, escalation.Comments);
            return Success(result, "Leave request escalated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating leave request {Id}", id);
            return Error("An error occurred while escalating the leave request");
        }
    }

    #endregion

    #region Leave Balances

    [HttpGet("balances")]
    public async Task<IActionResult> GetMyLeaveBalances()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.GetEmployeeLeaveBalancesAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave balances");
            return Error("An error occurred while retrieving leave balances");
        }
    }

    [HttpGet("balances/employee/{employeeId}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetEmployeeLeaveBalances(int employeeId)
    {
        try
        {
            var result = await _leaveManagementService.GetEmployeeLeaveBalancesAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave balances for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving leave balances");
        }
    }

    [HttpGet("balances/{employeeId}/{policyId}/{year}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetLeaveBalance(int employeeId, int policyId, int year)
    {
        try
        {
            var result = await _leaveManagementService.GetLeaveBalanceAsync(employeeId, policyId, year);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave balance");
            return Error("An error occurred while retrieving leave balance");
        }
    }

    #endregion

    #region Leave Policies

    [HttpGet("policies")]
    public async Task<IActionResult> GetLeavePolicies()
    {
        try
        {
            var branchId = GetCurrentBranchId();
            var result = await _leaveManagementService.GetBranchLeavePoliciesAsync(branchId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave policies");
            return Error("An error occurred while retrieving leave policies");
        }
    }

    [HttpGet("policies/{id}")]
    public async Task<IActionResult> GetLeavePolicy(int id)
    {
        try
        {
            var result = await _leaveManagementService.GetLeavePolicyAsync(id);
            return Success(result);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave policy {Id}", id);
            return Error("An error occurred while retrieving leave policy");
        }
    }

    #endregion

    #region Leave Calendar

    [HttpGet("calendar")]
    public async Task<IActionResult> GetLeaveCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var branchId = GetCurrentBranchId();
            var result = await _leaveManagementService.GetLeaveCalendarAsync(startDate, endDate, branchId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave calendar");
            return Error("An error occurred while retrieving leave calendar");
        }
    }

    [HttpGet("calendar/employee/{employeeId}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> GetEmployeeLeaveCalendar(int employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _leaveManagementService.GetEmployeeLeaveCalendarAsync(employeeId, startDate, endDate);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee leave calendar");
            return Error("An error occurred while retrieving employee leave calendar");
        }
    }

    [HttpGet("calendar/team")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetTeamLeaveCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var managerId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.GetTeamLeaveCalendarAsync(managerId, startDate, endDate);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team leave calendar");
            return Error("An error occurred while retrieving team leave calendar");
        }
    }

    #endregion

    #region Conflict Detection

    [HttpGet("conflicts")]
    public async Task<IActionResult> DetectLeaveConflicts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.DetectLeaveConflictsAsync(employeeId, startDate, endDate);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting leave conflicts");
            return Error("An error occurred while detecting leave conflicts");
        }
    }

    [HttpGet("conflicts/team")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetTeamLeaveConflicts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var managerId = GetCurrentEmployeeId();
            var result = await _leaveManagementService.GetTeamLeaveConflictsAsync(managerId, startDate, endDate);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team leave conflicts");
            return Error("An error occurred while retrieving team leave conflicts");
        }
    }

    #endregion

    #region Utility Methods

    [HttpGet("calculate-days")]
    public async Task<IActionResult> CalculateLeaveDays([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var branchId = GetCurrentBranchId();
            var result = await _leaveManagementService.CalculateLeaveDaysAsync(startDate, endDate, branchId);
            return Success(new { Days = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating leave days");
            return Error("An error occurred while calculating leave days");
        }
    }

    #endregion

    #region Private Helper Methods

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(employeeIdClaim, out int employeeId))
            return employeeId;
        
        throw new UnauthorizedAccessException("Employee ID not found in token");
    }

    private int GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        if (int.TryParse(branchIdClaim, out int branchId))
            return branchId;
        
        throw new UnauthorizedAccessException("Branch ID not found in token");
    }

    #endregion
}

// Additional DTO for escalation
public class EscalateLeaveRequestDto
{
    public int EscalateToId { get; set; }
    public string? Comments { get; set; }
}