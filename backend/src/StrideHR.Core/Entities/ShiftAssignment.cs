namespace StrideHR.Core.Entities;

public class ShiftAssignment : BaseEntity
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Shift Shift { get; set; } = null!;
}