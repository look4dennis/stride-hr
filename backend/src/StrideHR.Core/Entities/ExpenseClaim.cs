using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseClaim : BaseEntity
{
    public int EmployeeId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ExpenseDate { get; set; }
    public DateTime SubmissionDate { get; set; }
    public ExpenseClaimStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ReimbursedDate { get; set; }
    public string? ReimbursementReference { get; set; }
    public bool IsAdvanceClaim { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
    public virtual ICollection<ExpenseApprovalHistory> ApprovalHistory { get; set; } = new List<ExpenseApprovalHistory>();
    public virtual ICollection<ExpenseDocument> Documents { get; set; } = new List<ExpenseDocument>();
}