using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SurveyRepository : Repository<Survey>, ISurveyRepository
{
    public SurveyRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Survey>> GetByBranchAsync(int branchId)
    {
        return await _context.Surveys
            .Where(s => s.BranchId == branchId && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> GetActiveAsync()
    {
        return await _context.Surveys
            .Where(s => s.Status == SurveyStatus.Active && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status)
    {
        return await _context.Surveys
            .Where(s => s.Status == status && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> GetByTypeAsync(SurveyType type)
    {
        return await _context.Surveys
            .Where(s => s.Type == type && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> GetByCreatorAsync(int createdByEmployeeId)
    {
        return await _context.Surveys
            .Where(s => s.CreatedByEmployeeId == createdByEmployeeId && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Survey?> GetWithQuestionsAsync(int id)
    {
        return await _context.Surveys
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .Include(s => s.Questions.Where(q => q.IsActive))
                .ThenInclude(q => q.Options.Where(o => o.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<Survey?> GetWithResponsesAsync(int id)
    {
        return await _context.Surveys
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answers)
            .FirstOrDefaultAsync();
    }

    public async Task<Survey?> GetWithAnalyticsAsync(int id)
    {
        return await _context.Surveys
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .Include(s => s.Analytics)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Survey>> GetGlobalSurveysAsync()
    {
        return await _context.Surveys
            .Where(s => s.IsGlobal && !s.IsDeleted)
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Survey>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.Surveys
            .Where(s => !s.IsDeleted && 
                       (s.Title.ToLower().Contains(lowerSearchTerm) ||
                        s.Description.ToLower().Contains(lowerSearchTerm) ||
                        (s.Tags != null && s.Tags.ToLower().Contains(lowerSearchTerm))))
            .Include(s => s.CreatedByEmployee)
            .Include(s => s.Branch)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasActiveResponsesAsync(int surveyId)
    {
        return await _context.SurveyResponses
            .AnyAsync(r => r.SurveyId == surveyId && 
                          r.Status != SurveyResponseStatus.NotStarted);
    }
}