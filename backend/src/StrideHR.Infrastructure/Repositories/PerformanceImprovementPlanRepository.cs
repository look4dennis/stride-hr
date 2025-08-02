using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PerformanceImprovementPlanRepository : Repository<PerformanceImprovementPlan>, IPerformanceImprovementPlanRepository
{
    public PerformanceImprovementPlanRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Include(p => p.PerformanceReview)
            .Include(p => p.Goals)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.ReviewedByEmployee)
            .Where(p => p.EmployeeId == employeeId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetByManagerIdAsync(int managerId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Include(p => p.Goals)
            .Include(p => p.Reviews)
            .Where(p => p.ManagerId == managerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetByStatusAsync(PIPStatus status)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetActivePIPsAsync()
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Include(p => p.Goals)
            .Where(p => (p.Status == PIPStatus.Active || p.Status == PIPStatus.InProgress) && 
                       !p.IsDeleted)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsRequiringReviewAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Include(p => p.Reviews)
            .Where(p => (p.Status == PIPStatus.Active || p.Status == PIPStatus.InProgress) && 
                       !p.IsDeleted)
            .Where(p => !p.Reviews.Any() || 
                       p.Reviews.OrderByDescending(r => r.ReviewDate)
                                .First().NextReviewDate <= today)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<PerformanceImprovementPlan?> GetActiveByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(p => p.Employee)
            .Include(p => p.Manager)
            .Include(p => p.HR)
            .Include(p => p.Goals)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.ReviewedByEmployee)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && 
                                     (p.Status == PIPStatus.Active || p.Status == PIPStatus.InProgress) && 
                                     !p.IsDeleted);
    }
}