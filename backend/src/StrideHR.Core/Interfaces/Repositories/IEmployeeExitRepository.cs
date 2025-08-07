using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeExitRepository : IRepository<EmployeeExit>
{
    Task<EmployeeExit?> GetByEmployeeIdAsync(int employeeId);
    Task<EmployeeExit?> GetActiveExitByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeExit>> GetPendingExitsAsync();
    Task<IEnumerable<EmployeeExit>> GetOverdueExitsAsync();
}