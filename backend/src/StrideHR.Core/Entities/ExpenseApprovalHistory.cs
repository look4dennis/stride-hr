using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseApprovalHistory : BaseEntity
{
    public int ExpenseClaimId { get; set; }
    public int ApproverId { get; set; }
    public ApprovalLevel ApprovalLevel { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public virtual ExpenseClaim ExpenseClaim { get; set; } = null!;
    public virtual Employee Approver { get; set; } = null!;
}