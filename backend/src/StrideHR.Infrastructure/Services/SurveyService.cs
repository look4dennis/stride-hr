using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Survey;

namespace StrideHR.Infrastructure.Services;

public class SurveyService : ISurveyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SurveyService> _logger;

    public SurveyService(IUnitOfWork unitOfWork, ILogger<SurveyService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Basic CRUD Operations

    public async Task<SurveyDto?> GetByIdAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id, 
            s => s.CreatedByEmployee, 
            s => s.Branch);
        
        return survey != null ? MapToDto(survey) : null;
    }

    public async Task<IEnumerable<SurveyDto>> GetAllAsync()
    {
        var surveys = await _unitOfWork.Surveys.GetAllAsync(
            s => s.CreatedByEmployee, 
            s => s.Branch);
        
        return surveys.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyDto>> GetByBranchAsync(int branchId)
    {
        var surveys = await _unitOfWork.Surveys.GetByBranchAsync(branchId);
        return surveys.Select(MapToDto);
    }

    public async Task<SurveyDto> CreateAsync(CreateSurveyDto dto, int createdByEmployeeId)
    {
        var survey = new Survey
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Status = SurveyStatus.Draft,
            IsAnonymous = dto.IsAnonymous,
            AllowMultipleResponses = dto.AllowMultipleResponses,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedByEmployeeId = createdByEmployeeId,
            BranchId = dto.BranchId,
            IsGlobal = dto.IsGlobal,
            Instructions = dto.Instructions,
            ThankYouMessage = dto.ThankYouMessage,
            EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
            RequireAuthentication = dto.RequireAuthentication,
            ShowProgressBar = dto.ShowProgressBar,
            RandomizeQuestions = dto.RandomizeQuestions,
            Tags = dto.Tags,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        // Add questions if provided
        if (dto.Questions.Any())
        {
            foreach (var questionDto in dto.Questions)
            {
                await AddQuestionAsync(survey.Id, questionDto);
            }
        }

        _logger.LogInformation("Survey created with ID: {SurveyId}", survey.Id);
        
        return await GetByIdAsync(survey.Id) ?? throw new InvalidOperationException("Failed to retrieve created survey");
    }

    public async Task<SurveyDto> UpdateAsync(int id, UpdateSurveyDto dto)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null)
            throw new ArgumentException($"Survey with ID {id} not found");

        // Check if survey can be edited
        if (!await CanEditAsync(id))
            throw new InvalidOperationException("Survey cannot be edited as it has active responses");

        survey.Title = dto.Title;
        survey.Description = dto.Description;
        survey.Type = dto.Type;
        survey.Status = dto.Status;
        survey.IsAnonymous = dto.IsAnonymous;
        survey.AllowMultipleResponses = dto.AllowMultipleResponses;
        survey.StartDate = dto.StartDate;
        survey.EndDate = dto.EndDate;
        survey.IsGlobal = dto.IsGlobal;
        survey.Instructions = dto.Instructions;
        survey.ThankYouMessage = dto.ThankYouMessage;
        survey.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
        survey.RequireAuthentication = dto.RequireAuthentication;
        survey.ShowProgressBar = dto.ShowProgressBar;
        survey.RandomizeQuestions = dto.RandomizeQuestions;
        survey.Tags = dto.Tags;
        survey.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey updated with ID: {SurveyId}", id);
        
        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated survey");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await CanDeleteAsync(id))
            return false;

        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null)
            return false;

        survey.IsDeleted = true;
        survey.DeletedAt = DateTime.UtcNow;
        
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey deleted with ID: {SurveyId}", id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Surveys.AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    #endregion

    #region Survey Management

    public async Task<SurveyDto?> GetWithQuestionsAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetWithQuestionsAsync(id);
        return survey != null ? MapToDtoWithQuestions(survey) : null;
    }

    public async Task<IEnumerable<SurveyDto>> GetActiveAsync()
    {
        var surveys = await _unitOfWork.Surveys.GetActiveAsync();
        return surveys.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyDto>> GetByStatusAsync(SurveyStatus status)
    {
        var surveys = await _unitOfWork.Surveys.GetByStatusAsync(status);
        return surveys.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyDto>> GetByTypeAsync(SurveyType type)
    {
        var surveys = await _unitOfWork.Surveys.GetByTypeAsync(type);
        return surveys.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyDto>> GetByCreatorAsync(int createdByEmployeeId)
    {
        var surveys = await _unitOfWork.Surveys.GetByCreatorAsync(createdByEmployeeId);
        return surveys.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyDto>> SearchAsync(string searchTerm)
    {
        var surveys = await _unitOfWork.Surveys.SearchAsync(searchTerm);
        return surveys.Select(MapToDto);
    }

    #endregion

    #region Survey Lifecycle

    public async Task<bool> ActivateAsync(int id)
    {
        if (!await CanActivateAsync(id))
            return false;

        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null)
            return false;

        survey.Status = SurveyStatus.Active;
        survey.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey activated with ID: {SurveyId}", id);
        return true;
    }

    public async Task<bool> PauseAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        survey.Status = SurveyStatus.Paused;
        survey.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey paused with ID: {SurveyId}", id);
        return true;
    }

    public async Task<bool> CompleteAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null)
            return false;

        survey.Status = SurveyStatus.Completed;
        survey.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey completed with ID: {SurveyId}", id);
        return true;
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
        if (survey == null)
            return false;

        survey.Status = SurveyStatus.Archived;
        survey.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey archived with ID: {SurveyId}", id);
        return true;
    }

    public async Task<SurveyDto> DuplicateAsync(int id, string newTitle)
    {
        var originalSurvey = await _unitOfWork.Surveys.GetWithQuestionsAsync(id);
        if (originalSurvey == null)
            throw new ArgumentException($"Survey with ID {id} not found");

        var duplicatedSurvey = new Survey
        {
            Title = newTitle,
            Description = originalSurvey.Description,
            Type = originalSurvey.Type,
            Status = SurveyStatus.Draft,
            IsAnonymous = originalSurvey.IsAnonymous,
            AllowMultipleResponses = originalSurvey.AllowMultipleResponses,
            CreatedByEmployeeId = originalSurvey.CreatedByEmployeeId,
            BranchId = originalSurvey.BranchId,
            IsGlobal = originalSurvey.IsGlobal,
            Instructions = originalSurvey.Instructions,
            ThankYouMessage = originalSurvey.ThankYouMessage,
            EstimatedDurationMinutes = originalSurvey.EstimatedDurationMinutes,
            RequireAuthentication = originalSurvey.RequireAuthentication,
            ShowProgressBar = originalSurvey.ShowProgressBar,
            RandomizeQuestions = originalSurvey.RandomizeQuestions,
            Tags = originalSurvey.Tags,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Surveys.AddAsync(duplicatedSurvey);
        await _unitOfWork.SaveChangesAsync();

        // Duplicate questions
        foreach (var question in originalSurvey.Questions)
        {
            var duplicatedQuestion = new SurveyQuestion
            {
                SurveyId = duplicatedSurvey.Id,
                QuestionText = question.QuestionText,
                Type = question.Type,
                OrderIndex = question.OrderIndex,
                IsRequired = question.IsRequired,
                HelpText = question.HelpText,
                ValidationRules = question.ValidationRules,
                MinLength = question.MinLength,
                MaxLength = question.MaxLength,
                MinValue = question.MinValue,
                MaxValue = question.MaxValue,
                PlaceholderText = question.PlaceholderText,
                AllowOther = question.AllowOther,
                ConditionalLogic = question.ConditionalLogic,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SurveyQuestions.AddAsync(duplicatedQuestion);
            await _unitOfWork.SaveChangesAsync();

            // Duplicate options
            foreach (var option in question.Options)
            {
                var duplicatedOption = new SurveyQuestionOption
                {
                    QuestionId = duplicatedQuestion.Id,
                    OptionText = option.OptionText,
                    OrderIndex = option.OrderIndex,
                    Value = option.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.SurveyQuestionOptions.AddAsync(duplicatedOption);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey duplicated from ID: {OriginalId} to new ID: {NewId}", id, duplicatedSurvey.Id);
        
        return await GetByIdAsync(duplicatedSurvey.Id) ?? throw new InvalidOperationException("Failed to retrieve duplicated survey");
    }

    #endregion

    #region Question Management

    public async Task<SurveyQuestionDto> AddQuestionAsync(int surveyId, CreateSurveyQuestionDto dto)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null)
            throw new ArgumentException($"Survey with ID {surveyId} not found");

        if (!await CanEditAsync(surveyId))
            throw new InvalidOperationException("Survey cannot be edited as it has active responses");

        var maxOrder = await _unitOfWork.SurveyQuestions.GetMaxOrderIndexAsync(surveyId);
        
        var question = new SurveyQuestion
        {
            SurveyId = surveyId,
            QuestionText = dto.QuestionText,
            Type = dto.Type,
            OrderIndex = dto.OrderIndex > 0 ? dto.OrderIndex : maxOrder + 1,
            IsRequired = dto.IsRequired,
            HelpText = dto.HelpText,
            ValidationRules = dto.ValidationRules,
            MinLength = dto.MinLength,
            MaxLength = dto.MaxLength,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            PlaceholderText = dto.PlaceholderText,
            AllowOther = dto.AllowOther,
            ConditionalLogic = dto.ConditionalLogic,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyQuestions.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        // Add options if provided
        foreach (var optionDto in dto.Options)
        {
            var option = new SurveyQuestionOption
            {
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                OrderIndex = optionDto.OrderIndex,
                Value = optionDto.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SurveyQuestionOptions.AddAsync(option);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Question added to survey ID: {SurveyId}, Question ID: {QuestionId}", surveyId, question.Id);
        
        var createdQuestion = await _unitOfWork.SurveyQuestions.GetWithOptionsAsync(question.Id);
        return MapQuestionToDto(createdQuestion!);
    }

    public async Task<SurveyQuestionDto> UpdateQuestionAsync(int questionId, CreateSurveyQuestionDto dto)
    {
        var question = await _unitOfWork.SurveyQuestions.GetWithOptionsAsync(questionId);
        if (question == null)
            throw new ArgumentException($"Question with ID {questionId} not found");

        if (!await CanEditAsync(question.SurveyId))
            throw new InvalidOperationException("Survey cannot be edited as it has active responses");

        question.QuestionText = dto.QuestionText;
        question.Type = dto.Type;
        question.OrderIndex = dto.OrderIndex;
        question.IsRequired = dto.IsRequired;
        question.HelpText = dto.HelpText;
        question.ValidationRules = dto.ValidationRules;
        question.MinLength = dto.MinLength;
        question.MaxLength = dto.MaxLength;
        question.MinValue = dto.MinValue;
        question.MaxValue = dto.MaxValue;
        question.PlaceholderText = dto.PlaceholderText;
        question.AllowOther = dto.AllowOther;
        question.ConditionalLogic = dto.ConditionalLogic;
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SurveyQuestions.UpdateAsync(question);

        // Update options - remove existing and add new ones
        foreach (var existingOption in question.Options)
        {
            existingOption.IsActive = false;
            existingOption.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SurveyQuestionOptions.UpdateAsync(existingOption);
        }

        foreach (var optionDto in dto.Options)
        {
            var option = new SurveyQuestionOption
            {
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                OrderIndex = optionDto.OrderIndex,
                Value = optionDto.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SurveyQuestionOptions.AddAsync(option);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Question updated with ID: {QuestionId}", questionId);
        
        var updatedQuestion = await _unitOfWork.SurveyQuestions.GetWithOptionsAsync(questionId);
        return MapQuestionToDto(updatedQuestion!);
    }

    public async Task<bool> DeleteQuestionAsync(int questionId)
    {
        var question = await _unitOfWork.SurveyQuestions.GetByIdAsync(questionId);
        if (question == null)
            return false;

        if (!await CanEditAsync(question.SurveyId))
            return false;

        question.IsActive = false;
        question.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.SurveyQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Question deleted with ID: {QuestionId}", questionId);
        return true;
    }

    public async Task<bool> ReorderQuestionsAsync(int surveyId, Dictionary<int, int> questionOrderMap)
    {
        if (!await CanEditAsync(surveyId))
            return false;

        await _unitOfWork.SurveyQuestions.ReorderQuestionsAsync(surveyId, questionOrderMap);
        
        _logger.LogInformation("Questions reordered for survey ID: {SurveyId}", surveyId);
        return true;
    }

    #endregion

    #region Distribution

    public async Task<bool> DistributeToEmployeeAsync(int surveyId, int employeeId, string? invitationMessage = null)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        var distribution = new SurveyDistribution
        {
            SurveyId = surveyId,
            TargetEmployeeId = employeeId,
            InvitationMessage = invitationMessage,
            SentAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyDistributions.AddAsync(distribution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey distributed to employee. Survey ID: {SurveyId}, Employee ID: {EmployeeId}", surveyId, employeeId);
        return true;
    }

    public async Task<bool> DistributeToBranchAsync(int surveyId, int branchId, string? invitationMessage = null)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        var distribution = new SurveyDistribution
        {
            SurveyId = surveyId,
            TargetBranchId = branchId,
            InvitationMessage = invitationMessage,
            SentAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyDistributions.AddAsync(distribution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey distributed to branch. Survey ID: {SurveyId}, Branch ID: {BranchId}", surveyId, branchId);
        return true;
    }

    public async Task<bool> DistributeToRoleAsync(int surveyId, string role, string? invitationMessage = null)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        var distribution = new SurveyDistribution
        {
            SurveyId = surveyId,
            TargetRole = role,
            InvitationMessage = invitationMessage,
            SentAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyDistributions.AddAsync(distribution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey distributed to role. Survey ID: {SurveyId}, Role: {Role}", surveyId, role);
        return true;
    }

    public async Task<bool> DistributeGloballyAsync(int surveyId, string? invitationMessage = null)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        var distribution = new SurveyDistribution
        {
            SurveyId = surveyId,
            InvitationMessage = invitationMessage,
            SentAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyDistributions.AddAsync(distribution);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey distributed globally. Survey ID: {SurveyId}", surveyId);
        return true;
    }

    public async Task<IEnumerable<SurveyDistributionDto>> GetDistributionsAsync(int surveyId)
    {
        var distributions = await _unitOfWork.SurveyDistributions.GetBySurveyAsync(surveyId);
        return distributions.Select(MapDistributionToDto);
    }

    #endregion

    #region Validation

    public async Task<bool> CanActivateAsync(int id)
    {
        var survey = await _unitOfWork.Surveys.GetWithQuestionsAsync(id);
        if (survey == null)
            return false;

        // Survey must have at least one question
        if (!survey.Questions.Any())
            return false;

        // Survey must not be already active
        if (survey.Status == SurveyStatus.Active)
            return false;

        return true;
    }

    public async Task<bool> CanEditAsync(int id)
    {
        return !await HasActiveResponsesAsync(id);
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        return !await HasActiveResponsesAsync(id);
    }

    public async Task<bool> HasActiveResponsesAsync(int id)
    {
        return await _unitOfWork.Surveys.HasActiveResponsesAsync(id);
    }

    #endregion

    #region Private Helper Methods

    private static SurveyDto MapToDto(Survey survey)
    {
        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            Type = survey.Type,
            Status = survey.Status,
            IsAnonymous = survey.IsAnonymous,
            AllowMultipleResponses = survey.AllowMultipleResponses,
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            CreatedByEmployeeId = survey.CreatedByEmployeeId,
            CreatedByEmployeeName = $"{survey.CreatedByEmployee?.FirstName} {survey.CreatedByEmployee?.LastName}".Trim(),
            BranchId = survey.BranchId,
            BranchName = survey.Branch?.Name ?? string.Empty,
            IsGlobal = survey.IsGlobal,
            Instructions = survey.Instructions,
            ThankYouMessage = survey.ThankYouMessage,
            EstimatedDurationMinutes = survey.EstimatedDurationMinutes,
            RequireAuthentication = survey.RequireAuthentication,
            ShowProgressBar = survey.ShowProgressBar,
            RandomizeQuestions = survey.RandomizeQuestions,
            Tags = survey.Tags,
            CreatedAt = survey.CreatedAt,
            UpdatedAt = survey.UpdatedAt,
            TotalQuestions = survey.Questions?.Count(q => q.IsActive) ?? 0,
            TotalResponses = survey.Responses?.Count ?? 0,
            CompletedResponses = survey.Responses?.Count(r => r.Status == SurveyResponseStatus.Completed) ?? 0
        };
    }

    private static SurveyDto MapToDtoWithQuestions(Survey survey)
    {
        var dto = MapToDto(survey);
        dto.Questions = survey.Questions
            .Where(q => q.IsActive)
            .OrderBy(q => q.OrderIndex)
            .Select(MapQuestionToDto)
            .ToList();
        return dto;
    }

    private static SurveyQuestionDto MapQuestionToDto(SurveyQuestion question)
    {
        return new SurveyQuestionDto
        {
            Id = question.Id,
            SurveyId = question.SurveyId,
            QuestionText = question.QuestionText,
            Type = question.Type,
            OrderIndex = question.OrderIndex,
            IsRequired = question.IsRequired,
            HelpText = question.HelpText,
            ValidationRules = question.ValidationRules,
            MinLength = question.MinLength,
            MaxLength = question.MaxLength,
            MinValue = question.MinValue,
            MaxValue = question.MaxValue,
            PlaceholderText = question.PlaceholderText,
            AllowOther = question.AllowOther,
            ConditionalLogic = question.ConditionalLogic,
            IsActive = question.IsActive,
            Options = question.Options
                .Where(o => o.IsActive)
                .OrderBy(o => o.OrderIndex)
                .Select(o => new SurveyQuestionOptionDto
                {
                    Id = o.Id,
                    QuestionId = o.QuestionId,
                    OptionText = o.OptionText,
                    OrderIndex = o.OrderIndex,
                    Value = o.Value,
                    IsActive = o.IsActive
                })
                .ToList()
        };
    }

    private static SurveyDistributionDto MapDistributionToDto(SurveyDistribution distribution)
    {
        return new SurveyDistributionDto
        {
            Id = distribution.Id,
            SurveyId = distribution.SurveyId,
            TargetEmployeeId = distribution.TargetEmployeeId,
            TargetEmployeeName = distribution.TargetEmployee != null 
                ? $"{distribution.TargetEmployee.FirstName} {distribution.TargetEmployee.LastName}".Trim()
                : null,
            TargetBranchId = distribution.TargetBranchId,
            TargetBranchName = distribution.TargetBranch?.Name,
            TargetRole = distribution.TargetRole,
            TargetCriteria = distribution.TargetCriteria,
            SentAt = distribution.SentAt,
            ViewedAt = distribution.ViewedAt,
            StartedAt = distribution.StartedAt,
            CompletedAt = distribution.CompletedAt,
            ReminderCount = distribution.ReminderCount,
            LastReminderSent = distribution.LastReminderSent,
            IsActive = distribution.IsActive,
            InvitationMessage = distribution.InvitationMessage,
            AccessToken = distribution.AccessToken
        };
    }

    #endregion
}