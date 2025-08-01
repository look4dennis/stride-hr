using StrideHR.Core.Models.Project;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IProjectService
{
    // Project Management
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, int createdByEmployeeId);
    Task<ProjectDto> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int updatedByEmployeeId);
    Task<bool> DeleteProjectAsync(int projectId, int deletedByEmployeeId);
    Task<ProjectDto?> GetProjectByIdAsync(int projectId);
    Task<IEnumerable<ProjectDto>> GetProjectsByBranchAsync(int branchId);
    Task<IEnumerable<ProjectDto>> GetProjectsByEmployeeAsync(int employeeId);
    Task<IEnumerable<ProjectDto>> GetProjectsByTeamLeadAsync(int teamLeadId);
    
    // Team Assignment
    Task<bool> AssignTeamMembersAsync(int projectId, List<int> employeeIds, int assignedByEmployeeId);
    Task<bool> UnassignTeamMemberAsync(int projectId, int employeeId, int unassignedByEmployeeId);
    Task<bool> SetTeamLeadAsync(int projectId, int employeeId, int setByEmployeeId);
    Task<List<ProjectTeamMemberDto>> GetProjectTeamMembersAsync(int projectId);
    
    // Task Management
    Task<ProjectTaskDto> CreateTaskAsync(CreateTaskDto dto, int createdByEmployeeId);
    Task<ProjectTaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int updatedByEmployeeId);
    Task<bool> DeleteTaskAsync(int taskId, int deletedByEmployeeId);
    Task<ProjectTaskDto?> GetTaskByIdAsync(int taskId);
    Task<IEnumerable<ProjectTaskDto>> GetTasksByProjectAsync(int projectId);
    Task<IEnumerable<ProjectTaskDto>> GetTasksByEmployeeAsync(int employeeId);
    Task<bool> AssignTaskToEmployeeAsync(int taskId, int employeeId, int assignedByEmployeeId);
    Task<bool> UpdateTaskStatusAsync(int taskId, ProjectTaskStatus status, int updatedByEmployeeId);
    
    // Project Progress and Reporting
    Task<ProjectProgressDto> GetProjectProgressAsync(int projectId);
    Task<IEnumerable<ProjectHoursReportDto>> GetHoursTrackingReportAsync(int teamLeaderId);
    Task<IEnumerable<ProjectHoursReportDto>> GetProjectHoursReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Project Hours Estimation and Tracking
    Task<bool> UpdateProjectEstimatedHoursAsync(int projectId, int estimatedHours, int updatedByEmployeeId);
    Task<decimal> GetActualHoursWorkedAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> IsProjectOnTrackAsync(int projectId);
}