using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SurveyDistributionRepository : Repository<SurveyDistribution>, ISurveyDistributionRepository
{
    public SurveyDistributionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SurveyDistribution>> GetBySurveyAsync(int surveyId)
    {
        return await _context.SurveyDistributions
            .Where(d => d.SurveyId == surveyId && d.IsActive && !d.IsDeleted)
            .Include(d => d.TargetEmployee)
            .Include(d => d.TargetBranch)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyDistribution>> GetByEmployeeAsync(int employeeId)
    {
        return await _context.SurveyDistributions
            .Where(d => d.TargetEmployeeId == employeeId && d.IsActive && !d.IsDeleted)
            .Include(d => d.Survey)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<SurveyDistribution?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId)
    {
        return await _context.SurveyDistributions
            .Where(d => d.TargetEmployeeId == employeeId && 
                       d.SurveyId == surveyId && d.IsActive && !d.IsDeleted)
            .Include(d => d.Survey)
            .Include(d => d.TargetEmployee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SurveyDistribution>> GetPendingRemindersAsync()
    {
        var reminderThreshold = DateTime.UtcNow.AddDays(-3); // Send reminder after 3 days
        
        return await _context.SurveyDistributions
            .Where(d => d.IsActive && !d.IsDeleted &&
                       d.SentAt.HasValue &&
                       !d.CompletedAt.HasValue &&
                       (d.LastReminderSent == null || d.LastReminderSent < reminderThreshold) &&
                       d.ReminderCount < 3) // Max 3 reminders
            .Include(d => d.Survey)
            .Include(d => d.TargetEmployee)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyDistribution>> GetByBranchAsync(int branchId)
    {
        return await _context.SurveyDistributions
            .Where(d => d.TargetBranchId == branchId && d.IsActive && !d.IsDeleted)
            .Include(d => d.Survey)
            .Include(d => d.TargetBranch)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetDistributionCountAsync(int surveyId)
    {
        return await _context.SurveyDistributions
            .CountAsync(d => d.SurveyId == surveyId && d.IsActive && !d.IsDeleted);
    }

    public async Task<int> GetViewedCountAsync(int surveyId)
    {
        return await _context.SurveyDistributions
            .CountAsync(d => d.SurveyId == surveyId && d.ViewedAt.HasValue && 
                            d.IsActive && !d.IsDeleted);
    }

    public async Task<int> GetStartedCountAsync(int surveyId)
    {
        return await _context.SurveyDistributions
            .CountAsync(d => d.SurveyId == surveyId && d.StartedAt.HasValue && 
                            d.IsActive && !d.IsDeleted);
    }

    public async Task<int> GetCompletedCountAsync(int surveyId)
    {
        return await _context.SurveyDistributions
            .CountAsync(d => d.SurveyId == surveyId && d.CompletedAt.HasValue && 
                            d.IsActive && !d.IsDeleted);
    }
}