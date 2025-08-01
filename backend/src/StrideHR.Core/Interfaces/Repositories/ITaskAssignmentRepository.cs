using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ITaskAssignmentRepository : IRepository<TaskAssignment>
{
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByTaskAsync(int taskId);
    Task<IEnumerable<TaskAssignment>> GetAssignmentsByEmployeeAsync(int employeeId);
    Task<TaskAssignment?> GetAssignmentAsync(int taskId, int employeeId);
    Task<IEnumerable<TaskAssignment>> GetActiveAssignmentsByEmployeeAsync(int employeeId);
}