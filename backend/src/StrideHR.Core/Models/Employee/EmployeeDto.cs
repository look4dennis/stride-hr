using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? ProfilePhoto { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? BloodGroup { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string? VisaStatus { get; set; }
    public decimal BasicSalary { get; set; }
    public EmployeeStatus Status { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}