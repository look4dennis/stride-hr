using StrideHR.Core.Entities;
using System.Linq.Expressions;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Repository interface for Employee entity with specialized operations
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    // Specialized queries
    Task<Employee?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId, CancellationToken cancellationToken = default);
    
    // Search operations
    Task<(IEnumerable<Employee> Employees, int TotalCount)> SearchAsync(
        EmployeeSearchCriteria criteria,
        CancellationToken cancellationToken = default);
    
    // Validation queries
    Task<bool> IsEmployeeIdUniqueAsync(string employeeId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    
    // Employee ID generation
    Task<int> GetNextEmployeeSequenceAsync(int branchId, CancellationToken cancellationToken = default);
    
    // Hierarchical queries
    Task<IEnumerable<Employee>> GetHierarchyAsync(int? rootEmployeeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetSubordinatesAsync(int managerId, bool includeIndirect = false, CancellationToken cancellationToken = default);
    Task<bool> IsCircularReferenceAsync(int employeeId, int? newManagerId, CancellationToken cancellationToken = default);
    
    // Include related data
    Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetWithRolesAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetWithAttendanceAsync(int id, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetEmployeesWithExpiringVisasAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetBirthdayEmployeesAsync(DateTime date, CancellationToken cancellationToken = default);
}