using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IEmployeeService
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<IEnumerable<Employee>> GetByBranchAsync(int branchId);
    Task<Employee> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByEmployeeIdAsync(string employeeId);
}