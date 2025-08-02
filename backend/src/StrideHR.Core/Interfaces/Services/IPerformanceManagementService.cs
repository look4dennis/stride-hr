using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IPerformanceManagementService
{
    // Performance Goals
    Task<PerformanceGoal> CreateGoalAsync(PerformanceGoal goal);
    Task<PerformanceGoal> UpdateGoalAsync(PerformanceGoal goal);
    Task<PerformanceGoal?> GetGoalByIdAsync(int goalId);
    Task<IEnumerable<PerformanceGoal>> GetEmployeeGoalsAsync(int employeeId);
    Task<IEnumerable<PerformanceGoal>> GetManagerGoalsAsync(int managerId);
    Task<PerformanceGoalCheckIn> AddGoalCheckInAsync(PerformanceGoalCheckIn checkIn);
    Task<bool> UpdateGoalProgressAsync(int goalId, decimal progressPercentage, string? notes = null);
    
    // Performance Reviews
    Task<PerformanceReview> CreateReviewAsync(PerformanceReview review);
    Task<PerformanceReview> UpdateReviewAsync(PerformanceReview review);
    Task<PerformanceReview?> GetReviewByIdAsync(int reviewId);
    Task<IEnumerable<PerformanceReview>> GetEmployeeReviewsAsync(int employeeId);
    Task<IEnumerable<PerformanceReview>> GetManagerReviewsAsync(int managerId);
    Task<bool> SubmitSelfAssessmentAsync(int reviewId, string selfAssessment);
    Task<bool> CompleteManagerReviewAsync(int reviewId, string managerComments, PerformanceRating overallRating);
    
    // 360-Degree Feedback
    Task<PerformanceFeedback> SubmitFeedbackAsync(PerformanceFeedback feedback);
    Task<IEnumerable<PerformanceFeedback>> GetReviewFeedbackAsync(int reviewId);
    Task<IEnumerable<PerformanceFeedback>> GetPendingFeedbackRequestsAsync(int reviewerId);
    Task<bool> RequestFeedbackAsync(int reviewId, int reviewerId, FeedbackType feedbackType);
    
    // Analytics
    Task<decimal> GetEmployeePerformanceScoreAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetTeamPerformanceMetricsAsync(int managerId);
    Task<IEnumerable<PerformanceGoal>> GetOverdueGoalsAsync();
    Task<IEnumerable<PerformanceReview>> GetOverdueReviewsAsync();
}