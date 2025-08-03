using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExpenseCategory>> GetActiveByOrganizationAsync(int organizationId)
    {
        return await _dbSet
            .Where(c => c.OrganizationId == organizationId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ExpenseCategory?> GetByCodeAsync(string code, int organizationId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == code && c.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<ExpenseCategory>> GetMileageBasedCategoriesAsync(int organizationId)
    {
        return await _dbSet
            .Where(c => c.OrganizationId == organizationId && c.IsActive && c.IsMileageBased)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int organizationId, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Code == code && c.OrganizationId == organizationId);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}