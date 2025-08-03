using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.DocumentTemplate;

public class DocumentTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string[] MergeFields { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public bool IsSystemTemplate { get; set; }
    public string Category { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? PreviewImageUrl { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public bool RequiresApproval { get; set; }
    public string? ApprovalWorkflow { get; set; }
    public int UsageCount { get; set; }
}

public class CreateDocumentTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string[] MergeFields { get; set; } = Array.Empty<string>();
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public bool RequiresApproval { get; set; }
    public string? ApprovalWorkflow { get; set; }
}

public class UpdateDocumentTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string[] MergeFields { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public bool RequiresApproval { get; set; }
    public string? ApprovalWorkflow { get; set; }
    public string ChangeLog { get; set; } = string.Empty;
}

public class DocumentTemplatePreviewDto
{
    public string PreviewHtml { get; set; } = string.Empty;
    public Dictionary<string, object> SampleData { get; set; } = new();
}