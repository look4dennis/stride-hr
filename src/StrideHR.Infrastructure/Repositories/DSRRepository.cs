using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for DSR data access operations
/// </summary>
public class DSRRepository : Repository<DSR>, IDSRRepository
{
    public DSRRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<DSR?> GetByEmployeeAndDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Include(d => d.Reviewer)
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId && d.Date.Date == date.Date && !d.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetEmployeeDSRsAsync(
        int employeeId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DSRStatus? status = null,
        int? projectId = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Include(d => d.Reviewer)
            .Where(d => d.EmployeeId == employeeId && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (projectId.HasValue)
            query = query.Where(d => d.ProjectId == projectId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var dsrs = await query
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (dsrs, totalCount);
    }

    public async Task<(IEnumerable<DSR> DSRs, int TotalCount)> GetPendingDSRsForReviewAsync(
        int reviewerId,
        DateTime? date = null,
        int? employeeId = null,
        int? projectId = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Get employees that report to this reviewer
        var subordinateIds = await _context.Employees
            .Where(e => e.ReportingManagerId == reviewerId && !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var query = _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Where(d => subordinateIds.Contains(d.EmployeeId) && 
                       d.Status == DSRStatus.Submitted && 
                       !d.IsDeleted);

        if (date.HasValue)
            query = query.Where(d => d.Date.Date == date.Value.Date);

        if (employeeId.HasValue)
            query = query.Where(d => d.EmployeeId == employeeId.Value);

        if (projectId.HasValue)
            query = query.Where(d => d.ProjectId == projectId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var dsrs = await query
            .OrderBy(d => d.Date)
            .ThenBy(d => d.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (dsrs, totalCount);
    }

    public async Task<IEnumerable<DSR>> GetDSRsByStatusAsync(
        DSRStatus status,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Where(d => d.Status == status && !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query
            .OrderByDescending(d => d.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DSR>> GetEmployeeProductivityDataAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DSRs
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Where(d => d.EmployeeId == employeeId && 
                       d.Date >= startDate && 
                       d.Date <= endDate && 
                       d.Status == DSRStatus.Approved && 
                       !d.IsDeleted)
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DSR>> GetTeamProductivityDataAsync(
        int managerId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Get employees that report to this manager
        var subordinateIds = await _context.Employees
            .Where(e => e.ReportingManagerId == managerId && !e.IsDeleted)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        return await _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Where(d => subordinateIds.Contains(d.EmployeeId) && 
                       d.Date >= startDate && 
                       d.Date <= endDate && 
                       d.Status == DSRStatus.Approved && 
                       !d.IsDeleted)
            .OrderBy(d => d.Date)
            .ThenBy(d => d.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesWithoutDSRAsync(
        DateTime date,
        int? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var employeesWithDSR = await _context.DSRs
            .Where(d => d.Date.Date == date.Date && !d.IsDeleted)
            .Select(d => d.EmployeeId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = _context.Employees
            .Where(e => !employeesWithDSR.Contains(e.Id) && !e.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        return await query
            .OrderBy(e => e.Department)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetIdleEmployeesTodayAsync(
        int? branchId = null,
        decimal productivityThreshold = 6.0m,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        
        // Get employees with low productivity (less than threshold hours)
        var lowProductivityEmployees = await _context.DSRs
            .Where(d => d.Date.Date == today && !d.IsDeleted)
            .GroupBy(d => d.EmployeeId)
            .Where(g => g.Sum(d => d.HoursWorked) < productivityThreshold)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        // Get employees without DSR
        var employeesWithoutDSR = await GetEmployeesWithoutDSRAsync(today, branchId, cancellationToken);
        var employeesWithoutDSRIds = employeesWithoutDSR.Select(e => e.Id).ToList();

        // Combine both lists
        var idleEmployeeIds = lowProductivityEmployees.Concat(employeesWithoutDSRIds).Distinct();

        var query = _context.Employees
            .Include(e => e.ReportingManager)
            .Where(e => idleEmployeeIds.Contains(e.Id) && !e.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        return await query
            .OrderBy(e => e.Department)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DSR>> GetProjectHoursDataAsync(
        int projectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs
            .Include(d => d.Employee)
            .Include(d => d.Task)
            .Where(d => d.ProjectId == projectId && 
                       d.Status == DSRStatus.Approved && 
                       !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query
            .OrderBy(d => d.Date)
            .ThenBy(d => d.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DSR>> GetEmployeeProjectHoursAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.DSRs
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Where(d => d.EmployeeId == employeeId && 
                       d.Date >= startDate && 
                       d.Date <= endDate && 
                       d.Status == DSRStatus.Approved && 
                       d.ProjectId.HasValue && 
                       !d.IsDeleted)
            .OrderBy(d => d.Date)
            .ThenBy(d => d.ProjectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalProjectHoursAsync(
        int projectId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs
            .Where(d => d.ProjectId == projectId && 
                       d.Status == DSRStatus.Approved && 
                       !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query.SumAsync(d => d.HoursWorked, cancellationToken);
    }

    public async Task<decimal> GetTotalTaskHoursAsync(
        int taskId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs
            .Where(d => d.TaskId == taskId && 
                       d.Status == DSRStatus.Approved && 
                       !d.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        return await query.SumAsync(d => d.HoursWorked, cancellationToken);
    }

    public async Task<DSRStatistics> GetDSRStatisticsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DSRs.Where(d => !d.IsDeleted);

        if (branchId.HasValue)
        {
            var branchEmployeeIds = await _context.Employees
                .Where(e => e.BranchId == branchId.Value && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);
            
            query = query.Where(d => branchEmployeeIds.Contains(d.EmployeeId));
        }

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= endDate.Value);

        var dsrs = await query.ToListAsync(cancellationToken);

        return new DSRStatistics
        {
            TotalDSRs = dsrs.Count,
            PendingReviews = dsrs.Count(d => d.Status == DSRStatus.Submitted),
            ApprovedDSRs = dsrs.Count(d => d.Status == DSRStatus.Approved),
            RejectedDSRs = dsrs.Count(d => d.Status == DSRStatus.Rejected),
            TotalHours = dsrs.Sum(d => d.HoursWorked),
            AverageHoursPerDSR = dsrs.Count > 0 ? dsrs.Average(d => d.HoursWorked) : 0,
            SubmissionRate = await GetDSRSubmissionRateAsync(
                startDate ?? DateTime.Today.AddDays(-30), 
                endDate ?? DateTime.Today, 
                branchId, 
                cancellationToken)
        };
    }

    public async Task<IEnumerable<DepartmentProductivityStats>> GetDepartmentProductivityStatsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = from d in _context.DSRs
                    join e in _context.Employees on d.EmployeeId equals e.Id
                    where !d.IsDeleted && !e.IsDeleted && d.Status == DSRStatus.Approved
                    select new { DSR = d, Employee = e };

        if (branchId.HasValue)
            query = query.Where(x => x.Employee.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(x => x.DSR.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.DSR.Date <= endDate.Value);

        var data = await query.ToListAsync(cancellationToken);

        return data
            .GroupBy(x => x.Employee.Department)
            .Select(g => new DepartmentProductivityStats
            {
                Department = g.Key ?? "Unknown",
                EmployeeCount = g.Select(x => x.Employee.Id).Distinct().Count(),
                DSRCount = g.Count(),
                TotalHours = g.Sum(x => x.DSR.HoursWorked),
                AverageHoursPerEmployee = g.Select(x => x.Employee.Id).Distinct().Count() > 0 
                    ? g.Sum(x => x.DSR.HoursWorked) / g.Select(x => x.Employee.Id).Distinct().Count() 
                    : 0,
                ProductivityPercentage = g.Count() > 0 ? (g.Sum(x => x.DSR.HoursWorked) / g.Count()) * 100 / 8 : 0 // Assuming 8 hours as 100%
            })
            .OrderByDescending(x => x.ProductivityPercentage)
            .ToList();
    }

    public async Task<IEnumerable<ProjectProductivityStats>> GetProjectProductivityStatsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = from d in _context.DSRs
                    join e in _context.Employees on d.EmployeeId equals e.Id
                    join p in _context.Projects on d.ProjectId equals p.Id
                    where !d.IsDeleted && !e.IsDeleted && !p.IsDeleted && 
                          d.Status == DSRStatus.Approved && d.ProjectId.HasValue
                    select new { DSR = d, Employee = e, Project = p };

        if (branchId.HasValue)
            query = query.Where(x => x.Employee.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(x => x.DSR.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.DSR.Date <= endDate.Value);

        var data = await query.ToListAsync(cancellationToken);

        return data
            .GroupBy(x => new { x.Project.Id, x.Project.Name })
            .Select(g => new ProjectProductivityStats
            {
                ProjectId = g.Key.Id,
                ProjectName = g.Key.Name,
                ContributorCount = g.Select(x => x.Employee.Id).Distinct().Count(),
                DSRCount = g.Count(),
                TotalHours = g.Sum(x => x.DSR.HoursWorked),
                AverageHoursPerContributor = g.Select(x => x.Employee.Id).Distinct().Count() > 0 
                    ? g.Sum(x => x.DSR.HoursWorked) / g.Select(x => x.Employee.Id).Distinct().Count() 
                    : 0
            })
            .OrderByDescending(x => x.TotalHours)
            .ToList();
    }

    public async Task<IEnumerable<EmployeeProductivityStats>> GetEmployeeProductivityRankingsAsync(
        int? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var query = from d in _context.DSRs
                    join e in _context.Employees on d.EmployeeId equals e.Id
                    where !d.IsDeleted && !e.IsDeleted && d.Status == DSRStatus.Approved
                    select new { DSR = d, Employee = e };

        if (branchId.HasValue)
            query = query.Where(x => x.Employee.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(x => x.DSR.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.DSR.Date <= endDate.Value);

        var data = await query.ToListAsync(cancellationToken);

        var rankings = data
            .GroupBy(x => new { x.Employee.Id, x.Employee.FirstName, x.Employee.LastName, x.Employee.Department })
            .Select(g => new EmployeeProductivityStats
            {
                EmployeeId = g.Key.Id,
                EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                Department = g.Key.Department ?? "Unknown",
                DSRCount = g.Count(),
                TotalHours = g.Sum(x => x.DSR.HoursWorked),
                AverageHoursPerDay = g.Count() > 0 ? g.Sum(x => x.DSR.HoursWorked) / g.Count() : 0,
                ProductivityScore = g.Sum(x => x.DSR.HoursWorked) // Simple scoring based on total hours
            })
            .OrderByDescending(x => x.ProductivityScore)
            .Take(topCount)
            .ToList();

        // Add ranking
        for (int i = 0; i < rankings.Count; i++)
        {
            rankings[i].Rank = i + 1;
        }

        return rankings;
    }

    public async Task<int> BulkUpdateStatusAsync(
        IEnumerable<int> dsrIds,
        DSRStatus status,
        int reviewerId,
        string? reviewComments = null,
        CancellationToken cancellationToken = default)
    {
        var dsrs = await _context.DSRs
            .Where(d => dsrIds.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var dsr in dsrs)
        {
            dsr.Status = status;
            dsr.ReviewedBy = reviewerId;
            dsr.ReviewedAt = DateTime.UtcNow;
            dsr.ReviewComments = reviewComments;
            dsr.UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasEmployeeSubmittedDSRAsync(
        int employeeId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        return await _context.DSRs
            .AnyAsync(d => d.EmployeeId == employeeId && 
                          d.Date.Date == date.Date && 
                          !d.IsDeleted, cancellationToken);
    }

    public async Task<decimal> GetDSRSubmissionRateAsync(
        DateTime startDate,
        DateTime endDate,
        int? branchId = null,
        CancellationToken cancellationToken = default)
    {
        // Get total working days in the period
        var workingDays = 0;
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                workingDays++;
        }

        if (workingDays == 0) return 0;

        // Get total employees
        var employeeQuery = _context.Employees.Where(e => !e.IsDeleted);
        if (branchId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.BranchId == branchId.Value);

        var totalEmployees = await employeeQuery.CountAsync(cancellationToken);
        if (totalEmployees == 0) return 0;

        // Get total expected DSRs
        var expectedDSRs = totalEmployees * workingDays;

        // Get actual DSRs submitted
        var dsrQuery = _context.DSRs.Where(d => d.Date >= startDate && d.Date <= endDate && !d.IsDeleted);
        
        if (branchId.HasValue)
        {
            var branchEmployeeIds = await employeeQuery.Select(e => e.Id).ToListAsync(cancellationToken);
            dsrQuery = dsrQuery.Where(d => branchEmployeeIds.Contains(d.EmployeeId));
        }

        var actualDSRs = await dsrQuery.CountAsync(cancellationToken);

        return expectedDSRs > 0 ? (decimal)actualDSRs / expectedDSRs * 100 : 0;
    }
}