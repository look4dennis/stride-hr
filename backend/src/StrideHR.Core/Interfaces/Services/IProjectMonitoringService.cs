using StrideHR.Core.Models.Project;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IProjectMonitoringService
{
    // Real-time Project Progress Tracking
    Task<ProjectMonitoringDto> GetProjectMonitoringDataAsync(int projectId);
    Task<List<ProjectMonitoringDto>> GetProjectsMonitoringDataAsync(List<int> projectIds);
    Task<ProjectVarianceDto> CalculateProjectVarianceAsync(int projectId);
    
    // Team Leader Dashboard
    Task<TeamLeaderDashboardDto> GetTeamLeaderDashboardAsync(int teamLeaderId);
    Task<List<ProjectMonitoringDto>> GetProjectsForTeamLeaderAsync(int teamLeaderId);
    
    // Project Hours Analysis and Variance Reporting
    Task<List<ProjectHoursReportDto>> GetProjectHoursAnalysisAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ProjectHoursReportDto>> GetTeamHoursAnalysisAsync(int teamLeaderId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ProjectVarianceDto> GetProjectVarianceReportAsync(int projectId);
    
    // Automated Alerts for Project Delays
    Task<List<ProjectAlertDto>> GetProjectAlertsAsync(int projectId);
    Task<List<ProjectAlertDto>> GetTeamLeaderAlertsAsync(int teamLeaderId);
    Task<ProjectAlertDto> CreateProjectAlertAsync(int projectId, ProjectAlertType alertType, string message, AlertSeverity severity);
    Task<bool> ResolveProjectAlertAsync(int alertId, int resolvedByEmployeeId, string? resolutionNotes = null);
    Task CheckAndCreateAutomatedAlertsAsync(int projectId);
    Task CheckAndCreateAutomatedAlertsForAllProjectsAsync();
    
    // Performance Metrics
    Task<decimal> CalculateProjectEfficiencyAsync(int projectId);
    Task<decimal> CalculateTeamEfficiencyAsync(int teamLeaderId);
    Task<bool> IsProjectAtRiskAsync(int projectId);
    Task<List<int>> GetAtRiskProjectsAsync(int teamLeaderId);
}