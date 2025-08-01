namespace StrideHR.Core.Models.Project;

public class ProjectProgressDto
{
    public int ProjectId { get; set; }
    public int TotalEstimatedHours { get; set; }
    public decimal ActualHoursWorked { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOnTrack { get; set; }
    public decimal RemainingHours { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TodoTasks { get; set; }
    public decimal BudgetUtilization { get; set; }
    public bool IsOverBudget { get; set; }
}