using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Project entity operations
/// </summary>
public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(StrideHRDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get projects by branch with filtering and pagination
    /// </summary>
    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsByBranchAsync(
        int branchId,
        int pageNumber = 1,
        int pageSize = 10,
        ProjectStatus? status = null,
        ProjectPriority? priority = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
            .Where(p => p.CreatedByEmployee.BranchId == branchId);

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(p => p.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   (p.Description != null && p.Description.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (projects, totalCount);
    }

    /// <summary>
    /// Get project with all related data (assignments, tasks)
    /// </summary>
    public async Task<Project?> GetProjectWithDetailsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.CreatedByEmployee)
                .ThenInclude(e => e.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.Employee)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    /// <summary>
    /// Get projects assigned to a specific employee
    /// </summary>
    public async Task<IEnumerable<Project>> GetProjectsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Where(p => p.ProjectAssignments.Any(pa => pa.EmployeeId == employeeId && pa.Status == AssignmentStatus.Active))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get projects created by a specific employee
    /// </summary>
    public async Task<IEnumerable<Project>> GetProjectsByCreatorAsync(int creatorId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Where(p => p.CreatedByEmployeeId == creatorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get project statistics for dashboard
    /// </summary>
    public async Task<ProjectStatistics> GetProjectStatisticsAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        var projectQuery = _context.Projects.AsQueryable();
        var taskQuery = _context.Tasks.AsQueryable();

        if (branchId.HasValue)
        {
            projectQuery = projectQuery.Where(p => p.CreatedByEmployee.BranchId == branchId.Value);
            taskQuery = taskQuery.Where(t => t.Project.CreatedByEmployee.BranchId == branchId.Value);
        }

        var totalProjects = await projectQuery.CountAsync(cancellationToken);
        var activeProjects = await projectQuery.CountAsync(p => p.Status == ProjectStatus.InProgress, cancellationToken);
        var completedProjects = await projectQuery.CountAsync(p => p.Status == ProjectStatus.Completed, cancellationToken);
        var overdueProjects = await projectQuery.CountAsync(p => p.EndDate < DateTime.UtcNow && p.Status != ProjectStatus.Completed, cancellationToken);

        var totalTasks = await taskQuery.CountAsync(cancellationToken);
        var completedTasks = await taskQuery.CountAsync(t => t.Status == Core.Entities.TaskStatus.Done, cancellationToken);
        var overdueTasks = await taskQuery.CountAsync(t => t.DueDate < DateTime.UtcNow && t.Status != Core.Entities.TaskStatus.Done, cancellationToken);

        var totalBudget = await projectQuery.SumAsync(p => p.Budget, cancellationToken);
        var totalEstimatedHours = await projectQuery.SumAsync(p => p.EstimatedHours, cancellationToken);

        // Get actual hours from DSR records
        var actualHoursWorked = await _context.DSRs
            .Where(d => d.ProjectId != null && (branchId == null || d.Employee.BranchId == branchId.Value))
            .SumAsync(d => (int)d.HoursWorked, cancellationToken);

        return new ProjectStatistics
        {
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            CompletedProjects = completedProjects,
            OverdueProjects = overdueProjects,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = overdueTasks,
            TotalBudget = totalBudget,
            SpentBudget = 0, // This would need payroll integration
            TotalEstimatedHours = totalEstimatedHours,
            ActualHoursWorked = actualHoursWorked
        };
    }
}