namespace StrideHR.Core.Models.Payroll;

public class PayrollAnalyticsRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PayrollAnalyticsType AnalyticsType { get; set; }
    public string? Currency { get; set; }
    public List<string>? Metrics { get; set; }
    public Dictionary<string, object>? Filters { get; set; } = new();
}

public enum PayrollAnalyticsType
{
    CostAnalysis,
    TrendAnalysis,
    ComparisonAnalysis,
    DistributionAnalysis,
    EfficiencyAnalysis,
    BudgetVariance
}

public class PayrollAnalyticsResult
{
    public PayrollAnalyticsType AnalyticsType { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PayrollAnalyticsSummary Summary { get; set; } = new();
    public List<PayrollAnalyticsMetric> Metrics { get; set; } = new();
    public List<PayrollTrendData> TrendData { get; set; } = new();
    public Dictionary<string, object> Charts { get; set; } = new();
}

public class PayrollAnalyticsSummary
{
    public decimal TotalPayrollCost { get; set; }
    public decimal AveragePayrollCost { get; set; }
    public decimal PayrollCostGrowth { get; set; }
    public decimal PayrollCostPerEmployee { get; set; }
    public decimal PayrollEfficiencyRatio { get; set; }
    public Dictionary<string, decimal> CostBreakdown { get; set; } = new();
}

public class PayrollAnalyticsMetric
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty; // Up, Down, Stable
    public string Unit { get; set; } = string.Empty;
}

public class PayrollTrendData
{
    public DateTime Period { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
    public int EmployeeCount { get; set; }
    public Dictionary<string, decimal> Breakdown { get; set; } = new();
}