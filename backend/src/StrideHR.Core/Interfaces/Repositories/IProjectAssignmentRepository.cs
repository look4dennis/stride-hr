using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IProjectAssignmentRepository : IRepository<ProjectAssignment>
{
    Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId);
    Task<IEnumerable<ProjectAssignment>> GetAssignmentsByEmployeeAsync(int employeeId);
    Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int employeeId);
    Task<IEnumerable<ProjectAssignment>> GetTeamLeadAssignmentsAsync(int employeeId);
    Task<bool> IsTeamLeadAsync(int projectId, int employeeId);
    Task<IEnumerable<Employee>> GetProjectTeamMembersAsync(int projectId);
}