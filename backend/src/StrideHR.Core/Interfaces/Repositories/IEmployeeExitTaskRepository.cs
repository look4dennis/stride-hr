using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeExitTaskRepository : IRepository<EmployeeExitTask>
{
    Task<IEnumerable<EmployeeExitTask>> GetByExitIdAsync(int exitId);
    Task<IEnumerable<EmployeeExitTask>> GetPendingTasksByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeExitTask>> GetOverdueTasksAsync();
}