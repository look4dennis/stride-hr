using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class EmployeeRole : BaseEntity
{
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public int AssignedBy { get; set; }
    public int? RevokedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Employee AssignedByEmployee { get; set; } = null!;
    public virtual Employee? RevokedByEmployee { get; set; }
}