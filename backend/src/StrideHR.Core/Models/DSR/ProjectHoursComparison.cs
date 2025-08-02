namespace StrideHR.Core.Models.DSR;

public class ProjectHoursComparison
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectStatus { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal ActualHoursWorked { get; set; }
    public decimal HoursVariance { get; set; }
    public decimal VariancePercentage { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOverBudget { get; set; }
    public bool IsDelayed { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    public decimal ProjectedTotalHours { get; set; }
    public int TeamMembersCount { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}