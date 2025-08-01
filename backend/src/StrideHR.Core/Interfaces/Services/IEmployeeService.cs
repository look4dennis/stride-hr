using StrideHR.Core.Entities;
using StrideHR.Core.Models.Employee;

namespace StrideHR.Core.Interfaces.Services;

public interface IEmployeeService
{
    // Basic CRUD operations
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<IEnumerable<Employee>> GetByBranchAsync(int branchId);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> CreateAsync(CreateEmployeeDto dto);
    Task UpdateAsync(Employee employee);
    Task UpdateAsync(int id, UpdateEmployeeDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByEmployeeIdAsync(string employeeId);
    Task<Employee?> GetEmployeeByIdAsync(int id);

    // Enhanced functionality
    Task<PagedResult<EmployeeDto>> SearchEmployeesAsync(EmployeeSearchCriteria criteria);
    Task<EmployeeDto?> GetEmployeeDtoAsync(int id);
    Task<IEnumerable<EmployeeDto>> GetEmployeeDtosAsync();
    Task<IEnumerable<EmployeeDto>> GetEmployeeDtosByBranchAsync(int branchId);

    // Profile photo management
    Task<string> UploadProfilePhotoAsync(ProfilePhotoUploadDto dto);
    Task<byte[]?> GetProfilePhotoAsync(int employeeId);
    Task DeleteProfilePhotoAsync(int employeeId);

    // Employee ID generation
    Task<string> GenerateEmployeeIdAsync(int branchId);

    // Onboarding workflow
    Task<bool> StartOnboardingAsync(EmployeeOnboardingDto dto);
    Task<bool> CompleteOnboardingTaskAsync(int employeeId, string taskName);
    Task<EmployeeOnboardingDto?> GetOnboardingStatusAsync(int employeeId);

    // Exit workflow
    Task<bool> InitiateExitProcessAsync(EmployeeExitDto dto);
    Task<bool> CompleteExitTaskAsync(int employeeId, string taskName);
    Task<EmployeeExitDto?> GetExitStatusAsync(int employeeId);
    Task<bool> FinalizeExitAsync(int employeeId);

    // Validation
    Task<bool> ValidateEmployeeDataAsync(CreateEmployeeDto dto);
    Task<bool> ValidateEmployeeUpdateAsync(int id, UpdateEmployeeDto dto);
}