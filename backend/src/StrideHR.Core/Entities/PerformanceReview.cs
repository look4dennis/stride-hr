using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PerformanceReview : BaseEntity
{
    public int EmployeeId { get; set; }
    public int? ManagerId { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty; // e.g., "Q1 2025", "Annual 2024"
    public DateTime ReviewStartDate { get; set; }
    public DateTime ReviewEndDate { get; set; }
    public DateTime DueDate { get; set; }
    public PerformanceReviewStatus Status { get; set; } = PerformanceReviewStatus.NotStarted;
    public PerformanceRating? OverallRating { get; set; }
    public decimal? OverallScore { get; set; }
    public string? EmployeeSelfAssessment { get; set; }
    public string? ManagerComments { get; set; }
    public string? DevelopmentPlan { get; set; }
    public string? StrengthsIdentified { get; set; }
    public string? AreasForImprovement { get; set; }
    public bool RequiresPIP { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ApprovedBy { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? Manager { get; set; }
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual ICollection<PerformanceFeedback> Feedbacks { get; set; } = new List<PerformanceFeedback>();
    public virtual ICollection<PerformanceGoal> Goals { get; set; } = new List<PerformanceGoal>();
}