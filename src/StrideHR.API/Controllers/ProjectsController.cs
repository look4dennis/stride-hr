using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Security.Claims;
using TaskStatus = StrideHR.Core.Entities.TaskStatus;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for project management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get projects with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedProjectsResponseDto>> GetProjects([FromQuery] GetProjectsQueryDto query)
    {
        try
        {
            var request = new GetProjectsRequest
            {
                BranchId = query.BranchId,
                PageNumber = query.PageNumber,
                PageSize = Math.Min(query.PageSize, 100), // Limit page size
                Status = query.Status,
                Priority = query.Priority,
                SearchTerm = query.SearchTerm,
                CreatedBy = query.CreatedBy,
                AssignedTo = query.AssignedTo
            };

            var (projects, totalCount) = await _projectService.GetProjectsAsync(request);

            var projectDtos = projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                Priority = p.Priority,
                CreatedByEmployeeName = $"{p.CreatedByEmployee.FirstName} {p.CreatedByEmployee.LastName}",
                TeamMemberCount = p.ProjectAssignments.Count(pa => pa.Status == AssignmentStatus.Active),
                TaskCount = p.Tasks.Count,
                CompletedTaskCount = p.Tasks.Count(t => t.Status == TaskStatus.Done),
                CompletionPercentage = p.Tasks.Count > 0 ? (decimal)p.Tasks.Count(t => t.Status == TaskStatus.Done) / p.Tasks.Count * 100 : 0,
                IsOverdue = p.EndDate < DateTime.UtcNow && p.Status != ProjectStatus.Completed
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            var response = new PagedProjectsResponseDto
            {
                Projects = projectDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                HasNextPage = query.PageNumber < totalPages,
                HasPreviousPage = query.PageNumber > 1
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects");
            return StatusCode(500, "An error occurred while retrieving projects");
        }
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                EstimatedHours = project.EstimatedHours,
                Budget = project.Budget,
                Status = project.Status,
                Priority = project.Priority,
                CreatedByEmployeeId = project.CreatedByEmployeeId,
                CreatedByEmployeeName = $"{project.CreatedByEmployee.FirstName} {project.CreatedByEmployee.LastName}",
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                TeamMembers = project.ProjectAssignments.Where(pa => pa.Status == AssignmentStatus.Active).Select(pa => new ProjectAssignmentDto
                {
                    Id = pa.Id,
                    EmployeeId = pa.EmployeeId,
                    EmployeeName = $"{pa.Employee.FirstName} {pa.Employee.LastName}",
                    EmployeeEmail = pa.Employee.Email,
                    Role = pa.Role,
                    StartDate = pa.StartDate,
                    EndDate = pa.EndDate,
                    Status = pa.Status
                }).ToList(),
                Tasks = project.Tasks.Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = project.Name,
                    Title = t.Title,
                    Description = t.Description,
                    EstimatedHours = t.EstimatedHours,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    IsOverdue = t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Done,
                    Assignments = t.TaskAssignments.Select(ta => new TaskAssignmentDto
                    {
                        Id = ta.Id,
                        EmployeeId = ta.EmployeeId,
                        EmployeeName = $"{ta.Employee.FirstName} {ta.Employee.LastName}",
                        EmployeeEmail = ta.Employee.Email,
                        AssignedDate = ta.AssignedDate
                    }).ToList()
                }).ToList()
            };

            // Get project progress
            var progress = await _projectService.GetProjectProgressAsync(id);
            projectDto.Progress = new ProjectProgressDto
            {
                ProjectId = progress.ProjectId,
                ProjectName = progress.ProjectName,
                TotalTasks = progress.TotalTasks,
                CompletedTasks = progress.CompletedTasks,
                InProgressTasks = progress.InProgressTasks,
                TodoTasks = progress.TodoTasks,
                CompletionPercentage = progress.CompletionPercentage,
                TotalEstimatedHours = progress.TotalEstimatedHours,
                ActualHoursWorked = progress.ActualHoursWorked,
                HoursVariance = progress.HoursVariance,
                IsOnTrack = progress.IsOnTrack,
                RemainingHours = progress.RemainingHours,
                EstimatedCompletionDate = progress.EstimatedCompletionDate,
                TaskProgress = progress.TaskProgress.Select(tp => new TaskProgressDto
                {
                    TaskId = tp.TaskId,
                    Title = tp.Title,
                    Status = tp.Status,
                    EstimatedHours = tp.EstimatedHours,
                    ActualHours = tp.ActualHours,
                    CompletionPercentage = tp.CompletionPercentage,
                    IsOverdue = tp.IsOverdue,
                    DueDate = tp.DueDate,
                    AssignedEmployees = tp.AssignedEmployees
                }).ToList()
            };

            return Ok(projectDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, "An error occurred while retrieving the project");
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new CreateProjectRequest
            {
                Name = createDto.Name,
                Description = createDto.Description,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                EstimatedHours = createDto.EstimatedHours,
                Budget = createDto.Budget,
                Priority = createDto.Priority,
                CreatedByEmployeeId = createDto.CreatedByEmployeeId
            };

            var project = await _projectService.CreateProjectAsync(request);

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                EstimatedHours = project.EstimatedHours,
                Budget = project.Budget,
                Status = project.Status,
                Priority = project.Priority,
                CreatedByEmployeeId = project.CreatedByEmployeeId,
                CreatedAt = project.CreatedAt
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, "An error occurred while creating the project");
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new UpdateProjectRequest
            {
                Name = updateDto.Name,
                Description = updateDto.Description,
                StartDate = updateDto.StartDate,
                EndDate = updateDto.EndDate,
                EstimatedHours = updateDto.EstimatedHours,
                Budget = updateDto.Budget,
                Status = updateDto.Status,
                Priority = updateDto.Priority
            };

            var project = await _projectService.UpdateProjectAsync(id, request);

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                EstimatedHours = project.EstimatedHours,
                Budget = project.Budget,
                Status = project.Status,
                Priority = project.Priority,
                CreatedByEmployeeId = project.CreatedByEmployeeId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };

            return Ok(projectDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, "An error occurred while updating the project");
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        try
        {
            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            var result = await _projectService.DeleteProjectAsync(id, currentUser);

            if (!result)
            {
                return NotFound($"Project with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, "An error occurred while deleting the project");
        }
    }

    /// <summary>
    /// Assign team members to a project
    /// </summary>
    [HttpPost("{id}/team-members")]
    public async Task<ActionResult<List<ProjectAssignmentDto>>> AssignTeamMembers(int id, [FromBody] AssignTeamMembersDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var request = new AssignTeamMembersRequest
            {
                TeamMembers = assignDto.TeamMembers.Select(tm => new TeamMemberAssignment
                {
                    EmployeeId = tm.EmployeeId,
                    Role = tm.Role,
                    StartDate = tm.StartDate,
                    EndDate = tm.EndDate
                }).ToList()
            };

            var assignments = await _projectService.AssignTeamMembersAsync(id, request);

            var assignmentDtos = assignments.Select(a => new ProjectAssignmentDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                Role = a.Role,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Status = a.Status
            }).ToList();

            return Ok(assignmentDtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning team members to project {ProjectId}", id);
            return StatusCode(500, "An error occurred while assigning team members");
        }
    }

    /// <summary>
    /// Remove team member from project
    /// </summary>
    [HttpDelete("{id}/team-members/{employeeId}")]
    public async Task<ActionResult> RemoveTeamMember(int id, int employeeId)
    {
        try
        {
            var result = await _projectService.RemoveTeamMemberAsync(id, employeeId);

            if (!result)
            {
                return NotFound($"Team member assignment not found for employee {employeeId} in project {id}");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team member {EmployeeId} from project {ProjectId}", employeeId, id);
            return StatusCode(500, "An error occurred while removing the team member");
        }
    }

    /// <summary>
    /// Get project team members
    /// </summary>
    [HttpGet("{id}/team-members")]
    public async Task<ActionResult<List<ProjectAssignmentDto>>> GetProjectTeamMembers(int id)
    {
        try
        {
            var teamMembers = await _projectService.GetProjectTeamMembersAsync(id);

            var teamMemberDtos = teamMembers.Select(e => new ProjectAssignmentDto
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                EmployeeEmail = e.Email
            }).ToList();

            return Ok(teamMemberDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for project {ProjectId}", id);
            return StatusCode(500, "An error occurred while retrieving team members");
        }
    }

    /// <summary>
    /// Get project progress
    /// </summary>
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<ProjectProgressDto>> GetProjectProgress(int id)
    {
        try
        {
            var progress = await _projectService.GetProjectProgressAsync(id);

            var progressDto = new ProjectProgressDto
            {
                ProjectId = progress.ProjectId,
                ProjectName = progress.ProjectName,
                TotalTasks = progress.TotalTasks,
                CompletedTasks = progress.CompletedTasks,
                InProgressTasks = progress.InProgressTasks,
                TodoTasks = progress.TodoTasks,
                CompletionPercentage = progress.CompletionPercentage,
                TotalEstimatedHours = progress.TotalEstimatedHours,
                ActualHoursWorked = progress.ActualHoursWorked,
                HoursVariance = progress.HoursVariance,
                IsOnTrack = progress.IsOnTrack,
                RemainingHours = progress.RemainingHours,
                EstimatedCompletionDate = progress.EstimatedCompletionDate,
                TaskProgress = progress.TaskProgress.Select(tp => new TaskProgressDto
                {
                    TaskId = tp.TaskId,
                    Title = tp.Title,
                    Status = tp.Status,
                    EstimatedHours = tp.EstimatedHours,
                    ActualHours = tp.ActualHours,
                    CompletionPercentage = tp.CompletionPercentage,
                    IsOverdue = tp.IsOverdue,
                    DueDate = tp.DueDate,
                    AssignedEmployees = tp.AssignedEmployees
                }).ToList()
            };

            return Ok(progressDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for project {ProjectId}", id);
            return StatusCode(500, "An error occurred while retrieving project progress");
        }
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ProjectStatisticsDto>> GetProjectStatistics([FromQuery] int? branchId = null)
    {
        try
        {
            var statistics = await _projectService.GetProjectStatisticsAsync(branchId);

            var statisticsDto = new ProjectStatisticsDto
            {
                TotalProjects = statistics.TotalProjects,
                ActiveProjects = statistics.ActiveProjects,
                CompletedProjects = statistics.CompletedProjects,
                OverdueProjects = statistics.OverdueProjects,
                TotalTasks = statistics.TotalTasks,
                CompletedTasks = statistics.CompletedTasks,
                OverdueTasks = statistics.OverdueTasks,
                TotalBudget = statistics.TotalBudget,
                SpentBudget = statistics.SpentBudget,
                TotalEstimatedHours = statistics.TotalEstimatedHours,
                ActualHoursWorked = statistics.ActualHoursWorked
            };

            return Ok(statisticsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statistics");
            return StatusCode(500, "An error occurred while retrieving project statistics");
        }
    }

    /// <summary>
    /// Get overdue projects
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<ProjectSummaryDto>>> GetOverdueProjects([FromQuery] int? branchId = null)
    {
        try
        {
            var overdueProjects = await _projectService.GetOverdueProjectsAsync(branchId);

            var projectDtos = overdueProjects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                Priority = p.Priority,
                CreatedByEmployeeName = $"{p.CreatedByEmployee.FirstName} {p.CreatedByEmployee.LastName}",
                IsOverdue = true
            }).ToList();

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue projects");
            return StatusCode(500, "An error occurred while retrieving overdue projects");
        }
    }
}