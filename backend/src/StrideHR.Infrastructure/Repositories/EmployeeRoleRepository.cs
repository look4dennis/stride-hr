using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeRoleRepository : Repository<EmployeeRole>, IEmployeeRoleRepository
{
    public EmployeeRoleRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeeRole>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employeeId)
            .Include(er => er.Role)
            .Include(er => er.AssignedByEmployee)
            .Include(er => er.RevokedByEmployee)
            .OrderByDescending(er => er.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeRole>> GetActiveRolesByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employeeId && er.IsActive && er.RevokedDate == null)
            .Include(er => er.Role)
            .Include(er => er.AssignedByEmployee)
            .OrderBy(er => er.Role.Name)
            .ToListAsync();
    }

    public async Task<EmployeeRole?> GetByEmployeeAndRoleIdAsync(int employeeId, int roleId)
    {
        return await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employeeId && er.RoleId == roleId)
            .Include(er => er.Role)
            .Include(er => er.AssignedByEmployee)
            .Include(er => er.RevokedByEmployee)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasRoleAsync(int employeeId, int roleId)
    {
        return await _context.EmployeeRoles
            .AnyAsync(er => er.EmployeeId == employeeId && er.RoleId == roleId);
    }

    public async Task<bool> HasActiveRoleAsync(int employeeId, int roleId)
    {
        return await _context.EmployeeRoles
            .AnyAsync(er => er.EmployeeId == employeeId && er.RoleId == roleId && 
                           er.IsActive && er.RevokedDate == null);
    }
}