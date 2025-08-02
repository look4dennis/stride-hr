using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Leave;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveBalanceController : ControllerBase
{
    private readonly ILeaveManagementService _leaveManagementService;
    private readonly ILogger<LeaveBalanceController> _logger;

    public LeaveBalanceController(
        ILeaveManagementService leaveManagementService,
        ILogger<LeaveBalanceController> logger)
    {
        _leaveManagementService = leaveManagementService;
        _logger = logger;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetEmployeeLeaveBalances(int employeeId)
    {
        try
        {
            var balances = await _leaveManagementService.GetEmployeeLeaveBalancesAsync(employeeId);
            return Ok(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balances for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving leave balances");
        }
    }

    [HttpGet("employee/{employeeId}/policy/{leavePolicyId}/year/{year}")]
    public async Task<ActionResult<LeaveBalanceDto>> GetLeaveBalance(int employeeId, int leavePolicyId, int year)
    {
        try
        {
            var balance = await _leaveManagementService.GetLeaveBalanceAsync(employeeId, leavePolicyId, year);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balance for employee {EmployeeId}, policy {PolicyId}, year {Year}", 
                employeeId, leavePolicyId, year);
            return StatusCode(500, "An error occurred while retrieving leave balance");
        }
    }

    [HttpPost("employee/{employeeId}/recalculate/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> RecalculateLeaveBalances(int employeeId, int year)
    {
        try
        {
            var balances = await _leaveManagementService.RecalculateLeaveBalancesAsync(employeeId, year);
            return Ok(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating leave balances for employee {EmployeeId}, year {Year}", 
                employeeId, year);
            return StatusCode(500, "An error occurred while recalculating leave balances");
        }
    }

    [HttpGet("employee/{employeeId}/history/{year}")]
    public async Task<ActionResult<LeaveHistoryDto>> GetEmployeeLeaveHistory(int employeeId, int year)
    {
        try
        {
            var history = await _leaveManagementService.GetEmployeeLeaveHistoryAsync(employeeId, year);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave history for employee {EmployeeId}, year {Year}", 
                employeeId, year);
            return StatusCode(500, "An error occurred while retrieving leave history");
        }
    }

    [HttpGet("employee/{employeeId}/transactions/{leavePolicyId}/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveTransactionDto>>> GetLeaveTransactions(
        int employeeId, int leavePolicyId, int year)
    {
        try
        {
            var transactions = await _leaveManagementService.GetLeaveTransactionsAsync(employeeId, leavePolicyId, year);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave transactions for employee {EmployeeId}, policy {PolicyId}, year {Year}", 
                employeeId, leavePolicyId, year);
            return StatusCode(500, "An error occurred while retrieving leave transactions");
        }
    }

    [HttpGet("branch/{branchId}/history/{year}")]
    public async Task<ActionResult<IEnumerable<LeaveHistoryDto>>> GetBranchLeaveHistory(int branchId, int year)
    {
        try
        {
            var history = await _leaveManagementService.GetBranchLeaveHistoryAsync(branchId, year);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch leave history for branch {BranchId}, year {Year}", 
                branchId, year);
            return StatusCode(500, "An error occurred while retrieving branch leave history");
        }
    }

    [HttpGet("branch/{branchId}/analytics/{year}")]
    public async Task<ActionResult<LeaveAnalyticsDto>> GetLeaveAnalytics(int branchId, int year)
    {
        try
        {
            var analytics = await _leaveManagementService.GetLeaveAnalyticsAsync(branchId, year);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave analytics for branch {BranchId}, year {Year}", 
                branchId, year);
            return StatusCode(500, "An error occurred while retrieving leave analytics");
        }
    }
}