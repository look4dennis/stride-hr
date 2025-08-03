using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class KnowledgeBaseDocumentApproval : BaseEntity
{
    public int DocumentId { get; set; }
    public int ApproverId { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
    public ApprovalLevel Level { get; set; }
    public int StepOrder { get; set; }
    public bool IsRequired { get; set; } = true;

    // Navigation Properties
    public virtual KnowledgeBaseDocument Document { get; set; } = null!;
    public virtual Employee Approver { get; set; } = null!;
}