using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Organization entity
/// </summary>
public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(o => o.Name == name && !o.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetWithBranchesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .Include(o => o.Branches.Where(b => !b.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Organizations.Where(o => o.Name == name && !o.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}