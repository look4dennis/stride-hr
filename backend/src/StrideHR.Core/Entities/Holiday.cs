using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Holiday : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public HolidayType Type { get; set; }
    public bool IsOptional { get; set; } = false;
    public string? Description { get; set; }
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}