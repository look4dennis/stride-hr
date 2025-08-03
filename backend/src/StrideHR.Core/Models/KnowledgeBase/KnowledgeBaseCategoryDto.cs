namespace StrideHR.Core.Models.KnowledgeBase;

public class KnowledgeBaseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Color { get; set; } = "#007bff";
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Slug { get; set; }
    public string? MetaDescription { get; set; }
    public int DocumentCount { get; set; }
    public List<KnowledgeBaseCategoryDto> SubCategories { get; set; } = new();
}