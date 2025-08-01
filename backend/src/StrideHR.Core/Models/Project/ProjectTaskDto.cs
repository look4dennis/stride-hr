using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Project;

public class ProjectTaskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public string? AssignedToEmployeeName { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public decimal ActualHoursWorked { get; set; }
    public bool IsOverdue { get; set; }
    public List<TaskAssignmentDto> Assignments { get; set; } = new List<TaskAssignmentDto>();
}