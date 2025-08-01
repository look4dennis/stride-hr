using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Project;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new List<ProjectTeamMemberDto>();
    public List<ProjectTaskDto> Tasks { get; set; } = new List<ProjectTaskDto>();
    public ProjectProgressDto Progress { get; set; } = new ProjectProgressDto();
}