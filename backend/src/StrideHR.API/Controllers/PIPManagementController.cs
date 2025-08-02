using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;

namespace StrideHR.API.Controllers;

[Authorize]
public class PIPManagementController : BaseController
{
    private readonly IPIPManagementService _pipService;

    public PIPManagementController(IPIPManagementService pipService)
    {
        _pipService = pipService;
    }

    #region PIP Management

    [HttpPost("pips")]
    public async Task<IActionResult> CreatePIP([FromBody] CreatePIPDto dto)
    {
        try
        {
            var pip = new PerformanceImprovementPlan
            {
                EmployeeId = dto.EmployeeId,
                ManagerId = dto.ManagerId,
                HRId = dto.HRId,
                PerformanceReviewId = dto.PerformanceReviewId,
                Title = dto.Title,
                Description = dto.Description,
                PerformanceIssues = dto.PerformanceIssues,
                ExpectedImprovements = dto.ExpectedImprovements,
                SupportProvided = dto.SupportProvided,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ReviewFrequencyDays = dto.ReviewFrequencyDays
            };

            var createdPIP = await _pipService.CreatePIPAsync(pip);

            // Add goals if provided
            foreach (var goalDto in dto.Goals)
            {
                var goal = new PIPGoal
                {
                    PIPId = createdPIP.Id,
                    Title = goalDto.Title,
                    Description = goalDto.Description,
                    MeasurableObjective = goalDto.MeasurableObjective,
                    TargetDate = goalDto.TargetDate
                };

                await _pipService.AddPIPGoalAsync(goal);
            }

            return Success(createdPIP, "PIP created successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error creating PIP: {ex.Message}");
        }
    }

    [HttpGet("pips/{pipId}")]
    public async Task<IActionResult> GetPIP(int pipId)
    {
        try
        {
            var pip = await _pipService.GetPIPByIdAsync(pipId);
            if (pip == null)
                return Error("PIP not found");

            return Success(pip);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIP: {ex.Message}");
        }
    }

    [HttpGet("employees/{employeeId}/pips")]
    public async Task<IActionResult> GetEmployeePIPs(int employeeId)
    {
        try
        {
            var pips = await _pipService.GetEmployeePIPsAsync(employeeId);
            return Success(pips);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving employee PIPs: {ex.Message}");
        }
    }

    [HttpGet("managers/{managerId}/pips")]
    public async Task<IActionResult> GetManagerPIPs(int managerId)
    {
        try
        {
            var pips = await _pipService.GetManagerPIPsAsync(managerId);
            return Success(pips);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving manager PIPs: {ex.Message}");
        }
    }

    [HttpGet("employees/{employeeId}/active-pip")]
    public async Task<IActionResult> GetActivePIP(int employeeId)
    {
        try
        {
            var pip = await _pipService.GetActivePIPByEmployeeAsync(employeeId);
            return Success(pip);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving active PIP: {ex.Message}");
        }
    }

    [HttpPut("pips/{pipId}")]
    public async Task<IActionResult> UpdatePIP(int pipId, [FromBody] UpdatePIPDto dto)
    {
        try
        {
            var pip = await _pipService.GetPIPByIdAsync(pipId);
            if (pip == null)
                return Error("PIP not found");

            // Update properties
            if (!string.IsNullOrEmpty(dto.Title))
                pip.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                pip.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.PerformanceIssues))
                pip.PerformanceIssues = dto.PerformanceIssues;
            if (!string.IsNullOrEmpty(dto.ExpectedImprovements))
                pip.ExpectedImprovements = dto.ExpectedImprovements;
            if (!string.IsNullOrEmpty(dto.SupportProvided))
                pip.SupportProvided = dto.SupportProvided;
            if (dto.StartDate.HasValue)
                pip.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue)
                pip.EndDate = dto.EndDate.Value;
            if (dto.ReviewFrequencyDays.HasValue)
                pip.ReviewFrequencyDays = dto.ReviewFrequencyDays.Value;
            if (dto.Status.HasValue)
                pip.Status = dto.Status.Value;
            if (!string.IsNullOrEmpty(dto.FinalOutcome))
                pip.FinalOutcome = dto.FinalOutcome;
            if (dto.IsSuccessful.HasValue)
                pip.IsSuccessful = dto.IsSuccessful.Value;
            if (!string.IsNullOrEmpty(dto.ManagerNotes))
                pip.ManagerNotes = dto.ManagerNotes;
            if (!string.IsNullOrEmpty(dto.HRNotes))
                pip.HRNotes = dto.HRNotes;

            var updatedPIP = await _pipService.UpdatePIPAsync(pip);
            return Success(updatedPIP, "PIP updated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error updating PIP: {ex.Message}");
        }
    }

    [HttpPut("pips/{pipId}/start")]
    public async Task<IActionResult> StartPIP(int pipId)
    {
        try
        {
            var success = await _pipService.StartPIPAsync(pipId);
            if (!success)
                return Error("PIP not found");

            return Success("PIP started successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error starting PIP: {ex.Message}");
        }
    }

    [HttpPut("pips/{pipId}/complete")]
    public async Task<IActionResult> CompletePIP(int pipId, [FromBody] CompletePIPRequest request)
    {
        try
        {
            var success = await _pipService.CompletePIPAsync(pipId, request.IsSuccessful, request.FinalOutcome);
            if (!success)
                return Error("PIP not found");

            return Success("PIP completed successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error completing PIP: {ex.Message}");
        }
    }

    #endregion

    #region PIP Goals

    [HttpPost("pips/{pipId}/goals")]
    public async Task<IActionResult> AddPIPGoal(int pipId, [FromBody] CreatePIPGoalDto dto)
    {
        try
        {
            var goal = new PIPGoal
            {
                PIPId = pipId,
                Title = dto.Title,
                Description = dto.Description,
                MeasurableObjective = dto.MeasurableObjective,
                TargetDate = dto.TargetDate
            };

            var createdGoal = await _pipService.AddPIPGoalAsync(goal);
            return Success(createdGoal, "PIP goal added successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error adding PIP goal: {ex.Message}");
        }
    }

    [HttpGet("pips/{pipId}/goals")]
    public async Task<IActionResult> GetPIPGoals(int pipId)
    {
        try
        {
            var goals = await _pipService.GetPIPGoalsAsync(pipId);
            return Success(goals);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIP goals: {ex.Message}");
        }
    }

    [HttpPut("pip-goals/{goalId}/progress")]
    public async Task<IActionResult> UpdatePIPGoalProgress(int goalId, [FromBody] UpdatePIPGoalProgressRequest request)
    {
        try
        {
            var success = await _pipService.UpdatePIPGoalProgressAsync(goalId, request.ProgressPercentage, request.Comments);
            if (!success)
                return Error("PIP goal not found");

            return Success("PIP goal progress updated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error updating PIP goal progress: {ex.Message}");
        }
    }

    #endregion

    #region PIP Reviews

    [HttpPost("pips/{pipId}/reviews")]
    public async Task<IActionResult> AddPIPReview(int pipId, [FromBody] CreatePIPReviewDto dto)
    {
        try
        {
            var review = new PIPReview
            {
                PIPId = pipId,
                ReviewedBy = int.Parse(User.FindFirst("EmployeeId")?.Value ?? "0"),
                ProgressSummary = dto.ProgressSummary,
                EmployeeFeedback = dto.EmployeeFeedback,
                ManagerFeedback = dto.ManagerFeedback,
                ChallengesFaced = dto.ChallengesFaced,
                SupportProvided = dto.SupportProvided,
                NextSteps = dto.NextSteps,
                OverallProgress = dto.OverallProgress,
                IsOnTrack = dto.IsOnTrack,
                NextReviewDate = dto.NextReviewDate,
                RecommendedActions = dto.RecommendedActions
            };

            var createdReview = await _pipService.AddPIPReviewAsync(review);
            return Success(createdReview, "PIP review added successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error adding PIP review: {ex.Message}");
        }
    }

    [HttpGet("pips/{pipId}/reviews")]
    public async Task<IActionResult> GetPIPReviews(int pipId)
    {
        try
        {
            var reviews = await _pipService.GetPIPReviewsAsync(pipId);
            return Success(reviews);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIP reviews: {ex.Message}");
        }
    }

    [HttpGet("pips/requiring-review")]
    public async Task<IActionResult> GetPIPsRequiringReview()
    {
        try
        {
            var pips = await _pipService.GetPIPsRequiringReviewAsync();
            return Success(pips);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIPs requiring review: {ex.Message}");
        }
    }

    [HttpPut("pips/{pipId}/schedule-review")]
    public async Task<IActionResult> ScheduleNextReview(int pipId, [FromBody] ScheduleReviewRequest request)
    {
        try
        {
            var success = await _pipService.ScheduleNextReviewAsync(pipId, request.NextReviewDate);
            if (!success)
                return Error("PIP not found");

            return Success("Next review scheduled successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error scheduling next review: {ex.Message}");
        }
    }

    #endregion

    #region PIP Analytics

    [HttpGet("analytics/success-rate")]
    public async Task<IActionResult> GetPIPSuccessRate([FromQuery] int? managerId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var successRate = await _pipService.GetPIPSuccessRateAsync(managerId, startDate, endDate);
            return Success(new { SuccessRate = successRate });
        }
        catch (Exception ex)
        {
            return Error($"Error calculating PIP success rate: {ex.Message}");
        }
    }

    [HttpGet("analytics/status-distribution")]
    public async Task<IActionResult> GetPIPStatusDistribution()
    {
        try
        {
            var distribution = await _pipService.GetPIPStatusDistributionAsync();
            return Success(distribution);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIP status distribution: {ex.Message}");
        }
    }

    [HttpGet("pips/nearing-deadline")]
    public async Task<IActionResult> GetPIPsNearingDeadline([FromQuery] int daysThreshold = 7)
    {
        try
        {
            var pips = await _pipService.GetPIPsNearingDeadlineAsync(daysThreshold);
            return Success(pips);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving PIPs nearing deadline: {ex.Message}");
        }
    }

    #endregion

    #region PIP Templates and Workflows

    [HttpPost("pips/from-template")]
    public async Task<IActionResult> CreatePIPFromTemplate([FromBody] CreatePIPFromTemplateRequest request)
    {
        try
        {
            var pip = await _pipService.CreatePIPFromTemplateAsync(request.EmployeeId, request.ManagerId, request.TemplateName);
            return Success(pip, "PIP created from template successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error creating PIP from template: {ex.Message}");
        }
    }

    [HttpPost("pips/{pipId}/notifications")]
    public async Task<IActionResult> SendPIPNotification(int pipId, [FromBody] SendNotificationRequest request)
    {
        try
        {
            var success = await _pipService.SendPIPNotificationAsync(pipId, request.NotificationType);
            if (!success)
                return Error("PIP not found");

            return Success("Notification sent successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error sending PIP notification: {ex.Message}");
        }
    }

    #endregion
}

// Request DTOs
public class CompletePIPRequest
{
    public bool IsSuccessful { get; set; }
    public string FinalOutcome { get; set; } = string.Empty;
}

public class UpdatePIPGoalProgressRequest
{
    public decimal ProgressPercentage { get; set; }
    public string? Comments { get; set; }
}

public class ScheduleReviewRequest
{
    public DateTime NextReviewDate { get; set; }
}

public class CreatePIPFromTemplateRequest
{
    public int EmployeeId { get; set; }
    public int ManagerId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
}

public class SendNotificationRequest
{
    public string NotificationType { get; set; } = string.Empty;
}