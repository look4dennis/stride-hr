using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;

namespace StrideHR.API.Controllers;

[Authorize]
public class PerformanceManagementController : BaseController
{
    private readonly IPerformanceManagementService _performanceService;

    public PerformanceManagementController(IPerformanceManagementService performanceService)
    {
        _performanceService = performanceService;
    }

    #region Performance Goals

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreatePerformanceGoalDto dto)
    {
        try
        {
            var goal = new PerformanceGoal
            {
                EmployeeId = dto.EmployeeId,
                Title = dto.Title,
                Description = dto.Description,
                SuccessCriteria = dto.SuccessCriteria,
                StartDate = dto.StartDate,
                TargetDate = dto.TargetDate,
                WeightPercentage = dto.WeightPercentage,
                Notes = dto.Notes
            };

            var createdGoal = await _performanceService.CreateGoalAsync(goal);
            return Success(createdGoal, "Performance goal created successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error creating performance goal: {ex.Message}");
        }
    }

    [HttpGet("goals/{goalId}")]
    public async Task<IActionResult> GetGoal(int goalId)
    {
        try
        {
            var goal = await _performanceService.GetGoalByIdAsync(goalId);
            if (goal == null)
                return Error("Performance goal not found");

            return Success(goal);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving performance goal: {ex.Message}");
        }
    }

    [HttpGet("employees/{employeeId}/goals")]
    public async Task<IActionResult> GetEmployeeGoals(int employeeId)
    {
        try
        {
            var goals = await _performanceService.GetEmployeeGoalsAsync(employeeId);
            return Success(goals);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving employee goals: {ex.Message}");
        }
    }

    [HttpGet("managers/{managerId}/goals")]
    public async Task<IActionResult> GetManagerGoals(int managerId)
    {
        try
        {
            var goals = await _performanceService.GetManagerGoalsAsync(managerId);
            return Success(goals);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving manager goals: {ex.Message}");
        }
    }

    [HttpPut("goals/{goalId}")]
    public async Task<IActionResult> UpdateGoal(int goalId, [FromBody] UpdatePerformanceGoalDto dto)
    {
        try
        {
            var goal = await _performanceService.GetGoalByIdAsync(goalId);
            if (goal == null)
                return Error("Performance goal not found");

            // Update properties
            if (!string.IsNullOrEmpty(dto.Title))
                goal.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description))
                goal.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.SuccessCriteria))
                goal.SuccessCriteria = dto.SuccessCriteria;
            if (dto.StartDate.HasValue)
                goal.StartDate = dto.StartDate.Value;
            if (dto.TargetDate.HasValue)
                goal.TargetDate = dto.TargetDate.Value;
            if (dto.WeightPercentage.HasValue)
                goal.WeightPercentage = dto.WeightPercentage.Value;
            if (dto.Status.HasValue)
                goal.Status = dto.Status.Value;
            if (dto.ProgressPercentage.HasValue)
                goal.ProgressPercentage = dto.ProgressPercentage.Value;
            if (!string.IsNullOrEmpty(dto.Notes))
                goal.Notes = dto.Notes;
            if (dto.FinalRating.HasValue)
                goal.FinalRating = dto.FinalRating.Value;
            if (!string.IsNullOrEmpty(dto.ManagerComments))
                goal.ManagerComments = dto.ManagerComments;

            var updatedGoal = await _performanceService.UpdateGoalAsync(goal);
            return Success(updatedGoal, "Performance goal updated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error updating performance goal: {ex.Message}");
        }
    }

    [HttpPost("goals/{goalId}/checkins")]
    public async Task<IActionResult> AddGoalCheckIn(int goalId, [FromBody] PerformanceGoalCheckIn checkIn)
    {
        try
        {
            checkIn.PerformanceGoalId = goalId;
            var createdCheckIn = await _performanceService.AddGoalCheckInAsync(checkIn);
            return Success(createdCheckIn, "Goal check-in added successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error adding goal check-in: {ex.Message}");
        }
    }

    [HttpPut("goals/{goalId}/progress")]
    public async Task<IActionResult> UpdateGoalProgress(int goalId, [FromBody] UpdateGoalProgressRequest request)
    {
        try
        {
            var success = await _performanceService.UpdateGoalProgressAsync(goalId, request.ProgressPercentage, request.Notes);
            if (!success)
                return Error("Performance goal not found");

            return Success("Goal progress updated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error updating goal progress: {ex.Message}");
        }
    }

    #endregion

    #region Performance Reviews

    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReviewDto dto)
    {
        try
        {
            var review = new PerformanceReview
            {
                EmployeeId = dto.EmployeeId,
                ReviewPeriod = dto.ReviewPeriod,
                ReviewStartDate = dto.ReviewStartDate,
                ReviewEndDate = dto.ReviewEndDate,
                DueDate = dto.DueDate
            };

            var createdReview = await _performanceService.CreateReviewAsync(review);
            return Success(createdReview, "Performance review created successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error creating performance review: {ex.Message}");
        }
    }

    [HttpGet("reviews/{reviewId}")]
    public async Task<IActionResult> GetReview(int reviewId)
    {
        try
        {
            var review = await _performanceService.GetReviewByIdAsync(reviewId);
            if (review == null)
                return Error("Performance review not found");

            return Success(review);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving performance review: {ex.Message}");
        }
    }

    [HttpGet("employees/{employeeId}/reviews")]
    public async Task<IActionResult> GetEmployeeReviews(int employeeId)
    {
        try
        {
            var reviews = await _performanceService.GetEmployeeReviewsAsync(employeeId);
            return Success(reviews);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving employee reviews: {ex.Message}");
        }
    }

    [HttpGet("managers/{managerId}/reviews")]
    public async Task<IActionResult> GetManagerReviews(int managerId)
    {
        try
        {
            var reviews = await _performanceService.GetManagerReviewsAsync(managerId);
            return Success(reviews);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving manager reviews: {ex.Message}");
        }
    }

    [HttpPut("reviews/{reviewId}/self-assessment")]
    public async Task<IActionResult> SubmitSelfAssessment(int reviewId, [FromBody] SelfAssessmentRequest request)
    {
        try
        {
            var success = await _performanceService.SubmitSelfAssessmentAsync(reviewId, request.SelfAssessment);
            if (!success)
                return Error("Performance review not found");

            return Success("Self assessment submitted successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error submitting self assessment: {ex.Message}");
        }
    }

    [HttpPut("reviews/{reviewId}/manager-review")]
    public async Task<IActionResult> CompleteManagerReview(int reviewId, [FromBody] ManagerReviewRequest request)
    {
        try
        {
            var success = await _performanceService.CompleteManagerReviewAsync(reviewId, request.ManagerComments, request.OverallRating);
            if (!success)
                return Error("Performance review not found");

            return Success("Manager review completed successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error completing manager review: {ex.Message}");
        }
    }

    #endregion

    #region 360-Degree Feedback

    [HttpPost("feedback")]
    public async Task<IActionResult> SubmitFeedback([FromBody] CreatePerformanceFeedbackDto dto)
    {
        try
        {
            var feedback = new PerformanceFeedback
            {
                PerformanceReviewId = dto.PerformanceReviewId,
                RevieweeId = dto.RevieweeId,
                ReviewerId = int.Parse(User.FindFirst("EmployeeId")?.Value ?? "0"),
                FeedbackType = dto.FeedbackType,
                CompetencyArea = dto.CompetencyArea,
                Rating = dto.Rating,
                Comments = dto.Comments,
                Strengths = dto.Strengths,
                AreasForImprovement = dto.AreasForImprovement,
                SpecificExamples = dto.SpecificExamples
            };

            var submittedFeedback = await _performanceService.SubmitFeedbackAsync(feedback);
            return Success(submittedFeedback, "Feedback submitted successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error submitting feedback: {ex.Message}");
        }
    }

    [HttpGet("reviews/{reviewId}/feedback")]
    public async Task<IActionResult> GetReviewFeedback(int reviewId)
    {
        try
        {
            var feedback = await _performanceService.GetReviewFeedbackAsync(reviewId);
            return Success(feedback);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving review feedback: {ex.Message}");
        }
    }

    [HttpGet("employees/{employeeId}/pending-feedback")]
    public async Task<IActionResult> GetPendingFeedbackRequests(int employeeId)
    {
        try
        {
            var pendingFeedback = await _performanceService.GetPendingFeedbackRequestsAsync(employeeId);
            return Success(pendingFeedback);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving pending feedback requests: {ex.Message}");
        }
    }

    [HttpPost("reviews/{reviewId}/request-feedback")]
    public async Task<IActionResult> RequestFeedback(int reviewId, [FromBody] FeedbackRequestDto dto)
    {
        try
        {
            var success = await _performanceService.RequestFeedbackAsync(reviewId, dto.ReviewerId, dto.FeedbackType);
            if (!success)
                return Error("Performance review not found");

            return Success("Feedback request sent successfully");
        }
        catch (Exception ex)
        {
            return Error($"Error requesting feedback: {ex.Message}");
        }
    }

    #endregion

    #region Analytics

    [HttpGet("employees/{employeeId}/performance-score")]
    public async Task<IActionResult> GetEmployeePerformanceScore(int employeeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var score = await _performanceService.GetEmployeePerformanceScoreAsync(employeeId, startDate, endDate);
            return Success(new { PerformanceScore = score });
        }
        catch (Exception ex)
        {
            return Error($"Error calculating performance score: {ex.Message}");
        }
    }

    [HttpGet("managers/{managerId}/team-metrics")]
    public async Task<IActionResult> GetTeamPerformanceMetrics(int managerId)
    {
        try
        {
            var metrics = await _performanceService.GetTeamPerformanceMetricsAsync(managerId);
            return Success(metrics);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving team performance metrics: {ex.Message}");
        }
    }

    [HttpGet("overdue-goals")]
    public async Task<IActionResult> GetOverdueGoals()
    {
        try
        {
            var overdueGoals = await _performanceService.GetOverdueGoalsAsync();
            return Success(overdueGoals);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving overdue goals: {ex.Message}");
        }
    }

    [HttpGet("overdue-reviews")]
    public async Task<IActionResult> GetOverdueReviews()
    {
        try
        {
            var overdueReviews = await _performanceService.GetOverdueReviewsAsync();
            return Success(overdueReviews);
        }
        catch (Exception ex)
        {
            return Error($"Error retrieving overdue reviews: {ex.Message}");
        }
    }

    #endregion
}

// Request DTOs
public class UpdateGoalProgressRequest
{
    public decimal ProgressPercentage { get; set; }
    public string? Notes { get; set; }
}

public class SelfAssessmentRequest
{
    public string SelfAssessment { get; set; } = string.Empty;
}

public class ManagerReviewRequest
{
    public string ManagerComments { get; set; } = string.Empty;
    public PerformanceRating OverallRating { get; set; }
}