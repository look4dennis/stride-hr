using StrideHR.Core.Enums;
using StrideHR.Core.Models.Survey;

namespace StrideHR.Core.Interfaces.Services;

public interface ISurveyResponseService
{
    // Response management
    Task<SurveyResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<SurveyResponseDto>> GetBySurveyAsync(int surveyId);
    Task<IEnumerable<SurveyResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<SurveyResponseDto?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId);

    // Response submission
    Task<SurveyResponseDto> StartResponseAsync(int surveyId, int? employeeId = null, string? anonymousId = null);
    Task<SurveyResponseDto> SaveProgressAsync(int responseId, List<SubmitSurveyAnswerDto> answers);
    Task<SurveyResponseDto> SubmitResponseAsync(SubmitSurveyResponseDto dto);
    Task<bool> DeleteResponseAsync(int id);

    // Response queries
    Task<IEnumerable<SurveyResponseDto>> GetByStatusAsync(int surveyId, SurveyResponseStatus status);
    Task<IEnumerable<SurveyResponseDto>> GetCompletedResponsesAsync(int surveyId);
    Task<int> GetResponseCountAsync(int surveyId);
    Task<int> GetCompletedResponseCountAsync(int surveyId);
    Task<double> GetResponseRateAsync(int surveyId);
    Task<double> GetCompletionRateAsync(int surveyId);
    Task<TimeSpan?> GetAverageCompletionTimeAsync(int surveyId);

    // Access control
    Task<bool> CanEmployeeAccessSurveyAsync(int employeeId, int surveyId);
    Task<bool> HasEmployeeRespondedAsync(int employeeId, int surveyId);
    Task<string> GenerateAnonymousAccessTokenAsync(int surveyId);
    Task<bool> ValidateAnonymousAccessTokenAsync(int surveyId, string token);

    // Export
    Task<byte[]> ExportResponsesAsync(int surveyId, string format = "xlsx");
}