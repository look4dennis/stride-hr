using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Organization;

public class CreateOrganizationDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Url]
    [StringLength(200)]
    public string? Website { get; set; }

    [StringLength(50)]
    public string? TaxId { get; set; }

    [StringLength(50)]
    public string? RegistrationNumber { get; set; }

    [Required]
    [Range(1, 24)]
    public int NormalWorkingHours { get; set; } = 8;

    [Required]
    [Range(1.0, 5.0)]
    public decimal OvertimeRate { get; set; } = 1.5m;

    [Required]
    [Range(1, 12)]
    public int ProductiveHoursThreshold { get; set; } = 6;

    public bool BranchIsolationEnabled { get; set; } = true;

    [StringLength(2000)]
    public string? ConfigurationSettings { get; set; }
}