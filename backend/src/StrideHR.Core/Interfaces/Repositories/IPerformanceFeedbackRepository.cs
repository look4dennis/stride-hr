using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPerformanceFeedbackRepository : IRepository<PerformanceFeedback>
{
    Task<IEnumerable<PerformanceFeedback>> GetByReviewIdAsync(int reviewId);
    Task<IEnumerable<PerformanceFeedback>> GetByRevieweeIdAsync(int revieweeId);
    Task<IEnumerable<PerformanceFeedback>> GetByReviewerIdAsync(int reviewerId);
    Task<IEnumerable<PerformanceFeedback>> GetByFeedbackTypeAsync(FeedbackType feedbackType);
    Task<IEnumerable<PerformanceFeedback>> GetPendingFeedbackAsync(int reviewerId);
}