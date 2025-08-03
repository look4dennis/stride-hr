using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseBudget : BaseEntity
{
    public int OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }
    public int? CategoryId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public string Currency { get; set; } = "USD";
    public ExpenseAnalyticsPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal AlertThreshold { get; set; } = 80; // Percentage
    public bool SendAlerts { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Employee? Employee { get; set; }
    public virtual ExpenseCategory? Category { get; set; }
    public virtual ICollection<ExpenseBudgetAlert> BudgetAlerts { get; set; } = new List<ExpenseBudgetAlert>();
}

public class ExpenseBudgetAlert : BaseEntity
{
    public int ExpenseBudgetId { get; set; }
    public string AlertType { get; set; } = string.Empty; // Warning, Critical, Exceeded
    public string Message { get; set; } = string.Empty;
    public decimal CurrentUtilization { get; set; }
    public decimal ThresholdPercentage { get; set; }
    public DateTime AlertDate { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public int? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }

    // Navigation Properties
    public virtual ExpenseBudget ExpenseBudget { get; set; } = null!;
    public virtual Employee? ResolvedByEmployee { get; set; }
}