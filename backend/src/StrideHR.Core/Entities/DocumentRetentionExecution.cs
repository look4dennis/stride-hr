namespace StrideHR.Core.Entities;

public class DocumentRetentionExecution : BaseEntity
{
    public int DocumentRetentionPolicyId { get; set; }
    public int GeneratedDocumentId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ExecutedDate { get; set; }
    public string Status { get; set; } = string.Empty; // Scheduled, Executed, Failed, Cancelled
    public string? ExecutionNotes { get; set; }
    public int? ExecutedBy { get; set; }
    public bool RequiredApproval { get; set; }
    public bool IsApproved { get; set; } = false;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalComments { get; set; }

    // Navigation Properties
    public virtual DocumentRetentionPolicy DocumentRetentionPolicy { get; set; } = null!;
    public virtual GeneratedDocument GeneratedDocument { get; set; } = null!;
    public virtual Employee? ExecutedByEmployee { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
}