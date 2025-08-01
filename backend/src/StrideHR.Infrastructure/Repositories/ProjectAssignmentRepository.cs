using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ProjectAssignmentRepository : Repository<ProjectAssignment>, IProjectAssignmentRepository
{
    public ProjectAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId)
    {
        return await _dbSet
            .Where(pa => pa.ProjectId == projectId && pa.UnassignedDate == null && !pa.IsDeleted)
            .Include(pa => pa.Employee)
            .Include(pa => pa.Project)
            .OrderByDescending(pa => pa.IsTeamLead)
            .ThenBy(pa => pa.Employee.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(pa => pa.EmployeeId == employeeId && pa.UnassignedDate == null && !pa.IsDeleted)
            .Include(pa => pa.Project)
                .ThenInclude(p => p.Branch)
            .Include(pa => pa.Employee)
            .OrderByDescending(pa => pa.AssignedDate)
            .ToListAsync();
    }

    public async Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int employeeId)
    {
        return await _dbSet
            .Where(pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId && 
                        pa.UnassignedDate == null && !pa.IsDeleted)
            .Include(pa => pa.Project)
            .Include(pa => pa.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProjectAssignment>> GetTeamLeadAssignmentsAsync(int employeeId)
    {
        return await _dbSet
            .Where(pa => pa.EmployeeId == employeeId && pa.IsTeamLead && 
                        pa.UnassignedDate == null && !pa.IsDeleted)
            .Include(pa => pa.Project)
                .ThenInclude(p => p.Branch)
            .Include(pa => pa.Employee)
            .OrderByDescending(pa => pa.AssignedDate)
            .ToListAsync();
    }

    public async Task<bool> IsTeamLeadAsync(int projectId, int employeeId)
    {
        return await _dbSet
            .AnyAsync(pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId && 
                           pa.IsTeamLead && pa.UnassignedDate == null && !pa.IsDeleted);
    }
}