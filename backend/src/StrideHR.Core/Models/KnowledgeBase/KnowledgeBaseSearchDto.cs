using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.KnowledgeBase;

public class KnowledgeBaseSearchDto
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string[]? Tags { get; set; }
    public DocumentStatus? Status { get; set; }
    public int? AuthorId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }
    public bool? IsPublic { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsExpired { get; set; }
    public string SortBy { get; set; } = "UpdatedAt";
    public string SortDirection { get; set; } = "DESC";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}