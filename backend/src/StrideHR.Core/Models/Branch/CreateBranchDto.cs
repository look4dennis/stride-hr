using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Branch;

public class CreateBranchDto
{
    [Required]
    public int OrganizationId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string CurrencySymbol { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TimeZone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public List<LocalHolidayDto> LocalHolidays { get; set; } = new List<LocalHolidayDto>();

    public BranchComplianceDto? ComplianceSettings { get; set; }

    public bool IsActive { get; set; } = true;
}