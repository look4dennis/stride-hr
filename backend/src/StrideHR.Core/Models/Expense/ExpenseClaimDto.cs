using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Expense;

public class ExpenseClaimDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public DateTime SubmissionDate { get; set; }
    public ExpenseClaimStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ReimbursedDate { get; set; }
    public string? ReimbursementReference { get; set; }
    public bool IsAdvanceClaim { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public string? Notes { get; set; }
    public List<ExpenseItemDto> ExpenseItems { get; set; } = new();
    public List<ExpenseDocumentDto> Documents { get; set; } = new();
    public List<ExpenseApprovalHistoryDto> ApprovalHistory { get; set; } = new();
}

public class ExpenseItemDto
{
    public int Id { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public bool IsBillable { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? Notes { get; set; }
    public decimal? MileageDistance { get; set; }
    public decimal? MileageRate { get; set; }
    public List<ExpenseDocumentDto> Documents { get; set; } = new();
}

public class ExpenseDocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public DateTime UploadedDate { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ExpenseApprovalHistoryDto
{
    public int Id { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalLevel ApprovalLevel { get; set; }
    public string ApprovalLevelText { get; set; } = string.Empty;
    public ApprovalAction Action { get; set; }
    public string ActionText { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? RejectionReason { get; set; }
}