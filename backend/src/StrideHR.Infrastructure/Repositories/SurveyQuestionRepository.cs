using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SurveyQuestionRepository : Repository<SurveyQuestion>, ISurveyQuestionRepository
{
    public SurveyQuestionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SurveyQuestion>> GetBySurveyAsync(int surveyId)
    {
        return await _context.SurveyQuestions
            .Where(q => q.SurveyId == surveyId && q.IsActive && !q.IsDeleted)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyQuestion>> GetBySurveyWithOptionsAsync(int surveyId)
    {
        return await _context.SurveyQuestions
            .Where(q => q.SurveyId == surveyId && q.IsActive && !q.IsDeleted)
            .Include(q => q.Options.Where(o => o.IsActive))
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<SurveyQuestion?> GetWithOptionsAsync(int id)
    {
        return await _context.SurveyQuestions
            .Where(q => q.Id == id && q.IsActive && !q.IsDeleted)
            .Include(q => q.Options.Where(o => o.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetMaxOrderIndexAsync(int surveyId)
    {
        var maxOrder = await _context.SurveyQuestions
            .Where(q => q.SurveyId == surveyId && q.IsActive && !q.IsDeleted)
            .MaxAsync(q => (int?)q.OrderIndex);
        
        return maxOrder ?? 0;
    }

    public async Task ReorderQuestionsAsync(int surveyId, Dictionary<int, int> questionOrderMap)
    {
        var questions = await _context.SurveyQuestions
            .Where(q => q.SurveyId == surveyId && q.IsActive && !q.IsDeleted)
            .ToListAsync();

        foreach (var question in questions)
        {
            if (questionOrderMap.ContainsKey(question.Id))
            {
                question.OrderIndex = questionOrderMap[question.Id];
                question.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }
}