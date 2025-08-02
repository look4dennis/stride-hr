using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ProjectTaskRepository : Repository<ProjectTask>, IProjectTaskRepository
{
    public ProjectTaskRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId)
    {
        return await _dbSet
            .Where(t => t.ProjectId == projectId && !t.IsDeleted)
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(t => (t.AssignedToEmployeeId == employeeId || 
                        t.TaskAssignments.Any(ta => ta.EmployeeId == employeeId && ta.CompletedDate == null)) && 
                        !t.IsDeleted)
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetTasksByEmployeeAsync(int employeeId, int? projectId)
    {
        var query = _dbSet
            .Where(t => (t.AssignedToEmployeeId == employeeId || 
                        t.TaskAssignments.Any(ta => ta.EmployeeId == employeeId && ta.CompletedDate == null)) && 
                        !t.IsDeleted);

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        return await query
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetTasksByStatusAsync(ProjectTaskStatus status)
    {
        return await _dbSet
            .Where(t => t.Status == status && !t.IsDeleted)
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectTask?> GetTaskWithDetailsAsync(int taskId)
    {
        return await _dbSet
            .Where(t => t.Id == taskId && !t.IsDeleted)
            .Include(t => t.Project)
                .ThenInclude(p => p.Branch)
            .Include(t => t.AssignedToEmployee)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Employee)
            .Include(t => t.DSRs)
                .ThenInclude(d => d.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(t => t.DueDate.HasValue && t.DueDate.Value < today && 
                       t.Status != ProjectTaskStatus.Done && !t.IsDeleted)
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.DueDate.HasValue && t.DueDate.Value >= startDate && 
                       t.DueDate.Value <= endDate && !t.IsDeleted)
            .Include(t => t.Project)
            .Include(t => t.AssignedToEmployee)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<int> GetMaxDisplayOrderAsync(int projectId)
    {
        var maxOrder = await _dbSet
            .Where(t => t.ProjectId == projectId && !t.IsDeleted)
            .MaxAsync(t => (int?)t.DisplayOrder);
        
        return maxOrder ?? 0;
    }
}