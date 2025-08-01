using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Project;

public class CreateProjectDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Range(1, int.MaxValue)]
    public int EstimatedHours { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }
    
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    
    [Required]
    public int BranchId { get; set; }
    
    public List<int> TeamMemberIds { get; set; } = new List<int>();
    
    public int? TeamLeadId { get; set; }
}