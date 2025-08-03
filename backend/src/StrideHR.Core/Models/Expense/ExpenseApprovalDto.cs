using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Expense;

public class ExpenseApprovalDto
{
    [Required]
    public ApprovalAction Action { get; set; }

    [StringLength(1000)]
    public string? Comments { get; set; }

    public decimal? ApprovedAmount { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}

public class BulkExpenseApprovalDto
{
    [Required]
    public List<int> ExpenseClaimIds { get; set; } = new();

    [Required]
    public ApprovalAction Action { get; set; }

    [StringLength(1000)]
    public string? Comments { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}