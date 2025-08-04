using StrideHR.Core.Models.Financial;

namespace StrideHR.Core.Interfaces.Services;

public interface IFinancialReportingService
{
    Task<FinancialSummaryReport> GenerateFinancialSummaryAsync(FinancialReportRequest request);
    Task<PayrollCostAnalysisReport> GeneratePayrollCostAnalysisAsync(PayrollCostAnalysisRequest request);
    Task<BudgetVarianceReport> GenerateBudgetVarianceReportAsync(BudgetVarianceRequest request);
    Task<CurrencyConversionReport> GenerateCurrencyConversionReportAsync(CurrencyConversionRequest request);
    Task<DepartmentWiseFinancialReport> GenerateDepartmentWiseReportAsync(DepartmentFinancialRequest request);
    Task<MonthlyFinancialTrendReport> GenerateMonthlyTrendReportAsync(MonthlyTrendRequest request);
    Task<List<FinancialMetric>> GetFinancialMetricsAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<List<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(string baseCurrency, DateTime startDate, DateTime endDate);
}