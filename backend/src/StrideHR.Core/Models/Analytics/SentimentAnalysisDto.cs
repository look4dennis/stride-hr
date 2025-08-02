namespace StrideHR.Core.Models.Analytics;

public class SentimentAnalysisRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<FeedbackSource> Sources { get; set; } = new();
    public bool IncludeAnonymousFeedback { get; set; } = true;
    public List<string>? Keywords { get; set; }
}

public class SentimentAnalysisResultDto
{
    public DateTime GeneratedAt { get; set; }
    public OverallSentimentScore OverallSentiment { get; set; } = new();
    public List<DepartmentSentiment> DepartmentSentiments { get; set; } = new();
    public List<TopicSentiment> TopicSentiments { get; set; } = new();
    public List<SentimentTrend> SentimentTrends { get; set; } = new();
    public List<FeedbackInsight> KeyInsights { get; set; } = new();
    public List<ActionableRecommendation> Recommendations { get; set; } = new();
    public SentimentComparison ComparisonData { get; set; } = new();
}

public class OverallSentimentScore
{
    public decimal PositivePercentage { get; set; }
    public decimal NeutralPercentage { get; set; }
    public decimal NegativePercentage { get; set; }
    public decimal AverageSentimentScore { get; set; }
    public SentimentCategory OverallCategory { get; set; }
    public int TotalFeedbackAnalyzed { get; set; }
    public decimal ConfidenceLevel { get; set; }
}

public class DepartmentSentiment
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal SentimentScore { get; set; }
    public SentimentCategory Category { get; set; }
    public int FeedbackCount { get; set; }
    public List<string> TopPositiveThemes { get; set; } = new();
    public List<string> TopNegativeThemes { get; set; } = new();
    public decimal TrendDirection { get; set; }
}

public class TopicSentiment
{
    public string Topic { get; set; } = string.Empty;
    public decimal SentimentScore { get; set; }
    public SentimentCategory Category { get; set; }
    public int MentionCount { get; set; }
    public List<string> KeyPhrases { get; set; } = new();
    public decimal ImportanceScore { get; set; }
    public List<string> SampleFeedback { get; set; } = new();
}

public class SentimentTrend
{
    public DateTime Date { get; set; }
    public decimal SentimentScore { get; set; }
    public int FeedbackVolume { get; set; }
    public List<string> DominantThemes { get; set; } = new();
    public List<string> EmergingIssues { get; set; } = new();
}

public class FeedbackInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public decimal Impact { get; set; }
    public int AffectedEmployees { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}

public class ActionableRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationType Type { get; set; }
    public int Priority { get; set; }
    public decimal ExpectedImpact { get; set; }
    public List<string> ActionSteps { get; set; } = new();
    public string ResponsibleRole { get; set; } = string.Empty;
    public DateTime SuggestedTimeline { get; set; }
}

public class SentimentComparison
{
    public decimal PreviousPeriodScore { get; set; }
    public decimal CurrentPeriodScore { get; set; }
    public decimal ChangePercentage { get; set; }
    public string TrendDescription { get; set; } = string.Empty;
    public List<string> KeyChanges { get; set; } = new();
}

public class TeamMoraleAnalysisRequest
{
    public int? TeamId { get; set; }
    public int? ManagerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludePerformanceData { get; set; } = true;
    public bool IncludeAttendanceData { get; set; } = true;
}

public class TeamMoraleInsightDto
{
    public DateTime GeneratedAt { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public decimal MoraleScore { get; set; }
    public MoraleLevel MoraleLevel { get; set; }
    public List<MoraleIndicator> Indicators { get; set; } = new();
    public List<TeamMemberMorale> MemberMorale { get; set; } = new();
    public List<MoraleDriver> PositiveDrivers { get; set; } = new();
    public List<MoraleDriver> NegativeDrivers { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public MoraleTrendData TrendData { get; set; } = new();
}

public class MoraleIndicator
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Benchmark { get; set; }
    public IndicatorStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TeamMemberMorale
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal MoraleScore { get; set; }
    public MoraleLevel Level { get; set; }
    public List<string> PositiveFactors { get; set; } = new();
    public List<string> ConcernAreas { get; set; } = new();
    public bool RequiresAttention { get; set; }
}

public class MoraleDriver
{
    public string Factor { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Evidence { get; set; } = new();
}

public class MoraleTrendData
{
    public List<MoraleTrendPoint> TrendPoints { get; set; } = new();
    public decimal TrendDirection { get; set; }
    public string TrendDescription { get; set; } = string.Empty;
}

public class MoraleTrendPoint
{
    public DateTime Date { get; set; }
    public decimal MoraleScore { get; set; }
    public List<string> KeyEvents { get; set; } = new();
}

public enum FeedbackSource
{
    PerformanceReviews,
    Surveys,
    ExitInterviews,
    OneOnOnes,
    Grievances,
    ChatbotConversations,
    DSRComments
}

public enum SentimentCategory
{
    VeryPositive,
    Positive,
    Neutral,
    Negative,
    VeryNegative
}

public enum InsightType
{
    Positive,
    Negative,
    Neutral,
    Trending,
    Emerging,
    Critical
}

public enum RecommendationType
{
    Immediate,
    ShortTerm,
    LongTerm,
    Strategic,
    Tactical
}

public enum MoraleLevel
{
    VeryLow,
    Low,
    Average,
    High,
    VeryHigh
}

public enum IndicatorStatus
{
    Excellent,
    Good,
    Average,
    BelowAverage,
    Poor
}