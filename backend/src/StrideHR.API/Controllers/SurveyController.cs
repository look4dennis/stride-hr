using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Survey;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SurveyController : ControllerBase
{
    private readonly ISurveyService _surveyService;
    private readonly ISurveyResponseService _responseService;
    private readonly ISurveyAnalyticsService _analyticsService;
    private readonly ILogger<SurveyController> _logger;

    public SurveyController(
        ISurveyService surveyService,
        ISurveyResponseService responseService,
        ISurveyAnalyticsService analyticsService,
        ILogger<SurveyController> logger)
    {
        _surveyService = surveyService;
        _responseService = responseService;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    #region Survey Management

    /// <summary>
    /// Get all surveys
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetSurveys()
    {
        try
        {
            var surveys = await _surveyService.GetAllAsync();
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving surveys");
            return StatusCode(500, "An error occurred while retrieving surveys");
        }
    }

    /// <summary>
    /// Get survey by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SurveyDto>> GetSurvey(int id)
    {
        try
        {
            var survey = await _surveyService.GetByIdAsync(id);
            if (survey == null)
                return NotFound($"Survey with ID {id} not found");

            return Ok(survey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while retrieving the survey");
        }
    }

    /// <summary>
    /// Get survey with questions
    /// </summary>
    [HttpGet("{id}/with-questions")]
    public async Task<ActionResult<SurveyDto>> GetSurveyWithQuestions(int id)
    {
        try
        {
            var survey = await _surveyService.GetWithQuestionsAsync(id);
            if (survey == null)
                return NotFound($"Survey with ID {id} not found");

            return Ok(survey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving survey with questions for ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while retrieving the survey");
        }
    }

    /// <summary>
    /// Get surveys by branch
    /// </summary>
    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetSurveysByBranch(int branchId)
    {
        try
        {
            var surveys = await _surveyService.GetByBranchAsync(branchId);
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving surveys for branch: {BranchId}", branchId);
            return StatusCode(500, "An error occurred while retrieving surveys");
        }
    }

    /// <summary>
    /// Get active surveys
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> GetActiveSurveys()
    {
        try
        {
            var surveys = await _surveyService.GetActiveAsync();
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active surveys");
            return StatusCode(500, "An error occurred while retrieving active surveys");
        }
    }

    /// <summary>
    /// Search surveys
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SurveyDto>>> SearchSurveys([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var surveys = await _surveyService.SearchAsync(searchTerm);
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching surveys with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching surveys");
        }
    }

    /// <summary>
    /// Create a new survey
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SurveyDto>> CreateSurvey([FromBody] CreateSurveyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
                return Unauthorized("Employee ID not found in token");

            var survey = await _surveyService.CreateAsync(dto, employeeId.Value);
            return CreatedAtAction(nameof(GetSurvey), new { id = survey.Id }, survey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating survey");
            return StatusCode(500, "An error occurred while creating the survey");
        }
    }

    /// <summary>
    /// Update a survey
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SurveyDto>> UpdateSurvey(int id, [FromBody] UpdateSurveyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var survey = await _surveyService.UpdateAsync(id, dto);
            return Ok(survey);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while updating the survey");
        }
    }

    /// <summary>
    /// Delete a survey
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSurvey(int id)
    {
        try
        {
            var result = await _surveyService.DeleteAsync(id);
            if (!result)
                return NotFound($"Survey with ID {id} not found or cannot be deleted");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while deleting the survey");
        }
    }

    /// <summary>
    /// Duplicate a survey
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<SurveyDto>> DuplicateSurvey(int id, [FromBody] string newTitle)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return BadRequest("New title is required");

            var survey = await _surveyService.DuplicateAsync(id, newTitle);
            return CreatedAtAction(nameof(GetSurvey), new { id = survey.Id }, survey);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while duplicating the survey");
        }
    }

    #endregion

    #region Survey Lifecycle

    /// <summary>
    /// Activate a survey
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateSurvey(int id)
    {
        try
        {
            var result = await _surveyService.ActivateAsync(id);
            if (!result)
                return BadRequest("Survey cannot be activated. Check if it has questions and is not already active.");

            return Ok(new { message = "Survey activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while activating the survey");
        }
    }

    /// <summary>
    /// Pause a survey
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<ActionResult> PauseSurvey(int id)
    {
        try
        {
            var result = await _surveyService.PauseAsync(id);
            if (!result)
                return BadRequest("Survey cannot be paused or is not currently active");

            return Ok(new { message = "Survey paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while pausing the survey");
        }
    }

    /// <summary>
    /// Complete a survey
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteSurvey(int id)
    {
        try
        {
            var result = await _surveyService.CompleteAsync(id);
            if (!result)
                return NotFound($"Survey with ID {id} not found");

            return Ok(new { message = "Survey completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing survey with ID: {SurveyId}", id);
            return StatusCode(500, "An error occurred while completing the survey");
        }
    }

    #endregion

    #region Question Management

    /// <summary>
    /// Add a question to a survey
    /// </summary>
    [HttpPost("{surveyId}/questions")]
    public async Task<ActionResult<SurveyQuestionDto>> AddQuestion(int surveyId, [FromBody] CreateSurveyQuestionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var question = await _surveyService.AddQuestionAsync(surveyId, dto);
            return CreatedAtAction(nameof(GetSurvey), new { id = surveyId }, question);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding question to survey with ID: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while adding the question");
        }
    }

    /// <summary>
    /// Update a question
    /// </summary>
    [HttpPut("questions/{questionId}")]
    public async Task<ActionResult<SurveyQuestionDto>> UpdateQuestion(int questionId, [FromBody] CreateSurveyQuestionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var question = await _surveyService.UpdateQuestionAsync(questionId, dto);
            return Ok(question);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question with ID: {QuestionId}", questionId);
            return StatusCode(500, "An error occurred while updating the question");
        }
    }

    /// <summary>
    /// Delete a question
    /// </summary>
    [HttpDelete("questions/{questionId}")]
    public async Task<ActionResult> DeleteQuestion(int questionId)
    {
        try
        {
            var result = await _surveyService.DeleteQuestionAsync(questionId);
            if (!result)
                return NotFound($"Question with ID {questionId} not found or cannot be deleted");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question with ID: {QuestionId}", questionId);
            return StatusCode(500, "An error occurred while deleting the question");
        }
    }

    #endregion

    #region Distribution

    /// <summary>
    /// Distribute survey to an employee
    /// </summary>
    [HttpPost("{surveyId}/distribute/employee/{employeeId}")]
    public async Task<ActionResult> DistributeToEmployee(int surveyId, int employeeId, [FromBody] string? invitationMessage = null)
    {
        try
        {
            var result = await _surveyService.DistributeToEmployeeAsync(surveyId, employeeId, invitationMessage);
            if (!result)
                return BadRequest("Survey cannot be distributed. Check if survey is active.");

            return Ok(new { message = "Survey distributed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing survey {SurveyId} to employee {EmployeeId}", surveyId, employeeId);
            return StatusCode(500, "An error occurred while distributing the survey");
        }
    }

    /// <summary>
    /// Distribute survey to a branch
    /// </summary>
    [HttpPost("{surveyId}/distribute/branch/{branchId}")]
    public async Task<ActionResult> DistributeToBranch(int surveyId, int branchId, [FromBody] string? invitationMessage = null)
    {
        try
        {
            var result = await _surveyService.DistributeToBranchAsync(surveyId, branchId, invitationMessage);
            if (!result)
                return BadRequest("Survey cannot be distributed. Check if survey is active.");

            return Ok(new { message = "Survey distributed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing survey {SurveyId} to branch {BranchId}", surveyId, branchId);
            return StatusCode(500, "An error occurred while distributing the survey");
        }
    }

    /// <summary>
    /// Get survey distributions
    /// </summary>
    [HttpGet("{surveyId}/distributions")]
    public async Task<ActionResult<IEnumerable<SurveyDistributionDto>>> GetDistributions(int surveyId)
    {
        try
        {
            var distributions = await _surveyService.GetDistributionsAsync(surveyId);
            return Ok(distributions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving distributions for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while retrieving distributions");
        }
    }

    #endregion

    #region Response Management

    /// <summary>
    /// Get survey responses
    /// </summary>
    [HttpGet("{surveyId}/responses")]
    public async Task<ActionResult<IEnumerable<SurveyResponseDto>>> GetResponses(int surveyId)
    {
        try
        {
            var responses = await _responseService.GetBySurveyAsync(surveyId);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving responses for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while retrieving responses");
        }
    }

    /// <summary>
    /// Start a survey response
    /// </summary>
    [HttpPost("{surveyId}/responses/start")]
    public async Task<ActionResult<SurveyResponseDto>> StartResponse(int surveyId)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var response = await _responseService.StartResponseAsync(surveyId, employeeId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting response for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while starting the response");
        }
    }

    /// <summary>
    /// Submit a survey response
    /// </summary>
    [HttpPost("responses")]
    public async Task<ActionResult<SurveyResponseDto>> SubmitResponse([FromBody] SubmitSurveyResponseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set the respondent employee ID from the current user if not provided
            if (!dto.RespondentEmployeeId.HasValue)
            {
                dto.RespondentEmployeeId = GetCurrentEmployeeId();
            }

            var response = await _responseService.SubmitResponseAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting survey response");
            return StatusCode(500, "An error occurred while submitting the response");
        }
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get survey analytics
    /// </summary>
    [HttpGet("{surveyId}/analytics")]
    public async Task<ActionResult<SurveyAnalyticsDto>> GetAnalytics(int surveyId)
    {
        try
        {
            var analytics = await _analyticsService.GetCachedAnalyticsAsync(surveyId);
            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    /// <summary>
    /// Refresh survey analytics
    /// </summary>
    [HttpPost("{surveyId}/analytics/refresh")]
    public async Task<ActionResult> RefreshAnalytics(int surveyId)
    {
        try
        {
            await _analyticsService.RefreshAnalyticsAsync(surveyId);
            return Ok(new { message = "Analytics refreshed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing analytics for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while refreshing analytics");
        }
    }

    /// <summary>
    /// Export survey analytics
    /// </summary>
    [HttpGet("{surveyId}/analytics/export")]
    public async Task<ActionResult> ExportAnalytics(int surveyId, [FromQuery] string format = "pdf")
    {
        try
        {
            var exportData = await _analyticsService.ExportAnalyticsAsync(surveyId, format);
            var contentType = format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/json"
            };

            return File(exportData, contentType, $"survey-analytics-{surveyId}.{format}");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics for survey: {SurveyId}", surveyId);
            return StatusCode(500, "An error occurred while exporting analytics");
        }
    }

    #endregion

    #region Private Helper Methods

    private int? GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : null;
    }

    #endregion
}