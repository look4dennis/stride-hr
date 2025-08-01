using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Branch;

public class LocalHolidayDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsRecurring { get; set; } = false;

    [StringLength(50)]
    public string Type { get; set; } = "National"; // National, Regional, Religious, Company, Optional
}