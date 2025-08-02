using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PerformanceGoal : BaseEntity
{
    public int EmployeeId { get; set; }
    public int? ManagerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime TargetDate { get; set; }
    public int WeightPercentage { get; set; } // Weight in overall performance (0-100)
    public PerformanceGoalStatus Status { get; set; } = PerformanceGoalStatus.Draft;
    public decimal ProgressPercentage { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime? CompletedDate { get; set; }
    public PerformanceRating? FinalRating { get; set; }
    public string? ManagerComments { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? Manager { get; set; }
    public virtual ICollection<PerformanceGoalCheckIn> CheckIns { get; set; } = new List<PerformanceGoalCheckIn>();
}