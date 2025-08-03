using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class DocumentApproval : BaseEntity
{
    public int GeneratedDocumentId { get; set; }
    public int ApproverId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime? ActionDate { get; set; }
    public bool IsRequired { get; set; } = true;
    public int ApprovalOrder { get; set; } // For sequential approvals
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; } = false;
    public string? EscalationReason { get; set; }
    public int? EscalatedTo { get; set; }
    public DateTime? EscalatedAt { get; set; }

    // Navigation Properties
    public virtual GeneratedDocument GeneratedDocument { get; set; } = null!;
    public virtual Employee Approver { get; set; } = null!;
    public virtual Employee? EscalatedToEmployee { get; set; }
}