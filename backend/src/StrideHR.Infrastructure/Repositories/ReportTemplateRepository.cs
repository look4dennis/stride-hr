using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ReportTemplateRepository : Repository<ReportTemplate>, IReportTemplateRepository
{
    public ReportTemplateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReportTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.ReportTemplates
            .Include(rt => rt.CreatedByEmployee)
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportTemplate>> GetTemplatesByTypeAsync(ReportType type)
    {
        return await _context.ReportTemplates
            .Include(rt => rt.CreatedByEmployee)
            .Where(rt => rt.Type == type && rt.IsActive)
            .OrderBy(rt => rt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        return await _context.ReportTemplates
            .Include(rt => rt.CreatedByEmployee)
            .Where(rt => rt.Category == category && rt.IsActive)
            .OrderBy(rt => rt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportTemplate>> GetSystemTemplatesAsync()
    {
        return await _context.ReportTemplates
            .Where(rt => rt.IsSystemTemplate && rt.IsActive)
            .OrderBy(rt => rt.Category)
            .ThenBy(rt => rt.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportTemplate>> GetUserTemplatesAsync(int userId)
    {
        return await _context.ReportTemplates
            .Include(rt => rt.CreatedByEmployee)
            .Where(rt => rt.CreatedBy == userId && rt.IsActive)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }
}