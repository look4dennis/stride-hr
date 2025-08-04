namespace StrideHR.Core.Models.Project;

public class ProjectAnalyticsDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public ProjectMetricsDto Metrics { get; set; } = new();
    public ProjectTrendsDto Trends { get; set; } = new();
    public ProjectPerformanceDto Performance { get; set; } = new();
    public List<ProjectRiskDto> Risks { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ProjectMetricsDto
{
    public decimal TotalHoursWorked { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal HoursVariance { get; set; }
    public decimal BudgetUtilized { get; set; }
    public decimal BudgetVariance { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TeamMembersCount { get; set; }
    public decimal AverageTaskCompletionTime { get; set; }
}

public class ProjectTrendsDto
{
    public List<DailyProgressDto> DailyProgress { get; set; } = new();
    public List<WeeklyHoursDto> WeeklyHours { get; set; } = new();
    public List<TeamMemberProductivityDto> TeamProductivity { get; set; } = new();
    public List<TaskStatusTrendDto> TaskStatusTrends { get; set; } = new();
}

public class DailyProgressDto
{
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public int TasksCompleted { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class WeeklyHoursDto
{
    public DateTime WeekStartDate { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Variance { get; set; }
}

public class TeamMemberProductivityDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public int TasksCompleted { get; set; }
    public decimal ProductivityScore { get; set; }
    public decimal EfficiencyRating { get; set; }
}

public class TaskStatusTrendDto
{
    public DateTime Date { get; set; }
    public int TodoTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}

public class ProjectPerformanceDto
{
    public decimal OverallEfficiency { get; set; }
    public decimal QualityScore { get; set; }
    public decimal TimelineAdherence { get; set; }
    public decimal BudgetAdherence { get; set; }
    public decimal TeamSatisfaction { get; set; }
    public string PerformanceGrade { get; set; } = string.Empty;
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

public class ProjectRiskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
    public string MitigationPlan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedTo { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime IdentifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ProjectDashboardDto
{
    public int TeamLeaderId { get; set; }
    public string TeamLeaderName { get; set; } = string.Empty;
    public List<ProjectAnalyticsDto> ProjectAnalytics { get; set; } = new();
    public TeamOverviewDto TeamOverview { get; set; } = new();
    public List<ProjectAlertDto> CriticalAlerts { get; set; } = new();
    public List<ProjectRiskDto> HighRisks { get; set; } = new();
}

public class TeamOverviewDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int DelayedProjects { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal BudgetUtilized { get; set; }
    public int TotalTeamMembers { get; set; }
    public decimal OverallProductivity { get; set; }
    public decimal AverageProjectHealth { get; set; }
}