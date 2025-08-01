using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Project assignment entity - placeholder for project management task
/// </summary>
public class ProjectAssignment : AuditableEntity
{
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Assignment start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Assignment end date
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Employee role in the project
    /// </summary>
    [MaxLength(100)]
    public string? Role { get; set; }
    
    /// <summary>
    /// Assignment status
    /// </summary>
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}

/// <summary>
/// Project entity - placeholder for project management task
/// </summary>
public class Project : AuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Project start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Project end date
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Estimated hours for the project
    /// </summary>
    public int EstimatedHours { get; set; }
    
    /// <summary>
    /// Project budget
    /// </summary>
    public decimal Budget { get; set; }
    
    /// <summary>
    /// Project status
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    
    /// <summary>
    /// Project priority
    /// </summary>
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    
    /// <summary>
    /// Project created by
    /// </summary>
    public int CreatedByEmployeeId { get; set; }
    
    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}

/// <summary>
/// ProjectTask entity - placeholder for project management task
/// </summary>
public class ProjectTask : AuditableEntity
{
    public int ProjectId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Estimated hours for the task
    /// </summary>
    public int EstimatedHours { get; set; }
    
    /// <summary>
    /// Task status
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    
    /// <summary>
    /// Task priority
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    /// <summary>
    /// Task due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
}

/// <summary>
/// Task assignment entity
/// </summary>
public class TaskAssignment : BaseEntity
{
    public int TaskId { get; set; }
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// Assignment date
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ProjectTask Task { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}

/// <summary>
/// Assignment status enumeration
/// </summary>
public enum AssignmentStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
/// Project status enumeration
/// </summary>
public enum ProjectStatus
{
    Planning = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>
/// Project priority enumeration
/// </summary>
public enum ProjectPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Task status enumeration
/// </summary>
public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4,
    Cancelled = 5
}

/// <summary>
/// Task priority enumeration
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}