using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Project;

public class UpdateProjectDto
{
    [StringLength(200)]
    public string? Name { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? EstimatedHours { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }
    
    public ProjectStatus? Status { get; set; }
    
    public ProjectPriority? Priority { get; set; }
}