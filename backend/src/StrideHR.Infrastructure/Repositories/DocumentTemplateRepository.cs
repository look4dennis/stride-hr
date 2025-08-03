using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class DocumentTemplateRepository : Repository<DocumentTemplate>, IDocumentTemplateRepository
{
    public DocumentTemplateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DocumentTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.DocumentTemplates
            .Where(t => t.IsActive)
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentTemplate>> GetTemplatesByTypeAsync(DocumentType type)
    {
        return await _context.DocumentTemplates
            .Where(t => t.Type == type && t.IsActive)
            .Include(t => t.CreatedByEmployee)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        return await _context.DocumentTemplates
            .Where(t => t.Category == category && t.IsActive)
            .Include(t => t.CreatedByEmployee)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<DocumentTemplate?> GetTemplateWithVersionsAsync(int id)
    {
        return await _context.DocumentTemplates
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .Include(t => t.Versions.OrderByDescending(v => v.VersionNumber))
                .ThenInclude(v => v.CreatedByEmployee)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
    {
        return await _context.DocumentTemplates
            .Where(t => !string.IsNullOrEmpty(t.Category))
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<int> GetUsageCountAsync(int templateId)
    {
        return await _context.GeneratedDocuments
            .CountAsync(d => d.DocumentTemplateId == templateId);
    }

    public async Task<bool> IsTemplateNameUniqueAsync(string name, int? excludeId = null)
    {
        var query = _context.DocumentTemplates.Where(t => t.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}