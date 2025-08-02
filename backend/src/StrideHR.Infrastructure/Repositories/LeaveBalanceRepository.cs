using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveBalanceRepository : Repository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeavePolicy)
            .Where(lb => lb.EmployeeId == employeeId && !lb.IsDeleted)
            .OrderBy(lb => lb.LeavePolicy.LeaveType)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(int employeeId, int year)
    {
        return await _dbSet
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeavePolicy)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year && !lb.IsDeleted)
            .OrderBy(lb => lb.LeavePolicy.LeaveType)
            .ToListAsync();
    }

    public async Task<LeaveBalance?> GetByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeavePolicy)
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId &&
                                      lb.LeavePolicyId == leavePolicyId &&
                                      lb.Year == year &&
                                      !lb.IsDeleted);
    }

    public async Task<IEnumerable<LeaveBalance>> GetByYearAsync(int year, int branchId)
    {
        return await _dbSet
            .Include(lb => lb.Employee)
            .Include(lb => lb.LeavePolicy)
            .Where(lb => lb.Year == year &&
                        lb.Employee.BranchId == branchId &&
                        !lb.IsDeleted)
            .OrderBy(lb => lb.Employee.FirstName)
            .ThenBy(lb => lb.LeavePolicy.LeaveType)
            .ToListAsync();
    }

    public async Task<decimal> GetRemainingBalanceAsync(int employeeId, int leavePolicyId, int year)
    {
        var balance = await GetByEmployeeAndPolicyAsync(employeeId, leavePolicyId, year);
        return balance?.RemainingDays ?? 0;
    }
}