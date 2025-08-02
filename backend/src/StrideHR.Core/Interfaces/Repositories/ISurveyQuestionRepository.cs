using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ISurveyQuestionRepository : IRepository<SurveyQuestion>
{
    Task<IEnumerable<SurveyQuestion>> GetBySurveyAsync(int surveyId);
    Task<IEnumerable<SurveyQuestion>> GetBySurveyWithOptionsAsync(int surveyId);
    Task<SurveyQuestion?> GetWithOptionsAsync(int id);
    Task<int> GetMaxOrderIndexAsync(int surveyId);
    Task ReorderQuestionsAsync(int surveyId, Dictionary<int, int> questionOrderMap);
}