using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IProjectAlertRepository : IRepository<ProjectAlert>
{
    Task<IEnumerable<ProjectAlert>> GetAlertsByProjectAsync(int projectId);
    Task<IEnumerable<ProjectAlert>> GetAlertsByTeamLeadAsync(int teamLeadId);
    Task<IEnumerable<ProjectAlert>> GetUnresolvedAlertsAsync();
    Task<IEnumerable<ProjectAlert>> GetAlertsBySeverityAsync(AlertSeverity severity);
    Task<IEnumerable<ProjectAlert>> GetAlertsByTypeAsync(ProjectAlertType alertType);
    Task<bool> ResolveAlertAsync(int alertId, int resolvedByEmployeeId, string? resolutionNotes = null);
    Task<int> GetUnresolvedAlertCountAsync(int projectId);
}