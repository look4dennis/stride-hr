namespace StrideHR.Core.Models.Payroll;

public class BudgetVarianceRequest
{
    public int BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int BudgetYear { get; set; }
    public int? BudgetMonth { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BudgetVarianceType VarianceType { get; set; } = BudgetVarianceType.Monthly;
}

public enum BudgetVarianceType
{
    Monthly,
    Quarterly,
    Annual,
    YearToDate
}

public class BudgetVarianceResult
{
    public int BudgetYear { get; set; }
    public int? BudgetMonth { get; set; }
    public BudgetVarianceType VarianceType { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public BudgetVarianceSummary Summary { get; set; } = new();
    public List<BudgetVarianceItem> Items { get; set; } = new();
    public List<BudgetVarianceAlert> Alerts { get; set; } = new();
}

public class BudgetVarianceSummary
{
    public decimal TotalBudget { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal VariancePercentage { get; set; }
    public decimal FavorableVariance { get; set; }
    public decimal UnfavorableVariance { get; set; }
    public string OverallStatus { get; set; } = string.Empty; // OnTrack, OverBudget, UnderBudget
}

public class BudgetVarianceItem
{
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string VarianceType { get; set; } = string.Empty; // Favorable, Unfavorable
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class BudgetVarianceAlert
{
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public decimal ActualValue { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}