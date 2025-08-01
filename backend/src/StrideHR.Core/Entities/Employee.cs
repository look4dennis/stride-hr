using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Employee : BaseEntity
{
    public string EmployeeId { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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
    public string? NationalId { get; set; } // SSN, Aadhar, etc.
    public string? PassportNumber { get; set; }
    public string? VisaStatus { get; set; }
    public decimal BasicSalary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public int? ReportingManagerId { get; set; }
    public string? Notes { get; set; }
    
    // Computed property for full name
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual Employee? ReportingManager { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
}