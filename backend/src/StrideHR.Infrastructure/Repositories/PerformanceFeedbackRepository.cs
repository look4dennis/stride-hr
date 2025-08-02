using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PerformanceFeedbackRepository : Repository<PerformanceFeedback>, IPerformanceFeedbackRepository
{
    public PerformanceFeedbackRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetByReviewIdAsync(int reviewId)
    {
        return await _dbSet
            .Include(f => f.PerformanceReview)
            .Include(f => f.Reviewee)
            .Include(f => f.Reviewer)
            .Where(f => f.PerformanceReviewId == reviewId && !f.IsDeleted)
            .OrderBy(f => f.FeedbackType)
            .ThenBy(f => f.CompetencyArea)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetByRevieweeIdAsync(int revieweeId)
    {
        return await _dbSet
            .Include(f => f.PerformanceReview)
            .Include(f => f.Reviewee)
            .Include(f => f.Reviewer)
            .Where(f => f.RevieweeId == revieweeId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetByReviewerIdAsync(int reviewerId)
    {
        return await _dbSet
            .Include(f => f.PerformanceReview)
            .Include(f => f.Reviewee)
            .Include(f => f.Reviewer)
            .Where(f => f.ReviewerId == reviewerId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetByFeedbackTypeAsync(FeedbackType feedbackType)
    {
        return await _dbSet
            .Include(f => f.PerformanceReview)
            .Include(f => f.Reviewee)
            .Include(f => f.Reviewer)
            .Where(f => f.FeedbackType == feedbackType && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceFeedback>> GetPendingFeedbackAsync(int reviewerId)
    {
        return await _dbSet
            .Include(f => f.PerformanceReview)
            .Include(f => f.Reviewee)
            .Include(f => f.Reviewer)
            .Where(f => f.ReviewerId == reviewerId && 
                       !f.IsSubmitted && 
                       !f.IsDeleted)
            .OrderBy(f => f.PerformanceReview.DueDate)
            .ToListAsync();
    }
}