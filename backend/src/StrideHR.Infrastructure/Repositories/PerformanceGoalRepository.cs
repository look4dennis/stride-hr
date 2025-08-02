using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PerformanceGoalRepository : Repository<PerformanceGoal>, IPerformanceGoalRepository
{
    public PerformanceGoalRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PerformanceGoal>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(g => g.Employee)
            .Include(g => g.Manager)
            .Include(g => g.CheckIns)
            .Where(g => g.EmployeeId == employeeId && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceGoal>> GetByManagerIdAsync(int managerId)
    {
        return await _dbSet
            .Include(g => g.Employee)
            .Include(g => g.Manager)
            .Include(g => g.CheckIns)
            .Where(g => g.ManagerId == managerId && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceGoal>> GetByStatusAsync(PerformanceGoalStatus status)
    {
        return await _dbSet
            .Include(g => g.Employee)
            .Include(g => g.Manager)
            .Where(g => g.Status == status && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceGoal>> GetOverdueGoalsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(g => g.Employee)
            .Include(g => g.Manager)
            .Where(g => g.TargetDate < today && 
                       g.Status != PerformanceGoalStatus.Completed && 
                       g.Status != PerformanceGoalStatus.Cancelled && 
                       !g.IsDeleted)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceGoal>> GetGoalsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(g => g.Employee)
            .Include(g => g.Manager)
            .Where(g => g.StartDate >= startDate && g.TargetDate <= endDate && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetEmployeeGoalCompletionRateAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet.Where(g => g.EmployeeId == employeeId && !g.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(g => g.StartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(g => g.TargetDate <= endDate.Value);

        var totalGoals = await query.CountAsync();
        if (totalGoals == 0) return 0;

        var completedGoals = await query.CountAsync(g => g.Status == PerformanceGoalStatus.Completed);
        
        return (decimal)completedGoals / totalGoals * 100;
    }
}