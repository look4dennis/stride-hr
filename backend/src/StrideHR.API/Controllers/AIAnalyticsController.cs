using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Analytics;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIAnalyticsController : ControllerBase
{
    private readonly IAIAnalyticsService _aiAnalyticsService;
    private readonly ILogger<AIAnalyticsController> _logger;

    public AIAnalyticsController(
        IAIAnalyticsService aiAnalyticsService,
        ILogger<AIAnalyticsController> logger)
    {
        _aiAnalyticsService = aiAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Predict workforce needs for planning purposes
    /// </summary>
    [HttpPost("workforce-prediction")]
    public async Task<ActionResult<WorkforcePredictionDto>> PredictWorkforceNeeds([FromBody] WorkforcePredictionRequest request)
    {
        try
        {
            var prediction = await _aiAnalyticsService.PredictWorkforceNeedsAsync(request);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting workforce needs");
            return StatusCode(500, "An error occurred while predicting workforce needs");
        }
    }

    /// <summary>
    /// Predict employee turnover risk
    /// </summary>
    [HttpPost("turnover-prediction")]
    public async Task<ActionResult<TurnoverPredictionDto>> PredictEmployeeTurnover([FromBody] TurnoverPredictionRequest request)
    {
        try
        {
            var prediction = await _aiAnalyticsService.PredictEmployeeTurnoverAsync(request);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting employee turnover");
            return StatusCode(500, "An error occurred while predicting employee turnover");
        }
    }

    /// <summary>
    /// Analyze skill gaps in the organization
    /// </summary>
    [HttpPost("skill-gap-analysis")]
    public async Task<ActionResult<SkillGapAnalysisDto>> AnalyzeSkillGaps([FromBody] SkillGapAnalysisRequest request)
    {
        try
        {
            var analysis = await _aiAnalyticsService.AnalyzeSkillGapsAsync(request);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing skill gaps");
            return StatusCode(500, "An error occurred while analyzing skill gaps");
        }
    }

    /// <summary>
    /// Forecast recruitment needs
    /// </summary>
    [HttpPost("recruitment-forecast")]
    public async Task<ActionResult<RecruitmentForecastDto>> ForecastRecruitmentNeeds([FromBody] RecruitmentForecastRequest request)
    {
        try
        {
            var forecast = await _aiAnalyticsService.ForecastRecruitmentNeedsAsync(request);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting recruitment needs");
            return StatusCode(500, "An error occurred while forecasting recruitment needs");
        }
    }

    /// <summary>
    /// Analyze employee feedback sentiment
    /// </summary>
    [HttpPost("sentiment-analysis")]
    public async Task<ActionResult<SentimentAnalysisResultDto>> AnalyzeEmployeeFeedbackSentiment([FromBody] SentimentAnalysisRequest request)
    {
        try
        {
            var analysis = await _aiAnalyticsService.AnalyzeEmployeeFeedbackSentimentAsync(request);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing employee feedback sentiment");
            return StatusCode(500, "An error occurred while analyzing employee feedback sentiment");
        }
    }

    /// <summary>
    /// Analyze team morale
    /// </summary>
    [HttpPost("team-morale-analysis")]
    public async Task<ActionResult<TeamMoraleInsightDto>> AnalyzeTeamMorale([FromBody] TeamMoraleAnalysisRequest request)
    {
        try
        {
            var analysis = await _aiAnalyticsService.AnalyzeTeamMoraleAsync(request);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing team morale");
            return StatusCode(500, "An error occurred while analyzing team morale");
        }
    }

    /// <summary>
    /// Analyze engagement trends
    /// </summary>
    [HttpPost("engagement-trends")]
    public async Task<ActionResult<EngagementTrendDto>> AnalyzeEngagementTrends([FromBody] EngagementTrendRequest request)
    {
        try
        {
            var trends = await _aiAnalyticsService.AnalyzeEngagementTrendsAsync(request);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing engagement trends");
            return StatusCode(500, "An error occurred while analyzing engagement trends");
        }
    }

    /// <summary>
    /// Categorize employee feedback
    /// </summary>
    [HttpPost("feedback-categorization")]
    public async Task<ActionResult<FeedbackCategoriesDto>> CategorizeEmployeeFeedback([FromBody] FeedbackCategorizationRequest request)
    {
        try
        {
            var categories = await _aiAnalyticsService.CategorizeEmployeeFeedbackAsync(request);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing employee feedback");
            return StatusCode(500, "An error occurred while categorizing employee feedback");
        }
    }

    /// <summary>
    /// Forecast employee performance
    /// </summary>
    [HttpPost("performance-forecast")]
    public async Task<ActionResult<PerformanceForecastDto>> ForecastEmployeePerformance([FromBody] PerformanceForecastRequest request)
    {
        try
        {
            var forecast = await _aiAnalyticsService.ForecastEmployeePerformanceAsync(request);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting employee performance");
            return StatusCode(500, "An error occurred while forecasting employee performance");
        }
    }

    /// <summary>
    /// Analyze performance trends
    /// </summary>
    [HttpPost("performance-trends")]
    public async Task<ActionResult<PerformanceTrendDto>> AnalyzePerformanceTrends([FromBody] PerformanceTrendRequest request)
    {
        try
        {
            var trends = await _aiAnalyticsService.AnalyzePerformanceTrendsAsync(request);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance trends");
            return StatusCode(500, "An error occurred while analyzing performance trends");
        }
    }

    /// <summary>
    /// Analyze productivity patterns
    /// </summary>
    [HttpPost("productivity-analysis")]
    public async Task<ActionResult<ProductivityInsightDto>> AnalyzeProductivityPatterns([FromBody] ProductivityAnalysisRequest request)
    {
        try
        {
            var insights = await _aiAnalyticsService.AnalyzeProductivityPatternsAsync(request);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing productivity patterns");
            return StatusCode(500, "An error occurred while analyzing productivity patterns");
        }
    }

    /// <summary>
    /// Predict PIP success probability
    /// </summary>
    [HttpPost("pip-success-prediction")]
    public async Task<ActionResult<PIPSuccessPredictionDto>> PredictPIPSuccess([FromBody] PIPSuccessPredictionRequest request)
    {
        try
        {
            var prediction = await _aiAnalyticsService.PredictPIPSuccessAsync(request);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting PIP success");
            return StatusCode(500, "An error occurred while predicting PIP success");
        }
    }

    /// <summary>
    /// Generate automated insights
    /// </summary>
    [HttpPost("automated-insights")]
    public async Task<ActionResult<List<AIInsightDto>>> GenerateAutomatedInsights([FromBody] InsightGenerationRequest request)
    {
        try
        {
            var insights = await _aiAnalyticsService.GenerateAutomatedInsightsAsync(request);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating automated insights");
            return StatusCode(500, "An error occurred while generating automated insights");
        }
    }

    /// <summary>
    /// Generate recommendations
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<ActionResult<List<RecommendationDto>>> GenerateRecommendations([FromBody] RecommendationRequest request)
    {
        try
        {
            var recommendations = await _aiAnalyticsService.GenerateRecommendationsAsync(request);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return StatusCode(500, "An error occurred while generating recommendations");
        }
    }

    /// <summary>
    /// Assess organizational risks
    /// </summary>
    [HttpPost("risk-assessment")]
    public async Task<ActionResult<RiskAssessmentDto>> AssessOrganizationalRisks([FromBody] RiskAssessmentRequest request)
    {
        try
        {
            var assessment = await _aiAnalyticsService.AssessOrganizationalRisksAsync(request);
            return Ok(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing organizational risks");
            return StatusCode(500, "An error occurred while assessing organizational risks");
        }
    }

    /// <summary>
    /// Suggest process optimizations
    /// </summary>
    [HttpPost("process-optimization")]
    public async Task<ActionResult<OptimizationSuggestionDto>> SuggestProcessOptimizations([FromBody] OptimizationRequest request)
    {
        try
        {
            var suggestions = await _aiAnalyticsService.SuggestProcessOptimizationsAsync(request);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting process optimizations");
            return StatusCode(500, "An error occurred while suggesting process optimizations");
        }
    }

    /// <summary>
    /// Get analytics dashboard data
    /// </summary>
    [HttpPost("dashboard-data")]
    public async Task<ActionResult<AIAnalyticsDashboardDto>> GetAnalyticsDashboardData([FromBody] DashboardDataRequest request)
    {
        try
        {
            var dashboardData = await _aiAnalyticsService.GetAnalyticsDashboardDataAsync(request);
            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics dashboard data");
            return StatusCode(500, "An error occurred while getting analytics dashboard data");
        }
    }

    /// <summary>
    /// Get trend insights
    /// </summary>
    [HttpPost("trend-insights")]
    public async Task<ActionResult<List<TrendInsightDto>>> GetTrendInsights([FromBody] TrendInsightRequest request)
    {
        try
        {
            var insights = await _aiAnalyticsService.GetTrendInsightsAsync(request);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trend insights");
            return StatusCode(500, "An error occurred while getting trend insights");
        }
    }

    /// <summary>
    /// Get benchmark comparison
    /// </summary>
    [HttpPost("benchmark-comparison")]
    public async Task<ActionResult<BenchmarkComparisonDto>> GetBenchmarkComparison([FromBody] BenchmarkRequest request)
    {
        try
        {
            var comparison = await _aiAnalyticsService.GetBenchmarkComparisonAsync(request);
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting benchmark comparison");
            return StatusCode(500, "An error occurred while getting benchmark comparison");
        }
    }
}