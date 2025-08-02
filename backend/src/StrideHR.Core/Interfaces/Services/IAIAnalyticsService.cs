using StrideHR.Core.Models.Analytics;

namespace StrideHR.Core.Interfaces.Services;

/// <summary>
/// Service for AI-powered analytics and insights
/// </summary>
public interface IAIAnalyticsService
{
    // Predictive Analytics for Workforce Planning
    Task<WorkforcePredictionDto> PredictWorkforceNeedsAsync(WorkforcePredictionRequest request);
    Task<TurnoverPredictionDto> PredictEmployeeTurnoverAsync(TurnoverPredictionRequest request);
    Task<SkillGapAnalysisDto> AnalyzeSkillGapsAsync(SkillGapAnalysisRequest request);
    Task<RecruitmentForecastDto> ForecastRecruitmentNeedsAsync(RecruitmentForecastRequest request);
    
    // Sentiment Analysis for Employee Feedback
    Task<SentimentAnalysisResultDto> AnalyzeEmployeeFeedbackSentimentAsync(SentimentAnalysisRequest request);
    Task<TeamMoraleInsightDto> AnalyzeTeamMoraleAsync(TeamMoraleAnalysisRequest request);
    Task<EngagementTrendDto> AnalyzeEngagementTrendsAsync(EngagementTrendRequest request);
    Task<FeedbackCategoriesDto> CategorizeEmployeeFeedbackAsync(FeedbackCategorizationRequest request);
    
    // Performance Forecasting and Trend Analysis
    Task<PerformanceForecastDto> ForecastEmployeePerformanceAsync(PerformanceForecastRequest request);
    Task<PerformanceTrendDto> AnalyzePerformanceTrendsAsync(PerformanceTrendRequest request);
    Task<ProductivityInsightDto> AnalyzeProductivityPatternsAsync(ProductivityAnalysisRequest request);
    Task<PIPSuccessPredictionDto> PredictPIPSuccessAsync(PIPSuccessPredictionRequest request);
    
    // Automated Insights and Recommendations
    Task<List<AIInsightDto>> GenerateAutomatedInsightsAsync(InsightGenerationRequest request);
    Task<List<RecommendationDto>> GenerateRecommendationsAsync(RecommendationRequest request);
    Task<RiskAssessmentDto> AssessOrganizationalRisksAsync(RiskAssessmentRequest request);
    Task<OptimizationSuggestionDto> SuggestProcessOptimizationsAsync(OptimizationRequest request);
    
    // Analytics Dashboard Data
    Task<AIAnalyticsDashboardDto> GetAnalyticsDashboardDataAsync(DashboardDataRequest request);
    Task<List<TrendInsightDto>> GetTrendInsightsAsync(TrendInsightRequest request);
    Task<BenchmarkComparisonDto> GetBenchmarkComparisonAsync(BenchmarkRequest request);
}