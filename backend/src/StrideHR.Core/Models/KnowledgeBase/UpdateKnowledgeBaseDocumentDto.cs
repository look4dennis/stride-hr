using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.KnowledgeBase;

public class UpdateKnowledgeBaseDocumentDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] Keywords { get; set; } = Array.Empty<string>();

    [Required]
    public int CategoryId { get; set; }

    public bool IsPublic { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int Priority { get; set; } = 0;

    [StringLength(300)]
    public string? MetaDescription { get; set; }

    public DateTime? ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? VersionNotes { get; set; }
}