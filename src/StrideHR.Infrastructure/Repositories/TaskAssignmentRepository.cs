using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TaskAssignment entity operations
/// </summary>
public class TaskAssignmentRepository : Repository<TaskAssignment>, ITaskAssignmentRepository
{
    public TaskAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get assignments by task
    /// </summary>
    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByTaskAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Employee)
                .ThenInclude(e => e.Branch)
            .Include(ta => ta.Task)
                .ThenInclude(t => t.Project)
            .Where(ta => ta.TaskId == taskId)
            .OrderBy(ta => ta.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get assignments by employee
    /// </summary>
    public async Task<IEnumerable<TaskAssignment>> GetAssignmentsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Employee)
            .Include(ta => ta.Task)
                .ThenInclude(t => t.Project)
            .Where(ta => ta.EmployeeId == employeeId)
            .OrderByDescending(ta => ta.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if employee is assigned to task
    /// </summary>
    public async Task<bool> IsEmployeeAssignedToTaskAsync(int employeeId, int taskId, CancellationToken cancellationToken = default)
    {
        return await _context.TaskAssignments
            .AnyAsync(ta => ta.EmployeeId == employeeId && ta.TaskId == taskId, cancellationToken);
    }
}