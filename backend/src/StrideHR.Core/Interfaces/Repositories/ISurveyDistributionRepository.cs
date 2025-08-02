using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISurveyDistributionRepository : IRepository<SurveyDistribution>
{
    Task<IEnumerable<SurveyDistribution>> GetBySurveyAsync(int surveyId);
    Task<IEnumerable<SurveyDistribution>> GetByEmployeeAsync(int employeeId);
    Task<SurveyDistribution?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId);
    Task<IEnumerable<SurveyDistribution>> GetPendingRemindersAsync();
    Task<IEnumerable<SurveyDistribution>> GetByBranchAsync(int branchId);
    Task<int> GetDistributionCountAsync(int surveyId);
    Task<int> GetViewedCountAsync(int surveyId);
    Task<int> GetStartedCountAsync(int surveyId);
    Task<int> GetCompletedCountAsync(int surveyId);
}