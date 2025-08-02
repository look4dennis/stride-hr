using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PIPReview : BaseEntity
{
    public int PIPId { get; set; }
    public int ReviewedBy { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ProgressSummary { get; set; } = string.Empty;
    public string EmployeeFeedback { get; set; } = string.Empty;
    public string ManagerFeedback { get; set; } = string.Empty;
    public string? ChallengesFaced { get; set; }
    public string? SupportProvided { get; set; }
    public string? NextSteps { get; set; }
    public PerformanceRating OverallProgress { get; set; }
    public bool IsOnTrack { get; set; } = true;
    public DateTime? NextReviewDate { get; set; }
    public string? RecommendedActions { get; set; }
    
    // Navigation Properties
    public virtual PerformanceImprovementPlan PIP { get; set; } = null!;
    public virtual Employee ReviewedByEmployee { get; set; } = null!;
}