namespace StrideHR.Core.Models.DSR;

public class ProjectHoursReport
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal ActualHoursWorked { get; set; }
    public decimal HoursVariance { get; set; }
    public decimal VariancePercentage { get; set; }
    public bool IsOnTrack { get; set; }
    public decimal RemainingHours { get; set; }
    public List<EmployeeHoursContribution> EmployeeContributions { get; set; } = new();
    public List<TaskHoursBreakdown> TaskBreakdown { get; set; } = new();
}

public class EmployeeHoursContribution
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public decimal ContributionPercentage { get; set; }
    public int DSRsSubmitted { get; set; }
}

public class TaskHoursBreakdown
{
    public int? TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal Variance { get; set; }
    public string Status { get; set; } = string.Empty;
}