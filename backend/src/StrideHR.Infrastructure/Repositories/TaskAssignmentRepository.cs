using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class TaskAssignmentRepository : Repository<TaskAssignment>, ITaskAssignmentRepository
{
    public TaskAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByTaskAsync(int taskId)
    {
        return await _dbSet
            .Where(ta => ta.TaskId == taskId && !ta.IsDeleted)
            .Include(ta => ta.Task)
            .Include(ta => ta.Employee)
            .OrderByDescending(ta => ta.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(ta => ta.EmployeeId == employeeId && !ta.IsDeleted)
            .Include(ta => ta.Task)
                .ThenInclude(t => t.Project)
            .Include(ta => ta.Employee)
            .OrderByDescending(ta => ta.AssignedDate)
            .ToListAsync();
    }

    public async Task<TaskAssignment?> GetAssignmentAsync(int taskId, int employeeId)
    {
        return await _dbSet
            .Where(ta => ta.TaskId == taskId && ta.EmployeeId == employeeId && !ta.IsDeleted)
            .Include(ta => ta.Task)
            .Include(ta => ta.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TaskAssignment>> GetActiveAssignmentsByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(ta => ta.EmployeeId == employeeId && ta.CompletedDate == null && !ta.IsDeleted)
            .Include(ta => ta.Task)
                .ThenInclude(t => t.Project)
            .Include(ta => ta.Employee)
            .OrderBy(ta => ta.Task.DueDate)
            .ThenByDescending(ta => ta.Task.Priority)
            .ToListAsync();
    }
}