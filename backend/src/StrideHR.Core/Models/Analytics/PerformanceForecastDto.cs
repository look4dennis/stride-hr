namespace StrideHR.Core.Models.Analytics;

public class PerformanceForecastRequest
{
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ForecastMonths { get; set; } = 6;
    public bool IncludeTrainingImpact { get; set; } = true;
    public bool IncludePIPData { get; set; } = true;
}

public class PerformanceForecastDto
{
    public DateTime GeneratedAt { get; set; }
    public List<EmployeePerformanceForecast> EmployeeForecasts { get; set; } = new();
    public DepartmentPerformanceForecast DepartmentForecast { get; set; } = new();
    public List<PerformanceRiskAlert> RiskAlerts { get; set; } = new();
    public List<PerformanceOpportunity> Opportunities { get; set; } = new();
    public PerformanceBenchmark Benchmarks { get; set; } = new();
    public decimal OverallConfidenceScore { get; set; }
}

public class EmployeePerformanceForecast
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal CurrentPerformanceScore { get; set; }
    public List<PerformanceForecastPoint> ForecastPoints { get; set; } = new();
    public PerformanceTrend Trend { get; set; }
    public List<PerformanceInfluencer> KeyInfluencers { get; set; } = new();
    public List<string> RecommendedInterventions { get; set; } = new();
    public decimal ImprovementPotential { get; set; }
    public RiskLevel PerformanceRisk { get; set; }
}

public class PerformanceForecastPoint
{
    public DateTime Date { get; set; }
    public decimal PredictedScore { get; set; }
    public decimal MinRange { get; set; }
    public decimal MaxRange { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public List<string> InfluencingFactors { get; set; } = new();
}

public class DepartmentPerformanceForecast
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal CurrentAverageScore { get; set; }
    public decimal PredictedAverageScore { get; set; }
    public int HighPerformers { get; set; }
    public int AveragePerformers { get; set; }
    public int LowPerformers { get; set; }
    public List<PerformanceDistributionForecast> DistributionForecast { get; set; } = new();
    public List<string> DepartmentTrends { get; set; } = new();
}

public class PerformanceRiskAlert
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public RiskType RiskType { get; set; }
    public RiskLevel Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ProbabilityScore { get; set; }
    public DateTime PredictedDate { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
    public bool RequiresImmediateAttention { get; set; }
}

public class PerformanceOpportunity
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public OpportunityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal PotentialImpact { get; set; }
    public decimal SuccessProbability { get; set; }
    public List<string> RequiredActions { get; set; } = new();
    public DateTime RecommendedTimeline { get; set; }
}

public class PerformanceTrendRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string>? MetricTypes { get; set; }
    public bool IncludeComparisons { get; set; } = true;
}

public class PerformanceTrendDto
{
    public DateTime GeneratedAt { get; set; }
    public List<TrendAnalysis> TrendAnalyses { get; set; } = new();
    public List<PerformanceCorrelation> Correlations { get; set; } = new();
    public List<SeasonalPattern> SeasonalPatterns { get; set; } = new();
    public List<PerformanceAnomaly> Anomalies { get; set; } = new();
    public BenchmarkComparison BenchmarkComparison { get; set; } = new();
    public List<TrendInsight> Insights { get; set; } = new();
}

public class TrendAnalysis
{
    public string MetricName { get; set; } = string.Empty;
    public TrendDirection Direction { get; set; }
    public decimal TrendStrength { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public string TrendDescription { get; set; } = string.Empty;
    public decimal StatisticalSignificance { get; set; }
}

public class PerformanceCorrelation
{
    public string Metric1 { get; set; } = string.Empty;
    public string Metric2 { get; set; } = string.Empty;
    public decimal CorrelationCoefficient { get; set; }
    public CorrelationStrength Strength { get; set; }
    public string Interpretation { get; set; } = string.Empty;
    public List<string> PracticalImplications { get; set; } = new();
}

public class SeasonalPattern
{
    public string MetricName { get; set; } = string.Empty;
    public string PatternType { get; set; } = string.Empty;
    public List<SeasonalDataPoint> SeasonalData { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public decimal PatternStrength { get; set; }
}

public class PerformanceAnomaly
{
    public DateTime Date { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public decimal DeviationScore { get; set; }
    public AnomalyType Type { get; set; }
    public string PossibleCauses { get; set; } = string.Empty;
    public bool RequiresInvestigation { get; set; }
}

public class ProductivityAnalysisRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeProjectData { get; set; } = true;
    public bool IncludeAttendanceData { get; set; } = true;
}

public class ProductivityInsightDto
{
    public DateTime GeneratedAt { get; set; }
    public ProductivityOverview Overview { get; set; } = new();
    public List<ProductivityPattern> Patterns { get; set; } = new();
    public List<ProductivityDriver> TopDrivers { get; set; } = new();
    public List<ProductivityBottleneck> Bottlenecks { get; set; } = new();
    public List<ProductivityRecommendation> Recommendations { get; set; } = new();
    public ProductivityBenchmark Benchmarks { get; set; } = new();
    public List<ProductivityTrend> Trends { get; set; } = new();
}

public class ProductivityOverview
{
    public decimal AverageProductivityScore { get; set; }
    public decimal ProductivityGrowth { get; set; }
    public int HighProductivityEmployees { get; set; }
    public int LowProductivityEmployees { get; set; }
    public decimal EfficiencyRatio { get; set; }
    public TimeSpan AverageWorkingHours { get; set; }
    public decimal UtilizationRate { get; set; }
}

public class ProductivityPattern
{
    public string PatternName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Frequency { get; set; }
    public decimal Impact { get; set; }
    public List<string> AffectedEmployees { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class ProductivityDriver
{
    public string DriverName { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public DriverType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ActionableInsights { get; set; } = new();
}

public class ProductivityBottleneck
{
    public string BottleneckName { get; set; } = string.Empty;
    public decimal ImpactScore { get; set; }
    public int AffectedEmployees { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> SuggestedSolutions { get; set; } = new();
    public decimal EstimatedImprovementPotential { get; set; }
}

public class PIPSuccessPredictionRequest
{
    public int? PIPId { get; set; }
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public bool IncludeHistoricalData { get; set; } = true;
}

public class PIPSuccessPredictionDto
{
    public DateTime GeneratedAt { get; set; }
    public List<PIPSuccessPredictor> Predictions { get; set; } = new();
    public PIPSuccessFactors SuccessFactors { get; set; } = new();
    public List<PIPRiskFactor> RiskFactors { get; set; } = new();
    public List<PIPRecommendation> Recommendations { get; set; } = new();
    public PIPBenchmarkData BenchmarkData { get; set; } = new();
}

public class PIPSuccessPredictor
{
    public int PIPId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal SuccessProbability { get; set; }
    public SuccessLikelihood Likelihood { get; set; }
    public List<string> PositiveIndicators { get; set; } = new();
    public List<string> NegativeIndicators { get; set; } = new();
    public DateTime PredictedCompletionDate { get; set; }
    public List<string> CriticalMilestones { get; set; } = new();
}

// Supporting classes and enums
public class PerformanceInfluencer
{
    public string Name { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public InfluencerType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PerformanceDistributionForecast
{
    public DateTime Date { get; set; }
    public int HighPerformers { get; set; }
    public int AveragePerformers { get; set; }
    public int LowPerformers { get; set; }
}

public class PerformanceBenchmark
{
    public decimal IndustryAverage { get; set; }
    public decimal CompanyAverage { get; set; }
    public decimal DepartmentAverage { get; set; }
    public string BenchmarkSource { get; set; } = string.Empty;
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string Context { get; set; } = string.Empty;
}

public class SeasonalDataPoint
{
    public int Month { get; set; }
    public decimal AverageValue { get; set; }
    public decimal Variance { get; set; }
}

public class BenchmarkComparison
{
    public decimal CurrentScore { get; set; }
    public decimal IndustryBenchmark { get; set; }
    public decimal CompanyBenchmark { get; set; }
    public string ComparisonSummary { get; set; } = string.Empty;
}

public class TrendInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public decimal Confidence { get; set; }
    public List<string> ActionableItems { get; set; } = new();
}

public class ProductivityBenchmark
{
    public decimal IndustryAverage { get; set; }
    public decimal CompanyAverage { get; set; }
    public decimal TopPerformerAverage { get; set; }
    public string BenchmarkPeriod { get; set; } = string.Empty;
}

public class ProductivityTrend
{
    public DateTime Date { get; set; }
    public decimal ProductivityScore { get; set; }
    public TrendDirection Direction { get; set; }
    public List<string> KeyFactors { get; set; } = new();
}

public class ProductivityRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedImpact { get; set; }
    public ImplementationDifficulty Difficulty { get; set; }
    public List<string> ActionSteps { get; set; } = new();
}

public class PIPSuccessFactors
{
    public List<string> PositiveFactors { get; set; } = new();
    public List<string> NegativeFactors { get; set; } = new();
    public decimal OverallSuccessRate { get; set; }
}

public class PIPRiskFactor
{
    public string Factor { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PIPRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<string> ActionItems { get; set; } = new();
}

public class PIPBenchmarkData
{
    public decimal CompanySuccessRate { get; set; }
    public decimal IndustrySuccessRate { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
}

public enum PerformanceTrend
{
    Improving,
    Declining,
    Stable,
    Volatile
}

public enum RiskType
{
    PerformanceDecline,
    Burnout,
    Disengagement,
    SkillGap,
    Turnover
}

public enum OpportunityType
{
    SkillDevelopment,
    Leadership,
    Mentoring,
    ProjectLead,
    Promotion,
    CrossTraining
}

public enum CorrelationStrength
{
    VeryWeak,
    Weak,
    Moderate,
    Strong,
    VeryStrong
}

public enum AnomalyType
{
    Positive,
    Negative,
    Unusual,
    Outlier
}

public enum DriverType
{
    Positive,
    Negative,
    Neutral
}

public enum InfluencerType
{
    Internal,
    External,
    Behavioral,
    Environmental
}

public enum SuccessLikelihood
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}