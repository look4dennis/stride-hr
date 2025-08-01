using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Employee;

public class UpdateEmployeeDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? AlternatePhone { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(100)]
    public string Designation { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? EmergencyContactPhone { get; set; }

    [StringLength(10)]
    public string? BloodGroup { get; set; }

    [StringLength(50)]
    public string? NationalId { get; set; }

    [StringLength(50)]
    public string? PassportNumber { get; set; }

    [StringLength(50)]
    public string? VisaStatus { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    public int? ReportingManagerId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public EmployeeStatus Status { get; set; }
}