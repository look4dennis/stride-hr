using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class KnowledgeBaseDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Rich text content (HTML)
    public string Summary { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public int CategoryId { get; set; }
    public DocumentStatus Status { get; set; }
    public int AuthorId { get; set; }
    public int? ReviewerId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int Priority { get; set; } = 0;
    public string? MetaDescription { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Version { get; set; } = 1;
    public int? ParentDocumentId { get; set; } // For version control
    public bool IsCurrentVersion { get; set; } = true;

    // Navigation Properties
    public virtual KnowledgeBaseCategory Category { get; set; } = null!;
    public virtual Employee Author { get; set; } = null!;
    public virtual Employee? Reviewer { get; set; }
    public virtual KnowledgeBaseDocument? ParentDocument { get; set; }
    public virtual ICollection<KnowledgeBaseDocument> ChildVersions { get; set; } = new List<KnowledgeBaseDocument>();
    public virtual ICollection<KnowledgeBaseDocumentApproval> Approvals { get; set; } = new List<KnowledgeBaseDocumentApproval>();
    public virtual ICollection<KnowledgeBaseDocumentAttachment> Attachments { get; set; } = new List<KnowledgeBaseDocumentAttachment>();
    public virtual ICollection<KnowledgeBaseDocumentView> Views { get; set; } = new List<KnowledgeBaseDocumentView>();
    public virtual ICollection<KnowledgeBaseDocumentComment> Comments { get; set; } = new List<KnowledgeBaseDocumentComment>();
}