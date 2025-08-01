using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs.Employee;

/// <summary>
/// Employee data transfer object for API responses
/// </summary>
public class EmployeeDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public decimal BasicSalary { get; set; }
    public EmployeeStatus Status { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
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
    public DateTime? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create employee request DTO
/// </summary>
public class CreateEmployeeDto
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
/// Update employee request DTO
/// </summary>
public class UpdateEmployeeDto
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
/// Employee search request DTO
/// </summary>
public class EmployeeSearchDto
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
/// Paginated employee response DTO
/// </summary>
public class PagedEmployeeResponseDto
{
    public IEnumerable<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Employee onboarding request DTO
/// </summary>
public class OnboardingDto
{
    public List<string> CompletedDocuments { get; set; } = new();
    public List<string> PendingDocuments { get; set; } = new();
    public string? OnboardingNotes { get; set; }
    public DateTime? OrientationDate { get; set; }
    public string? BuddyEmployeeId { get; set; }
}

/// <summary>
/// Employee exit process request DTO
/// </summary>
public class ExitProcessDto
{
    public DateTime LastWorkingDay { get; set; }
    public string ExitReason { get; set; } = string.Empty;
    public string? ExitNotes { get; set; }
    public bool IsVoluntary { get; set; } = true;
    public List<string> AssetsToReturn { get; set; } = new();
    public string? HandoverNotes { get; set; }
}

/// <summary>
/// Profile photo upload response DTO
/// </summary>
public class ProfilePhotoResponseDto
{
    public string FilePath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}