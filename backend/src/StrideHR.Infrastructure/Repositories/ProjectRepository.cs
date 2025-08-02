using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetProjectsByBranchAsync(int branchId)
    {
        return await _dbSet
            .Where(p => p.BranchId == branchId && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(p => p.ProjectAssignments.Any(pa => pa.EmployeeId == employeeId && pa.UnassignedDate == null) && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetProjectWithDetailsAsync(int projectId)
    {
        return await _dbSet
            .Where(p => p.Id == projectId && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedToEmployee)
            .Include(p => p.DSRs)
                .ThenInclude(d => d.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status)
    {
        return await _dbSet
            .Where(p => p.Status == status && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(p => p.StartDate >= startDate && p.EndDate <= endDate && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsEmployeeAssignedToProjectAsync(int projectId, int employeeId)
    {
        return await _context.Set<ProjectAssignment>()
            .AnyAsync(pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId && pa.UnassignedDate == null && !pa.IsDeleted);
    }

    public async Task<IEnumerable<Project>> GetProjectsByTeamLeadAsync(int teamLeadId)
    {
        return await _dbSet
            .Where(p => p.ProjectAssignments.Any(pa => pa.EmployeeId == teamLeadId && pa.IsTeamLead && pa.UnassignedDate == null) && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByTeamLeaderAsync(int teamLeaderId)
    {
        return await GetProjectsByTeamLeadAsync(teamLeaderId);
    }

    public async Task<int> GetProjectTeamMembersCountAsync(int projectId)
    {
        return await _context.Set<ProjectAssignment>()
            .CountAsync(pa => pa.ProjectId == projectId && pa.UnassignedDate == null && !pa.IsDeleted);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await _dbSet
            .Where(p => p.Status == ProjectStatus.Active && !p.IsDeleted)
            .Include(p => p.CreatedByEmployee)
            .Include(p => p.Branch)
            .Include(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Employee)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeAsync(int employeeId)
    {
        return await _context.Set<Employee>()
            .Where(e => e.Id == employeeId && !e.IsDeleted)
            .FirstOrDefaultAsync();
    }
}