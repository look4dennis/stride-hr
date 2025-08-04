using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ProjectAlertRepository : Repository<ProjectAlert>, IProjectAlertRepository
{
    public ProjectAlertRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectAlert>> GetAlertsByProjectAsync(int projectId)
    {
        return await _context.ProjectAlerts
            .Where(a => a.ProjectId == projectId && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectAlert>> GetAlertsByTeamLeadAsync(int teamLeadId)
    {
        return await _context.ProjectAlerts
            .Where(a => a.Project.ProjectAssignments.Any(pa => pa.EmployeeId == teamLeadId && pa.IsTeamLead) && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectAlert>> GetUnresolvedAlertsAsync()
    {
        return await _context.ProjectAlerts
            .Where(a => !a.IsResolved && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectAlert>> GetAlertsBySeverityAsync(AlertSeverity severity)
    {
        return await _context.ProjectAlerts
            .Where(a => a.Severity == severity && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectAlert>> GetAlertsByTypeAsync(ProjectAlertType alertType)
    {
        return await _context.ProjectAlerts
            .Where(a => a.AlertType == alertType && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ResolveAlertAsync(int alertId, int resolvedByEmployeeId, string? resolutionNotes = null)
    {
        var alert = await _context.ProjectAlerts.FindAsync(alertId);
        if (alert == null || alert.IsDeleted)
            return false;

        alert.IsResolved = true;
        alert.ResolvedByEmployeeId = resolvedByEmployeeId;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolutionNotes = resolutionNotes;
        alert.UpdatedAt = DateTime.UtcNow;
        alert.UpdatedBy = resolvedByEmployeeId.ToString();

        _context.ProjectAlerts.Update(alert);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<int> GetUnresolvedAlertCountAsync(int projectId)
    {
        return await _context.ProjectAlerts
            .CountAsync(a => a.ProjectId == projectId && !a.IsResolved && !a.IsDeleted);
    }

    public async Task<IEnumerable<ProjectAlert>> GetCriticalAlertsAsync()
    {
        return await _context.ProjectAlerts
            .Where(a => a.Severity == AlertSeverity.Critical && !a.IsResolved && !a.IsDeleted)
            .Include(a => a.Project)
            .Include(a => a.ResolvedByEmployee)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}