using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Branch entity
/// </summary>
public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Where(b => b.OrganizationId == organizationId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Branch>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Where(b => b.Country == country && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesAsync(int? organizationId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches
            .Include(b => b.Organization)
            .Where(b => b.IsActive && !b.IsDeleted);

        if (organizationId.HasValue)
        {
            query = query.Where(b => b.OrganizationId == organizationId.Value);
        }

        return await query
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Branch?> GetByNameAsync(string name, int organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Name == name && b.OrganizationId == organizationId && !b.IsDeleted, cancellationToken);
    }

    public async Task<Branch?> GetWithEmployeesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Include(b => b.Organization)
            .Include(b => b.Employees.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, int organizationId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches.Where(b => b.Name == name && b.OrganizationId == organizationId && !b.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(b => !b.IsDeleted)
            .Select(b => b.Country)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(b => !b.IsDeleted)
            .Select(b => b.Currency)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(b => !b.IsDeleted)
            .Select(b => b.TimeZone)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(cancellationToken);
    }
}