using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPerformanceGoalRepository : IRepository<PerformanceGoal>
{
    Task<IEnumerable<PerformanceGoal>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<PerformanceGoal>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<PerformanceGoal>> GetByStatusAsync(PerformanceGoalStatus status);
    Task<IEnumerable<PerformanceGoal>> GetOverdueGoalsAsync();
    Task<IEnumerable<PerformanceGoal>> GetGoalsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetEmployeeGoalCompletionRateAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
}