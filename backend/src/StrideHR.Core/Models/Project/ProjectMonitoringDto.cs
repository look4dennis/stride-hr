namespace StrideHR.Core.Models.Project;

public class ProjectMonitoringDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public ProjectProgressDto Progress { get; set; } = new();
    public ProjectVarianceDto Variance { get; set; } = new();
    public List<ProjectAlertDto> Alerts { get; set; } = new();
    public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class ProjectVarianceDto
{
    public decimal HoursVariance { get; set; }
    public decimal BudgetVariance { get; set; }
    public int ScheduleVarianceDays { get; set; }
    public decimal PerformanceIndex { get; set; }
    public bool IsOverBudget { get; set; }
    public bool IsBehindSchedule { get; set; }
    public string VarianceReason { get; set; } = string.Empty;
}

public class ProjectAlertDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public int? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class TeamLeaderDashboardDto
{
    public int TeamLeaderId { get; set; }
    public string TeamLeaderName { get; set; } = string.Empty;
    public List<ProjectMonitoringDto> Projects { get; set; } = new();
    public ProjectSummaryDto Summary { get; set; } = new();
    public List<ProjectAlertDto> CriticalAlerts { get; set; } = new();
}

public class ProjectSummaryDto
{
    public int TotalProjects { get; set; }
    public int OnTrackProjects { get; set; }
    public int DelayedProjects { get; set; }
    public int OverBudgetProjects { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal OverallEfficiency { get; set; }
    public int TotalTeamMembers { get; set; }
}