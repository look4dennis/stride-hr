using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISurveyRepository : IRepository<Survey>
{
    Task<IEnumerable<Survey>> GetByBranchAsync(int branchId);
    Task<IEnumerable<Survey>> GetActiveAsync();
    Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status);
    Task<IEnumerable<Survey>> GetByTypeAsync(SurveyType type);
    Task<IEnumerable<Survey>> GetByCreatorAsync(int createdByEmployeeId);
    Task<Survey?> GetWithQuestionsAsync(int id);
    Task<Survey?> GetWithResponsesAsync(int id);
    Task<Survey?> GetWithAnalyticsAsync(int id);
    Task<IEnumerable<Survey>> GetGlobalSurveysAsync();
    Task<IEnumerable<Survey>> SearchAsync(string searchTerm);
    Task<bool> HasActiveResponsesAsync(int surveyId);
}