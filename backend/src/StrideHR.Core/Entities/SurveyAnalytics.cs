using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class SurveyAnalytics : BaseEntity
{
    public int SurveyId { get; set; }
    public int? QuestionId { get; set; }
    public string MetricType { get; set; } = string.Empty; // ResponseRate, CompletionRate, AverageRating, etc.
    public string MetricValue { get; set; } = string.Empty; // JSON data
    public DateTime CalculatedAt { get; set; }
    public string? Segment { get; set; } // Department, Branch, Role, etc.
    public string? SegmentValue { get; set; }
    public SentimentScore? SentimentScore { get; set; }
    public double? ConfidenceScore { get; set; }
    public string? Keywords { get; set; } // JSON array of extracted keywords
    public string? Themes { get; set; } // JSON array of identified themes

    // Navigation Properties
    public virtual Survey Survey { get; set; } = null!;
    public virtual SurveyQuestion? Question { get; set; }
}