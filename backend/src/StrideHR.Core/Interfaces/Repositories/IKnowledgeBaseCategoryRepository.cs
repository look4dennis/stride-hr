using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IKnowledgeBaseCategoryRepository : IRepository<KnowledgeBaseCategory>
{
    Task<IEnumerable<KnowledgeBaseCategory>> GetActiveCategoriesAsync();
    Task<IEnumerable<KnowledgeBaseCategory>> GetRootCategoriesAsync();
    Task<IEnumerable<KnowledgeBaseCategory>> GetSubCategoriesAsync(int parentCategoryId);
    Task<KnowledgeBaseCategory?> GetBySlugAsync(string slug);
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
    Task<int> GetDocumentCountAsync(int categoryId);
}