using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeRoleRepository : IRepository<EmployeeRole>
{
    Task<IEnumerable<EmployeeRole>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeRole>> GetActiveRolesByEmployeeIdAsync(int employeeId);
    Task<EmployeeRole?> GetByEmployeeAndRoleIdAsync(int employeeId, int roleId);
    Task<bool> HasRoleAsync(int employeeId, int roleId);
    Task<bool> HasActiveRoleAsync(int employeeId, int roleId);
}