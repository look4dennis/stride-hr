using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Survey;

public class SurveyAnalyticsDto
{
    public int SurveyId { get; set; }
    public string SurveyTitle { get; set; } = string.Empty;
    public int TotalDistributed { get; set; }
    public int TotalResponses { get; set; }
    public int CompletedResponses { get; set; }
    public double ResponseRate { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public DateTime? LastResponseAt { get; set; }
    
    // Sentiment Analysis
    public SentimentScore? OverallSentiment { get; set; }
    public double? SentimentConfidence { get; set; }
    public List<string> TopKeywords { get; set; } = new();
    public List<string> IdentifiedThemes { get; set; } = new();
    
    // Question Analytics
    public List<QuestionAnalyticsDto> QuestionAnalytics { get; set; } = new();
    
    // Demographic Breakdown
    public List<DemographicAnalyticsDto> DemographicBreakdown { get; set; } = new();
}

public class QuestionAnalyticsDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int TotalResponses { get; set; }
    public int SkippedResponses { get; set; }
    public double ResponseRate { get; set; }
    
    // For rating/numeric questions
    public double? AverageRating { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    
    // For choice questions
    public List<OptionAnalyticsDto> OptionAnalytics { get; set; } = new();
    
    // For text questions
    public SentimentScore? SentimentScore { get; set; }
    public List<string> CommonKeywords { get; set; } = new();
}

public class OptionAnalyticsDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int ResponseCount { get; set; }
    public double Percentage { get; set; }
}

public class DemographicAnalyticsDto
{
    public string Segment { get; set; } = string.Empty; // Department, Branch, Role, etc.
    public string SegmentValue { get; set; } = string.Empty;
    public int ResponseCount { get; set; }
    public double ResponseRate { get; set; }
    public SentimentScore? AverageSentiment { get; set; }
}