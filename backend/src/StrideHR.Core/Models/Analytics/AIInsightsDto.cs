using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Analytics;

public class InsightGenerationRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<InsightCategory> Categories { get; set; } = new();
    public int MaxInsights { get; set; } = 10;
    public decimal MinConfidenceThreshold { get; set; } = 0.7m;
}

public class AIInsightDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightCategory Category { get; set; }
    public InsightType Type { get; set; }
    public decimal ConfidenceScore { get; set; }
    public InsightPriority Priority { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public List<string> SupportingData { get; set; } = new();
    public List<ActionableRecommendation> Recommendations { get; set; } = new();
    public InsightImpact Impact { get; set; } = new();
    public List<string> AffectedAreas { get; set; } = new();
    public bool RequiresAction { get; set; }
    public DateTime? ActionDeadline { get; set; }
}

public class InsightImpact
{
    public decimal FinancialImpact { get; set; }
    public int AffectedEmployees { get; set; }
    public ImpactLevel Level { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Metrics { get; set; } = new();
}

public class RecommendationRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public List<RecommendationArea> Areas { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeCostBenefit { get; set; } = true;
    public int MaxRecommendations { get; set; } = 15;
}

public class RecommendationDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationArea Area { get; set; }
    public RecommendationType Type { get; set; }
    public int Priority { get; set; }
    public decimal ExpectedImpact { get; set; }
    public ImplementationDifficulty Difficulty { get; set; }
    public List<string> ActionSteps { get; set; } = new();
    public CostBenefitAnalysis CostBenefit { get; set; } = new();
    public Timeline Timeline { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public List<string> RisksAndMitigation { get; set; } = new();
    public List<string> SuccessMetrics { get; set; } = new();
    public string ResponsibleRole { get; set; } = string.Empty;
    public List<string> Stakeholders { get; set; } = new();
    public decimal ConfidenceLevel { get; set; }
}

public class CostBenefitAnalysis
{
    public decimal EstimatedCost { get; set; }
    public decimal EstimatedBenefit { get; set; }
    public decimal ROI { get; set; }
    public TimeSpan PaybackPeriod { get; set; }
    public List<CostBreakdown> CostBreakdown { get; set; } = new();
    public List<BenefitBreakdown> BenefitBreakdown { get; set; } = new();
}

public class CostBreakdown
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class BenefitBreakdown
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsQuantifiable { get; set; }
}

public class Timeline
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
}

public class Milestone
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Deliverables { get; set; } = new();
}

public class RiskAssessmentRequest
{
    public int? BranchId { get; set; }
    public List<RiskCategory> Categories { get; set; } = new();
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
    public bool IncludeMitigationStrategies { get; set; } = true;
    public RiskAssessmentScope Scope { get; set; }
}

public class RiskAssessmentDto
{
    public DateTime GeneratedAt { get; set; }
    public decimal OverallRiskScore { get; set; }
    public RiskLevel OverallRiskLevel { get; set; }
    public List<OrganizationalRisk> Risks { get; set; } = new();
    public List<RiskTrend> RiskTrends { get; set; } = new();
    public RiskMatrix RiskMatrix { get; set; } = new();
    public List<MitigationStrategy> MitigationStrategies { get; set; } = new();
    public List<string> KeyRecommendations { get; set; } = new();
    public RiskComparison HistoricalComparison { get; set; } = new();
}

public class OrganizationalRisk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskCategory Category { get; set; }
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
    public decimal RiskScore { get; set; }
    public RiskLevel Level { get; set; }
    public List<string> Indicators { get; set; } = new();
    public List<string> PotentialConsequences { get; set; } = new();
    public List<string> CurrentControls { get; set; } = new();
    public decimal ResidualRisk { get; set; }
    public bool RequiresImmediateAction { get; set; }
    public DateTime? LastReviewed { get; set; }
}

public class RiskTrend
{
    public RiskCategory Category { get; set; }
    public TrendDirection Direction { get; set; }
    public decimal TrendStrength { get; set; }
    public List<RiskTrendPoint> TrendPoints { get; set; } = new();
    public string TrendDescription { get; set; } = string.Empty;
}

public class RiskTrendPoint
{
    public DateTime Date { get; set; }
    public decimal RiskScore { get; set; }
    public List<string> KeyEvents { get; set; } = new();
}

public class RiskMatrix
{
    public List<RiskMatrixCell> Cells { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class RiskMatrixCell
{
    public decimal ProbabilityRange { get; set; }
    public decimal ImpactRange { get; set; }
    public RiskLevel Level { get; set; }
    public List<string> RiskIds { get; set; } = new();
}

public class MitigationStrategy
{
    public string RiskId { get; set; } = string.Empty;
    public string StrategyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MitigationType Type { get; set; }
    public decimal EffectivenessScore { get; set; }
    public decimal ImplementationCost { get; set; }
    public List<string> ActionItems { get; set; } = new();
    public string ResponsibleParty { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public MitigationStatus Status { get; set; }
}

public class RiskComparison
{
    public decimal PreviousOverallScore { get; set; }
    public decimal CurrentOverallScore { get; set; }
    public decimal ChangePercentage { get; set; }
    public List<string> NewRisks { get; set; } = new();
    public List<string> ResolvedRisks { get; set; } = new();
    public List<string> EscalatedRisks { get; set; } = new();
}

public class OptimizationRequest
{
    public int? BranchId { get; set; }
    public List<OptimizationArea> Areas { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeAutomationOpportunities { get; set; } = true;
    public decimal MinImpactThreshold { get; set; } = 0.1m;
}

public class OptimizationSuggestionDto
{
    public DateTime GeneratedAt { get; set; }
    public List<ProcessOptimization> Optimizations { get; set; } = new();
    public List<AutomationOpportunity> AutomationOpportunities { get; set; } = new();
    public List<EfficiencyGain> EfficiencyGains { get; set; } = new();
    public OptimizationSummary Summary { get; set; } = new();
    public List<string> QuickWins { get; set; } = new();
    public List<string> LongTermInitiatives { get; set; } = new();
}

public class ProcessOptimization
{
    public string ProcessName { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string ProposedState { get; set; } = string.Empty;
    public decimal ExpectedImprovement { get; set; }
    public List<string> Benefits { get; set; } = new();
    public List<string> ImplementationSteps { get; set; } = new();
    public decimal ImplementationCost { get; set; }
    public TimeSpan ImplementationTime { get; set; }
    public List<string> RequiredResources { get; set; } = new();
    public decimal ROI { get; set; }
}

public class AutomationOpportunity
{
    public string ProcessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AutomationType Type { get; set; }
    public decimal AutomationPotential { get; set; }
    public decimal TimeSavings { get; set; }
    public decimal CostSavings { get; set; }
    public List<string> TechnicalRequirements { get; set; } = new();
    public ImplementationDifficulty Complexity { get; set; }
    public List<string> RisksAndChallenges { get; set; } = new();
}

public class EfficiencyGain
{
    public string Area { get; set; } = string.Empty;
    public decimal CurrentEfficiency { get; set; }
    public decimal PotentialEfficiency { get; set; }
    public decimal ImprovementPercentage { get; set; }
    public List<string> ImprovementMethods { get; set; } = new();
    public decimal EstimatedSavings { get; set; }
}

public class OptimizationSummary
{
    public decimal TotalPotentialSavings { get; set; }
    public decimal TotalImplementationCost { get; set; }
    public decimal OverallROI { get; set; }
    public int TotalOptimizations { get; set; }
    public int HighImpactOptimizations { get; set; }
    public TimeSpan AverageImplementationTime { get; set; }
}

// Dashboard and Trend Analysis DTOs
public class DashboardDataRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DashboardWidget> RequestedWidgets { get; set; } = new();
    public bool IncludeRealTimeData { get; set; } = true;
}

public class AIAnalyticsDashboardDto
{
    public DateTime GeneratedAt { get; set; }
    public List<DashboardInsight> Insights { get; set; } = new();
    public List<KPIWidget> KPIWidgets { get; set; } = new();
    public List<ChartWidget> Charts { get; set; } = new();
    public List<AlertWidget> Alerts { get; set; } = new();
    public DashboardSummary Summary { get; set; } = new();
}

public class DashboardInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public decimal Value { get; set; }
    public string Trend { get; set; } = string.Empty;
    public List<string> ActionItems { get; set; } = new();
}

public class KPIWidget
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Target { get; set; }
    public decimal PreviousValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public TrendDirection Trend { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ChartWidget
{
    public string Title { get; set; } = string.Empty;
    public ChartType Type { get; set; }
    public List<ChartDataPoint> DataPoints { get; set; } = new();
    public string XAxisLabel { get; set; } = string.Empty;
    public string YAxisLabel { get; set; } = string.Empty;
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime? Date { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class AlertWidget
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool RequiresAction { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
}

public class DashboardSummary
{
    public int TotalInsights { get; set; }
    public int CriticalAlerts { get; set; }
    public int ActiveRecommendations { get; set; }
    public decimal OverallHealthScore { get; set; }
    public List<string> KeyHighlights { get; set; } = new();
}

// Supporting Enums
public enum InsightCategory
{
    Performance,
    Productivity,
    Engagement,
    Turnover,
    Recruitment,
    Training,
    Compliance,
    Financial,
    Operational
}

public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ImpactLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Severe
}

public enum RecommendationArea
{
    HumanResources,
    Performance,
    Training,
    Recruitment,
    Retention,
    Productivity,
    ProcessImprovement,
    Technology,
    Compliance,
    Culture
}

public enum RiskCategory
{
    Operational,
    Financial,
    Compliance,
    Strategic,
    Reputational,
    Technology,
    Human,
    Legal
}

public enum RiskAssessmentScope
{
    Organization,
    Branch,
    Department,
    Process,
    Project
}

public enum MitigationType
{
    Avoid,
    Mitigate,
    Transfer,
    Accept
}

public enum MitigationStatus
{
    Planned,
    InProgress,
    Completed,
    OnHold,
    Cancelled
}

public enum OptimizationArea
{
    Recruitment,
    Onboarding,
    Performance,
    Training,
    Payroll,
    Attendance,
    Leave,
    Communication,
    Reporting,
    Compliance
}

public enum AutomationType
{
    FullAutomation,
    PartialAutomation,
    ProcessImprovement,
    DigitalTransformation
}

public enum DashboardWidget
{
    KPIs,
    Trends,
    Alerts,
    Insights,
    Recommendations,
    Charts,
    Metrics
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

// Additional supporting classes
public class TrendInsightRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> MetricTypes { get; set; } = new();
    public int MaxInsights { get; set; } = 5;
}

public class TrendInsightDto
{
    public string MetricName { get; set; } = string.Empty;
    public TrendDirection Direction { get; set; }
    public decimal TrendStrength { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> KeyFactors { get; set; } = new();
    public List<string> Implications { get; set; } = new();
    public decimal ConfidenceLevel { get; set; }
}

public class BenchmarkRequest
{
    public int? BranchId { get; set; }
    public List<string> Metrics { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeIndustryBenchmarks { get; set; } = true;
}

public class BenchmarkComparisonDto
{
    public DateTime GeneratedAt { get; set; }
    public List<MetricBenchmark> MetricBenchmarks { get; set; } = new();
    public BenchmarkSummary Summary { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class MetricBenchmark
{
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal IndustryBenchmark { get; set; }
    public decimal CompanyBenchmark { get; set; }
    public decimal PerformanceGap { get; set; }
    public BenchmarkStatus Status { get; set; }
    public string Analysis { get; set; } = string.Empty;
}

public class BenchmarkSummary
{
    public int MetricsAboveBenchmark { get; set; }
    public int MetricsBelowBenchmark { get; set; }
    public decimal OverallPerformanceScore { get; set; }
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
}

public enum BenchmarkStatus
{
    ExceedsExpectations,
    MeetsExpectations,
    BelowExpectations,
    NeedsImprovement
}

// Additional DTOs for skill gap analysis and recruitment forecasting
public class SkillGapAnalysisRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public List<string>? SkillCategories { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public bool IncludeFutureNeeds { get; set; } = true;
}

public class SkillGapAnalysisDto
{
    public DateTime GeneratedAt { get; set; }
    public List<SkillGap> SkillGaps { get; set; } = new();
    public List<SkillSurplus> SkillSurpluses { get; set; } = new();
    public SkillGapSummary Summary { get; set; } = new();
    public List<SkillDevelopmentRecommendation> Recommendations { get; set; } = new();
    public List<RecruitmentNeed> RecruitmentNeeds { get; set; } = new();
}

public class SkillGap
{
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int RequiredCount { get; set; }
    public int CurrentCount { get; set; }
    public int GapSize { get; set; }
    public decimal CriticalityScore { get; set; }
    public List<string> AffectedProjects { get; set; } = new();
    public List<string> MitigationOptions { get; set; } = new();
}

public class SkillSurplus
{
    public string SkillName { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int RequiredCount { get; set; }
    public int SurplusCount { get; set; }
    public List<string> RedeploymentOptions { get; set; } = new();
}

public class SkillGapSummary
{
    public int TotalGaps { get; set; }
    public int CriticalGaps { get; set; }
    public int TotalSurpluses { get; set; }
    public decimal OverallSkillHealthScore { get; set; }
}

public class SkillDevelopmentRecommendation
{
    public string SkillName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public List<string> TrainingOptions { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public int Priority { get; set; }
}

public class RecruitmentNeed
{
    public string Position { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public int RequiredCount { get; set; }
    public decimal UrgencyScore { get; set; }
    public DateTime RecommendedStartDate { get; set; }
}

public class RecruitmentForecastRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeTurnoverPredictions { get; set; } = true;
    public bool IncludeGrowthPlans { get; set; } = true;
}

public class RecruitmentForecastDto
{
    public DateTime GeneratedAt { get; set; }
    public List<RecruitmentNeedForecast> Forecasts { get; set; } = new();
    public RecruitmentSummary Summary { get; set; } = new();
    public List<RecruitmentChallenge> Challenges { get; set; } = new();
    public List<RecruitmentStrategy> Strategies { get; set; } = new();
}

public class RecruitmentNeedForecast
{
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int ForecastedNeed { get; set; }
    public DateTime TimeFrame { get; set; }
    public List<string> Drivers { get; set; } = new();
    public decimal ConfidenceLevel { get; set; }
}

public class RecruitmentSummary
{
    public int TotalForecastedHires { get; set; }
    public decimal EstimatedRecruitmentCost { get; set; }
    public TimeSpan AverageTimeToHire { get; set; }
    public List<string> HighDemandSkills { get; set; } = new();
}

public class RecruitmentChallenge
{
    public string Challenge { get; set; } = string.Empty;
    public decimal ImpactScore { get; set; }
    public List<string> MitigationStrategies { get; set; } = new();
}

public class RecruitmentStrategy
{
    public string Strategy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EffectivenessScore { get; set; }
    public List<string> ActionItems { get; set; } = new();
}

public class EngagementTrendRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<EngagementMetric> Metrics { get; set; } = new();
}

public class EngagementTrendDto
{
    public DateTime GeneratedAt { get; set; }
    public List<EngagementTrendPoint> TrendPoints { get; set; } = new();
    public EngagementSummary Summary { get; set; } = new();
    public List<EngagementDriver> TopDrivers { get; set; } = new();
    public List<EngagementRisk> Risks { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class EngagementTrendPoint
{
    public DateTime Date { get; set; }
    public decimal EngagementScore { get; set; }
    public List<string> KeyEvents { get; set; } = new();
    public Dictionary<string, decimal> MetricBreakdown { get; set; } = new();
}

public class EngagementSummary
{
    public decimal CurrentEngagementScore { get; set; }
    public decimal TrendDirection { get; set; }
    public int HighlyEngagedEmployees { get; set; }
    public int DisengagedEmployees { get; set; }
    public decimal EngagementVolatility { get; set; }
}

public class EngagementDriver
{
    public string Driver { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public DriverType Type { get; set; }
    public List<string> ActionableInsights { get; set; } = new();
}

public class EngagementRisk
{
    public string Risk { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
    public List<string> Indicators { get; set; } = new();
    public List<string> MitigationActions { get; set; } = new();
}

public class FeedbackCategorizationRequest
{
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<FeedbackSource> Sources { get; set; } = new();
    public bool IncludeAnonymous { get; set; } = true;
}

public class FeedbackCategoriesDto
{
    public DateTime GeneratedAt { get; set; }
    public List<FeedbackCategory> Categories { get; set; } = new();
    public List<FeedbackTheme> Themes { get; set; } = new();
    public FeedbackSummary Summary { get; set; } = new();
    public List<string> ActionPriorities { get; set; } = new();
}

public class FeedbackCategory
{
    public string Name { get; set; } = string.Empty;
    public int FeedbackCount { get; set; }
    public decimal SentimentScore { get; set; }
    public List<string> KeyTopics { get; set; } = new();
    public List<string> SampleFeedback { get; set; } = new();
    public decimal UrgencyScore { get; set; }
}

public class FeedbackTheme
{
    public string Theme { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public decimal SentimentScore { get; set; }
    public List<string> RelatedCategories { get; set; } = new();
    public TrendDirection Trend { get; set; }
}

public class FeedbackSummary
{
    public int TotalFeedback { get; set; }
    public int CategorizedFeedback { get; set; }
    public decimal OverallSentiment { get; set; }
    public List<string> TopConcerns { get; set; } = new();
    public List<string> TopPositives { get; set; } = new();
}

public enum EngagementMetric
{
    Satisfaction,
    Motivation,
    Commitment,
    Advocacy,
    Retention,
    Participation
}