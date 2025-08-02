using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class SurveyAnalyticsRepository : Repository<SurveyAnalytics>, ISurveyAnalyticsRepository
{
    public SurveyAnalyticsRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SurveyAnalytics>> GetBySurveyAsync(int surveyId)
    {
        return await _context.SurveyAnalytics
            .Where(a => a.SurveyId == surveyId && !a.IsDeleted)
            .Include(a => a.Question)
            .OrderByDescending(a => a.CalculatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SurveyAnalytics>> GetByQuestionAsync(int questionId)
    {
        return await _context.SurveyAnalytics
            .Where(a => a.QuestionId == questionId && !a.IsDeleted)
            .OrderByDescending(a => a.CalculatedAt)
            .ToListAsync();
    }

    public async Task<SurveyAnalytics?> GetByMetricTypeAsync(int surveyId, string metricType)
    {
        return await _context.SurveyAnalytics
            .Where(a => a.SurveyId == surveyId && 
                       a.MetricType == metricType && !a.IsDeleted)
            .OrderByDescending(a => a.CalculatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SurveyAnalytics>> GetBySegmentAsync(int surveyId, string segment, string segmentValue)
    {
        return await _context.SurveyAnalytics
            .Where(a => a.SurveyId == surveyId && 
                       a.Segment == segment && 
                       a.SegmentValue == segmentValue && !a.IsDeleted)
            .OrderByDescending(a => a.CalculatedAt)
            .ToListAsync();
    }

    public async Task<DateTime?> GetLastCalculationDateAsync(int surveyId)
    {
        return await _context.SurveyAnalytics
            .Where(a => a.SurveyId == surveyId && !a.IsDeleted)
            .MaxAsync(a => (DateTime?)a.CalculatedAt);
    }

    public async Task DeleteBySurveyAsync(int surveyId)
    {
        var analytics = await _context.SurveyAnalytics
            .Where(a => a.SurveyId == surveyId)
            .ToListAsync();

        foreach (var analytic in analytics)
        {
            analytic.IsDeleted = true;
            analytic.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}