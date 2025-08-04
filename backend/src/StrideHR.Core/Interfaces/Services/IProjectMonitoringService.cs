using StrideHR.Core.Models.Project;

namespace StrideHR.Core.Interfaces.Services;

public interface IProjectMonitoringService
{
    // Project Hours Tracking Dashboard
    Task<ProjectHoursReportDto> GetProjectHoursTrackingAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ProjectHoursReportDto>> GetTeamHoursTrackingAsync(int teamLeaderId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ProjectDashboardDto> GetTeamLeaderDashboardAsync(int teamLeaderId);
    
    // Project Analytics and Reporting
    Task<ProjectAnalyticsDto> GetProjectAnalyticsAsync(int projectId);
    Task<List<ProjectAnalyticsDto>> GetTeamProjectAnalyticsAsync(int teamLeaderId);
    Task<ProjectPerformanceDto> GetProjectPerformanceAsync(int projectId);
    Task<List<ProjectTrendsDto>> GetProjectTrendsAsync(int projectId, int days = 30);
    
    // Project Alerts and Notifications
    Task<List<ProjectAlertDto>> GetProjectAlertsAsync(int projectId);
    Task<List<ProjectAlertDto>> GetTeamAlertsAsync(int teamLeaderId);
    Task<ProjectAlertDto> CreateProjectAlertAsync(int projectId, string alertType, string message, string severity);
    Task<bool> ResolveProjectAlertAsync(int alertId, int resolvedBy);
    Task<List<ProjectAlertDto>> GetCriticalAlertsAsync(int teamLeaderId);
    
    // Risk Management
    Task<List<ProjectRiskDto>> GetProjectRisksAsync(int projectId);
    Task<ProjectRiskDto> CreateProjectRiskAsync(int projectId, string riskType, string description, string severity, decimal probability, decimal impact);
    Task<bool> UpdateProjectRiskAsync(int riskId, string mitigationPlan, string status, int? assignedTo);
    Task<List<ProjectRiskDto>> GetHighRisksAsync(int teamLeaderId);
    
    // Automated Monitoring
    Task CheckProjectHealthAsync(int projectId);
    Task GenerateAutomaticAlertsAsync();
    Task<bool> IsProjectAtRiskAsync(int projectId);
    Task<decimal> CalculateProjectHealthScoreAsync(int projectId);
}