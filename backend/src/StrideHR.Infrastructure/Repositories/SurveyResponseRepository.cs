using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SurveyResponseRepository : Repository<SurveyResponse>, ISurveyResponseRepository
{
    public SurveyResponseRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SurveyResponse>> GetBySurveyAsync(int surveyId)
    {
        return await _context.SurveyResponses
            .Where(r => r.SurveyId == surveyId && !r.IsDeleted)
            .Include(r => r.RespondentEmployee)
            .Include(r => r.Survey)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyResponse>> GetByEmployeeAsync(int employeeId)
    {
        return await _context.SurveyResponses
            .Where(r => r.RespondentEmployeeId == employeeId && !r.IsDeleted)
            .Include(r => r.Survey)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<SurveyResponse?> GetByEmployeeAndSurveyAsync(int employeeId, int surveyId)
    {
        return await _context.SurveyResponses
            .Where(r => r.RespondentEmployeeId == employeeId && 
                       r.SurveyId == surveyId && !r.IsDeleted)
            .Include(r => r.Survey)
            .Include(r => r.RespondentEmployee)
            .FirstOrDefaultAsync();
    }

    public async Task<SurveyResponse?> GetWithAnswersAsync(int id)
    {
        return await _context.SurveyResponses
            .Where(r => r.Id == id && !r.IsDeleted)
            .Include(r => r.Survey)
            .Include(r => r.RespondentEmployee)
            .Include(r => r.Answers)
                .ThenInclude(a => a.Question)
            .Include(r => r.Answers)
                .ThenInclude(a => a.SelectedOption)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SurveyResponse>> GetByStatusAsync(int surveyId, SurveyResponseStatus status)
    {
        return await _context.SurveyResponses
            .Where(r => r.SurveyId == surveyId && r.Status == status && !r.IsDeleted)
            .Include(r => r.RespondentEmployee)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyResponse>> GetCompletedResponsesAsync(int surveyId)
    {
        return await _context.SurveyResponses
            .Where(r => r.SurveyId == surveyId && 
                       r.Status == SurveyResponseStatus.Completed && !r.IsDeleted)
            .Include(r => r.RespondentEmployee)
            .Include(r => r.Answers)
                .ThenInclude(a => a.Question)
            .OrderByDescending(r => r.CompletedAt)
            .ToListAsync();
    }

    public async Task<int> GetResponseCountAsync(int surveyId)
    {
        return await _context.SurveyResponses
            .CountAsync(r => r.SurveyId == surveyId && !r.IsDeleted);
    }

    public async Task<int> GetCompletedResponseCountAsync(int surveyId)
    {
        return await _context.SurveyResponses
            .CountAsync(r => r.SurveyId == surveyId && 
                            r.Status == SurveyResponseStatus.Completed && !r.IsDeleted);
    }

    public async Task<TimeSpan?> GetAverageCompletionTimeAsync(int surveyId)
    {
        var completedResponses = await _context.SurveyResponses
            .Where(r => r.SurveyId == surveyId && 
                       r.Status == SurveyResponseStatus.Completed && 
                       r.TimeTaken.HasValue && !r.IsDeleted)
            .Select(r => r.TimeTaken!.Value)
            .ToListAsync();

        if (!completedResponses.Any())
            return null;

        var averageTicks = completedResponses.Average(t => t.Ticks);
        return new TimeSpan((long)averageTicks);
    }

    public async Task<bool> HasEmployeeRespondedAsync(int employeeId, int surveyId)
    {
        return await _context.SurveyResponses
            .AnyAsync(r => r.RespondentEmployeeId == employeeId && 
                          r.SurveyId == surveyId && 
                          r.Status != SurveyResponseStatus.NotStarted && !r.IsDeleted);
    }
}