using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveAccrualRepository : Repository<LeaveAccrual>, ILeaveAccrualRepository
{
    public LeaveAccrualRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveAccrual>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(la => la.Employee)
            .Include(la => la.LeavePolicy)
            .Where(la => la.EmployeeId == employeeId)
            .OrderByDescending(la => la.Year)
            .ThenByDescending(la => la.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveAccrual>> GetByEmployeeAndYearAsync(int employeeId, int year)
    {
        return await _dbSet
            .Include(la => la.Employee)
            .Include(la => la.LeavePolicy)
            .Where(la => la.EmployeeId == employeeId && la.Year == year)
            .OrderBy(la => la.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveAccrual>> GetByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .Include(la => la.Employee)
            .Include(la => la.LeavePolicy)
            .Where(la => la.EmployeeId == employeeId && 
                        la.LeavePolicyId == leavePolicyId && 
                        la.Year == year)
            .OrderBy(la => la.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveAccrual>> GetPendingAccrualsAsync()
    {
        return await _dbSet
            .Include(la => la.Employee)
            .Include(la => la.LeavePolicy)
            .Where(la => !la.IsProcessed)
            .OrderBy(la => la.AccrualDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveAccrual>> GetAccrualsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(la => la.Employee)
            .Include(la => la.LeavePolicy)
            .Where(la => la.AccrualDate >= startDate && la.AccrualDate <= endDate)
            .OrderBy(la => la.AccrualDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAccruedDaysAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .Where(la => la.EmployeeId == employeeId && 
                        la.LeavePolicyId == leavePolicyId && 
                        la.Year == year &&
                        la.IsProcessed)
            .SumAsync(la => la.AccruedDays);
    }

    public async Task<bool> HasAccrualForPeriodAsync(int employeeId, int leavePolicyId, int year, int month)
    {
        return await _dbSet
            .AnyAsync(la => la.EmployeeId == employeeId && 
                           la.LeavePolicyId == leavePolicyId && 
                           la.Year == year && 
                           la.Month == month);
    }
}