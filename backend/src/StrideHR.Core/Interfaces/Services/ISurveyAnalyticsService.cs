using StrideHR.Core.Enums;
using StrideHR.Core.Models.Survey;

namespace StrideHR.Core.Interfaces.Services;

public interface ISurveyAnalyticsService
{
    // Analytics generation
    Task<SurveyAnalyticsDto> GenerateAnalyticsAsync(int surveyId);
    Task<SurveyAnalyticsDto> GetCachedAnalyticsAsync(int surveyId);
    Task RefreshAnalyticsAsync(int surveyId);
    Task<bool> ScheduleAnalyticsRefreshAsync(int surveyId, TimeSpan interval);

    // Question analytics
    Task<QuestionAnalyticsDto> GetQuestionAnalyticsAsync(int questionId);
    Task<IEnumerable<QuestionAnalyticsDto>> GetAllQuestionAnalyticsAsync(int surveyId);

    // Sentiment analysis
    Task<SentimentScore> AnalyzeSentimentAsync(string text);
    Task<SentimentScore> GetOverallSentimentAsync(int surveyId);
    Task<IEnumerable<string>> ExtractKeywordsAsync(int surveyId);
    Task<IEnumerable<string>> IdentifyThemesAsync(int surveyId);

    // Demographic analytics
    Task<IEnumerable<DemographicAnalyticsDto>> GetDemographicBreakdownAsync(int surveyId);
    Task<DemographicAnalyticsDto> GetDemographicAnalyticsAsync(int surveyId, string segment, string segmentValue);

    // Comparative analytics
    Task<SurveyAnalyticsDto> CompareSurveysAsync(int[] surveyIds);
    Task<SurveyAnalyticsDto> GetTrendAnalyticsAsync(int surveyId, DateTime fromDate, DateTime toDate);

    // Real-time analytics
    Task<SurveyAnalyticsDto> GetRealTimeAnalyticsAsync(int surveyId);
    Task NotifyAnalyticsUpdateAsync(int surveyId);

    // Export
    Task<byte[]> ExportAnalyticsAsync(int surveyId, string format = "pdf");
    Task<byte[]> ExportAnalyticsReportAsync(int surveyId, bool includeCharts = true);
}