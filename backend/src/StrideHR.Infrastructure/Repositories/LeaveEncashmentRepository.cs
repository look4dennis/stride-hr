using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveEncashmentRepository : Repository<LeaveEncashment>, ILeaveEncashmentRepository
{
    public LeaveEncashmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveEncashment>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(le => le.Employee)
            .Include(le => le.LeavePolicy)
            .Include(le => le.ApprovedByEmployee)
            .Where(le => le.EmployeeId == employeeId)
            .OrderByDescending(le => le.EncashmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveEncashment>> GetByEmployeeAndYearAsync(int employeeId, int year)
    {
        return await _dbSet
            .Include(le => le.Employee)
            .Include(le => le.LeavePolicy)
            .Include(le => le.ApprovedByEmployee)
            .Where(le => le.EmployeeId == employeeId && le.Year == year)
            .OrderByDescending(le => le.EncashmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveEncashment>> GetPendingEncashmentsAsync(int branchId)
    {
        return await _dbSet
            .Include(le => le.Employee)
            .Include(le => le.LeavePolicy)
            .Where(le => le.Employee.BranchId == branchId && le.Status == EncashmentStatus.Pending)
            .OrderBy(le => le.EncashmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveEncashment>> GetByStatusAsync(EncashmentStatus status)
    {
        return await _dbSet
            .Include(le => le.Employee)
            .Include(le => le.LeavePolicy)
            .Include(le => le.ApprovedByEmployee)
            .Where(le => le.Status == status)
            .OrderBy(le => le.EncashmentDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalEncashedDaysAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .Where(le => le.EmployeeId == employeeId && 
                        le.LeavePolicyId == leavePolicyId && 
                        le.Year == year &&
                        (le.Status == EncashmentStatus.Approved || le.Status == EncashmentStatus.Processed))
            .SumAsync(le => le.EncashedDays);
    }

    public async Task<decimal> GetTotalEncashmentAmountAsync(int employeeId, int year)
    {
        return await _dbSet
            .Where(le => le.EmployeeId == employeeId && 
                        le.Year == year &&
                        (le.Status == EncashmentStatus.Approved || le.Status == EncashmentStatus.Processed))
            .SumAsync(le => le.EncashmentAmount);
    }

    public async Task<bool> HasPendingEncashmentAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .AnyAsync(le => le.EmployeeId == employeeId && 
                           le.LeavePolicyId == leavePolicyId && 
                           le.Year == year && 
                           le.Status == EncashmentStatus.Pending);
    }
}