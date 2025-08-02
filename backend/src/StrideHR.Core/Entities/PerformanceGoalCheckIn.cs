namespace StrideHR.Core.Entities;

public class PerformanceGoalCheckIn : BaseEntity
{
    public int PerformanceGoalId { get; set; }
    public int EmployeeId { get; set; }
    public int? ManagerId { get; set; }
    public DateTime CheckInDate { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string EmployeeComments { get; set; } = string.Empty;
    public string? ManagerComments { get; set; }
    public string? Challenges { get; set; }
    public string? SupportNeeded { get; set; }
    public DateTime? NextCheckInDate { get; set; }
    
    // Navigation Properties
    public virtual PerformanceGoal PerformanceGoal { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? Manager { get; set; }
}