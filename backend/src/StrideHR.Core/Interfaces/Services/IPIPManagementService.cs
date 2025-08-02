using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IPIPManagementService
{
    // PIP Management
    Task<PerformanceImprovementPlan> CreatePIPAsync(PerformanceImprovementPlan pip);
    Task<PerformanceImprovementPlan> UpdatePIPAsync(PerformanceImprovementPlan pip);
    Task<PerformanceImprovementPlan?> GetPIPByIdAsync(int pipId);
    Task<IEnumerable<PerformanceImprovementPlan>> GetEmployeePIPsAsync(int employeeId);
    Task<IEnumerable<PerformanceImprovementPlan>> GetManagerPIPsAsync(int managerId);
    Task<PerformanceImprovementPlan?> GetActivePIPByEmployeeAsync(int employeeId);
    Task<bool> StartPIPAsync(int pipId);
    Task<bool> CompletePIPAsync(int pipId, bool isSuccessful, string finalOutcome);
    
    // PIP Goals
    Task<PIPGoal> AddPIPGoalAsync(PIPGoal goal);
    Task<PIPGoal> UpdatePIPGoalAsync(PIPGoal goal);
    Task<bool> UpdatePIPGoalProgressAsync(int goalId, decimal progressPercentage, string? comments = null);
    Task<IEnumerable<PIPGoal>> GetPIPGoalsAsync(int pipId);
    
    // PIP Reviews
    Task<PIPReview> AddPIPReviewAsync(PIPReview review);
    Task<IEnumerable<PIPReview>> GetPIPReviewsAsync(int pipId);
    Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsRequiringReviewAsync();
    Task<bool> ScheduleNextReviewAsync(int pipId, DateTime nextReviewDate);
    
    // PIP Analytics
    Task<decimal> GetPIPSuccessRateAsync(int? managerId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetPIPStatusDistributionAsync();
    Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsNearingDeadlineAsync(int daysThreshold = 7);
    
    // PIP Templates and Workflows
    Task<PerformanceImprovementPlan> CreatePIPFromTemplateAsync(int employeeId, int managerId, string templateName);
    Task<bool> SendPIPNotificationAsync(int pipId, string notificationType);
}