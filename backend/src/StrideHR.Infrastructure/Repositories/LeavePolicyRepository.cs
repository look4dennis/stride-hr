using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeavePolicyRepository : Repository<LeavePolicy>, ILeavePolicyRepository
{
    public LeavePolicyRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeavePolicy>> GetByBranchIdAsync(int branchId)
    {
        return await _dbSet
            .Include(lp => lp.Branch)
            .Where(lp => lp.BranchId == branchId && !lp.IsDeleted)
            .OrderBy(lp => lp.LeaveType)
            .ToListAsync();
    }

    public async Task<LeavePolicy?> GetByBranchAndTypeAsync(int branchId, LeaveType leaveType)
    {
        return await _dbSet
            .Include(lp => lp.Branch)
            .FirstOrDefaultAsync(lp => lp.BranchId == branchId &&
                                      lp.LeaveType == leaveType &&
                                      !lp.IsDeleted);
    }

    public async Task<IEnumerable<LeavePolicy>> GetActiveByBranchIdAsync(int branchId)
    {
        return await _dbSet
            .Include(lp => lp.Branch)
            .Where(lp => lp.BranchId == branchId &&
                        lp.IsActive &&
                        !lp.IsDeleted)
            .OrderBy(lp => lp.LeaveType)
            .ToListAsync();
    }
}