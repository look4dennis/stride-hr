using StrideHR.Core.Entities;
using StrideHR.Core.Models.Employee;

namespace StrideHR.Core.Interfaces.Services;

/// <summary>
/// Service interface for managing employee operations including CRUD operations,
/// profile management, and organizational hierarchy management
/// </summary>
public interface IEmployeeService
{
    #region Basic CRUD Operations
    
    /// <summary>
    /// Retrieves an employee by their internal database ID
    /// </summary>
    /// <param name="id">The internal database ID of the employee</param>
    /// <returns>The employee entity if found, null otherwise</returns>
    Task<Employee?> GetByIdAsync(int id);
    
    /// <summary>
    /// Retrieves an employee by their unique employee ID (e.g., "NYC-HR-2025-001")
    /// </summary>
    /// <param name="employeeId">The unique employee identifier</param>
    /// <returns>The employee entity if found, null otherwise</returns>
    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
    
    /// <summary>
    /// Retrieves all employees across all branches (admin use only)
    /// </summary>
    /// <returns>Collection of all employee entities</returns>
    Task<IEnumerable<Employee>> GetAllAsync();
    
    /// <summary>
    /// Retrieves all employees within a specific branch
    /// </summary>
    /// <param name="branchId">The branch identifier</param>
    /// <returns>Collection of employees in the specified branch</returns>
    Task<IEnumerable<Employee>> GetByBranchAsync(int branchId);
    
    /// <summary>
    /// Creates a new employee from an entity object
    /// </summary>
    /// <param name="employee">The employee entity to create</param>
    /// <returns>The created employee with generated ID and system fields</returns>
    Task<Employee> CreateAsync(Employee employee);
    
    /// <summary>
    /// Creates a new employee from a DTO with validation and business rules
    /// </summary>
    /// <param name="dto">Employee creation data transfer object</param>
    /// <returns>The created employee entity</returns>
    Task<Employee> CreateAsync(CreateEmployeeDto dto);
    
    /// <summary>
    /// Updates an existing employee entity
    /// </summary>
    /// <param name="employee">The employee entity with updated values</param>
    Task UpdateAsync(Employee employee);
    
    /// <summary>
    /// Updates an employee using a DTO with validation
    /// </summary>
    /// <param name="id">The employee ID to update</param>
    /// <param name="dto">Employee update data transfer object</param>
    Task UpdateAsync(int id, UpdateEmployeeDto dto);
    
    /// <summary>
    /// Soft deletes an employee (marks as inactive rather than physical deletion)
    /// </summary>
    /// <param name="id">The employee ID to delete</param>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Checks if an employee exists by internal ID
    /// </summary>
    /// <param name="id">The internal database ID</param>
    /// <returns>True if employee exists, false otherwise</returns>
    Task<bool> ExistsAsync(int id);
    
    /// <summary>
    /// Checks if an employee exists by employee ID
    /// </summary>
    /// <param name="employeeId">The unique employee identifier</param>
    /// <returns>True if employee exists, false otherwise</returns>
    Task<bool> ExistsByEmployeeIdAsync(string employeeId);
    
    /// <summary>
    /// Retrieves an employee by ID (duplicate method for backward compatibility)
    /// </summary>
    /// <param name="id">The employee ID</param>
    /// <returns>The employee entity if found, null otherwise</returns>
    Task<Employee?> GetEmployeeByIdAsync(int id);
    
    #endregion

    #region Enhanced Functionality
    
    /// <summary>
    /// Searches employees with advanced filtering, sorting, and pagination
    /// </summary>
    /// <param name="criteria">Search criteria including filters, sorting, and pagination parameters</param>
    /// <returns>Paginated result containing matching employees as DTOs</returns>
    Task<PagedResult<EmployeeDto>> SearchEmployeesAsync(EmployeeSearchCriteria criteria);
    
    /// <summary>
    /// Retrieves an employee as a DTO with computed fields and related data
    /// </summary>
    /// <param name="id">The employee ID</param>
    /// <returns>Employee DTO with additional computed properties, null if not found</returns>
    Task<EmployeeDto?> GetEmployeeDtoAsync(int id);
    
    /// <summary>
    /// Retrieves all employees as DTOs with computed fields
    /// </summary>
    /// <returns>Collection of employee DTOs</returns>
    Task<IEnumerable<EmployeeDto>> GetEmployeeDtosAsync();
    
    /// <summary>
    /// Retrieves employees in a specific branch as DTOs
    /// </summary>
    /// <param name="branchId">The branch identifier</param>
    /// <returns>Collection of employee DTOs in the specified branch</returns>
    Task<IEnumerable<EmployeeDto>> GetEmployeeDtosByBranchAsync(int branchId);
    
    #endregion

    #region Profile Photo Management
    
    /// <summary>
    /// Uploads and processes a profile photo for an employee
    /// </summary>
    /// <param name="dto">Profile photo upload data including file and employee ID</param>
    /// <returns>The URL or path to the uploaded photo</returns>
    Task<string> UploadProfilePhotoAsync(ProfilePhotoUploadDto dto);
    
    /// <summary>
    /// Retrieves the profile photo binary data for an employee
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <returns>Binary photo data if exists, null otherwise</returns>
    Task<byte[]?> GetProfilePhotoAsync(int employeeId);
    Task DeleteProfilePhotoAsync(int employeeId);
    
    #endregion

    #region Employee Management Operations
    
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
    
    #endregion
}