using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PIPGoal : BaseEntity
{
    public int PIPId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MeasurableObjective { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public PerformanceGoalStatus Status { get; set; } = PerformanceGoalStatus.Active;
    public decimal ProgressPercentage { get; set; } = 0;
    public string? EmployeeComments { get; set; }
    public string? ManagerComments { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsAchieved { get; set; } = false;
    
    // Navigation Properties
    public virtual PerformanceImprovementPlan PIP { get; set; } = null!;
}