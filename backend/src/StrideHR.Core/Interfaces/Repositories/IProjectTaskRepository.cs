using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
    Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId);
    Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId);
    Task<IEnumerable<ProjectTask>> GetTasksByStatusAsync(ProjectTaskStatus status);
    Task<ProjectTask?> GetTaskWithDetailsAsync(int taskId);
    Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync();
    Task<IEnumerable<ProjectTask>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetMaxDisplayOrderAsync(int projectId);
}