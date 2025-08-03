using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class DocumentTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Content { get; set; } = string.Empty; // HTML template with merge fields
    public string[] MergeFields { get; set; } = Array.Empty<string>(); // Available merge fields
    public bool IsActive { get; set; } = true;
    public bool IsSystemTemplate { get; set; } = false; // System templates cannot be deleted
    public string Category { get; set; } = string.Empty;
    public new int CreatedBy { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? PreviewImageUrl { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public bool RequiresApproval { get; set; } = false;
    public string? ApprovalWorkflow { get; set; }

    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Employee? LastModifiedByEmployee { get; set; }
    public virtual ICollection<GeneratedDocument> GeneratedDocuments { get; set; } = new List<GeneratedDocument>();
    public virtual ICollection<DocumentTemplateVersion> Versions { get; set; } = new List<DocumentTemplateVersion>();
}