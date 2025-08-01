using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Repository interface for Project entity operations
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Get projects by branch with filtering and pagination
    /// </summary>
    Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsByBranchAsync(
        int branchId,
        int pageNumber = 1,
        int pageSize = 10,
        ProjectStatus? status = null,
        ProjectPriority? priority = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project with all related data (assignments, tasks)
    /// </summary>
    Task<Project?> GetProjectWithDetailsAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get projects assigned to a specific employee
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get projects created by a specific employee
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsByCreatorAsync(int creatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project statistics for dashboard
    /// </summary>
    Task<ProjectStatistics> GetProjectStatisticsAsync(int? branchId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProjectTask entity operations
/// </summary>
public interface IProjectTaskRepository : IRepository<ProjectTask>
{
    /// <summary>
    /// Get tasks by project with filtering
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(
        int projectId,
        Core.Entities.TaskStatus? status = null,
        TaskPriority? priority = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tasks assigned to a specific employee
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get task with assignments
    /// </summary>
    Task<ProjectTask?> GetTaskWithAssignmentsAsync(int taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(int? projectId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ProjectAssignment entity operations
/// </summary>
public interface IProjectAssignmentRepository : IRepository<ProjectAssignment>
{
    /// <summary>
    /// Get assignments by project
    /// </summary>
    Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get assignments by employee
    /// </summary>
    Task<IEnumerable<ProjectAssignment>> GetAssignmentsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if employee is assigned to project
    /// </summary>
    Task<bool> IsEmployeeAssignedToProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get team members for a project
    /// </summary>
    Task<IEnumerable<Employee>> GetProjectTeamMembersAsync(int projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for TaskAssignment entity operations
/// </summary>
public interface ITaskAssignmentRepository : IRepository<TaskAssignment>
{
    /// <summary>
    /// Get assignments by task
    /// </summary>
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByTaskAsync(int taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get assignments by employee
    /// </summary>
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if employee is assigned to task
    /// </summary>
    Task<bool> IsEmployeeAssignedToTaskAsync(int employeeId, int taskId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Project statistics model
/// </summary>
public class ProjectStatistics
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