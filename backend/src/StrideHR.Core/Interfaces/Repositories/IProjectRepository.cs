using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetProjectsByBranchAsync(int branchId);
    Task<IEnumerable<Project>> GetProjectsByEmployeeAsync(int employeeId);
    Task<Project?> GetProjectWithDetailsAsync(int projectId);
    Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status);
    Task<IEnumerable<Project>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsEmployeeAssignedToProjectAsync(int projectId, int employeeId);
    Task<IEnumerable<Project>> GetProjectsByTeamLeadAsync(int teamLeadId);
    Task<IEnumerable<Project>> GetProjectsByTeamLeaderAsync(int teamLeaderId);
    Task<int> GetProjectTeamMembersCountAsync(int projectId);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<Employee?> GetEmployeeAsync(int employeeId);
}