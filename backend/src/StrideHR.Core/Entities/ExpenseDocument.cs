using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseDocument : BaseEntity
{
    public int? ExpenseClaimId { get; set; }
    public int? ExpenseItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
    public DateTime UploadedDate { get; set; }
    public int UploadedBy { get; set; }
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ExpenseClaim? ExpenseClaim { get; set; }
    public virtual ExpenseItem? ExpenseItem { get; set; }
    public virtual Employee UploadedByEmployee { get; set; } = null!;
}