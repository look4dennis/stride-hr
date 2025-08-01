using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Linq.Expressions;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for project management operations
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly ITaskAssignmentRepository _taskAssignmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectTaskRepository taskRepository,
        IProjectAssignmentRepository assignmentRepository,
        ITaskAssignmentRepository taskAssignmentRepository,
        IEmployeeRepository employeeRepository,
        IAuditService auditService,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
        _taskAssignmentRepository = taskAssignmentRepository;
        _employeeRepository = employeeRepository;
        _auditService = auditService;
        _logger = logger;
    }

    #region Project Management

    /// <summary>
    /// Create a new project
    /// </summary>
    public async Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new project: {ProjectName}", request.Name);

        // Validate creator exists
        var creator = await _employeeRepository.GetByIdAsync(request.CreatedByEmployeeId, cancellationToken);
        if (creator == null)
        {
            throw new ArgumentException($"Employee with ID {request.CreatedByEmployeeId} not found");
        }

        // Validate dates
        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            EstimatedHours = request.EstimatedHours,
            Budget = request.Budget,
            Priority = request.Priority,
            Status = ProjectStatus.Planning,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creator.EmployeeId
        };

        var createdProject = await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Project", createdProject.Id, "Created", 
            $"Project '{createdProject.Name}' created", cancellationToken);

        _logger.LogInformation("Project created successfully: {ProjectId}", createdProject.Id);
        return createdProject;
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    public async Task<Project> UpdateProjectAsync(int projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            throw new ArgumentException($"Project with ID {projectId} not found");
        }

        // Validate dates
        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        var oldValues = $"Name: {project.Name}, Status: {project.Status}, Priority: {project.Priority}";

        project.Name = request.Name;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.EstimatedHours = request.EstimatedHours;
        project.Budget = request.Budget;
        project.Status = request.Status;
        project.Priority = request.Priority;
        project.UpdatedAt = DateTime.UtcNow;

        var updatedProject = await _projectRepository.UpdateAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        var newValues = $"Name: {project.Name}, Status: {project.Status}, Priority: {project.Priority}";
        await _auditService.LogAsync("Project", projectId, "Updated", 
            $"Project updated. Old: {oldValues}, New: {newValues}", cancellationToken);

        _logger.LogInformation("Project updated successfully: {ProjectId}", projectId);
        return updatedProject;
    }

    /// <summary>
    /// Get project by ID with details
    /// </summary>
    public async Task<Project?> GetProjectByIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetProjectWithDetailsAsync(projectId, cancellationToken);
    }

    /// <summary>
    /// Get projects with filtering and pagination
    /// </summary>
    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsAsync(
        GetProjectsRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request.BranchId.HasValue)
        {
            return await _projectRepository.GetProjectsByBranchAsync(
                request.BranchId.Value,
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.Priority,
                request.SearchTerm,
                cancellationToken);
        }

        // If no branch specified, get all projects with pagination
        return await _projectRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate: p => (request.Status == null || p.Status == request.Status) &&
                           (request.Priority == null || p.Priority == request.Priority) &&
                           (string.IsNullOrEmpty(request.SearchTerm) || 
                            p.Name.Contains(request.SearchTerm) || 
                            (p.Description != null && p.Description.Contains(request.SearchTerm))),
            orderBy: p => p.CreatedAt,
            ascending: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    public async Task<bool> DeleteProjectAsync(int projectId, string deletedBy, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            return false;
        }

        await _projectRepository.SoftDeleteAsync(project, deletedBy, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Project", projectId, "Deleted", 
            $"Project '{project.Name}' deleted", cancellationToken);

        _logger.LogInformation("Project deleted successfully: {ProjectId}", projectId);
        return true;
    }

    #endregion

    #region Team Assignment Management

    /// <summary>
    /// Assign team members to a project
    /// </summary>
    public async Task<IEnumerable<ProjectAssignment>> AssignTeamMembersAsync(
        int projectId, 
        AssignTeamMembersRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning team members to project: {ProjectId}", projectId);

        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            throw new ArgumentException($"Project with ID {projectId} not found");
        }

        var assignments = new List<ProjectAssignment>();

        foreach (var teamMember in request.TeamMembers)
        {
            // Check if employee exists
            var employee = await _employeeRepository.GetByIdAsync(teamMember.EmployeeId, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found: {EmployeeId}", teamMember.EmployeeId);
                continue;
            }

            // Check if already assigned
            var existingAssignment = await _assignmentRepository.IsEmployeeAssignedToProjectAsync(
                teamMember.EmployeeId, projectId, cancellationToken);

            if (existingAssignment)
            {
                _logger.LogWarning("Employee {EmployeeId} already assigned to project {ProjectId}", 
                    teamMember.EmployeeId, projectId);
                continue;
            }

            var assignment = new ProjectAssignment
            {
                EmployeeId = teamMember.EmployeeId,
                ProjectId = projectId,
                Role = teamMember.Role,
                StartDate = teamMember.StartDate,
                EndDate = teamMember.EndDate,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            assignments.Add(assignment);
        }

        if (assignments.Any())
        {
            await _assignmentRepository.AddRangeAsync(assignments, cancellationToken);
            await _assignmentRepository.SaveChangesAsync(cancellationToken);

            foreach (var assignment in assignments)
            {
                await _auditService.LogAsync("ProjectAssignment", assignment.Id, "Created", 
                    $"Employee {assignment.EmployeeId} assigned to project {projectId}", cancellationToken);
            }
        }

        _logger.LogInformation("Team members assigned successfully to project: {ProjectId}", projectId);
        return assignments;
    }

    /// <summary>
    /// Remove team member from project
    /// </summary>
    public async Task<bool> RemoveTeamMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing team member {EmployeeId} from project {ProjectId}", employeeId, projectId);

        var assignment = await _assignmentRepository.FirstOrDefaultAsync(
            pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId && pa.Status == AssignmentStatus.Active,
            cancellationToken);

        if (assignment == null)
        {
            return false;
        }

        assignment.Status = AssignmentStatus.Cancelled;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ProjectAssignment", assignment.Id, "Cancelled", 
            $"Employee {employeeId} removed from project {projectId}", cancellationToken);

        _logger.LogInformation("Team member removed successfully from project");
        return true;
    }

    /// <summary>
    /// Update team member role in project
    /// </summary>
    public async Task<ProjectAssignment> UpdateTeamMemberRoleAsync(
        int projectId, 
        int employeeId, 
        string role, 
        CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.FirstOrDefaultAsync(
            pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId && pa.Status == AssignmentStatus.Active,
            cancellationToken);

        if (assignment == null)
        {
            throw new ArgumentException($"Active assignment not found for employee {employeeId} in project {projectId}");
        }

        var oldRole = assignment.Role;
        assignment.Role = role;
        assignment.UpdatedAt = DateTime.UtcNow;

        var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ProjectAssignment", assignment.Id, "Updated", 
            $"Role updated from '{oldRole}' to '{role}' for employee {employeeId} in project {projectId}", cancellationToken);

        return updatedAssignment;
    }

    /// <summary>
    /// Get project team members
    /// </summary>
    public async Task<IEnumerable<Employee>> GetProjectTeamMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _assignmentRepository.GetProjectTeamMembersAsync(projectId, cancellationToken);
    }

    #endregion

    #region Task Management

    /// <summary>
    /// Create a new task in a project
    /// </summary>
    public async Task<ProjectTask> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new task in project: {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new ArgumentException($"Project with ID {request.ProjectId} not found");
        }

        var task = new ProjectTask
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            EstimatedHours = request.EstimatedHours,
            Priority = request.Priority,
            Status = Core.Entities.TaskStatus.ToDo,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        var createdTask = await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        // Assign employees to the task
        if (request.AssignedEmployeeIds.Any())
        {
            await AssignTaskToEmployeesAsync(createdTask.Id, request.AssignedEmployeeIds, cancellationToken);
        }

        await _auditService.LogAsync("ProjectTask", createdTask.Id, "Created", 
            $"Task '{createdTask.Title}' created in project {request.ProjectId}", cancellationToken);

        _logger.LogInformation("Task created successfully: {TaskId}", createdTask.Id);
        return createdTask;
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    public async Task<ProjectTask> UpdateTaskAsync(int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task: {TaskId}", taskId);

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new ArgumentException($"Task with ID {taskId} not found");
        }

        var oldValues = $"Title: {task.Title}, Status: {task.Status}, Priority: {task.Priority}";

        task.Title = request.Title;
        task.Description = request.Description;
        task.EstimatedHours = request.EstimatedHours;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        var newValues = $"Title: {task.Title}, Status: {task.Status}, Priority: {task.Priority}";
        await _auditService.LogAsync("ProjectTask", taskId, "Updated", 
            $"Task updated. Old: {oldValues}, New: {newValues}", cancellationToken);

        _logger.LogInformation("Task updated successfully: {TaskId}", taskId);
        return updatedTask;
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    public async Task<ProjectTask?> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetTaskWithAssignmentsAsync(taskId, cancellationToken);
    }

    /// <summary>
    /// Get tasks by project
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(
        int projectId, 
        Core.Entities.TaskStatus? status = null, 
        TaskPriority? priority = null, 
        CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetTasksByProjectAsync(projectId, status, priority, cancellationToken);
    }

    /// <summary>
    /// Delete a task (soft delete)
    /// </summary>
    public async Task<bool> DeleteTaskAsync(int taskId, string deletedBy, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task: {TaskId}", taskId);

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            return false;
        }

        await _taskRepository.SoftDeleteAsync(task, deletedBy, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ProjectTask", taskId, "Deleted", 
            $"Task '{task.Title}' deleted", cancellationToken);

        _logger.LogInformation("Task deleted successfully: {TaskId}", taskId);
        return true;
    }

    #endregion

    #region Task Assignment Management

    /// <summary>
    /// Assign employees to a task
    /// </summary>
    public async Task<IEnumerable<TaskAssignment>> AssignTaskToEmployeesAsync(
        int taskId, 
        IEnumerable<int> employeeIds, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning employees to task: {TaskId}", taskId);

        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null)
        {
            throw new ArgumentException($"Task with ID {taskId} not found");
        }

        var assignments = new List<TaskAssignment>();

        foreach (var employeeId in employeeIds)
        {
            // Check if employee exists
            var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found: {EmployeeId}", employeeId);
                continue;
            }

            // Check if already assigned
            var existingAssignment = await _taskAssignmentRepository.IsEmployeeAssignedToTaskAsync(
                employeeId, taskId, cancellationToken);

            if (existingAssignment)
            {
                _logger.LogWarning("Employee {EmployeeId} already assigned to task {TaskId}", employeeId, taskId);
                continue;
            }

            var assignment = new TaskAssignment
            {
                TaskId = taskId,
                EmployeeId = employeeId,
                AssignedDate = DateTime.UtcNow
            };

            assignments.Add(assignment);
        }

        if (assignments.Any())
        {
            await _taskAssignmentRepository.AddRangeAsync(assignments, cancellationToken);
            await _taskAssignmentRepository.SaveChangesAsync(cancellationToken);

            foreach (var assignment in assignments)
            {
                await _auditService.LogAsync("TaskAssignment", assignment.Id, "Created", 
                    $"Employee {assignment.EmployeeId} assigned to task {taskId}", cancellationToken);
            }
        }

        _logger.LogInformation("Employees assigned successfully to task: {TaskId}", taskId);
        return assignments;
    }

    /// <summary>
    /// Remove employee from task
    /// </summary>
    public async Task<bool> RemoveTaskAssignmentAsync(int taskId, int employeeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing employee {EmployeeId} from task {TaskId}", employeeId, taskId);

        var assignment = await _taskAssignmentRepository.FirstOrDefaultAsync(
            ta => ta.TaskId == taskId && ta.EmployeeId == employeeId,
            cancellationToken);

        if (assignment == null)
        {
            return false;
        }

        await _taskAssignmentRepository.DeleteAsync(assignment, cancellationToken);
        await _taskAssignmentRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("TaskAssignment", assignment.Id, "Deleted", 
            $"Employee {employeeId} removed from task {taskId}", cancellationToken);

        _logger.LogInformation("Employee removed successfully from task");
        return true;
    }

    /// <summary>
    /// Get tasks assigned to employee
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetTasksByEmployeeAsync(employeeId, cancellationToken);
    }

    #endregion

    #region Project Progress and Analytics

    /// <summary>
    /// Get project progress information
    /// </summary>
    public async Task<ProjectProgress> GetProjectProgressAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetProjectWithDetailsAsync(projectId, cancellationToken);
        if (project == null)
        {
            throw new ArgumentException($"Project with ID {projectId} not found");
        }

        var tasks = project.Tasks.ToList();
        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Done);
        var inProgressTasks = tasks.Count(t => t.Status == Core.Entities.TaskStatus.InProgress);
        var todoTasks = tasks.Count(t => t.Status == Core.Entities.TaskStatus.ToDo);

        var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

        // Calculate actual hours from DSR records
        var actualHours = await GetActualHoursWorkedAsync(projectId, cancellationToken);
        var hoursVariance = actualHours - project.EstimatedHours;
        var isOnTrack = actualHours <= project.EstimatedHours && DateTime.UtcNow <= project.EndDate;
        var remainingHours = Math.Max(0, project.EstimatedHours - actualHours);

        var taskProgress = tasks.Select(t => new TaskProgress
        {
            TaskId = t.Id,
            Title = t.Title,
            Status = t.Status,
            EstimatedHours = t.EstimatedHours,
            ActualHours = GetTaskActualHours(t.Id), // This would need DSR integration
            CompletionPercentage = t.Status == Core.Entities.TaskStatus.Done ? 100 : 
                                 t.Status == Core.Entities.TaskStatus.InProgress ? 50 : 0,
            IsOverdue = t.DueDate < DateTime.UtcNow && t.Status != Core.Entities.TaskStatus.Done,
            DueDate = t.DueDate,
            AssignedEmployees = t.TaskAssignments.Select(ta => $"{ta.Employee.FirstName} {ta.Employee.LastName}").ToList()
        }).ToList();

        return new ProjectProgress
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            TodoTasks = todoTasks,
            CompletionPercentage = completionPercentage,
            TotalEstimatedHours = project.EstimatedHours,
            ActualHoursWorked = actualHours,
            HoursVariance = hoursVariance,
            IsOnTrack = isOnTrack,
            RemainingHours = remainingHours,
            EstimatedCompletionDate = CalculateEstimatedCompletionDate(project, actualHours),
            TaskProgress = taskProgress
        };
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    public async Task<ProjectStatistics> GetProjectStatisticsAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        return await _projectRepository.GetProjectStatisticsAsync(branchId, cancellationToken);
    }

    /// <summary>
    /// Get overdue projects
    /// </summary>
    public async Task<IEnumerable<Project>> GetOverdueProjectsAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<Project, bool>> predicate = branchId.HasValue 
            ? p => p.EndDate < DateTime.UtcNow && p.Status != ProjectStatus.Completed && p.CreatedByEmployee.BranchId == branchId.Value
            : p => p.EndDate < DateTime.UtcNow && p.Status != ProjectStatus.Completed;

        return await _projectRepository.FindAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(int? projectId = null, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.GetOverdueTasksAsync(projectId, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    private Task<int> GetActualHoursWorkedAsync(int projectId, CancellationToken cancellationToken)
    {
        // This would integrate with DSR service to get actual hours
        // For now, returning 0 as placeholder
        return Task.FromResult(0);
    }

    private int GetTaskActualHours(int taskId)
    {
        // This would integrate with DSR service to get task-specific hours
        // For now, returning 0 as placeholder
        return 0;
    }

    private DateTime? CalculateEstimatedCompletionDate(Project project, int actualHours)
    {
        if (actualHours == 0) return project.EndDate;

        var progress = (decimal)actualHours / project.EstimatedHours;
        if (progress == 0) return project.EndDate;

        var daysElapsed = (DateTime.UtcNow - project.StartDate).Days;
        var estimatedTotalDays = (int)(daysElapsed / progress);
        
        return project.StartDate.AddDays(estimatedTotalDays);
    }

    #endregion
}