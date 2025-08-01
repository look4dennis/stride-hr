using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AttendanceCorrection entity
/// </summary>
public class AttendanceCorrectionRepository : Repository<AttendanceCorrection>, IAttendanceCorrectionRepository
{
    public AttendanceCorrectionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetByAttendanceRecordAsync(int attendanceRecordId, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceCorrections
            .Include(c => c.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Include(c => c.RequestedByEmployee)
            .Include(c => c.ApprovedByEmployee)
            .Where(c => c.AttendanceRecordId == attendanceRecordId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetByStatusAsync(CorrectionStatus status, int? branchId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AttendanceCorrections
            .Include(c => c.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Include(c => c.RequestedByEmployee)
            .Include(c => c.ApprovedByEmployee)
            .Where(c => c.Status == status && !c.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(c => c.AttendanceRecord.Employee.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetPendingCorrectionsAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(CorrectionStatus.Pending, branchId, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetByRequestedByAsync(int requestedBy, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceCorrections
            .Include(c => c.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Include(c => c.RequestedByEmployee)
            .Include(c => c.ApprovedByEmployee)
            .Where(c => c.RequestedBy == requestedBy && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetByApprovedByAsync(int approvedBy, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceCorrections
            .Include(c => c.AttendanceRecord)
                .ThenInclude(a => a.Employee)
            .Include(c => c.RequestedByEmployee)
            .Include(c => c.ApprovedByEmployee)
            .Where(c => c.ApprovedBy == approvedBy && !c.IsDeleted)
            .OrderByDescending(c => c.ApprovedAt)
            .ToListAsync(cancellationToken);
    }
}