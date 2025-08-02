using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PerformanceReviewRepository : Repository<PerformanceReview>, IPerformanceReviewRepository
{
    public PerformanceReviewRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.Feedbacks)
                .ThenInclude(f => f.Reviewer)
            .Include(r => r.Goals)
            .Where(r => r.EmployeeId == employeeId && !r.IsDeleted)
            .OrderByDescending(r => r.ReviewStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetByManagerIdAsync(int managerId)
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Include(r => r.Feedbacks)
            .Where(r => r.ManagerId == managerId && !r.IsDeleted)
            .OrderByDescending(r => r.ReviewStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetByStatusAsync(PerformanceReviewStatus status)
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Where(r => r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetOverdueReviewsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Where(r => r.DueDate < today && 
                       r.Status != PerformanceReviewStatus.Completed && 
                       r.Status != PerformanceReviewStatus.Approved && 
                       !r.IsDeleted)
            .OrderBy(r => r.DueDate)
            .ToListAsync();
    }

    public async Task<PerformanceReview?> GetByEmployeeAndPeriodAsync(int employeeId, string reviewPeriod)
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Include(r => r.Feedbacks)
                .ThenInclude(f => f.Reviewer)
            .Include(r => r.Goals)
            .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && 
                                     r.ReviewPeriod == reviewPeriod && 
                                     !r.IsDeleted);
    }

    public async Task<IEnumerable<PerformanceReview>> GetReviewsRequiringPIPAsync()
    {
        return await _dbSet
            .Include(r => r.Employee)
            .Include(r => r.Manager)
            .Where(r => r.RequiresPIP && 
                       r.Status == PerformanceReviewStatus.Completed && 
                       !r.IsDeleted)
            .OrderByDescending(r => r.CompletedDate)
            .ToListAsync();
    }
}