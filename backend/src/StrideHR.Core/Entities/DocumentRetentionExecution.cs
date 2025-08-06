namespace StrideHR.Core.Entities;

public class DocumentRetentionExecution : BaseEntity
{
    public int DocumentRetentionPolicyId { get; set; }
    public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;
    public int DocumentsProcessed { get; set; }
    public int DocumentsDeleted { get; set; }
    public int DocumentsArchived { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ExecutionLog { get; set; }
    
    // Additional properties referenced in code
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool RequiredApproval { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public int? GeneratedDocumentId { get; set; }
    public int? ApprovedByEmployeeId { get; set; }
    
    // Navigation properties
    public virtual DocumentRetentionPolicy DocumentRetentionPolicy { get; set; } = null!;
    public virtual GeneratedDocument? GeneratedDocument { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
}