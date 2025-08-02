using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PerformanceFeedback : BaseEntity
{
    public int PerformanceReviewId { get; set; }
    public int RevieweeId { get; set; } // Employee being reviewed
    public int ReviewerId { get; set; } // Employee providing feedback
    public FeedbackType FeedbackType { get; set; }
    public string CompetencyArea { get; set; } = string.Empty; // e.g., "Communication", "Technical Skills"
    public PerformanceRating Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? SpecificExamples { get; set; }
    public bool IsSubmitted { get; set; } = false;
    public DateTime? SubmittedDate { get; set; }
    
    // Navigation Properties
    public virtual PerformanceReview PerformanceReview { get; set; } = null!;
    public virtual Employee Reviewee { get; set; } = null!;
    public virtual Employee Reviewer { get; set; } = null!;
}