using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProjectTask entity operations
/// </summary>
public class ProjectTaskRepository : Repository<ProjectTask>, IProjectTaskRepository
{
    public ProjectTaskRepository(StrideHRDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get tasks by project with filtering
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(
        int projectId,
        Core.Entities.TaskStatus? status = null,
        TaskPriority? priority = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .Where(t => t.ProjectId == projectId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        return await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get tasks assigned to a specific employee
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .Where(t => t.TaskAssignments.Any(ta => ta.EmployeeId == employeeId))
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get task with assignments
    /// </summary>
    public async Task<ProjectTask?> GetTaskWithAssignmentsAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.Project)
                .ThenInclude(p => p.CreatedByEmployee)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    public async Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .Where(t => t.DueDate < DateTime.UtcNow && t.Status != Core.Entities.TaskStatus.Done && t.Status != Core.Entities.TaskStatus.Cancelled);

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }
}