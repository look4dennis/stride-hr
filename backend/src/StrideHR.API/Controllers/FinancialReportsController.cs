using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Financial;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialReportsController : ControllerBase
{
    private readonly IFinancialReportingService _financialReportingService;
    private readonly ILogger<FinancialReportsController> _logger;

    public FinancialReportsController(
        IFinancialReportingService financialReportingService,
        ILogger<FinancialReportsController> logger)
    {
        _financialReportingService = financialReportingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a comprehensive financial summary report
    /// </summary>
    [HttpPost("summary")]
    public async Task<ActionResult<FinancialSummaryReport>> GenerateFinancialSummary([FromBody] FinancialReportRequest request)
    {
        try
        {
            var report = await _financialReportingService.GenerateFinancialSummaryAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial summary report");
            return StatusCode(500, "An error occurred while generating the financial summary report");
        }
    }

    /// <summary>
    /// Generate payroll cost analysis report
    /// </summary>
    [HttpPost("payroll-cost-analysis")]
    public async Task<ActionResult<PayrollCostAnalysisReport>> GeneratePayrollCostAnalysis([FromBody] PayrollCostAnalysisRequest request)
    {
        try
        {
            var report = await _financialReportingService.GeneratePayrollCostAnalysisAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payroll cost analysis report");
            return StatusCode(500, "An error occurred while generating the payroll cost analysis report");
        }
    }

    /// <summary>
    /// Generate budget variance report
    /// </summary>
    [HttpPost("budget-variance")]
    public async Task<ActionResult<BudgetVarianceReport>> GenerateBudgetVariance([FromBody] BudgetVarianceRequest request)
    {
        try
        {
            var report = await _financialReportingService.GenerateBudgetVarianceReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating budget variance report");
            return StatusCode(500, "An error occurred while generating the budget variance report");
        }
    }

    /// <summary>
    /// Generate currency conversion report
    /// </summary>
    [HttpPost("currency-conversion")]
    public async Task<ActionResult<CurrencyConversionReport>> GenerateCurrencyConversion([FromBody] CurrencyConversionRequest request)
    {
        try
        {
            var report = await _financialReportingService.GenerateCurrencyConversionReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating currency conversion report");
            return StatusCode(500, "An error occurred while generating the currency conversion report");
        }
    }

    /// <summary>
    /// Generate department-wise financial report
    /// </summary>
    [HttpPost("department-wise")]
    public async Task<ActionResult<DepartmentWiseFinancialReport>> GenerateDepartmentWiseReport([FromBody] DepartmentFinancialRequest request)
    {
        try
        {
            var report = await _financialReportingService.GenerateDepartmentWiseReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating department-wise financial report");
            return StatusCode(500, "An error occurred while generating the department-wise financial report");
        }
    }

    /// <summary>
    /// Generate monthly financial trend report
    /// </summary>
    [HttpPost("monthly-trend")]
    public async Task<ActionResult<MonthlyFinancialTrendReport>> GenerateMonthlyTrend([FromBody] MonthlyTrendRequest request)
    {
        try
        {
            var report = await _financialReportingService.GenerateMonthlyTrendReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly trend report");
            return StatusCode(500, "An error occurred while generating the monthly trend report");
        }
    }

    /// <summary>
    /// Get financial metrics for a specific branch and period
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<List<FinancialMetric>>> GetFinancialMetrics(
        [FromQuery] int branchId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var metrics = await _financialReportingService.GetFinancialMetricsAsync(branchId, startDate, endDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial metrics");
            return StatusCode(500, "An error occurred while retrieving financial metrics");
        }
    }

    /// <summary>
    /// Get exchange rate history for currency analysis
    /// </summary>
    [HttpGet("exchange-rate-history")]
    public async Task<ActionResult<List<CurrencyExchangeRate>>> GetExchangeRateHistory(
        [FromQuery] string baseCurrency,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var history = await _financialReportingService.GetExchangeRateHistoryAsync(baseCurrency, startDate, endDate);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate history");
            return StatusCode(500, "An error occurred while retrieving exchange rate history");
        }
    }
}