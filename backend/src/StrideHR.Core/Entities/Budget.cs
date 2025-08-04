using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Budget : BaseEntity
{
    public int BranchId { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Salary", "Allowances", "Benefits", etc.
    public int Year { get; set; }
    public int Month { get; set; }
    
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    
    public string Currency { get; set; } = string.Empty;
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    
    public string? Notes { get; set; }
    public new int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
}