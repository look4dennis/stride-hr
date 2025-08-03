using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.DocumentTemplate;

public class DocumentRetentionPolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public int RetentionPeriodMonths { get; set; }
    public bool AutoDelete { get; set; }
    public bool RequiresApprovalForDeletion { get; set; }
    public string[] ApprovalRoles { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string LegalBasis { get; set; } = string.Empty;
    public string ComplianceNotes { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public int AffectedDocumentsCount { get; set; }
}

public class CreateDocumentRetentionPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public int RetentionPeriodMonths { get; set; }
    public bool AutoDelete { get; set; }
    public bool RequiresApprovalForDeletion { get; set; }
    public string[] ApprovalRoles { get; set; } = Array.Empty<string>();
    public string LegalBasis { get; set; } = string.Empty;
    public string ComplianceNotes { get; set; } = string.Empty;
}

public class UpdateDocumentRetentionPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RetentionPeriodMonths { get; set; }
    public bool AutoDelete { get; set; }
    public bool RequiresApprovalForDeletion { get; set; }
    public string[] ApprovalRoles { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string LegalBasis { get; set; } = string.Empty;
    public string ComplianceNotes { get; set; } = string.Empty;
}

public class DocumentRetentionExecutionDto
{
    public int Id { get; set; }
    public int DocumentRetentionPolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public int GeneratedDocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ExecutedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExecutionNotes { get; set; }
    public bool RequiredApproval { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalComments { get; set; }
}