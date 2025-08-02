using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class PIPManagementService : IPIPManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PIPManagementService> _logger;

    public PIPManagementService(
        IUnitOfWork unitOfWork,
        ILogger<PIPManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region PIP Management

    public async Task<PerformanceImprovementPlan> CreatePIPAsync(PerformanceImprovementPlan pip)
    {
        try
        {
            pip.CreatedAt = DateTime.UtcNow;
            pip.Status = PIPStatus.Draft;
            
            await _unitOfWork.PerformanceImprovementPlans.AddAsync(pip);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP created for employee {EmployeeId}: {Title}", 
                pip.EmployeeId, pip.Title);
            
            return pip;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIP for employee {EmployeeId}", pip.EmployeeId);
            throw;
        }
    }

    public async Task<PerformanceImprovementPlan> UpdatePIPAsync(PerformanceImprovementPlan pip)
    {
        try
        {
            pip.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceImprovementPlans.UpdateAsync(pip);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP updated: {PIPId}", pip.Id);
            
            return pip;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PIP {PIPId}", pip.Id);
            throw;
        }
    }

    public async Task<PerformanceImprovementPlan?> GetPIPByIdAsync(int pipId)
    {
        return await _unitOfWork.PerformanceImprovementPlans.GetByIdAsync(pipId,
            p => p.Employee,
            p => p.Manager,
            p => p.HR,
            p => p.PerformanceReview,
            p => p.Goals,
            p => p.Reviews);
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetEmployeePIPsAsync(int employeeId)
    {
        return await _unitOfWork.PerformanceImprovementPlans.GetByEmployeeIdAsync(employeeId);
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetManagerPIPsAsync(int managerId)
    {
        return await _unitOfWork.PerformanceImprovementPlans.GetByManagerIdAsync(managerId);
    }

    public async Task<PerformanceImprovementPlan?> GetActivePIPByEmployeeAsync(int employeeId)
    {
        return await _unitOfWork.PerformanceImprovementPlans.GetActiveByEmployeeIdAsync(employeeId);
    }

    public async Task<bool> StartPIPAsync(int pipId)
    {
        try
        {
            var pip = await _unitOfWork.PerformanceImprovementPlans.GetByIdAsync(pipId);
            if (pip == null) return false;
            
            pip.Status = PIPStatus.Active;
            pip.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceImprovementPlans.UpdateAsync(pip);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP started: {PIPId}", pipId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting PIP {PIPId}", pipId);
            throw;
        }
    }

    public async Task<bool> CompletePIPAsync(int pipId, bool isSuccessful, string finalOutcome)
    {
        try
        {
            var pip = await _unitOfWork.PerformanceImprovementPlans.GetByIdAsync(pipId);
            if (pip == null) return false;
            
            pip.Status = isSuccessful ? PIPStatus.Successful : PIPStatus.Failed;
            pip.IsSuccessful = isSuccessful;
            pip.FinalOutcome = finalOutcome;
            pip.CompletedDate = DateTime.UtcNow;
            pip.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PerformanceImprovementPlans.UpdateAsync(pip);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP completed: {PIPId}, Successful: {IsSuccessful}", pipId, isSuccessful);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing PIP {PIPId}", pipId);
            throw;
        }
    }

    #endregion

    #region PIP Goals

    public async Task<PIPGoal> AddPIPGoalAsync(PIPGoal goal)
    {
        try
        {
            goal.CreatedAt = DateTime.UtcNow;
            goal.Status = PerformanceGoalStatus.Active;
            
            await _unitOfWork.PIPGoals.AddAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP goal added for PIP {PIPId}: {Title}", goal.PIPId, goal.Title);
            
            return goal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding PIP goal for PIP {PIPId}", goal.PIPId);
            throw;
        }
    }

    public async Task<PIPGoal> UpdatePIPGoalAsync(PIPGoal goal)
    {
        try
        {
            goal.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.PIPGoals.UpdateAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP goal updated: {GoalId}", goal.Id);
            
            return goal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PIP goal {GoalId}", goal.Id);
            throw;
        }
    }

    public async Task<bool> UpdatePIPGoalProgressAsync(int goalId, decimal progressPercentage, string? comments = null)
    {
        try
        {
            var goal = await _unitOfWork.PIPGoals.GetByIdAsync(goalId);
            if (goal == null) return false;
            
            goal.ProgressPercentage = progressPercentage;
            goal.EmployeeComments = comments ?? goal.EmployeeComments;
            goal.UpdatedAt = DateTime.UtcNow;
            
            // Update status based on progress
            if (progressPercentage >= 100)
            {
                goal.Status = PerformanceGoalStatus.Completed;
                goal.CompletedDate = DateTime.UtcNow;
                goal.IsAchieved = true;
            }
            else if (progressPercentage > 0)
            {
                goal.Status = PerformanceGoalStatus.InProgress;
            }
            
            await _unitOfWork.PIPGoals.UpdateAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PIP goal progress for goal {GoalId}", goalId);
            throw;
        }
    }

    public async Task<IEnumerable<PIPGoal>> GetPIPGoalsAsync(int pipId)
    {
        return await _unitOfWork.PIPGoals.FindAsync(g => g.PIPId == pipId && !g.IsDeleted);
    }

    #endregion

    #region PIP Reviews

    public async Task<PIPReview> AddPIPReviewAsync(PIPReview review)
    {
        try
        {
            review.CreatedAt = DateTime.UtcNow;
            review.ReviewDate = DateTime.UtcNow;
            
            await _unitOfWork.PIPReviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP review added for PIP {PIPId} by {ReviewerId}", 
                review.PIPId, review.ReviewedBy);
            
            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding PIP review for PIP {PIPId}", review.PIPId);
            throw;
        }
    }

    public async Task<IEnumerable<PIPReview>> GetPIPReviewsAsync(int pipId)
    {
        return await _unitOfWork.PIPReviews.FindAsync(r => r.PIPId == pipId && !r.IsDeleted,
            r => r.ReviewedByEmployee);
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsRequiringReviewAsync()
    {
        return await _unitOfWork.PerformanceImprovementPlans.GetPIPsRequiringReviewAsync();
    }

    public async Task<bool> ScheduleNextReviewAsync(int pipId, DateTime nextReviewDate)
    {
        try
        {
            var pip = await _unitOfWork.PerformanceImprovementPlans.GetByIdAsync(pipId);
            if (pip == null) return false;
            
            // Get the latest review and update its next review date
            var latestReview = await _unitOfWork.PIPReviews
                .FindAsync(r => r.PIPId == pipId && !r.IsDeleted);
            
            var review = latestReview.OrderByDescending(r => r.ReviewDate).FirstOrDefault();
            if (review != null)
            {
                review.NextReviewDate = nextReviewDate;
                review.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.PIPReviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Next review scheduled for PIP {PIPId} on {NextReviewDate}", 
                    pipId, nextReviewDate);
                
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling next review for PIP {PIPId}", pipId);
            throw;
        }
    }

    #endregion

    #region PIP Analytics

    public async Task<decimal> GetPIPSuccessRateAsync(int? managerId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var pips = await _unitOfWork.PerformanceImprovementPlans.GetAllAsync();
            
            if (managerId.HasValue)
                pips = pips.Where(p => p.ManagerId == managerId.Value);
            
            if (startDate.HasValue)
                pips = pips.Where(p => p.StartDate >= startDate.Value);
            
            if (endDate.HasValue)
                pips = pips.Where(p => p.EndDate <= endDate.Value);
            
            var completedPips = pips.Where(p => p.Status == PIPStatus.Successful || p.Status == PIPStatus.Failed).ToList();
            
            if (!completedPips.Any()) return 0;
            
            var successfulPips = completedPips.Count(p => p.Status == PIPStatus.Successful);
            
            return (decimal)successfulPips / completedPips.Count * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating PIP success rate");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetPIPStatusDistributionAsync()
    {
        try
        {
            var pips = await _unitOfWork.PerformanceImprovementPlans.GetAllAsync();
            
            var distribution = new Dictionary<string, int>();
            
            foreach (PIPStatus status in Enum.GetValues<PIPStatus>())
            {
                distribution[status.ToString()] = pips.Count(p => p.Status == status);
            }
            
            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating PIP status distribution");
            throw;
        }
    }

    public async Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsNearingDeadlineAsync(int daysThreshold = 7)
    {
        try
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            
            var pips = await _unitOfWork.PerformanceImprovementPlans.GetActivePIPsAsync();
            
            return pips.Where(p => p.EndDate <= thresholdDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PIPs nearing deadline");
            throw;
        }
    }

    #endregion

    #region PIP Templates and Workflows

    public async Task<PerformanceImprovementPlan> CreatePIPFromTemplateAsync(int employeeId, int managerId, string templateName)
    {
        try
        {
            // This would typically load from a template repository
            // For now, creating a basic template
            var pip = new PerformanceImprovementPlan
            {
                EmployeeId = employeeId,
                ManagerId = managerId,
                Title = $"Performance Improvement Plan - {templateName}",
                Description = "This PIP is designed to help improve performance in identified areas.",
                PerformanceIssues = "Performance issues to be addressed will be detailed here.",
                ExpectedImprovements = "Expected improvements and success criteria will be outlined here.",
                SupportProvided = "Support and resources provided will be listed here.",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(90), // 90-day PIP
                ReviewFrequencyDays = 30,
                Status = PIPStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.PerformanceImprovementPlans.AddAsync(pip);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("PIP created from template {TemplateName} for employee {EmployeeId}", 
                templateName, employeeId);
            
            return pip;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIP from template for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> SendPIPNotificationAsync(int pipId, string notificationType)
    {
        try
        {
            var pip = await GetPIPByIdAsync(pipId);
            if (pip == null) return false;
            
            // This would typically integrate with a notification service
            // For now, just logging the notification
            _logger.LogInformation("PIP notification sent: {NotificationType} for PIP {PIPId} to employee {EmployeeId}", 
                notificationType, pipId, pip.EmployeeId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending PIP notification for PIP {PIPId}", pipId);
            throw;
        }
    }

    #endregion
}