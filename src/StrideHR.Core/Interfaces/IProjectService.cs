using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Service interface for project management operations
/// </summary>
public interface IProjectService
{
    // Project Management
    /// <summary>
    /// Create a new project
    /// </summary>
    Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing project
    /// </summary>
    Task<Project> UpdateProjectAsync(int projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project by ID with details
    /// </summary>
    Task<Project?> GetProjectByIdAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get projects with filtering and pagination
    /// </summary>
    Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsAsync(
        GetProjectsRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    Task<bool> DeleteProjectAsync(int projectId, string deletedBy, CancellationToken cancellationToken = default);

    // Team Assignment Management
    /// <summary>
    /// Assign team members to a project
    /// </summary>
    Task<IEnumerable<ProjectAssignment>> AssignTeamMembersAsync(
        int projectId, 
        AssignTeamMembersRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove team member from project
    /// </summary>
    Task<bool> RemoveTeamMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update team member role in project
    /// </summary>
    Task<ProjectAssignment> UpdateTeamMemberRoleAsync(
        int projectId, 
        int employeeId, 
        string role, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project team members
    /// </summary>
    Task<IEnumerable<Employee>> GetProjectTeamMembersAsync(int projectId, CancellationToken cancellationToken = default);

    // Task Management
    /// <summary>
    /// Create a new task in a project
    /// </summary>
    Task<ProjectTask> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing task
    /// </summary>
    Task<ProjectTask> UpdateTaskAsync(int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get task by ID
    /// </summary>
    Task<ProjectTask?> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks by project
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(
        int projectId, 
        Core.Entities.TaskStatus? status = null, 
        TaskPriority? priority = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a task (soft delete)
    /// </summary>
    Task<bool> DeleteTaskAsync(int taskId, string deletedBy, CancellationToken cancellationToken = default);

    // Task Assignment Management
    /// <summary>
    /// Assign employees to a task
    /// </summary>
    Task<IEnumerable<TaskAssignment>> AssignTaskToEmployeesAsync(
        int taskId, 
        IEnumerable<int> employeeIds, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove employee from task
    /// </summary>
    Task<bool> RemoveTaskAssignmentAsync(int taskId, int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks assigned to employee
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    // Project Progress and Analytics
    /// <summary>
    /// Get project progress information
    /// </summary>
    Task<ProjectProgress> GetProjectProgressAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project statistics
    /// </summary>
    Task<ProjectStatistics> GetProjectStatisticsAsync(int? branchId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue projects
    /// </summary>
    Task<IEnumerable<Project>> GetOverdueProjectsAsync(int? branchId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(int? projectId = null, CancellationToken cancellationToken = default);
}

// Request/Response Models
public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal Budget { get; set; }
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    public int CreatedByEmployeeId { get; set; }
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int EstimatedHours { get; set; }
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
}

public class GetProjectsRequest
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

public class AssignTeamMembersRequest
{
    public List<TeamMemberAssignment> TeamMembers { get; set; } = new();
}

public class TeamMemberAssignment
{
    public int EmployeeId { get; set; }
    public string? Role { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
}

public class CreateTaskRequest
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedHours { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public List<int> AssignedEmployeeIds { get; set; } = new();
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedHours { get; set; }
    public Core.Entities.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ProjectProgress
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
    public List<TaskProgress> TaskProgress { get; set; } = new();
}

public class TaskProgress
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