using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;
using StrideHR.Core.Enums;
using StrideHR.Core.Entities;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseAnalyticsController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpenseAnalyticsController> _logger;

    public ExpenseAnalyticsController(IExpenseService expenseService, ILogger<ExpenseAnalyticsController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive expense analytics for an organization
    /// </summary>
    [HttpGet("organization/{organizationId}")]
    public async Task<ActionResult<ExpenseAnalyticsDto>> GetExpenseAnalytics(
        int organizationId,
        [FromQuery] ExpenseAnalyticsPeriod period = ExpenseAnalyticsPeriod.Monthly,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _expenseService.GetExpenseAnalyticsAsync(organizationId, period, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense analytics for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving expense analytics");
        }
    }

    /// <summary>
    /// Get expense analytics by category
    /// </summary>
    [HttpGet("organization/{organizationId}/categories")]
    public async Task<ActionResult<IEnumerable<ExpenseCategoryAnalyticsDto>>> GetCategoryAnalytics(
        int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _expenseService.GetCategoryAnalyticsAsync(organizationId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category analytics for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving category analytics");
        }
    }

    /// <summary>
    /// Get expense analytics by employee
    /// </summary>
    [HttpGet("organization/{organizationId}/employees")]
    public async Task<ActionResult<IEnumerable<EmployeeExpenseAnalyticsDto>>> GetEmployeeAnalytics(
        int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _expenseService.GetEmployeeAnalyticsAsync(organizationId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee analytics for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving employee analytics");
        }
    }

    /// <summary>
    /// Get monthly expense trends
    /// </summary>
    [HttpGet("organization/{organizationId}/trends")]
    public async Task<ActionResult<IEnumerable<MonthlyExpenseTrendDto>>> GetMonthlyTrends(
        int organizationId,
        [FromQuery] int months = 12)
    {
        try
        {
            var trends = await _expenseService.GetMonthlyTrendsAsync(organizationId, months);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly trends for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving monthly trends");
        }
    }

    /// <summary>
    /// Get travel expense analytics
    /// </summary>
    [HttpGet("organization/{organizationId}/travel")]
    public async Task<ActionResult<IEnumerable<TravelExpenseAnalyticsDto>>> GetTravelAnalytics(
        int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var analytics = await _expenseService.GetTravelAnalyticsAsync(organizationId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving travel analytics for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving travel analytics");
        }
    }

    /// <summary>
    /// Get budget tracking information
    /// </summary>
    [HttpGet("organization/{organizationId}/budget")]
    public async Task<ActionResult<ExpenseBudgetTrackingDto>> GetBudgetTracking(
        int organizationId,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? employeeId = null)
    {
        try
        {
            var budgetTracking = await _expenseService.GetBudgetTrackingAsync(organizationId, departmentId, employeeId);
            return Ok(budgetTracking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget tracking for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving budget tracking");
        }
    }

    /// <summary>
    /// Get budget tracking by period
    /// </summary>
    [HttpGet("organization/{organizationId}/budget/period")]
    public async Task<ActionResult<IEnumerable<ExpenseBudgetTrackingDto>>> GetBudgetTrackingByPeriod(
        int organizationId,
        [FromQuery] ExpenseAnalyticsPeriod period)
    {
        try
        {
            var budgetTracking = await _expenseService.GetBudgetTrackingByPeriodAsync(organizationId, period);
            return Ok(budgetTracking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget tracking by period for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving budget tracking");
        }
    }

    /// <summary>
    /// Check budget compliance for an expense claim
    /// </summary>
    [HttpGet("claims/{expenseClaimId}/budget-compliance")]
    public async Task<ActionResult<bool>> CheckBudgetCompliance(int expenseClaimId)
    {
        try
        {
            var isCompliant = await _expenseService.CheckBudgetComplianceAsync(expenseClaimId);
            return Ok(isCompliant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking budget compliance for expense claim {ExpenseClaimId}", expenseClaimId);
            return StatusCode(500, "An error occurred while checking budget compliance");
        }
    }

    /// <summary>
    /// Get budget alerts for an organization
    /// </summary>
    [HttpGet("organization/{organizationId}/budget/alerts")]
    public async Task<ActionResult<IEnumerable<ExpenseBudgetAlert>>> GetBudgetAlerts(
        int organizationId,
        [FromQuery] bool unresolved = true)
    {
        try
        {
            var alerts = await _expenseService.GetBudgetAlertsAsync(organizationId, unresolved);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget alerts for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving budget alerts");
        }
    }

    /// <summary>
    /// Get expense dashboard data
    /// </summary>
    [HttpGet("organization/{organizationId}/dashboard")]
    public async Task<ActionResult<Dictionary<string, object>>> GetExpenseDashboardData(
        int organizationId,
        [FromQuery] int? employeeId = null)
    {
        try
        {
            var dashboardData = await _expenseService.GetExpenseDashboardDataAsync(organizationId, employeeId);
            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense dashboard data for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while retrieving dashboard data");
        }
    }
}