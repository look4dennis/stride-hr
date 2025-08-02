using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveAccrualRuleRepository : Repository<LeaveAccrualRule>, ILeaveAccrualRuleRepository
{
    public LeaveAccrualRuleRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveAccrualRule>> GetByLeavePolicyIdAsync(int leavePolicyId)
    {
        return await _dbSet
            .Include(lar => lar.LeavePolicy)
            .Where(lar => lar.LeavePolicyId == leavePolicyId)
            .OrderByDescending(lar => lar.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<LeaveAccrualRule?> GetActiveRuleAsync(int leavePolicyId, DateTime effectiveDate)
    {
        return await _dbSet
            .Include(lar => lar.LeavePolicy)
            .Where(lar => lar.LeavePolicyId == leavePolicyId &&
                         lar.IsActive &&
                         lar.EffectiveFrom <= effectiveDate &&
                         (lar.EffectiveTo == null || lar.EffectiveTo >= effectiveDate))
            .OrderByDescending(lar => lar.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LeaveAccrualRule>> GetActiveRulesAsync(DateTime effectiveDate)
    {
        return await _dbSet
            .Include(lar => lar.LeavePolicy)
            .Where(lar => lar.IsActive &&
                         lar.EffectiveFrom <= effectiveDate &&
                         (lar.EffectiveTo == null || lar.EffectiveTo >= effectiveDate))
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveAccrualRule>> GetByFrequencyAsync(AccrualFrequency frequency)
    {
        return await _dbSet
            .Include(lar => lar.LeavePolicy)
            .Where(lar => lar.AccrualFrequency == frequency && lar.IsActive)
            .ToListAsync();
    }
}