using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    public int CreatedByEmployeeId { get; set; }
    public int BranchId { get; set; }
    
    // Navigation Properties
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<DSR> DSRs { get; set; } = new List<DSR>();
}