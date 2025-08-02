using StrideHR.Core.Enums;

namespace StrideHR.Core.Models;

public class PerformanceFeedbackDto
{
    public int Id { get; set; }
    public int PerformanceReviewId { get; set; }
    public int RevieweeId { get; set; }
    public string RevieweeName { get; set; } = string.Empty;
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public FeedbackType FeedbackType { get; set; }
    public string CompetencyArea { get; set; } = string.Empty;
    public PerformanceRating Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? SpecificExamples { get; set; }
    public bool IsSubmitted { get; set; }
    public DateTime? SubmittedDate { get; set; }
}

public class CreatePerformanceFeedbackDto
{
    public int PerformanceReviewId { get; set; }
    public int RevieweeId { get; set; }
    public FeedbackType FeedbackType { get; set; }
    public string CompetencyArea { get; set; } = string.Empty;
    public PerformanceRating Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public string? SpecificExamples { get; set; }
}

public class FeedbackRequestDto
{
    public int ReviewId { get; set; }
    public int ReviewerId { get; set; }
    public FeedbackType FeedbackType { get; set; }
    public string CompetencyArea { get; set; } = string.Empty;
}