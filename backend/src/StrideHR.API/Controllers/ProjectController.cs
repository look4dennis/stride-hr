using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;
using StrideHR.Core.Enums;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[Authorize]
public class ProjectController : BaseController
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(IProjectService projectService, ILogger<ProjectController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : 0;
    }

    private int GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }

    // Project Management Endpoints
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var project = await _projectService.CreateProjectAsync(dto, currentEmployeeId);
            return Success(project, "Project created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return Error("Failed to create project");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var project = await _projectService.UpdateProjectAsync(id, dto, currentEmployeeId);
            return Success(project, "Project updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return Error("Failed to update project");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.DeleteProjectAsync(id, currentEmployeeId);
            if (!result)
                return Error("Project not found");

            return Success("Project deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return Error("Failed to delete project");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return Error("Project not found");

            return Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return Error("Failed to get project");
        }
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetProjectsByBranch(int branchId)
    {
        try
        {
            var projects = await _projectService.GetProjectsByBranchAsync(branchId);
            return Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for branch {BranchId}", branchId);
            return Error("Failed to get projects");
        }
    }

    [HttpGet("my-projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var projects = await _projectService.GetProjectsByEmployeeAsync(currentEmployeeId);
            return Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for current employee");
            return Error("Failed to get projects");
        }
    }

    [HttpGet("team-lead-projects")]
    public async Task<IActionResult> GetTeamLeadProjects()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var projects = await _projectService.GetProjectsByTeamLeadAsync(currentEmployeeId);
            return Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team lead projects");
            return Error("Failed to get team lead projects");
        }
    }

    // Team Assignment Endpoints
    [HttpPost("{id}/assign-team")]
    public async Task<IActionResult> AssignTeamMembers(int id, [FromBody] List<int> employeeIds)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.AssignTeamMembersAsync(id, employeeIds, currentEmployeeId);
            if (!result)
                return Error("Failed to assign team members");

            return Success("Team members assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning team members to project {ProjectId}", id);
            return Error("Failed to assign team members");
        }
    }

    [HttpPost("{id}/unassign-member/{employeeId}")]
    public async Task<IActionResult> UnassignTeamMember(int id, int employeeId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.UnassignTeamMemberAsync(id, employeeId, currentEmployeeId);
            if (!result)
                return Error("Failed to unassign team member");

            return Success("Team member unassigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning team member from project {ProjectId}", id);
            return Error("Failed to unassign team member");
        }
    }

    [HttpPost("{id}/set-team-lead/{employeeId}")]
    public async Task<IActionResult> SetTeamLead(int id, int employeeId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.SetTeamLeadAsync(id, employeeId, currentEmployeeId);
            if (!result)
                return Error("Failed to set team lead");

            return Success("Team lead set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting team lead for project {ProjectId}", id);
            return Error("Failed to set team lead");
        }
    }

    [HttpGet("{id}/team-members")]
    public async Task<IActionResult> GetProjectTeamMembers(int id)
    {
        try
        {
            var teamMembers = await _projectService.GetProjectTeamMembersAsync(id);
            return Success(teamMembers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for project {ProjectId}", id);
            return Error("Failed to get team members");
        }
    }

    // Task Management Endpoints
    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var task = await _projectService.CreateTaskAsync(dto, currentEmployeeId);
            return Success(task, "Task created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Error("Failed to create task");
        }
    }

    [HttpPut("tasks/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var task = await _projectService.UpdateTaskAsync(id, dto, currentEmployeeId);
            return Success(task, "Task updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return Error("Failed to update task");
        }
    }

    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.DeleteTaskAsync(id, currentEmployeeId);
            if (!result)
                return Error("Task not found");

            return Success("Task deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return Error("Failed to delete task");
        }
    }

    [HttpGet("tasks/{id}")]
    public async Task<IActionResult> GetTask(int id)
    {
        try
        {
            var task = await _projectService.GetTaskByIdAsync(id);
            if (task == null)
                return Error("Task not found");

            return Success(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", id);
            return Error("Failed to get task");
        }
    }

    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetProjectTasks(int id)
    {
        try
        {
            var tasks = await _projectService.GetTasksByProjectAsync(id);
            return Success(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for project {ProjectId}", id);
            return Error("Failed to get tasks");
        }
    }

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var tasks = await _projectService.GetTasksByEmployeeAsync(currentEmployeeId);
            return Success(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for current employee");
            return Error("Failed to get tasks");
        }
    }

    [HttpPost("tasks/{id}/assign/{employeeId}")]
    public async Task<IActionResult> AssignTaskToEmployee(int id, int employeeId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.AssignTaskToEmployeeAsync(id, employeeId, currentEmployeeId);
            if (!result)
                return Error("Failed to assign task");

            return Success("Task assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {TaskId} to employee {EmployeeId}", id, employeeId);
            return Error("Failed to assign task");
        }
    }

    [HttpPut("tasks/{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] ProjectTaskStatus status)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.UpdateTaskStatusAsync(id, status, currentEmployeeId);
            if (!result)
                return Error("Failed to update task status");

            return Success("Task status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status {TaskId}", id);
            return Error("Failed to update task status");
        }
    }

    // Project Progress and Reporting Endpoints
    [HttpGet("{id}/progress")]
    public async Task<IActionResult> GetProjectProgress(int id)
    {
        try
        {
            var progress = await _projectService.GetProjectProgressAsync(id);
            return Success(progress);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project progress {ProjectId}", id);
            return Error("Failed to get project progress");
        }
    }

    [HttpGet("hours-tracking-report")]
    public async Task<IActionResult> GetHoursTrackingReport()
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var report = await _projectService.GetHoursTrackingReportAsync(currentEmployeeId);
            return Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hours tracking report");
            return Error("Failed to get hours tracking report");
        }
    }

    [HttpGet("{id}/hours-report")]
    public async Task<IActionResult> GetProjectHoursReport(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _projectService.GetProjectHoursReportAsync(id, startDate, endDate);
            return Success(report);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours report {ProjectId}", id);
            return Error("Failed to get project hours report");
        }
    }

    [HttpPut("{id}/estimated-hours")]
    public async Task<IActionResult> UpdateProjectEstimatedHours(int id, [FromBody] int estimatedHours)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _projectService.UpdateProjectEstimatedHoursAsync(id, estimatedHours, currentEmployeeId);
            if (!result)
                return Error("Failed to update estimated hours");

            return Success("Estimated hours updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project estimated hours {ProjectId}", id);
            return Error("Failed to update estimated hours");
        }
    }

    [HttpGet("{id}/actual-hours")]
    public async Task<IActionResult> GetActualHoursWorked(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var actualHours = await _projectService.GetActualHoursWorkedAsync(id, startDate, endDate);
            return Success(actualHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actual hours worked {ProjectId}", id);
            return Error("Failed to get actual hours worked");
        }
    }

    [HttpGet("{id}/is-on-track")]
    public async Task<IActionResult> IsProjectOnTrack(int id)
    {
        try
        {
            var isOnTrack = await _projectService.IsProjectOnTrackAsync(id);
            return Success(isOnTrack);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is on track {ProjectId}", id);
            return Error("Failed to check project status");
        }
    }
}