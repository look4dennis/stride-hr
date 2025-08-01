using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Organization;

public class OrganizationConfigurationDto
{
    [Required]
    [Range(1, 24)]
    public int NormalWorkingHours { get; set; }

    [Required]
    [Range(1.0, 5.0)]
    public decimal OvertimeRate { get; set; }

    [Required]
    [Range(1, 12)]
    public int ProductiveHoursThreshold { get; set; }

    public bool BranchIsolationEnabled { get; set; }

    public Dictionary<string, object> AdditionalSettings { get; set; } = new Dictionary<string, object>();
}