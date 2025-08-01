using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProjectAssignment entity operations
/// </summary>
public class ProjectAssignmentRepository : Repository<ProjectAssignment>, IProjectAssignmentRepository
{
    public ProjectAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get assignments by project
    /// </summary>
    public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectAssignments
            .Include(pa => pa.Employee)
                .ThenInclude(e => e.Branch)
            .Include(pa => pa.Project)
            .Where(pa => pa.ProjectId == projectId)
            .OrderBy(pa => pa.StartDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get assignments by employee
    /// </summary>
    public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectAssignments
            .Include(pa => pa.Employee)
            .Include(pa => pa.Project)
                .ThenInclude(p => p.CreatedByEmployee)
            .Where(pa => pa.EmployeeId == employeeId)
            .OrderByDescending(pa => pa.StartDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if employee is assigned to project
    /// </summary>
    public async Task<bool> IsEmployeeAssignedToProjectAsync(int employeeId, int projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectAssignments
            .AnyAsync(pa => pa.EmployeeId == employeeId && 
                           pa.ProjectId == projectId && 
                           pa.Status == AssignmentStatus.Active, 
                     cancellationToken);
    }

    /// <summary>
    /// Get team members for a project
    /// </summary>
    public async Task<IEnumerable<Employee>> GetProjectTeamMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ProjectAssignments
            .Include(pa => pa.Employee)
                .ThenInclude(e => e.Branch)
            .Where(pa => pa.ProjectId == projectId && pa.Status == AssignmentStatus.Active)
            .Select(pa => pa.Employee)
            .Distinct()
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }
}