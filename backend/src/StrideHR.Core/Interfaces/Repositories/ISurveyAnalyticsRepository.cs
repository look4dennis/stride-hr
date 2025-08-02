using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISurveyAnalyticsRepository : IRepository<SurveyAnalytics>
{
    Task<IEnumerable<SurveyAnalytics>> GetBySurveyAsync(int surveyId);
    Task<IEnumerable<SurveyAnalytics>> GetByQuestionAsync(int questionId);
    Task<SurveyAnalytics?> GetByMetricTypeAsync(int surveyId, string metricType);
    Task<IEnumerable<SurveyAnalytics>> GetBySegmentAsync(int surveyId, string segment, string segmentValue);
    Task<DateTime?> GetLastCalculationDateAsync(int surveyId);
    Task DeleteBySurveyAsync(int surveyId);
}