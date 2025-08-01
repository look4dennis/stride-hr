using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for employee management operations
/// </summary>
public interface IEmployeeService
{
    // CRUD Operations
    Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<Employee> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmployeeAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default);
    
    // Search and Filtering
    Task<(IEnumerable<Employee> Employees, int TotalCount)> SearchEmployeesAsync(
        EmployeeSearchCriteria criteria, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Employee>> GetEmployeesByBranchAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetEmployeesByManagerAsync(int managerId, CancellationToken cancellationToken = default);
    
    // Profile Photo Management
    Task<string> UploadProfilePhotoAsync(int employeeId, Stream photoStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteProfilePhotoAsync(int employeeId, CancellationToken cancellationToken = default);
    
    // Employee Lifecycle
    Task<Employee> OnboardEmployeeAsync(int employeeId, OnboardingRequest request, CancellationToken cancellationToken = default);
    Task<Employee> InitiateExitProcessAsync(int employeeId, ExitProcessRequest request, CancellationToken cancellationToken = default);
    Task<Employee> CompleteExitProcessAsync(int employeeId, CancellationToken cancellationToken = default);
    
    // Validation
    Task<bool> IsEmployeeIdUniqueAsync(string employeeId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    
    // Employee ID Generation
    Task<string> GenerateEmployeeIdAsync(int branchId, CancellationToken cancellationToken = default);
    
    // Reporting Structure
    Task<IEnumerable<Employee>> GetOrganizationalHierarchyAsync(int? rootEmployeeId = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateReportingStructureAsync(int employeeId, int? newManagerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for creating a new employee
/// </summary>
public class CreateEmployeeRequest
{
    public int BranchId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public decimal BasicSalary { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContact { get; set; }
    public string? NationalId { get; set; }
    public string? TaxId { get; set; }
    public string? BankDetails { get; set; }
    public string? VisaStatus { get; set; }
    public DateTime? VisaExpiryDate { get; set; }
}

/// <summary>
/// Request model for updating an employee
/// </summary>
public class UpdateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public decimal? BasicSalary { get; set; }
    public EmployeeStatus? Status { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContact { get; set; }
    public string? NationalId { get; set; }
    public string? TaxId { get; set; }
    public string? BankDetails { get; set; }
    public string? VisaStatus { get; set; }
    public DateTime? VisaExpiryDate { get; set; }
}

/// <summary>
/// Search criteria for employee filtering
/// </summary>
public class EmployeeSearchCriteria
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public EmployeeStatus? Status { get; set; }
    public int? ReportingManagerId { get; set; }
    public DateTime? JoiningDateFrom { get; set; }
    public DateTime? JoiningDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Request model for employee onboarding
/// </summary>
public class OnboardingRequest
{
    public List<string> CompletedDocuments { get; set; } = new();
    public List<string> PendingDocuments { get; set; } = new();
    public string? OnboardingNotes { get; set; }
    public DateTime? OrientationDate { get; set; }
    public string? BuddyEmployeeId { get; set; }
}

/// <summary>
/// Request model for employee exit process
/// </summary>
public class ExitProcessRequest
{
    public DateTime LastWorkingDay { get; set; }
    public string ExitReason { get; set; } = string.Empty;
    public string? ExitNotes { get; set; }
    public bool IsVoluntary { get; set; } = true;
    public List<string> AssetsToReturn { get; set; } = new();
    public string? HandoverNotes { get; set; }
}