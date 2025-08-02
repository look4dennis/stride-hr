using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
{
    public EmailTemplateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<EmailTemplate?> GetByNameAsync(string name)
    {
        return await _context.EmailTemplates
            .Include(t => t.Branch)
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<List<EmailTemplate>> GetByTypeAsync(EmailTemplateType type)
    {
        return await _context.EmailTemplates
            .Include(t => t.Branch)
            .Where(t => t.Type == type && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<EmailTemplate>> GetByCategoryAsync(string category)
    {
        return await _context.EmailTemplates
            .Include(t => t.Branch)
            .Where(t => t.Category == category && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<EmailTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.EmailTemplates
            .Include(t => t.Branch)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<EmailTemplate>> GetGlobalTemplatesAsync()
    {
        return await _context.EmailTemplates
            .Where(t => t.IsGlobal && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<EmailTemplate>> GetBranchTemplatesAsync(int branchId)
    {
        return await _context.EmailTemplates
            .Include(t => t.Branch)
            .Where(t => (t.BranchId == branchId || t.IsGlobal) && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.EmailTemplates.Where(t => t.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<int> GetUsageCountAsync(int templateId)
    {
        return await _context.EmailLogs
            .Where(l => l.EmailTemplateId == templateId)
            .CountAsync();
    }

    public async Task<List<EmailTemplate>> SearchAsync(string searchTerm, int? branchId = null)
    {
        var query = _context.EmailTemplates
            .Include(t => t.Branch)
            .Where(t => t.IsActive);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId || t.IsGlobal);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => 
                t.Name.Contains(searchTerm) ||
                t.Subject.Contains(searchTerm) ||
                t.Category.Contains(searchTerm) ||
                (t.Description != null && t.Description.Contains(searchTerm)));
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}