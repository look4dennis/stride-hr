using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class DSRRepository : Repository<DSR>, IDSRRepository
{
    public DSRRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DSR>> GetDSRsByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(d => d.EmployeeId == employeeId && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Include(d => d.Reviewer)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<DSR>> GetDSRsByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(d => d.ProjectId == projectId && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .OrderByDescending(d => d.Date)
            .ThenBy(d => d.Employee.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<DSR>> GetDSRsByStatusAsync(DSRStatus status)
    {
        return await _dbSet
            .Where(d => d.Status == status && !d.IsDeleted)
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<DSR?> GetDSRByEmployeeAndDateAsync(int employeeId, DateTime date)
    {
        return await _dbSet
            .Where(d => d.EmployeeId == employeeId && d.Date.Date == date.Date && !d.IsDeleted)
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DSR>> GetPendingDSRsForReviewAsync(int reviewerId)
    {
        return await _dbSet
            .Where(d => d.Status == DSRStatus.UnderReview && !d.IsDeleted)
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .OrderByDescending(d => d.SubmittedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalHoursByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(d => d.ProjectId == projectId && d.Status == DSRStatus.Approved && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query.SumAsync(d => d.HoursWorked);
    }

    public async Task<decimal> GetTotalHoursByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(d => d.EmployeeId == employeeId && d.Status == DSRStatus.Approved && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query.SumAsync(d => d.HoursWorked);
    }

    public async Task<IEnumerable<DSR>> GetProjectDSRsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Where(d => d.ProjectId == projectId && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }
}