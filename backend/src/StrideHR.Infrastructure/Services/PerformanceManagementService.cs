using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class PerformanceManagementService : IPerformanceManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PerformanceManagementService> _logger;

    public PerformanceManagementService(
        IUnitOfWork unitOfWork,
        ILogger<PerformanceManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Performance Goals

    public async Task<PerformanceGoal> CreateGoalAsync(PerformanceGoal goal)
    {
        try
        {
            goal.CreatedAt = DateTime.UtcNow;
            goal.Status = PerformanceGoalStatus.Active;
            
            await _unitOfWork.PerformanceGoals.AddAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Performance goal created for employee {EmployeeId}: {Title}", 
                goal.EmployeeId, goal.Title);
            
            return goal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating performance goal for employee {EmployeeId}", goal.EmployeeId);
            throw;
        }
    }

    public async Task<PerformanceGoal> UpdateGoalAsync(PerformanceGoal goal)
    {
        try
        {
            goal.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceGoals.UpdateAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Performance goal updated: {GoalId}", goal.Id);
            
            return goal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating performance goal {GoalId}", goal.Id);
            throw;
        }
    }

    public async Task<PerformanceGoal?> GetGoalByIdAsync(int goalId)
    {
        return await _unitOfWork.PerformanceGoals.GetByIdAsync(goalId, 
            g => g.Employee, 
            g => g.Manager, 
            g => g.CheckIns);
    }

    public async Task<IEnumerable<PerformanceGoal>> GetEmployeeGoalsAsync(int employeeId)
    {
        return await _unitOfWork.PerformanceGoals.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<IEnumerable<PerformanceGoal>> GetManagerGoalsAsync(int managerId)
    {
        return await _unitOfWork.PerformanceGoals.GetByManagerIdAsync(managerId);
    }

    public async Task<PerformanceGoalCheckIn> AddGoalCheckInAsync(PerformanceGoalCheckIn checkIn)
    {
        try
        {
            checkIn.CreatedAt = DateTime.UtcNow;
            checkIn.CheckInDate = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceGoalCheckIns.AddAsync(checkIn);
            
            // Update goal progress
            var goal = await _unitOfWork.PerformanceGoals.GetByIdAsync(checkIn.PerformanceGoalId);
            if (goal != null)
            {
                goal.ProgressPercentage = checkIn.ProgressPercentage;
                goal.UpdatedAt = DateTime.UtcNow;
                
                // Update status based on progress
                if (checkIn.ProgressPercentage >= 100)
                {
                    goal.Status = PerformanceGoalStatus.Completed;
                    goal.CompletedDate = DateTime.UtcNow;
                }
                else if (checkIn.ProgressPercentage > 0)
                {
                    goal.Status = PerformanceGoalStatus.InProgress;
                }
                
                await _unitOfWork.PerformanceGoals.UpdateAsync(goal);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Goal check-in added for goal {GoalId} with progress {Progress}%", 
                checkIn.PerformanceGoalId, checkIn.ProgressPercentage);
            
            return checkIn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding goal check-in for goal {GoalId}", checkIn.PerformanceGoalId);
            throw;
        }
    }

    public async Task<bool> UpdateGoalProgressAsync(int goalId, decimal progressPercentage, string? notes = null)
    {
        try
        {
            var goal = await _unitOfWork.PerformanceGoals.GetByIdAsync(goalId);
            if (goal == null) return false;
            
            goal.ProgressPercentage = progressPercentage;
            goal.Notes = notes ?? goal.Notes;
            goal.UpdatedAt = DateTime.UtcNow;
            
            // Update status based on progress
            if (progressPercentage >= 100)
            {
                goal.Status = PerformanceGoalStatus.Completed;
                goal.CompletedDate = DateTime.UtcNow;
            }
            else if (progressPercentage > 0)
            {
                goal.Status = PerformanceGoalStatus.InProgress;
            }
            
            await _unitOfWork.PerformanceGoals.UpdateAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating goal progress for goal {GoalId}", goalId);
            throw;
        }
    }

    #endregion

    #region Performance Reviews

    public async Task<PerformanceReview> CreateReviewAsync(PerformanceReview review)
    {
        try
        {
            review.CreatedAt = DateTime.UtcNow;
            review.Status = PerformanceReviewStatus.NotStarted;
            
            await _unitOfWork.PerformanceReviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Performance review created for employee {EmployeeId} for period {Period}", 
                review.EmployeeId, review.ReviewPeriod);
            
            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating performance review for employee {EmployeeId}", review.EmployeeId);
            throw;
        }
    }

    public async Task<PerformanceReview> UpdateReviewAsync(PerformanceReview review)
    {
        try
        {
            review.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceReviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Performance review updated: {ReviewId}", review.Id);
            
            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating performance review {ReviewId}", review.Id);
            throw;
        }
    }

    public async Task<PerformanceReview?> GetReviewByIdAsync(int reviewId)
    {
        return await _unitOfWork.PerformanceReviews.GetByIdAsync(reviewId,
            r => r.Employee,
            r => r.Manager,
            r => r.ApprovedByEmployee,
            r => r.Feedbacks,
            r => r.Goals);
    }

    public async Task<IEnumerable<PerformanceReview>> GetEmployeeReviewsAsync(int employeeId)
    {
        return await _unitOfWork.PerformanceReviews.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<IEnumerable<PerformanceReview>> GetManagerReviewsAsync(int managerId)
    {
        return await _unitOfWork.PerformanceReviews.GetByManagerIdAsync(managerId);
    }

    public async Task<bool> SubmitSelfAssessmentAsync(int reviewId, string selfAssessment)
    {
        try
        {
            var review = await _unitOfWork.PerformanceReviews.GetByIdAsync(reviewId);
            if (review == null) return false;
            
            review.EmployeeSelfAssessment = selfAssessment;
            review.Status = PerformanceReviewStatus.SelfAssessmentComplete;
            review.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceReviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Self assessment submitted for review {ReviewId}", reviewId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting self assessment for review {ReviewId}", reviewId);
            throw;
        }
    }

    public async Task<bool> CompleteManagerReviewAsync(int reviewId, string managerComments, PerformanceRating overallRating)
    {
        try
        {
            var review = await _unitOfWork.PerformanceReviews.GetByIdAsync(reviewId);
            if (review == null) return false;
            
            review.ManagerComments = managerComments;
            review.OverallRating = overallRating;
            review.Status = PerformanceReviewStatus.ManagerReviewComplete;
            review.UpdatedAt = DateTime.UtcNow;
            
            // Calculate overall score based on rating
            review.OverallScore = (decimal)overallRating * 20; // Convert 1-5 rating to 20-100 score
            
            await _unitOfWork.PerformanceReviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Manager review completed for review {ReviewId} with rating {Rating}", 
                reviewId, overallRating);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing manager review for review {ReviewId}", reviewId);
            throw;
        }
    }

    #endregion

    #region 360-Degree Feedback

    public async Task<PerformanceFeedback> SubmitFeedbackAsync(PerformanceFeedback feedback)
    {
        try
        {
            feedback.CreatedAt = DateTime.UtcNow;
            feedback.IsSubmitted = true;
            feedback.SubmittedDate = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceFeedbacks.AddAsync(feedback);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Feedback submitted by {ReviewerId} for {RevieweeId} in review {ReviewId}", 
                feedback.ReviewerId, feedback.RevieweeId, feedback.PerformanceReviewId);
            
            return feedback;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for review {ReviewId}", feedback.PerformanceReviewId);
            throw;
        }
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetReviewFeedbackAsync(int reviewId)
    {
        return await _unitOfWork.PerformanceFeedbacks.GetByReviewIdAsync(reviewId);
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetPendingFeedbackRequestsAsync(int reviewerId)
    {
        return await _unitOfWork.PerformanceFeedbacks.GetPendingFeedbackAsync(reviewerId);
    }

    public async Task<bool> RequestFeedbackAsync(int reviewId, int reviewerId, FeedbackType feedbackType)
    {
        try
        {
            var review = await _unitOfWork.PerformanceReviews.GetByIdAsync(reviewId);
            if (review == null) return false;
            
            var feedback = new PerformanceFeedback
            {
                PerformanceReviewId = reviewId,
                RevieweeId = review.EmployeeId,
                ReviewerId = reviewerId,
                FeedbackType = feedbackType,
                CompetencyArea = "General", // Default, can be customized
                Rating = PerformanceRating.MeetsExpectations, // Default
                Comments = string.Empty,
                IsSubmitted = false,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.PerformanceFeedbacks.AddAsync(feedback);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Feedback requested from {ReviewerId} for review {ReviewId}", reviewerId, reviewId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting feedback for review {ReviewId}", reviewId);
            throw;
        }
    }

    #endregion

    #region Analytics

    public async Task<decimal> GetEmployeePerformanceScoreAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var reviews = await _unitOfWork.PerformanceReviews.GetByEmployeeIdAsync(employeeId);
            
            if (startDate.HasValue)
                reviews = reviews.Where(r => r.ReviewStartDate >= startDate.Value);
            
            if (endDate.HasValue)
                reviews = reviews.Where(r => r.ReviewEndDate <= endDate.Value);
            
            var completedReviews = reviews.Where(r => r.OverallScore.HasValue).ToList();
            
            if (!completedReviews.Any()) return 0;
            
            return completedReviews.Average(r => r.OverallScore!.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance score for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetTeamPerformanceMetricsAsync(int managerId)
    {
        try
        {
            var reviews = await _unitOfWork.PerformanceReviews.GetByManagerIdAsync(managerId);
            var completedReviews = reviews.Where(r => r.OverallScore.HasValue).ToList();
            
            var metrics = new Dictionary<string, decimal>
            {
                ["TotalReviews"] = reviews.Count(),
                ["CompletedReviews"] = completedReviews.Count,
                ["AverageScore"] = completedReviews.Any() ? completedReviews.Average(r => r.OverallScore!.Value) : 0,
                ["HighPerformers"] = completedReviews.Count(r => r.OverallScore >= 80),
                ["LowPerformers"] = completedReviews.Count(r => r.OverallScore < 60),
                ["RequirePIP"] = reviews.Count(r => r.RequiresPIP)
            };
            
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating team performance metrics for manager {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<PerformanceGoal>> GetOverdueGoalsAsync()
    {
        return await _unitOfWork.PerformanceGoals.GetOverdueGoalsAsync();
    }

    public async Task<IEnumerable<PerformanceReview>> GetOverdueReviewsAsync()
    {
        return await _unitOfWork.PerformanceReviews.GetOverdueReviewsAsync();
    }

    #endregion
}