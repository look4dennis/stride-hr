using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class DocumentRetentionPolicy : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public int RetentionPeriodMonths { get; set; }
    public bool AutoDelete { get; set; } = false;
    public bool RequiresApprovalForDeletion { get; set; } = true;
    public string[] ApprovalRoles { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; } = true;
    public string LegalBasis { get; set; } = string.Empty;
    public string ComplianceNotes { get; set; } = string.Empty;
    public new int CreatedBy { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }

    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<DocumentRetentionExecution> Executions { get; set; } = new List<DocumentRetentionExecution>();
}