using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseItem : BaseEntity
{
    public int ExpenseClaimId { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ExpenseDate { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public bool IsBillable { get; set; }
    public int? ProjectId { get; set; }
    public string? Notes { get; set; }
    public decimal? MileageDistance { get; set; }
    public decimal? MileageRate { get; set; }

    // Navigation Properties
    public virtual ExpenseClaim ExpenseClaim { get; set; } = null!;
    public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual ICollection<ExpenseDocument> Documents { get; set; } = new List<ExpenseDocument>();
}