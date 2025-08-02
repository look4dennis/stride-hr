using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Survey;

namespace StrideHR.Core.Interfaces.Services;

public interface ISurveyService
{
    // Basic CRUD operations
    Task<SurveyDto?> GetByIdAsync(int id);
    Task<IEnumerable<SurveyDto>> GetAllAsync();
    Task<IEnumerable<SurveyDto>> GetByBranchAsync(int branchId);
    Task<SurveyDto> CreateAsync(CreateSurveyDto dto, int createdByEmployeeId);
    Task<SurveyDto> UpdateAsync(int id, UpdateSurveyDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    // Survey management
    Task<SurveyDto?> GetWithQuestionsAsync(int id);
    Task<IEnumerable<SurveyDto>> GetActiveAsync();
    Task<IEnumerable<SurveyDto>> GetByStatusAsync(SurveyStatus status);
    Task<IEnumerable<SurveyDto>> GetByTypeAsync(SurveyType type);
    Task<IEnumerable<SurveyDto>> GetByCreatorAsync(int createdByEmployeeId);
    Task<IEnumerable<SurveyDto>> SearchAsync(string searchTerm);

    // Survey lifecycle
    Task<bool> ActivateAsync(int id);
    Task<bool> PauseAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ArchiveAsync(int id);
    Task<SurveyDto> DuplicateAsync(int id, string newTitle);

    // Question management
    Task<SurveyQuestionDto> AddQuestionAsync(int surveyId, CreateSurveyQuestionDto dto);
    Task<SurveyQuestionDto> UpdateQuestionAsync(int questionId, CreateSurveyQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int questionId);
    Task<bool> ReorderQuestionsAsync(int surveyId, Dictionary<int, int> questionOrderMap);

    // Distribution
    Task<bool> DistributeToEmployeeAsync(int surveyId, int employeeId, string? invitationMessage = null);
    Task<bool> DistributeToBranchAsync(int surveyId, int branchId, string? invitationMessage = null);
    Task<bool> DistributeToRoleAsync(int surveyId, string role, string? invitationMessage = null);
    Task<bool> DistributeGloballyAsync(int surveyId, string? invitationMessage = null);
    Task<IEnumerable<SurveyDistributionDto>> GetDistributionsAsync(int surveyId);

    // Validation
    Task<bool> CanActivateAsync(int id);
    Task<bool> CanEditAsync(int id);
    Task<bool> CanDeleteAsync(int id);
    Task<bool> HasActiveResponsesAsync(int id);
}