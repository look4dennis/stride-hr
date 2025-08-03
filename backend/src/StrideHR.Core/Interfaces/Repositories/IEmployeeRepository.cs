using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId);
    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId);
    Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId);
}