using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AttendanceRecord entity
/// </summary>
public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
                    .ThenInclude(b => b.Organization)
            .Include(a => a.BreakRecords)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today && !a.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetTodayBranchRecordsAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.BreakRecords)
            .Where(a => a.Employee.BranchId == branchId && a.Date == today && !a.IsDeleted)
            .OrderBy(a => a.Employee.FirstName)
            .ThenBy(a => a.Employee.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.BreakRecords)
            .Where(a => a.EmployeeId == employeeId && 
                       a.Date >= fromDate && 
                       a.Date <= toDate && 
                       !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceRecord?> GetEmployeeAttendanceByDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.BreakRecords)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date.Date && !a.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetBranchAttendanceAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.BreakRecords)
            .Where(a => a.Employee.BranchId == branchId && 
                       a.Date >= fromDate && 
                       a.Date <= toDate && 
                       !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetBranchAttendanceByDateAsync(int branchId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.BreakRecords)
            .Where(a => a.Employee.BranchId == branchId && a.Date == date.Date && !a.IsDeleted)
            .OrderBy(a => a.Employee.FirstName)
            .ThenBy(a => a.Employee.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AttendanceRecord> Records, int TotalCount)> SearchAsync(AttendanceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.BreakRecords)
            .Where(a => !a.IsDeleted);

        // Apply filters
        if (criteria.EmployeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == criteria.EmployeeId.Value);
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(a => a.Employee.BranchId == criteria.BranchId.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Department))
        {
            query = query.Where(a => a.Employee.Department == criteria.Department);
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(a => a.Date >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(a => a.Date <= criteria.ToDate.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(a => a.Status == criteria.Status.Value);
        }

        if (criteria.IsLate.HasValue)
        {
            if (criteria.IsLate.Value)
            {
                query = query.Where(a => a.LateArrivalDuration > TimeSpan.Zero);
            }
            else
            {
                query = query.Where(a => a.LateArrivalDuration == null || a.LateArrivalDuration == TimeSpan.Zero);
            }
        }

        if (criteria.HasOvertime.HasValue)
        {
            if (criteria.HasOvertime.Value)
            {
                query = query.Where(a => a.OvertimeHours > TimeSpan.Zero);
            }
            else
            {
                query = query.Where(a => a.OvertimeHours == null || a.OvertimeHours == TimeSpan.Zero);
            }
        }

        if (criteria.IsManualEntry.HasValue)
        {
            query = query.Where(a => a.IsManualEntry == criteria.IsManualEntry.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrEmpty(criteria.SortBy))
        {
            query = criteria.SortBy.ToLower() switch
            {
                "date" => criteria.SortDescending 
                    ? query.OrderByDescending(a => a.Date)
                    : query.OrderBy(a => a.Date),
                "employee" => criteria.SortDescending 
                    ? query.OrderByDescending(a => a.Employee.FirstName).ThenByDescending(a => a.Employee.LastName)
                    : query.OrderBy(a => a.Employee.FirstName).ThenBy(a => a.Employee.LastName),
                "checkin" => criteria.SortDescending 
                    ? query.OrderByDescending(a => a.CheckInTime)
                    : query.OrderBy(a => a.CheckInTime),
                "checkout" => criteria.SortDescending 
                    ? query.OrderByDescending(a => a.CheckOutTime)
                    : query.OrderBy(a => a.CheckOutTime),
                "status" => criteria.SortDescending 
                    ? query.OrderByDescending(a => a.Status)
                    : query.OrderBy(a => a.Status),
                _ => query.OrderByDescending(a => a.Date).ThenBy(a => a.Employee.FirstName)
            };
        }
        else
        {
            query = query.OrderByDescending(a => a.Date).ThenBy(a => a.Employee.FirstName);
        }

        // Apply pagination
        var records = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (records, totalCount);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetByStatusAsync(AttendanceStatus status, int? branchId = null, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Status == status && !a.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(a => a.Employee.BranchId == branchId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(a => a.Date == date.Value.Date);
        }

        return await query
            .OrderBy(a => a.Employee.FirstName)
            .ThenBy(a => a.Employee.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetLateArrivalsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.BranchId == branchId && 
                       a.Date >= fromDate && 
                       a.Date <= toDate && 
                       a.LateArrivalDuration > TimeSpan.Zero && 
                       !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetOvertimeRecordsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
                    .ThenInclude(b => b.Organization)
            .Where(a => a.Employee.BranchId == branchId && 
                       a.Date >= fromDate && 
                       a.Date <= toDate && 
                       a.OvertimeHours > TimeSpan.Zero && 
                       !a.IsDeleted)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Employee.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceRecord?> GetWithBreaksAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.BreakRecords)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<AttendanceRecord?> GetWithCorrectionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.AttendanceCorrections)
                .ThenInclude(c => c.RequestedByEmployee)
            .Include(a => a.AttendanceCorrections)
                .ThenInclude(c => c.ApprovedByEmployee)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<AttendanceRecord?> GetWithAllDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Branch)
            .Include(a => a.BreakRecords)
            .Include(a => a.AttendanceCorrections)
                .ThenInclude(c => c.RequestedByEmployee)
            .Include(a => a.AttendanceCorrections)
                .ThenInclude(c => c.ApprovedByEmployee)
            .Include(a => a.Shift)
            .Include(a => a.ManualEntryByEmployee)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<int> GetPresentCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .CountAsync(a => a.Employee.BranchId == branchId && 
                            a.Date == date.Date && 
                            (a.Status == AttendanceStatus.Present || 
                             a.Status == AttendanceStatus.Late || 
                             a.Status == AttendanceStatus.OnBreak) && 
                            !a.IsDeleted, cancellationToken);
    }

    public async Task<int> GetAbsentCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default)
    {
        var totalEmployees = await _context.Employees
            .CountAsync(e => e.BranchId == branchId && e.Status == EmployeeStatus.Active && !e.IsDeleted, cancellationToken);
        
        var presentCount = await GetPresentCountAsync(branchId, date, cancellationToken);
        
        return totalEmployees - presentCount;
    }

    public async Task<int> GetLateCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceRecords
            .CountAsync(a => a.Employee.BranchId == branchId && 
                            a.Date == date.Date && 
                            a.Status == AttendanceStatus.Late && 
                            !a.IsDeleted, cancellationToken);
    }

    public async Task<int> GetOnBreakCountAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.AttendanceRecords
            .CountAsync(a => a.Employee.BranchId == branchId && 
                            a.Date == today && 
                            a.Status == AttendanceStatus.OnBreak && 
                            !a.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetManualEntriesAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.ManualEntryByEmployee)
            .Where(a => a.IsManualEntry && !a.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(a => a.Employee.BranchId == branchId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Date <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}