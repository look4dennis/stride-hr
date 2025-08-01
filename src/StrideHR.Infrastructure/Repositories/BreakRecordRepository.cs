using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for BreakRecord entity
/// </summary>
public class BreakRecordRepository : Repository<BreakRecord>, IBreakRecordRepository
{
    public BreakRecordRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BreakRecord>> GetByAttendanceRecordAsync(int attendanceRecordId, CancellationToken cancellationToken = default)
    {
        return await _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Include(b => b.ApprovedByEmployee)
            .Where(b => b.AttendanceRecordId == attendanceRecordId && !b.IsDeleted)
            .OrderBy(b => b.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<BreakRecord?> GetActiveBreakAsync(int attendanceRecordId, CancellationToken cancellationToken = default)
    {
        return await _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .FirstOrDefaultAsync(b => b.AttendanceRecordId == attendanceRecordId && 
                                     b.EndTime == null && 
                                     !b.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<BreakRecord>> GetActiveBreaksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Where(b => b.AttendanceRecord.EmployeeId == employeeId && 
                       b.AttendanceRecord.Date == today &&
                       b.EndTime == null && 
                       !b.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BreakRecord>> GetBreaksByTypeAsync(BreakType type, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Where(b => b.Type == type && !b.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Employee.BranchId == branchId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Date <= toDate.Value);
        }

        return await query
            .OrderBy(b => b.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BreakRecord>> GetExceededBreaksAsync(int? branchId = null, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Where(b => b.IsExceeding && !b.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Employee.BranchId == branchId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Date == date.Value.Date);
        }

        return await query
            .OrderByDescending(b => b.ExceededDuration)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BreakRecord>> GetPendingApprovalBreaksAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BreakRecords
            .Include(b => b.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Where(b => b.ApprovalStatus == BreakApprovalStatus.Pending && !b.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.AttendanceRecord.Employee.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(b => b.StartTime)
            .ToListAsync(cancellationToken);
    }
}