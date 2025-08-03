using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseComplianceViolation : BaseEntity
{
    public int ExpenseClaimId { get; set; }
    public int? ExpenseItemId { get; set; }
    public int PolicyRuleId { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string ViolationDescription { get; set; } = string.Empty;
    public ExpenseViolationSeverity Severity { get; set; }
    public decimal? ViolationAmount { get; set; }
    public DateTime ViolationDate { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public int? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public bool IsWaived { get; set; }
    public string? WaiverReason { get; set; }
    public int? WaivedBy { get; set; }
    public DateTime? WaivedDate { get; set; }

    // Navigation Properties
    public virtual ExpenseClaim ExpenseClaim { get; set; } = null!;
    public virtual ExpenseItem? ExpenseItem { get; set; }
    public virtual ExpensePolicyRule PolicyRule { get; set; } = null!;
    public virtual Employee? ResolvedByEmployee { get; set; }
    public virtual Employee? WaivedByEmployee { get; set; }
}