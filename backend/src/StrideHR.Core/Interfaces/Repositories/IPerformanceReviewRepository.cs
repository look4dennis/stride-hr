using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPerformanceReviewRepository : IRepository<PerformanceReview>
{
    Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<PerformanceReview>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<PerformanceReview>> GetByStatusAsync(PerformanceReviewStatus status);
    Task<IEnumerable<PerformanceReview>> GetOverdueReviewsAsync();
    Task<PerformanceReview?> GetByEmployeeAndPeriodAsync(int employeeId, string reviewPeriod);
    Task<IEnumerable<PerformanceReview>> GetReviewsRequiringPIPAsync();
}