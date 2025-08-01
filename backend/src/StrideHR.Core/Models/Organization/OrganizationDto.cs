namespace StrideHR.Core.Models.Organization;

public class OrganizationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int NormalWorkingHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public int ProductiveHoursThreshold { get; set; }
    public bool BranchIsolationEnabled { get; set; }
    public string ConfigurationSettings { get; set; } = string.Empty;
    public int BranchCount { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}