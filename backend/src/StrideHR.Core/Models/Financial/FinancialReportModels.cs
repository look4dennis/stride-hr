namespace StrideHR.Core.Models.Financial;

public class FinancialReportRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public bool IncludeCurrencyConversion { get; set; } = true;
    public List<string>? Departments { get; set; }
}

public class FinancialSummaryReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal TotalPayrollCost { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalOvertimeCost { get; set; }
    
    public int TotalEmployees { get; set; }
    public decimal AverageGrossSalary { get; set; }
    public decimal AverageNetSalary { get; set; }
    
    public List<BranchFinancialSummary> BranchSummaries { get; set; } = new();
    public List<DepartmentFinancialSummary> DepartmentSummaries { get; set; } = new();
    public List<MonthlyFinancialData> MonthlyBreakdown { get; set; } = new();
}

public class BranchFinancialSummary
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string LocalCurrency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    
    public decimal TotalPayrollCost { get; set; }
    public decimal TotalPayrollCostInBaseCurrency { get; set; }
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
    
    public List<CurrencyBreakdown> CurrencyBreakdown { get; set; } = new();
}

public class DepartmentFinancialSummary
{
    public string Department { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal PercentageOfTotalCost { get; set; }
}

public class MonthlyFinancialData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal GrowthPercentage { get; set; }
    public int EmployeeCount { get; set; }
}

public class CurrencyBreakdown
{
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AmountInBaseCurrency { get; set; }
    public decimal ExchangeRate { get; set; }
    public int EmployeeCount { get; set; }
}

public class PayrollCostAnalysisRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public List<string>? Departments { get; set; }
    public bool IncludeProjections { get; set; } = false;
}

public class PayrollCostAnalysisReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    public PayrollCostBreakdown CostBreakdown { get; set; } = new();
    public List<PayrollTrendData> TrendData { get; set; } = new();
    public List<CostCategoryAnalysis> CategoryAnalysis { get; set; } = new();
    public PayrollProjection? Projection { get; set; }
}

public class PayrollCostBreakdown
{
    public decimal BasicSalaryTotal { get; set; }
    public decimal AllowancesTotal { get; set; }
    public decimal OvertimeTotal { get; set; }
    public decimal DeductionsTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public decimal NetTotal { get; set; }
    
    public Dictionary<string, decimal> AllowanceBreakdown { get; set; } = new();
    public Dictionary<string, decimal> DeductionBreakdown { get; set; } = new();
}

public class PayrollTrendData
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal PercentageChange { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class CostCategoryAnalysis
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public decimal VarianceFromBudget { get; set; }
    public string Status { get; set; } = string.Empty; // "Over Budget", "Under Budget", "On Track"
}

public class PayrollProjection
{
    public DateTime ProjectionDate { get; set; }
    public decimal ProjectedAmount { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public string ProjectionMethod { get; set; } = string.Empty;
}

public class BudgetVarianceRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public List<string>? Departments { get; set; }
}

public class BudgetVarianceReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    public decimal TotalBudget { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal VariancePercentage { get; set; }
    
    public List<BudgetVarianceItem> VarianceItems { get; set; } = new();
    public List<DepartmentBudgetVariance> DepartmentVariances { get; set; } = new();
}

public class BudgetVarianceItem
{
    public string Category { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class DepartmentBudgetVariance
{
    public string Department { get; set; } = string.Empty;
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public List<BudgetVarianceItem> CategoryVariances { get; set; } = new();
}

public class CurrencyConversionRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public List<string>? TargetCurrencies { get; set; }
    public bool IncludeHistoricalRates { get; set; } = true;
}

public class CurrencyConversionReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    
    public List<CurrencyConversionData> ConversionData { get; set; } = new();
    public List<ExchangeRateHistory> RateHistory { get; set; } = new();
    public CurrencyRiskAnalysis RiskAnalysis { get; set; } = new();
}

public class CurrencyConversionData
{
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public decimal CurrentRate { get; set; }
    public decimal TotalAmountInCurrency { get; set; }
    public decimal TotalAmountInBaseCurrency { get; set; }
    public decimal RateVariation { get; set; }
    public string Trend { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
}

public class ExchangeRateHistory
{
    public DateTime Date { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Change { get; set; }
    public decimal PercentageChange { get; set; }
}

public class CurrencyRiskAnalysis
{
    public decimal TotalExposure { get; set; }
    public decimal HighestRiskCurrency { get; set; }
    public string HighestRiskCurrencyCode { get; set; } = string.Empty;
    public decimal AverageVolatility { get; set; }
    public List<CurrencyRisk> CurrencyRisks { get; set; } = new();
}

public class CurrencyRisk
{
    public string Currency { get; set; } = string.Empty;
    public decimal Exposure { get; set; }
    public decimal Volatility { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // "Low", "Medium", "High"
    public string Recommendation { get; set; } = string.Empty;
}

public class DepartmentFinancialRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public List<string>? Departments { get; set; }
    public bool IncludeSubDepartments { get; set; } = false;
}

public class DepartmentWiseFinancialReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    public List<DepartmentFinancialDetail> DepartmentDetails { get; set; } = new();
    public DepartmentComparison Comparison { get; set; } = new();
}

public class DepartmentFinancialDetail
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal MedianSalary { get; set; }
    public decimal HighestSalary { get; set; }
    public decimal LowestSalary { get; set; }
    
    public PayrollCostBreakdown CostBreakdown { get; set; } = new();
    public List<EmployeeFinancialSummary> TopEarners { get; set; } = new();
}

public class EmployeeFinancialSummary
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
}

public class DepartmentComparison
{
    public string HighestCostDepartment { get; set; } = string.Empty;
    public string LowestCostDepartment { get; set; } = string.Empty;
    public decimal CostVariation { get; set; }
    public List<DepartmentRanking> Rankings { get; set; } = new();
}

public class DepartmentRanking
{
    public int Rank { get; set; }
    public string Department { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class MonthlyTrendRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public string TrendType { get; set; } = "Cost"; // "Cost", "Headcount", "Average Salary"
}

public class MonthlyFinancialTrendReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TrendType { get; set; } = string.Empty;
    
    public List<MonthlyTrendData> TrendData { get; set; } = new();
    public TrendAnalysis Analysis { get; set; } = new();
}

public class MonthlyTrendData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal PercentageChange { get; set; }
    public decimal MovingAverage { get; set; }
}

public class TrendAnalysis
{
    public string OverallTrend { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
    public decimal AverageGrowthRate { get; set; }
    public decimal HighestValue { get; set; }
    public decimal LowestValue { get; set; }
    public string HighestMonth { get; set; } = string.Empty;
    public string LowestMonth { get; set; } = string.Empty;
    public List<string> Insights { get; set; } = new();
}

public class FinancialMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class CurrencyExchangeRate
{
    public DateTime Date { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Change { get; set; }
    public decimal PercentageChange { get; set; }
}