namespace StrideHR.Core.Entities;

public class KnowledgeBaseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Color { get; set; } = "#007bff"; // Default blue color
    public int? ParentCategoryId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Slug { get; set; } // URL-friendly name
    public string? MetaDescription { get; set; }

    // Navigation Properties
    public virtual KnowledgeBaseCategory? ParentCategory { get; set; }
    public virtual ICollection<KnowledgeBaseCategory> SubCategories { get; set; } = new List<KnowledgeBaseCategory>();
    public virtual ICollection<KnowledgeBaseDocument> Documents { get; set; } = new List<KnowledgeBaseDocument>();
}