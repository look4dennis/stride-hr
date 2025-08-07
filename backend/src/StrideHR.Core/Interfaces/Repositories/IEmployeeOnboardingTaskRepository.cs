using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeOnboardingTaskRepository : IRepository<EmployeeOnboardingTask>
{
    Task<IEnumerable<EmployeeOnboardingTask>> GetByOnboardingIdAsync(int onboardingId);
    Task<IEnumerable<EmployeeOnboardingTask>> GetPendingTasksByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeOnboardingTask>> GetOverdueTasksAsync();
}