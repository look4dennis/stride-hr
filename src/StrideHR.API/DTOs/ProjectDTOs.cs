using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Entities;

namespace StrideHR.API.DTOs;

// Project DTOs
public class CreateProjectDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

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
    public int CreatedByEmployeeId { get; set; }
}

public class UpdateProjectDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int EstimatedHours { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    public ProjectStatus Status { get; set; }

    public ProjectPriority Priority { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProjectAssignmentDto> TeamMembers { get; set; } = new();
    public List<ProjectTaskDto> Tasks { get; set; } = new();
    public ProjectProgressDto? Progress { get; set; }
}

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;
    public int TeamMemberCount { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOverdue { get; set; }
}

// Task DTOs
public class CreateTaskDto
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int EstimatedHours { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public List<int> AssignedEmployeeIds { get; set; } = new();
}

public class UpdateTaskDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int EstimatedHours { get; set; }

    public Core.Entities.TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }
}

public class ProjectTaskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedHours { get; set; }
    public Core.Entities.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
    public bool IsOverdue { get; set; }
    public int ActualHours { get; set; }
    public decimal CompletionPercentage { get; set; }
}

// Assignment DTOs
public class AssignTeamMembersDto
{
    public List<TeamMemberAssignmentDto> TeamMembers { get; set; } = new();
}

public class TeamMemberAssignmentDto
{
    [Required]
    public int EmployeeId { get; set; }

    [StringLength(100)]
    public string? Role { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }
}

public class ProjectAssignmentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AssignmentStatus Status { get; set; }
}

public class TaskAssignmentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class AssignTaskDto
{
    [Required]
    public List<int> EmployeeIds { get; set; } = new();
}

// Progress and Analytics DTOs
public class ProjectProgressDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TodoTasks { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int TotalEstimatedHours { get; set; }
    public int ActualHoursWorked { get; set; }
    public decimal HoursVariance { get; set; }
    public bool IsOnTrack { get; set; }
    public int RemainingHours { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public List<TaskProgressDto> TaskProgress { get; set; } = new();
}

public class TaskProgressDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Core.Entities.TaskStatus Status { get; set; }
    public int EstimatedHours { get; set; }
    public int ActualHours { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> AssignedEmployees { get; set; } = new();
}

public class ProjectStatisticsDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OverdueProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal SpentBudget { get; set; }
    public int TotalEstimatedHours { get; set; }
    public int ActualHoursWorked { get; set; }
}

// Query DTOs
public class GetProjectsQueryDto
{
    public int? BranchId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ProjectStatus? Status { get; set; }
    public ProjectPriority? Priority { get; set; }
    public string? SearchTerm { get; set; }
    public int? CreatedBy { get; set; }
    public int? AssignedTo { get; set; }
}

public class GetTasksQueryDto
{
    public int? ProjectId { get; set; }
    public int? EmployeeId { get; set; }
    public Core.Entities.TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public bool? IsOverdue { get; set; }
}

// Response DTOs
public class PagedProjectsResponseDto
{
    public List<ProjectSummaryDto> Projects { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}