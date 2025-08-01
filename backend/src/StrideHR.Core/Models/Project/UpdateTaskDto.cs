using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Project;

public class UpdateTaskDto
{
    [StringLength(200)]
    public string? Title { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? EstimatedHours { get; set; }
    
    public ProjectTaskStatus? Status { get; set; }
    
    public TaskPriority? Priority { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public int? AssignedToEmployeeId { get; set; }
    
    public int? DisplayOrder { get; set; }
}