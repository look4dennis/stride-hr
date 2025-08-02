using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class AssetAssignment : BaseEntity
{
    public int AssetId { get; set; }
    public int? EmployeeId { get; set; }
    public int? ProjectId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnDate { get; set; }
    public AssetCondition AssignedCondition { get; set; }
    public AssetCondition? ReturnedCondition { get; set; }
    public string? AssignmentNotes { get; set; }
    public string? ReturnNotes { get; set; }
    public bool IsActive { get; set; } = true;
    public int AssignedBy { get; set; }
    public int? ReturnedBy { get; set; }

    // Navigation Properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual Employee? Employee { get; set; }
    public virtual Project? Project { get; set; }
    public virtual Employee AssignedByEmployee { get; set; } = null!;
    public virtual Employee? ReturnedByEmployee { get; set; }
}