using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Report>> GetReportsByUserAsync(int userId, int? branchId = null)
    {
        var query = _context.Reports
            .Include(r => r.CreatedByEmployee)
            .Include(r => r.Branch)
            .Where(r => r.CreatedBy == userId);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value || r.BranchId == null);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetPublicReportsAsync(int? branchId = null)
    {
        var query = _context.Reports
            .Include(r => r.CreatedByEmployee)
            .Include(r => r.Branch)
            .Where(r => r.IsPublic && r.Status == ReportStatus.Active);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value || r.BranchId == null);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetSharedReportsAsync(int userId)
    {
        return await _context.Reports
            .Include(r => r.CreatedByEmployee)
            .Include(r => r.Branch)
            .Include(r => r.ReportShares)
            .Where(r => r.ReportShares.Any(rs => rs.SharedWith == userId && rs.IsActive && 
                (rs.ExpiresAt == null || rs.ExpiresAt > DateTime.UtcNow)))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetReportWithExecutionsAsync(int reportId)
    {
        return await _context.Reports
            .Include(r => r.CreatedByEmployee)
            .Include(r => r.Branch)
            .Include(r => r.ReportExecutions.OrderByDescending(re => re.ExecutedAt).Take(10))
            .ThenInclude(re => re.ExecutedByEmployee)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    public async Task<IEnumerable<Report>> GetScheduledReportsAsync()
    {
        return await _context.Reports
            .Include(r => r.ReportSchedules)
            .Where(r => r.IsScheduled && r.Status == ReportStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetReportsByTypeAsync(ReportType type, int? branchId = null)
    {
        var query = _context.Reports
            .Include(r => r.CreatedByEmployee)
            .Include(r => r.Branch)
            .Where(r => r.Type == type && r.Status == ReportStatus.Active);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value || r.BranchId == null);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(int reportId, int userId, ReportPermission permission)
    {
        var report = await _context.Reports
            .Include(r => r.ReportShares)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return false;

        // Owner has full permissions
        if (report.CreatedBy == userId)
            return true;

        // Public reports allow view and execute permissions
        if (report.IsPublic && (permission == ReportPermission.View || permission == ReportPermission.Execute))
            return true;

        // Check shared permissions
        var share = report.ReportShares.FirstOrDefault(rs => 
            rs.SharedWith == userId && 
            rs.IsActive && 
            (rs.ExpiresAt == null || rs.ExpiresAt > DateTime.UtcNow));

        if (share == null)
            return false;

        return permission <= share.Permission;
    }
}