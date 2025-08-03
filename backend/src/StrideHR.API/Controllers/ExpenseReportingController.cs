using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseReportingController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpenseReportingController> _logger;

    public ExpenseReportingController(IExpenseService expenseService, ILogger<ExpenseReportingController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get advanced expense report with filtering
    /// </summary>
    [HttpPost("advanced")]
    public async Task<ActionResult<IEnumerable<ExpenseClaimDto>>> GetAdvancedExpenseReport([FromBody] ExpenseReportFilterDto filter)
    {
        try
        {
            var report = await _expenseService.GetAdvancedExpenseReportAsync(filter);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced expense report");
            return StatusCode(500, "An error occurred while generating the expense report");
        }
    }

    /// <summary>
    /// Export expense report in specified format
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportExpenseReport([FromBody] ExpenseReportFilterDto filter, [FromQuery] string format = "xlsx")
    {
        try
        {
            var exportData = await _expenseService.ExportExpenseReportAsync(filter, format);
            
            var contentType = format.ToLower() switch
            {
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            var fileName = $"expense_report_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
            
            return File(exportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting expense report in format {Format}", format);
            return StatusCode(500, "An error occurred while exporting the expense report");
        }
    }

    /// <summary>
    /// Get basic expense report (legacy endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseClaimDto>>> GetExpenseReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? employeeId = null,
        [FromQuery] string? status = null)
    {
        try
        {
            Core.Enums.ExpenseClaimStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Core.Enums.ExpenseClaimStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            var report = await _expenseService.GetExpenseReportAsync(startDate, endDate, employeeId, statusEnum);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating basic expense report");
            return StatusCode(500, "An error occurred while generating the expense report");
        }
    }

    /// <summary>
    /// Get expense report summary statistics
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetExpenseReportSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? employeeId = null)
    {
        try
        {
            var report = await _expenseService.GetExpenseReportAsync(startDate, endDate, employeeId);
            var reportList = report.ToList();

            var summary = new
            {
                TotalClaims = reportList.Count,
                TotalAmount = reportList.Sum(r => r.TotalAmount),
                AverageAmount = reportList.Any() ? reportList.Average(r => r.TotalAmount) : 0,
                ApprovedClaims = reportList.Count(r => r.Status == Core.Enums.ExpenseClaimStatus.Approved || r.Status == Core.Enums.ExpenseClaimStatus.Reimbursed),
                PendingClaims = reportList.Count(r => r.Status == Core.Enums.ExpenseClaimStatus.Submitted || r.Status == Core.Enums.ExpenseClaimStatus.UnderReview),
                RejectedClaims = reportList.Count(r => r.Status == Core.Enums.ExpenseClaimStatus.Rejected),
                ApprovedAmount = reportList.Where(r => r.Status == Core.Enums.ExpenseClaimStatus.Approved || r.Status == Core.Enums.ExpenseClaimStatus.Reimbursed).Sum(r => r.TotalAmount),
                PendingAmount = reportList.Where(r => r.Status == Core.Enums.ExpenseClaimStatus.Submitted || r.Status == Core.Enums.ExpenseClaimStatus.UnderReview).Sum(r => r.TotalAmount),
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expense report summary");
            return StatusCode(500, "An error occurred while generating the expense report summary");
        }
    }

    /// <summary>
    /// Get expense categories breakdown for reporting
    /// </summary>
    [HttpGet("categories-breakdown")]
    public async Task<ActionResult<object>> GetCategoriesBreakdown(
        [FromQuery] int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var categoryAnalytics = await _expenseService.GetCategoryAnalyticsAsync(organizationId, startDate, endDate);
            var categoryList = categoryAnalytics.ToList();

            var breakdown = new
            {
                Categories = categoryList.Select(ca => new
                {
                    ca.CategoryName,
                    ca.CategoryCode,
                    ca.TotalAmount,
                    ca.ClaimCount,
                    ca.AverageAmount,
                    ca.PercentageOfTotal
                }),
                TotalAmount = categoryList.Sum(ca => ca.TotalAmount),
                TotalClaims = categoryList.Sum(ca => ca.ClaimCount),
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating categories breakdown for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while generating the categories breakdown");
        }
    }

    /// <summary>
    /// Get employee expense breakdown for reporting
    /// </summary>
    [HttpGet("employees-breakdown")]
    public async Task<ActionResult<object>> GetEmployeesBreakdown(
        [FromQuery] int organizationId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var employeeAnalytics = await _expenseService.GetEmployeeAnalyticsAsync(organizationId, startDate, endDate);
            var employeeList = employeeAnalytics.ToList();

            var breakdown = new
            {
                Employees = employeeList.Select(ea => new
                {
                    ea.EmployeeName,
                    ea.Department,
                    ea.TotalExpenses,
                    ea.ClaimCount,
                    ea.AverageClaimAmount,
                    ea.TravelExpenses,
                    ea.MileageExpenses
                }),
                TotalAmount = employeeList.Sum(ea => ea.TotalExpenses),
                TotalClaims = employeeList.Sum(ea => ea.ClaimCount),
                AveragePerEmployee = employeeList.Any() ? employeeList.Average(ea => ea.TotalExpenses) : 0,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employees breakdown for organization {OrganizationId}", organizationId);
            return StatusCode(500, "An error occurred while generating the employees breakdown");
        }
    }
}