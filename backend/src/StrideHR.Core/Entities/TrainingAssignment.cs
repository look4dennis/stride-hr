using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

public class TrainingAssignment : BaseEntity
{
    public int TrainingModuleId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int AssignedBy { get; set; }
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DueDate { get; set; }
    
    public TrainingAssignmentStatus Status { get; set; } = TrainingAssignmentStatus.Assigned;
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation Properties
    public virtual TrainingModule TrainingModule { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee AssignedByEmployee { get; set; } = null!;
    public virtual TrainingProgress? TrainingProgress { get; set; }
}

public enum TrainingAssignmentStatus
{
    Assigned,
    InProgress,
    Completed,
    Overdue,
    Cancelled
}