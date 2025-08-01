using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Shift : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftType Type { get; set; } = ShiftType.Day;
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}

public class ShiftAssignment : BaseEntity
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Shift Shift { get; set; } = null!;
}