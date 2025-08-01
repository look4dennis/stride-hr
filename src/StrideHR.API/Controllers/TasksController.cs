using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Security.Claims;
using TaskStatus = StrideHR.Core.Entities.TaskStatus;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for task management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IProjectService projectService, ILogger<TasksController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get tasks with filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetTasks([FromQuery] GetTasksQueryDto query)
    {
        try
        {
            IEnumerable<ProjectTask> tasks;

            if (query.ProjectId.HasValue)
            {
                tasks = await _projectService.GetTasksByProjectAsync(query.ProjectId.Value, query.Status, query.Priority);
            }
            else if (query.EmployeeId.HasValue)
            {
                tasks = await _projectService.GetTasksByEmployeeAsync(query.EmployeeId.Value);
            }
            else
            {
                // Get overdue tasks if requested
                if (query.IsOverdue == true)
                {
                    tasks = await _projectService.GetOverdueTasksAsync();
                }
                else
                {
                    return BadRequest("Either ProjectId or EmployeeId must be specified, or set IsOverdue=true");
                }
            }

            // Apply additional filters
            if (query.Status.HasValue)
            {
                tasks = tasks.Where(t => t.Status == query.Status.Value);
            }

            if (query.Priority.HasValue)
            {
                tasks = tasks.Where(t => t.Priority == query.Priority.Value);
            }

            if (query.IsOverdue == true)
            {
                tasks = tasks.Where(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Done);
            }

            var taskDtos = tasks.Select(t => new ProjectTaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name ?? "",
                Title = t.Title,
                Description = t.Description,
                EstimatedHours = t.EstimatedHours,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                IsOverdue = t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Done,
                Assignments = t.TaskAssignments?.Select(ta => new TaskAssignmentDto
                {
                    Id = ta.Id,
                    EmployeeId = ta.EmployeeId,
                    EmployeeName = $"{ta.Employee.FirstName} {ta.Employee.LastName}",
                    EmployeeEmail = ta.Employee.Email,
                    AssignedDate = ta.AssignedDate
                }).ToList() ?? new List<TaskAssignmentDto>()
            }).ToList();

            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectTaskDto>> GetTask(int id)
    {
        try
        {
            var task = await _projectService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound($"Task with ID {id} not found");
            }

            var taskDto = new ProjectTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? "",
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsOverdue = task.DueDate < DateTime.UtcNow && task.Status != TaskStatus.Done,
                Assignments = task.TaskAssignments?.Select(ta => new TaskAssignmentDto
                {
                    Id = ta.Id,
                    EmployeeId = ta.EmployeeId,
                    EmployeeName = $"{ta.Employee.FirstName} {ta.Employee.LastName}",
                    EmployeeEmail = ta.Employee.Email,
                    AssignedDate = ta.AssignedDate
                }).ToList() ?? new List<TaskAssignmentDto>()
            };

            return Ok(taskDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectTaskDto>> CreateTask([FromBody] CreateTaskDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateTaskRequest
            {
                ProjectId = createDto.ProjectId,
                Title = createDto.Title,
                Description = createDto.Description,
                EstimatedHours = createDto.EstimatedHours,
                Priority = createDto.Priority,
                DueDate = createDto.DueDate,
                AssignedEmployeeIds = createDto.AssignedEmployeeIds
            };

            var task = await _projectService.CreateTaskAsync(request);

            var taskDto = new ProjectTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectTaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new UpdateTaskRequest
            {
                Title = updateDto.Title,
                Description = updateDto.Description,
                EstimatedHours = updateDto.EstimatedHours,
                Status = updateDto.Status,
                Priority = updateDto.Priority,
                DueDate = updateDto.DueDate
            };

            var task = await _projectService.UpdateTaskAsync(id, request);

            var taskDto = new ProjectTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };

            return Ok(taskDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        try
        {
            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            var result = await _projectService.DeleteTaskAsync(id, currentUser);

            if (!result)
            {
                return NotFound($"Task with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task");
        }
    }

    /// <summary>
    /// Assign employees to a task
    /// </summary>
    [HttpPost("{id}/assignments")]
    public async Task<ActionResult<List<TaskAssignmentDto>>> AssignTask(int id, [FromBody] AssignTaskDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignments = await _projectService.AssignTaskToEmployeesAsync(id, assignDto.EmployeeIds);

            var assignmentDtos = assignments.Select(a => new TaskAssignmentDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                AssignedDate = a.AssignedDate
            }).ToList();

            return Ok(assignmentDtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning employees to task {TaskId}", id);
            return StatusCode(500, "An error occurred while assigning employees to the task");
        }
    }

    /// <summary>
    /// Remove employee from task
    /// </summary>
    [HttpDelete("{id}/assignments/{employeeId}")]
    public async Task<ActionResult> RemoveTaskAssignment(int id, int employeeId)
    {
        try
        {
            var result = await _projectService.RemoveTaskAssignmentAsync(id, employeeId);

            if (!result)
            {
                return NotFound($"Task assignment not found for employee {employeeId} in task {id}");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing employee {EmployeeId} from task {TaskId}", employeeId, id);
            return StatusCode(500, "An error occurred while removing the task assignment");
        }
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetOverdueTasks([FromQuery] int? projectId = null)
    {
        try
        {
            var overdueTasks = await _projectService.GetOverdueTasksAsync(projectId);

            var taskDtos = overdueTasks.Select(t => new ProjectTaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name ?? "",
                Title = t.Title,
                Description = t.Description,
                EstimatedHours = t.EstimatedHours,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                IsOverdue = true,
                Assignments = t.TaskAssignments?.Select(ta => new TaskAssignmentDto
                {
                    Id = ta.Id,
                    EmployeeId = ta.EmployeeId,
                    EmployeeName = $"{ta.Employee.FirstName} {ta.Employee.LastName}",
                    EmployeeEmail = ta.Employee.Email,
                    AssignedDate = ta.AssignedDate
                }).ToList() ?? new List<TaskAssignmentDto>()
            }).ToList();

            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue tasks");
            return StatusCode(500, "An error occurred while retrieving overdue tasks");
        }
    }
}