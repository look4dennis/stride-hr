using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<Branch>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Where(b => b.OrganizationId == organizationId)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Branch?> GetByNameAsync(string name)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Name == name);
    }

    public async Task<List<Branch>> GetActiveAsync()
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Branches.Where(b => b.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}