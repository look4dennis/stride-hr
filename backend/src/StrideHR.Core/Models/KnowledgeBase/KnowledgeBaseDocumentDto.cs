using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.KnowledgeBase;

public class KnowledgeBaseDocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComments { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public bool IsPublic { get; set; }
    public bool IsFeatured { get; set; }
    public int Priority { get; set; }
    public string? MetaDescription { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Version { get; set; }
    public int? ParentDocumentId { get; set; }
    public bool IsCurrentVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<KnowledgeBaseDocumentAttachmentDto> Attachments { get; set; } = new();
    public List<KnowledgeBaseDocumentVersionDto> Versions { get; set; } = new();
}