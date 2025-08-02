using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayslipTemplateRepository : Repository<PayslipTemplate>, IPayslipTemplateRepository
{
    public PayslipTemplateRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<PayslipTemplate>> GetByOrganizationAsync(int organizationId)
    {
        return await _context.PayslipTemplates
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .Include(t => t.Branch)
            .Where(t => t.OrganizationId == organizationId && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<PayslipTemplate>> GetByBranchAsync(int branchId)
    {
        return await _context.PayslipTemplates
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .Where(t => (t.BranchId == branchId || t.BranchId == null) && t.IsActive)
            .OrderBy(t => t.BranchId == null ? 1 : 0) // Branch-specific templates first
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<PayslipTemplate?> GetDefaultTemplateAsync(int organizationId, int? branchId = null)
    {
        var query = _context.PayslipTemplates
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .Include(t => t.Branch)
            .Where(t => t.OrganizationId == organizationId && t.IsActive && t.IsDefault);

        if (branchId.HasValue)
        {
            // First try to find branch-specific default template
            var branchTemplate = await query
                .Where(t => t.BranchId == branchId)
                .FirstOrDefaultAsync();

            if (branchTemplate != null)
                return branchTemplate;
        }

        // Fall back to organization-wide default template
        return await query
            .Where(t => t.BranchId == null)
            .FirstOrDefaultAsync();
    }

    public async Task<PayslipTemplate?> GetActiveTemplateByNameAsync(int organizationId, string name)
    {
        return await _context.PayslipTemplates
            .Include(t => t.CreatedByEmployee)
            .Include(t => t.LastModifiedByEmployee)
            .Include(t => t.Branch)
            .Where(t => t.OrganizationId == organizationId && 
                       t.Name.ToLower() == name.ToLower() && 
                       t.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SetAsDefaultAsync(int templateId, int organizationId, int? branchId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove default flag from existing templates
            var existingDefaults = await _context.PayslipTemplates
                .Where(t => t.OrganizationId == organizationId && 
                           t.BranchId == branchId && 
                           t.IsDefault)
                .ToListAsync();

            foreach (var template in existingDefaults)
            {
                template.IsDefault = false;
            }

            // Set new default template
            var newDefaultTemplate = await _context.PayslipTemplates
                .FindAsync(templateId);

            if (newDefaultTemplate == null)
                return false;

            newDefaultTemplate.IsDefault = true;
            newDefaultTemplate.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}