using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ProjectTask : BaseEntity
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public int DisplayOrder { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual Employee? AssignedToEmployee { get; set; }
    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    public virtual ICollection<DSR> DSRs { get; set; } = new List<DSR>();
}