using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class KnowledgeBaseCategoryRepository : Repository<KnowledgeBaseCategory>, IKnowledgeBaseCategoryRepository
{
    public KnowledgeBaseCategoryRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<KnowledgeBaseCategory>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseCategory>> GetRootCategoriesAsync()
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseCategory>> GetSubCategoriesAsync(int parentCategoryId)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<KnowledgeBaseCategory?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<int> GetDocumentCountAsync(int categoryId)
    {
        return await _context.Set<KnowledgeBaseDocument>()
            .CountAsync(d => d.CategoryId == categoryId);
    }
}