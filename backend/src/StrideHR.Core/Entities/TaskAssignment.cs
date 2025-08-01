namespace StrideHR.Core.Entities;

public class TaskAssignment : BaseEntity
{
    public int TaskId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual ProjectTask Task { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}