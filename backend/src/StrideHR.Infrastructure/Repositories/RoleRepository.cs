using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower() && !r.IsDeleted);
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        return await _context.Roles
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.HierarchyLevel)
            .ToListAsync();
    }

    public async Task<Role?> GetWithPermissionsAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<List<Role>> GetRolesByNamesAsync(List<string> names)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return await _context.Roles
            .Where(r => lowerNames.Contains(r.Name.ToLower()) && r.IsActive && !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<bool> AssignPermissionsAsync(int roleId, List<int> permissionIds)
    {
        // Remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _context.RolePermissions.AddRangeAsync(newRolePermissions);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemovePermissionsAsync(int roleId, List<int> permissionIds)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, DateTime? expiryDate = null)
    {
        // Check if assignment already exists
        var existingAssignment = await _context.EmployeeRoles
            .FirstOrDefaultAsync(er => er.EmployeeId == employeeId && er.RoleId == roleId && er.IsActive);

        if (existingAssignment != null)
        {
            // Update existing assignment
            existingAssignment.ExpiryDate = expiryDate;
            existingAssignment.UpdatedAt = DateTime.UtcNow;
            _context.EmployeeRoles.Update(existingAssignment);
        }
        else
        {
            // Create new assignment
            var employeeRole = new EmployeeRole
            {
                EmployeeId = employeeId,
                RoleId = roleId,
                AssignedDate = DateTime.UtcNow,
                ExpiryDate = expiryDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.EmployeeRoles.AddAsync(employeeRole);
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId)
    {
        var employeeRole = await _context.EmployeeRoles
            .FirstOrDefaultAsync(er => er.EmployeeId == employeeId && er.RoleId == roleId && er.IsActive);

        if (employeeRole == null)
            return false;

        employeeRole.IsActive = false;
        employeeRole.UpdatedAt = DateTime.UtcNow;

        _context.EmployeeRoles.Update(employeeRole);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Role>> GetEmployeeRolesAsync(int employeeId)
    {
        return await _context.EmployeeRoles
            .Where(er => er.EmployeeId == employeeId && er.IsActive && 
                        (er.ExpiryDate == null || er.ExpiryDate > DateTime.UtcNow) && !er.IsDeleted)
            .Include(er => er.Role)
            .Select(er => er.Role)
            .Where(r => r.IsActive && !r.IsDeleted)
            .ToListAsync();
    }
}

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted);
    }

    public async Task<List<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module.ToLower() == module.ToLower() && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Permission>> GetByIdsAsync(List<int> ids)
    {
        return await _context.Permissions
            .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.Permissions
            .AnyAsync(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted);
    }
}