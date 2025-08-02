using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Survey;

namespace StrideHR.Infrastructure.Services;

public class SurveyResponseService : ISurveyResponseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SurveyResponseService> _logger;

    public SurveyResponseService(IUnitOfWork unitOfWork, ILogger<SurveyResponseService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Response Management

    public async Task<SurveyResponseDto?> GetByIdAsync(int id)
    {
        var response = await _unitOfWork.SurveyResponses.GetWithAnswersAsync(id);
        return response != null ? MapToDto(response) : null;
    }

    public async Task<IEnumerable<SurveyResponseDto>> GetBySurveyAsync(int surveyId)
    {
        var responses = await _unitOfWork.SurveyResponses.GetBySurveyAsync(surveyId);
        return responses.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        var responses = await _unitOfWork.SurveyResponses.GetByEmployeeAsync(employeeId);
        return responses.Select(MapToDto);
    }

    public async Task<SurveyResponseDto?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId)
    {
        var response = await _unitOfWork.SurveyResponses.GetByEmployeeAndSurveyAsync(employeeId, surveyId);
        return response != null ? MapToDto(response) : null;
    }

    #endregion

    #region Response Submission

    public async Task<SurveyResponseDto> StartResponseAsync(int surveyId, int? employeeId = null, string? anonymousId = null)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null)
            throw new ArgumentException($"Survey with ID {surveyId} not found");

        if (survey.Status != SurveyStatus.Active)
            throw new InvalidOperationException("Survey is not active");

        // Check if employee has already responded (if not allowing multiple responses)
        if (employeeId.HasValue && !survey.AllowMultipleResponses)
        {
            var existingResponse = await _unitOfWork.SurveyResponses.GetByEmployeeAndSurveyAsync(employeeId.Value, surveyId);
            if (existingResponse != null)
                throw new InvalidOperationException("Employee has already responded to this survey");
        }

        var response = new SurveyResponse
        {
            SurveyId = surveyId,
            RespondentEmployeeId = employeeId,
            AnonymousId = anonymousId,
            Status = SurveyResponseStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CompletionPercentage = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyResponses.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey response started. Survey ID: {SurveyId}, Response ID: {ResponseId}", surveyId, response.Id);
        
        return MapToDto(response);
    }

    public async Task<SurveyResponseDto> SaveProgressAsync(int responseId, List<SubmitSurveyAnswerDto> answers)
    {
        var response = await _unitOfWork.SurveyResponses.GetWithAnswersAsync(responseId);
        if (response == null)
            throw new ArgumentException($"Response with ID {responseId} not found");

        // Update existing answers or create new ones
        foreach (var answerDto in answers)
        {
            var existingAnswer = response.Answers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);
            
            if (existingAnswer != null)
            {
                UpdateAnswerFromDto(existingAnswer, answerDto);
                await _unitOfWork.SurveyAnswers.UpdateAsync(existingAnswer);
            }
            else
            {
                var newAnswer = CreateAnswerFromDto(responseId, answerDto);
                await _unitOfWork.SurveyAnswers.AddAsync(newAnswer);
            }
        }

        // Calculate completion percentage
        var survey = await _unitOfWork.Surveys.GetWithQuestionsAsync(response.SurveyId);
        var totalQuestions = survey?.Questions.Count(q => q.IsActive) ?? 0;
        var answeredQuestions = answers.Count(a => !a.IsSkipped);
        response.CompletionPercentage = totalQuestions > 0 ? (int)((double)answeredQuestions / totalQuestions * 100) : 0;
        response.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SurveyResponses.UpdateAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey response progress saved. Response ID: {ResponseId}, Completion: {Completion}%", 
            responseId, response.CompletionPercentage);
        
        return MapToDto(response);
    }

    public async Task<SurveyResponseDto> SubmitResponseAsync(SubmitSurveyResponseDto dto)
    {
        SurveyResponse response;

        // Check if this is a new response or updating existing one
        if (dto.RespondentEmployeeId.HasValue)
        {
            var existingResponse = await _unitOfWork.SurveyResponses.GetByEmployeeAndSurveyAsync(
                dto.RespondentEmployeeId.Value, dto.SurveyId);
            
            if (existingResponse != null)
            {
                response = existingResponse;
            }
            else
            {
                response = await CreateNewResponse(dto);
            }
        }
        else
        {
            response = await CreateNewResponse(dto);
        }

        // Save all answers
        foreach (var answerDto in dto.Answers)
        {
            var answer = CreateAnswerFromDto(response.Id, answerDto);
            await _unitOfWork.SurveyAnswers.AddAsync(answer);
        }

        // Update response status
        response.Status = SurveyResponseStatus.Completed;
        response.CompletedAt = DateTime.UtcNow;
        response.SubmittedAt = DateTime.UtcNow;
        response.CompletionPercentage = 100;
        response.Notes = dto.Notes;
        response.Location = dto.Location;
        
        if (response.StartedAt.HasValue)
        {
            response.TimeTaken = DateTime.UtcNow - response.StartedAt.Value;
        }

        await _unitOfWork.SurveyResponses.UpdateAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey response submitted. Survey ID: {SurveyId}, Response ID: {ResponseId}", 
            dto.SurveyId, response.Id);
        
        return MapToDto(response);
    }

    public async Task<bool> DeleteResponseAsync(int id)
    {
        var response = await _unitOfWork.SurveyResponses.GetByIdAsync(id);
        if (response == null)
            return false;

        response.IsDeleted = true;
        response.DeletedAt = DateTime.UtcNow;
        
        await _unitOfWork.SurveyResponses.UpdateAsync(response);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Survey response deleted. Response ID: {ResponseId}", id);
        return true;
    }

    #endregion

    #region Response Queries

    public async Task<IEnumerable<SurveyResponseDto>> GetByStatusAsync(int surveyId, SurveyResponseStatus status)
    {
        var responses = await _unitOfWork.SurveyResponses.GetByStatusAsync(surveyId, status);
        return responses.Select(MapToDto);
    }

    public async Task<IEnumerable<SurveyResponseDto>> GetCompletedResponsesAsync(int surveyId)
    {
        var responses = await _unitOfWork.SurveyResponses.GetCompletedResponsesAsync(surveyId);
        return responses.Select(MapToDto);
    }

    public async Task<int> GetResponseCountAsync(int surveyId)
    {
        return await _unitOfWork.SurveyResponses.GetResponseCountAsync(surveyId);
    }

    public async Task<int> GetCompletedResponseCountAsync(int surveyId)
    {
        return await _unitOfWork.SurveyResponses.GetCompletedResponseCountAsync(surveyId);
    }

    public async Task<double> GetResponseRateAsync(int surveyId)
    {
        var distributionCount = await _unitOfWork.SurveyDistributions.GetDistributionCountAsync(surveyId);
        var responseCount = await _unitOfWork.SurveyResponses.GetResponseCountAsync(surveyId);
        
        return distributionCount > 0 ? (double)responseCount / distributionCount * 100 : 0;
    }

    public async Task<double> GetCompletionRateAsync(int surveyId)
    {
        var responseCount = await _unitOfWork.SurveyResponses.GetResponseCountAsync(surveyId);
        var completedCount = await _unitOfWork.SurveyResponses.GetCompletedResponseCountAsync(surveyId);
        
        return responseCount > 0 ? (double)completedCount / responseCount * 100 : 0;
    }

    public async Task<TimeSpan?> GetAverageCompletionTimeAsync(int surveyId)
    {
        return await _unitOfWork.SurveyResponses.GetAverageCompletionTimeAsync(surveyId);
    }

    #endregion

    #region Access Control

    public async Task<bool> CanEmployeeAccessSurveyAsync(int employeeId, int surveyId)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null || survey.Status != SurveyStatus.Active)
            return false;

        // Check if survey is distributed to this employee
        var distribution = await _unitOfWork.SurveyDistributions.GetByEmployeeAndSurveyAsync(employeeId, surveyId);
        if (distribution != null)
            return true;

        // Check if survey is global or distributed to employee's branch
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            return false;

        if (survey.IsGlobal)
            return true;

        var branchDistribution = await _unitOfWork.SurveyDistributions
            .FirstOrDefaultAsync(d => d.SurveyId == surveyId && d.TargetBranchId == employee.BranchId);
        
        return branchDistribution != null;
    }

    public async Task<bool> HasEmployeeRespondedAsync(int employeeId, int surveyId)
    {
        return await _unitOfWork.SurveyResponses.HasEmployeeRespondedAsync(employeeId, surveyId);
    }

    public async Task<string> GenerateAnonymousAccessTokenAsync(int surveyId)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        if (survey == null)
            throw new ArgumentException($"Survey with ID {surveyId} not found");

        var tokenData = $"{surveyId}:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid()}";
        var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
        var hashBytes = SHA256.HashData(tokenBytes);
        
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<bool> ValidateAnonymousAccessTokenAsync(int surveyId, string token)
    {
        // For simplicity, we'll just check if the survey exists and is active
        // In a real implementation, you might want to store and validate tokens
        var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
        return survey != null && survey.Status == SurveyStatus.Active && !survey.RequireAuthentication;
    }

    #endregion

    #region Export

    public async Task<byte[]> ExportResponsesAsync(int surveyId, string format = "xlsx")
    {
        var responses = await _unitOfWork.SurveyResponses.GetCompletedResponsesAsync(surveyId);
        var survey = await _unitOfWork.Surveys.GetWithQuestionsAsync(surveyId);
        
        if (survey == null)
            throw new ArgumentException($"Survey with ID {surveyId} not found");

        // This is a simplified implementation
        // In a real scenario, you would use a library like EPPlus for Excel export
        var exportData = new
        {
            Survey = survey.Title,
            TotalResponses = responses.Count(),
            Responses = responses.Select(r => new
            {
                ResponseId = r.Id,
                RespondentName = r.RespondentEmployee != null 
                    ? $"{r.RespondentEmployee.FirstName} {r.RespondentEmployee.LastName}"
                    : "Anonymous",
                CompletedAt = r.CompletedAt,
                TimeTaken = r.TimeTaken,
                Answers = r.Answers.Select(a => new
                {
                    Question = a.Question.QuestionText,
                    Answer = GetAnswerText(a)
                })
            })
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    #endregion

    #region Private Helper Methods

    private async Task<SurveyResponse> CreateNewResponse(SubmitSurveyResponseDto dto)
    {
        var response = new SurveyResponse
        {
            SurveyId = dto.SurveyId,
            RespondentEmployeeId = dto.RespondentEmployeeId,
            AnonymousId = dto.AnonymousId,
            Status = SurveyResponseStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CompletionPercentage = 0,
            Notes = dto.Notes,
            Location = dto.Location,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.SurveyResponses.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();
        
        return response;
    }

    private static SurveyAnswer CreateAnswerFromDto(int responseId, SubmitSurveyAnswerDto dto)
    {
        return new SurveyAnswer
        {
            ResponseId = responseId,
            QuestionId = dto.QuestionId,
            SelectedOptionId = dto.SelectedOptionId,
            TextAnswer = dto.TextAnswer,
            NumericAnswer = dto.NumericAnswer,
            DateAnswer = dto.DateAnswer,
            BooleanAnswer = dto.BooleanAnswer,
            OtherAnswer = dto.OtherAnswer,
            MultipleSelections = dto.MultipleSelections != null ? JsonSerializer.Serialize(dto.MultipleSelections) : null,
            RatingValue = dto.RatingValue,
            IsSkipped = dto.IsSkipped,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateAnswerFromDto(SurveyAnswer answer, SubmitSurveyAnswerDto dto)
    {
        answer.SelectedOptionId = dto.SelectedOptionId;
        answer.TextAnswer = dto.TextAnswer;
        answer.NumericAnswer = dto.NumericAnswer;
        answer.DateAnswer = dto.DateAnswer;
        answer.BooleanAnswer = dto.BooleanAnswer;
        answer.OtherAnswer = dto.OtherAnswer;
        answer.MultipleSelections = dto.MultipleSelections != null ? JsonSerializer.Serialize(dto.MultipleSelections) : null;
        answer.RatingValue = dto.RatingValue;
        answer.IsSkipped = dto.IsSkipped;
        answer.UpdatedAt = DateTime.UtcNow;
    }

    private static SurveyResponseDto MapToDto(SurveyResponse response)
    {
        return new SurveyResponseDto
        {
            Id = response.Id,
            SurveyId = response.SurveyId,
            SurveyTitle = response.Survey?.Title ?? string.Empty,
            RespondentEmployeeId = response.RespondentEmployeeId,
            RespondentEmployeeName = response.RespondentEmployee != null 
                ? $"{response.RespondentEmployee.FirstName} {response.RespondentEmployee.LastName}".Trim()
                : null,
            AnonymousId = response.AnonymousId,
            Status = response.Status,
            StartedAt = response.StartedAt,
            CompletedAt = response.CompletedAt,
            SubmittedAt = response.SubmittedAt,
            CompletionPercentage = response.CompletionPercentage,
            TimeTaken = response.TimeTaken,
            Location = response.Location,
            Notes = response.Notes,
            CreatedAt = response.CreatedAt,
            Answers = response.Answers?.Select(a => new SurveyAnswerDto
            {
                Id = a.Id,
                ResponseId = a.ResponseId,
                QuestionId = a.QuestionId,
                QuestionText = a.Question?.QuestionText ?? string.Empty,
                SelectedOptionId = a.SelectedOptionId,
                SelectedOptionText = a.SelectedOption?.OptionText,
                TextAnswer = a.TextAnswer,
                NumericAnswer = a.NumericAnswer,
                DateAnswer = a.DateAnswer,
                BooleanAnswer = a.BooleanAnswer,
                OtherAnswer = a.OtherAnswer,
                MultipleSelections = !string.IsNullOrEmpty(a.MultipleSelections) 
                    ? JsonSerializer.Deserialize<List<string>>(a.MultipleSelections)
                    : null,
                RatingValue = a.RatingValue,
                IsSkipped = a.IsSkipped
            }).ToList() ?? new List<SurveyAnswerDto>()
        };
    }

    private static string GetAnswerText(SurveyAnswer answer)
    {
        if (answer.IsSkipped)
            return "Skipped";

        if (!string.IsNullOrEmpty(answer.TextAnswer))
            return answer.TextAnswer;

        if (answer.SelectedOption != null)
            return answer.SelectedOption.OptionText;

        if (answer.NumericAnswer.HasValue)
            return answer.NumericAnswer.Value.ToString();

        if (answer.DateAnswer.HasValue)
            return answer.DateAnswer.Value.ToString("yyyy-MM-dd");

        if (answer.BooleanAnswer.HasValue)
            return answer.BooleanAnswer.Value ? "Yes" : "No";

        if (answer.RatingValue.HasValue)
            return answer.RatingValue.Value.ToString();

        if (!string.IsNullOrEmpty(answer.MultipleSelections))
        {
            var selections = JsonSerializer.Deserialize<List<string>>(answer.MultipleSelections);
            return string.Join(", ", selections ?? new List<string>());
        }

        return "No answer";
    }

    #endregion
}