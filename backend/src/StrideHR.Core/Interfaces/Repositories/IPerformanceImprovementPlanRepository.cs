using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPerformanceImprovementPlanRepository : IRepository<PerformanceImprovementPlan>
{
    Task<IEnumerable<PerformanceImprovementPlan>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<PerformanceImprovementPlan>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<PerformanceImprovementPlan>> GetByStatusAsync(PIPStatus status);
    Task<IEnumerable<PerformanceImprovementPlan>> GetActivePIPsAsync();
    Task<IEnumerable<PerformanceImprovementPlan>> GetPIPsRequiringReviewAsync();
    Task<PerformanceImprovementPlan?> GetActiveByEmployeeIdAsync(int employeeId);
}