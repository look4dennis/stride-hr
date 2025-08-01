using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Interfaces;

namespace StrideHR.Core.Entities;

/// <summary>
/// Employee entity representing individual employees in the organization
/// </summary>
public class Employee : AuditableEntity, IBranchEntity
{
    public int BranchId { get; set; }
    
    /// <summary>
    /// Unique employee identifier (e.g., "NYC-HR-2025-001")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EmployeeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(20)]
    public string? AlternatePhone { get; set; }
    
    /// <summary>
    /// Profile photo file path
    /// </summary>
    [MaxLength(500)]
    public string? ProfilePhotoPath { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public DateTime JoiningDate { get; set; }
    
    [MaxLength(100)]
    public string? Designation { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    /// <summary>
    /// Basic salary amount
    /// </summary>
    public decimal BasicSalary { get; set; }
    
    /// <summary>
    /// Employee status (Active, Inactive, Terminated, etc.)
    /// </summary>
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    
    /// <summary>
    /// Reporting manager's employee ID
    /// </summary>
    public int? ReportingManagerId { get; set; }
    
    /// <summary>
    /// Employee's home address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(50)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(50)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Emergency contact information (stored as JSON)
    /// </summary>
    public string? EmergencyContact { get; set; }
    
    /// <summary>
    /// Employee's national ID/SSN/PF number
    /// </summary>
    [MaxLength(50)]
    public string? NationalId { get; set; }
    
    /// <summary>
    /// Tax identification number
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }
    
    /// <summary>
    /// Bank account details (stored as JSON)
    /// </summary>
    public string? BankDetails { get; set; }
    
    /// <summary>
    /// Visa/work permit status for international employees
    /// </summary>
    [MaxLength(100)]
    public string? VisaStatus { get; set; }
    
    /// <summary>
    /// Visa expiry date
    /// </summary>
    public DateTime? VisaExpiryDate { get; set; }
    
    /// <summary>
    /// Employee termination date (if applicable)
    /// </summary>
    public DateTime? TerminationDate { get; set; }
    
    /// <summary>
    /// Reason for termination
    /// </summary>
    [MaxLength(500)]
    public string? TerminationReason { get; set; }
    
    /// <summary>
    /// Additional employee settings and preferences (stored as JSON)
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual Employee? ReportingManager { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    public virtual ICollection<DSR> DSRs { get; set; } = new List<DSR>();
    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    
    /// <summary>
    /// Full name property for display purposes
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Display name with employee ID
    /// </summary>
    public string DisplayName => $"{FullName} ({EmployeeId})";
}

/// <summary>
/// Employee status enumeration
/// </summary>
public enum EmployeeStatus
{
    Active = 1,
    Inactive = 2,
    OnLeave = 3,
    Terminated = 4,
    Suspended = 5,
    Probation = 6
}