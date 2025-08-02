using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISurveyResponseRepository : IRepository<SurveyResponse>
{
    Task<IEnumerable<SurveyResponse>> GetBySurveyAsync(int surveyId);
    Task<IEnumerable<SurveyResponse>> GetByEmployeeAsync(int employeeId);
    Task<SurveyResponse?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId);
    Task<SurveyResponse?> GetWithAnswersAsync(int id);
    Task<IEnumerable<SurveyResponse>> GetByStatusAsync(int surveyId, SurveyResponseStatus status);
    Task<IEnumerable<SurveyResponse>> GetCompletedResponsesAsync(int surveyId);
    Task<int> GetResponseCountAsync(int surveyId);
    Task<int> GetCompletedResponseCountAsync(int surveyId);
    Task<TimeSpan?> GetAverageCompletionTimeAsync(int surveyId);
    Task<bool> HasEmployeeRespondedAsync(int employeeId, int surveyId);
}