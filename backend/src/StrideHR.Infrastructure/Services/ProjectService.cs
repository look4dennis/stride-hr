using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly ITaskAssignmentRepository _taskAssignmentRepository;
    private readonly IDSRRepository _dsrRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectTaskRepository taskRepository,
        IProjectAssignmentRepository assignmentRepository,
        ITaskAssignmentRepository taskAssignmentRepository,
        IDSRRepository dsrRepository,
        IMapper mapper,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
        _taskAssignmentRepository = taskAssignmentRepository;
        _dsrRepository = dsrRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, int createdByEmployeeId)
    {
        try
        {
            var project = _mapper.Map<Project>(dto);
            project.CreatedByEmployeeId = createdByEmployeeId;
            project.CreatedBy = createdByEmployeeId.ToString();
            project.Status = ProjectStatus.Planning;

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            // Assign team members if provided
            if (dto.TeamMemberIds.Any())
            {
                await AssignTeamMembersAsync(project.Id, dto.TeamMemberIds, createdByEmployeeId);
            }

            // Set team lead if provided
            if (dto.TeamLeadId.HasValue)
            {
                await SetTeamLeadAsync(project.Id, dto.TeamLeadId.Value, createdByEmployeeId);
            }

            _logger.LogInformation("Project created successfully with ID: {ProjectId}", project.Id);
            
            var createdProject = await _projectRepository.GetProjectWithDetailsAsync(project.Id);
            return _mapper.Map<ProjectDto>(createdProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            throw;
        }
    }

    public async Task<ProjectDto> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int updatedByEmployeeId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.Name))
                project.Name = dto.Name;
            
            if (!string.IsNullOrEmpty(dto.Description))
                project.Description = dto.Description;
            
            if (dto.StartDate.HasValue)
                project.StartDate = dto.StartDate.Value;
            
            if (dto.EndDate.HasValue)
                project.EndDate = dto.EndDate.Value;
            
            if (dto.EstimatedHours.HasValue)
                project.EstimatedHours = dto.EstimatedHours.Value;
            
            if (dto.Budget.HasValue)
                project.Budget = dto.Budget.Value;
            
            if (dto.Status.HasValue)
                project.Status = dto.Status.Value;
            
            if (dto.Priority.HasValue)
                project.Priority = dto.Priority.Value;

            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = updatedByEmployeeId.ToString();

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project updated successfully with ID: {ProjectId}", projectId);
            
            var updatedProject = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            return _mapper.Map<ProjectDto>(updatedProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project with ID: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(int projectId, int deletedByEmployeeId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return false;

            project.IsDeleted = true;
            project.DeletedAt = DateTime.UtcNow;
            project.DeletedBy = deletedByEmployeeId.ToString();

            await _projectRepository.UpdateAsync(project);
            var result = await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project soft deleted with ID: {ProjectId}", projectId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project with ID: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                return null;

            var projectDto = _mapper.Map<ProjectDto>(project);
            projectDto.Progress = await GetProjectProgressAsync(projectId);
            
            return projectDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project with ID: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByBranchAsync(int branchId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByBranchAsync(branchId);
            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            // Add progress information for each project
            foreach (var projectDto in projectDtos)
            {
                projectDto.Progress = await GetProjectProgressAsync(projectDto.Id);
            }

            return projectDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for branch: {BranchId}", branchId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByEmployeeAsync(int employeeId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByEmployeeAsync(employeeId);
            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            // Add progress information for each project
            foreach (var projectDto in projectDtos)
            {
                projectDto.Progress = await GetProjectProgressAsync(projectDto.Id);
            }

            return projectDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByTeamLeadAsync(int teamLeadId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeadId);
            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);

            // Add progress information for each project
            foreach (var projectDto in projectDtos)
            {
                projectDto.Progress = await GetProjectProgressAsync(projectDto.Id);
            }

            return projectDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for team lead: {TeamLeadId}", teamLeadId);
            throw;
        }
    }

    public async Task<bool> AssignTeamMembersAsync(int projectId, List<int> employeeIds, int assignedByEmployeeId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return false;

            var assignments = new List<ProjectAssignment>();
            
            foreach (var employeeId in employeeIds)
            {
                // Check if employee is already assigned
                var existingAssignment = await _assignmentRepository.GetAssignmentAsync(projectId, employeeId);
                if (existingAssignment == null)
                {
                    assignments.Add(new ProjectAssignment
                    {
                        ProjectId = projectId,
                        EmployeeId = employeeId,
                        AssignedDate = DateTime.UtcNow,
                        CreatedBy = assignedByEmployeeId.ToString()
                    });
                }
            }

            if (assignments.Any())
            {
                await _assignmentRepository.AddRangeAsync(assignments);
                await _assignmentRepository.SaveChangesAsync();
            }

            _logger.LogInformation("Team members assigned to project: {ProjectId}", projectId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning team members to project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> UnassignTeamMemberAsync(int projectId, int employeeId, int unassignedByEmployeeId)
    {
        try
        {
            var assignment = await _assignmentRepository.GetAssignmentAsync(projectId, employeeId);
            if (assignment == null)
                return false;

            assignment.UnassignedDate = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;
            assignment.UpdatedBy = unassignedByEmployeeId.ToString();

            await _assignmentRepository.UpdateAsync(assignment);
            var result = await _assignmentRepository.SaveChangesAsync();

            _logger.LogInformation("Team member unassigned from project: {ProjectId}, Employee: {EmployeeId}", projectId, employeeId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning team member from project: {ProjectId}, Employee: {EmployeeId}", projectId, employeeId);
            throw;
        }
    }

    public async Task<bool> SetTeamLeadAsync(int projectId, int employeeId, int setByEmployeeId)
    {
        try
        {
            // First, remove team lead status from all current team leads
            var currentAssignments = await _assignmentRepository.GetAssignmentsByProjectAsync(projectId);
            foreach (var assignment in currentAssignments.Where(a => a.IsTeamLead))
            {
                assignment.IsTeamLead = false;
                assignment.UpdatedAt = DateTime.UtcNow;
                assignment.UpdatedBy = setByEmployeeId.ToString();
                await _assignmentRepository.UpdateAsync(assignment);
            }

            // Set the new team lead
            var targetAssignment = await _assignmentRepository.GetAssignmentAsync(projectId, employeeId);
            if (targetAssignment == null)
            {
                // Create assignment if it doesn't exist
                targetAssignment = new ProjectAssignment
                {
                    ProjectId = projectId,
                    EmployeeId = employeeId,
                    AssignedDate = DateTime.UtcNow,
                    IsTeamLead = true,
                    CreatedBy = setByEmployeeId.ToString()
                };
                await _assignmentRepository.AddAsync(targetAssignment);
            }
            else
            {
                targetAssignment.IsTeamLead = true;
                targetAssignment.UpdatedAt = DateTime.UtcNow;
                targetAssignment.UpdatedBy = setByEmployeeId.ToString();
                await _assignmentRepository.UpdateAsync(targetAssignment);
            }

            var result = await _assignmentRepository.SaveChangesAsync();
            _logger.LogInformation("Team lead set for project: {ProjectId}, Employee: {EmployeeId}", projectId, employeeId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting team lead for project: {ProjectId}, Employee: {EmployeeId}", projectId, employeeId);
            throw;
        }
    }

    public async Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetAssignmentsByProjectAsync(projectId);
            return _mapper.Map<List<ProjectTeamMemberDto>>(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for project: {ProjectId}", projectId);
            throw;
        }
    }

    // Task Management Methods
    public async Task<ProjectTaskDto> CreateTaskAsync(CreateTaskDto dto, int createdByEmployeeId)
    {
        try
        {
            var task = _mapper.Map<ProjectTask>(dto);
            task.CreatedBy = createdByEmployeeId.ToString();
            
            // Set display order if not provided
            if (task.DisplayOrder == 0)
            {
                task.DisplayOrder = await _taskRepository.GetMaxDisplayOrderAsync(dto.ProjectId) + 1;
            }

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            // Assign task to employee if provided
            if (dto.AssignedToEmployeeId.HasValue)
            {
                await AssignTaskToEmployeeAsync(task.Id, dto.AssignedToEmployeeId.Value, createdByEmployeeId);
            }

            _logger.LogInformation("Task created successfully with ID: {TaskId}", task.Id);
            
            var createdTask = await _taskRepository.GetTaskWithDetailsAsync(task.Id);
            return _mapper.Map<ProjectTaskDto>(createdTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            throw;
        }
    }

    public async Task<ProjectTaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int updatedByEmployeeId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ArgumentException($"Task with ID {taskId} not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.Title))
                task.Title = dto.Title;
            
            if (!string.IsNullOrEmpty(dto.Description))
                task.Description = dto.Description;
            
            if (dto.EstimatedHours.HasValue)
                task.EstimatedHours = dto.EstimatedHours.Value;
            
            if (dto.Status.HasValue)
                task.Status = dto.Status.Value;
            
            if (dto.Priority.HasValue)
                task.Priority = dto.Priority.Value;
            
            if (dto.DueDate.HasValue)
                task.DueDate = dto.DueDate.Value;
            
            if (dto.AssignedToEmployeeId.HasValue)
                task.AssignedToEmployeeId = dto.AssignedToEmployeeId.Value;
            
            if (dto.DisplayOrder.HasValue)
                task.DisplayOrder = dto.DisplayOrder.Value;

            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = updatedByEmployeeId.ToString();

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task updated successfully with ID: {TaskId}", taskId);
            
            var updatedTask = await _taskRepository.GetTaskWithDetailsAsync(taskId);
            return _mapper.Map<ProjectTaskDto>(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with ID: {TaskId}", taskId);
            throw;
        }
    }

    public async Task<bool> DeleteTaskAsync(int taskId, int deletedByEmployeeId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return false;

            task.IsDeleted = true;
            task.DeletedAt = DateTime.UtcNow;
            task.DeletedBy = deletedByEmployeeId.ToString();

            await _taskRepository.UpdateAsync(task);
            var result = await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task soft deleted with ID: {TaskId}", taskId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with ID: {TaskId}", taskId);
            throw;
        }
    }

    public async Task<ProjectTaskDto?> GetTaskByIdAsync(int taskId)
    {
        try
        {
            var task = await _taskRepository.GetTaskWithDetailsAsync(taskId);
            if (task == null)
                return null;

            var taskDto = _mapper.Map<ProjectTaskDto>(task);
            
            // Calculate actual hours worked
            taskDto.ActualHoursWorked = await _dsrRepository.GetTotalHoursByProjectAsync(task.ProjectId);
            
            // Check if task is overdue
            taskDto.IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.Today && task.Status != ProjectTaskStatus.Done;
            
            return taskDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task with ID: {TaskId}", taskId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetTasksByProjectAsync(int projectId)
    {
        try
        {
            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);
            var taskDtos = _mapper.Map<IEnumerable<ProjectTaskDto>>(tasks);

            foreach (var taskDto in taskDtos)
            {
                // Calculate actual hours worked for each task
                var dsrs = await _dsrRepository.GetDSRsByProjectAsync(projectId);
                taskDto.ActualHoursWorked = dsrs.Where(d => d.TaskId == taskDto.Id).Sum(d => d.HoursWorked);
                
                // Check if task is overdue
                var task = tasks.First(t => t.Id == taskDto.Id);
                taskDto.IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.Today && task.Status != ProjectTaskStatus.Done;
            }

            return taskDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetTasksByEmployeeAsync(int employeeId)
    {
        try
        {
            var tasks = await _taskRepository.GetTasksByEmployeeAsync(employeeId);
            var taskDtos = _mapper.Map<IEnumerable<ProjectTaskDto>>(tasks);

            foreach (var taskDto in taskDtos)
            {
                // Calculate actual hours worked for each task
                var dsrs = await _dsrRepository.GetDSRsByEmployeeAsync(employeeId);
                taskDto.ActualHoursWorked = dsrs.Where(d => d.TaskId == taskDto.Id).Sum(d => d.HoursWorked);
                
                // Check if task is overdue
                var task = tasks.First(t => t.Id == taskDto.Id);
                taskDto.IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.Today && task.Status != ProjectTaskStatus.Done;
            }

            return taskDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> AssignTaskToEmployeeAsync(int taskId, int employeeId, int assignedByEmployeeId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return false;

            // Update the main assigned employee
            task.AssignedToEmployeeId = employeeId;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = assignedByEmployeeId.ToString();
            await _taskRepository.UpdateAsync(task);

            // Create task assignment record
            var existingAssignment = await _taskAssignmentRepository.GetAssignmentAsync(taskId, employeeId);
            if (existingAssignment == null)
            {
                var assignment = new TaskAssignment
                {
                    TaskId = taskId,
                    EmployeeId = employeeId,
                    AssignedDate = DateTime.UtcNow,
                    CreatedBy = assignedByEmployeeId.ToString()
                };
                await _taskAssignmentRepository.AddAsync(assignment);
            }

            var result = await _taskRepository.SaveChangesAsync();
            _logger.LogInformation("Task assigned to employee: {TaskId}, Employee: {EmployeeId}", taskId, employeeId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task to employee: {TaskId}, Employee: {EmployeeId}", taskId, employeeId);
            throw;
        }
    }

    public async Task<bool> UpdateTaskStatusAsync(int taskId, ProjectTaskStatus status, int updatedByEmployeeId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                return false;

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = updatedByEmployeeId.ToString();

            // If task is completed, update task assignments
            if (status == ProjectTaskStatus.Done)
            {
                var assignments = await _taskAssignmentRepository.GetAssignmentsByTaskAsync(taskId);
                foreach (var assignment in assignments.Where(a => a.CompletedDate == null))
                {
                    assignment.CompletedDate = DateTime.UtcNow;
                    assignment.UpdatedAt = DateTime.UtcNow;
                    assignment.UpdatedBy = updatedByEmployeeId.ToString();
                    await _taskAssignmentRepository.UpdateAsync(assignment);
                }
            }

            await _taskRepository.UpdateAsync(task);
            var result = await _taskRepository.SaveChangesAsync();

            _logger.LogInformation("Task status updated: {TaskId}, Status: {Status}", taskId, status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status: {TaskId}, Status: {Status}", taskId, status);
            throw;
        }
    }

    // Project Progress and Reporting Methods
    public async Task<ProjectProgressDto> GetProjectProgressAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);
            var actualHours = await _dsrRepository.GetTotalHoursByProjectAsync(projectId);

            var totalTasks = tasks.Count();
            var completedTasks = tasks.Count(t => t.Status == ProjectTaskStatus.Done);
            var inProgressTasks = tasks.Count(t => t.Status == ProjectTaskStatus.InProgress);
            var todoTasks = tasks.Count(t => t.Status == ProjectTaskStatus.ToDo);

            var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;
            var budgetUtilization = project.Budget > 0 ? actualHours / project.EstimatedHours * 100 : 0;

            return new ProjectProgressDto
            {
                ProjectId = projectId,
                TotalEstimatedHours = project.EstimatedHours,
                ActualHoursWorked = actualHours,
                CompletionPercentage = completionPercentage,
                IsOnTrack = actualHours <= project.EstimatedHours,
                RemainingHours = Math.Max(0, project.EstimatedHours - actualHours),
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                TodoTasks = todoTasks,
                BudgetUtilization = budgetUtilization,
                IsOverBudget = actualHours > project.EstimatedHours
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project progress: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectHoursReportDto>> GetHoursTrackingReportAsync(int teamLeaderId)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var reports = new List<ProjectHoursReportDto>();

            foreach (var project in projects)
            {
                var dsrs = await _dsrRepository.GetDSRsByProjectAsync(project.Id);
                var employeeGroups = dsrs.GroupBy(d => d.EmployeeId);

                foreach (var group in employeeGroups)
                {
                    var employee = group.First().Employee;
                    var totalHours = group.Sum(d => d.HoursWorked);
                    var dailyHours = group.Select(d => new DailyHoursDto
                    {
                        Date = d.Date,
                        HoursWorked = d.HoursWorked,
                        Description = d.Description,
                        TaskId = d.TaskId,
                        TaskTitle = d.Task?.Title
                    }).ToList();

                    reports.Add(new ProjectHoursReportDto
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        EmployeeId = employee.Id,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        TotalHoursWorked = totalHours,
                        EstimatedHours = project.EstimatedHours,
                        HoursVariance = totalHours - project.EstimatedHours,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        DailyHours = dailyHours
                    });
                }
            }

            return reports;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hours tracking report for team lead: {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectHoursReportDto>> GetProjectHoursReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            var dsrs = await _dsrRepository.GetDSRsByProjectAsync(projectId, startDate, endDate);
            var employeeGroups = dsrs.GroupBy(d => d.EmployeeId);
            var reports = new List<ProjectHoursReportDto>();

            foreach (var group in employeeGroups)
            {
                var employee = group.First().Employee;
                var totalHours = group.Sum(d => d.HoursWorked);
                var dailyHours = group.Select(d => new DailyHoursDto
                {
                    Date = d.Date,
                    HoursWorked = d.HoursWorked,
                    Description = d.Description,
                    TaskId = d.TaskId,
                    TaskTitle = d.Task?.Title
                }).ToList();

                reports.Add(new ProjectHoursReportDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    EmployeeId = employee.Id,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    TotalHoursWorked = totalHours,
                    EstimatedHours = project.EstimatedHours,
                    HoursVariance = totalHours - project.EstimatedHours,
                    StartDate = startDate ?? project.StartDate,
                    EndDate = endDate ?? project.EndDate,
                    DailyHours = dailyHours
                });
            }

            return reports;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project hours report: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> UpdateProjectEstimatedHoursAsync(int projectId, int estimatedHours, int updatedByEmployeeId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return false;

            project.EstimatedHours = estimatedHours;
            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = updatedByEmployeeId.ToString();

            await _projectRepository.UpdateAsync(project);
            var result = await _projectRepository.SaveChangesAsync();

            _logger.LogInformation("Project estimated hours updated: {ProjectId}, Hours: {EstimatedHours}", projectId, estimatedHours);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project estimated hours: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<decimal> GetActualHoursWorkedAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            return await _dsrRepository.GetTotalHoursByProjectAsync(projectId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting actual hours worked for project: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> IsProjectOnTrackAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return false;

            var actualHours = await _dsrRepository.GetTotalHoursByProjectAsync(projectId);
            return actualHours <= project.EstimatedHours;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project is on track: {ProjectId}", projectId);
            throw;
        }
    }
}