using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Role management service implementation
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;
    private readonly IRepository<EmployeeRole> _employeeRoleRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRepository<Role> roleRepository,
        IRepository<Permission> permissionRepository,
        IRepository<RolePermission> rolePermissionRepository,
        IRepository<EmployeeRole> employeeRoleRepository,
        IRepository<User> userRepository,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _employeeRoleRepository = employeeRoleRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.FindAsync(r => r.IsActive && !r.IsDeleted);
        return roles.OrderBy(r => r.HierarchyLevel).ToList();
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        return await _roleRepository.FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive && !r.IsDeleted);
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _roleRepository.FirstOrDefaultAsync(r => 
            r.Name.ToLower() == roleName.ToLower() && r.IsActive && !r.IsDeleted);
    }

    public async Task<Role> CreateRoleAsync(string name, string? description = null, int hierarchyLevel = 1)
    {
        // Check if role already exists
        var existingRole = await GetRoleByNameAsync(name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role '{name}' already exists");
        }

        var role = new Role
        {
            Name = name,
            Description = description,
            HierarchyLevel = hierarchyLevel,
            IsActive = true,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role);
        await _roleRepository.SaveChangesAsync();

        _logger.LogInformation("Role '{RoleName}' created with ID {RoleId}", name, role.Id);
        return role;
    }

    public async Task<Role> UpdateRoleAsync(int roleId, string name, string? description = null, int? hierarchyLevel = null)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
        {
            throw new ArgumentException($"Role with ID {roleId} not found");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot modify system roles");
        }

        role.Name = name;
        role.Description = description;
        if (hierarchyLevel.HasValue)
        {
            role.HierarchyLevel = hierarchyLevel.Value;
        }
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        await _roleRepository.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} updated", roleId);
        return role;
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return false;
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot delete system roles");
        }

        // Check if role is assigned to any employees
        var employeeRoles = await _employeeRoleRepository.FindAsync(er => er.RoleId == roleId && er.IsActive);
        if (employeeRoles.Any())
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to employees");
        }

        // Soft delete
        await _roleRepository.SoftDeleteAsync(role);
        await _roleRepository.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} deleted", roleId);
        return true;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        var permissions = await _permissionRepository.FindAsync(p => p.IsActive && !p.IsDeleted);
        return permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
    {
        var rolePermissions = await _rolePermissionRepository.FindAsync(rp => 
            rp.RoleId == roleId && !rp.IsDeleted);
        
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();
        var permissions = await _permissionRepository.FindAsync(p => 
            permissionIds.Contains(p.Id) && p.IsActive && !p.IsDeleted);
        
        return permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
    }

    public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        // Check if role exists
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return false;
        }

        // Check if permission exists
        var permission = await _permissionRepository.FirstOrDefaultAsync(p => 
            p.Id == permissionId && p.IsActive && !p.IsDeleted);
        if (permission == null)
        {
            return false;
        }

        // Check if assignment already exists
        var existingAssignment = await _rolePermissionRepository.FirstOrDefaultAsync(rp => 
            rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);
        
        if (existingAssignment != null)
        {
            // Assignment already exists
            return true;
        }

        // Create new assignment
        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _rolePermissionRepository.AddAsync(rolePermission);
        await _rolePermissionRepository.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", permissionId, roleId);
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _rolePermissionRepository.FirstOrDefaultAsync(rp => 
            rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);
        
        if (rolePermission == null)
        {
            return false;
        }

        // Soft delete the role permission
        await _rolePermissionRepository.SoftDeleteAsync(rolePermission);
        await _rolePermissionRepository.SaveChangesAsync();

        _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);
        return true;
    }

    public async Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, int? assignedBy = null)
    {
        // Check if role exists
        var role = await GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return false;
        }

        // Check if assignment already exists
        var existingAssignment = await _employeeRoleRepository.FirstOrDefaultAsync(er => 
            er.EmployeeId == employeeId && er.RoleId == roleId && !er.IsDeleted);
        
        if (existingAssignment != null)
        {
            if (!existingAssignment.IsActive)
            {
                existingAssignment.IsActive = true;
                existingAssignment.EndDate = null;
                existingAssignment.UpdatedAt = DateTime.UtcNow;
                await _employeeRoleRepository.UpdateAsync(existingAssignment);
                await _employeeRoleRepository.SaveChangesAsync();
            }
            return true;
        }

        // Create new assignment
        var employeeRole = new EmployeeRole
        {
            EmployeeId = employeeId,
            RoleId = roleId,
            AssignedDate = DateTime.UtcNow,
            IsActive = true,
            AssignedBy = assignedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRoleRepository.AddAsync(employeeRole);
        await _employeeRoleRepository.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} assigned to employee {EmployeeId}", roleId, employeeId);
        return true;
    }

    public async Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId)
    {
        var employeeRole = await _employeeRoleRepository.FirstOrDefaultAsync(er => 
            er.EmployeeId == employeeId && er.RoleId == roleId && er.IsActive && !er.IsDeleted);
        
        if (employeeRole == null)
        {
            return false;
        }

        employeeRole.IsActive = false;
        employeeRole.EndDate = DateTime.UtcNow;
        employeeRole.UpdatedAt = DateTime.UtcNow;
        
        await _employeeRoleRepository.UpdateAsync(employeeRole);
        await _employeeRoleRepository.SaveChangesAsync();

        _logger.LogInformation("Role {RoleId} removed from employee {EmployeeId}", roleId, employeeId);
        return true;
    }

    public async Task<List<Role>> GetEmployeeRolesAsync(int employeeId)
    {
        var employeeRoles = await _employeeRoleRepository.FindAsync(er => 
            er.EmployeeId == employeeId && er.IsActive && !er.IsDeleted);
        
        var roleIds = employeeRoles.Select(er => er.RoleId).ToList();
        var roles = await _roleRepository.FindAsync(r => 
            roleIds.Contains(r.Id) && r.IsActive && !r.IsDeleted);
        
        return roles.OrderBy(r => r.HierarchyLevel).ToList();
    }

    public async Task<List<Permission>> GetEmployeePermissionsAsync(int employeeId)
    {
        var roles = await GetEmployeeRolesAsync(employeeId);
        var roleIds = roles.Select(r => r.Id).ToList();
        
        var rolePermissions = await _rolePermissionRepository.FindAsync(rp => 
            roleIds.Contains(rp.RoleId) && !rp.IsDeleted);
        
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
        var permissions = await _permissionRepository.FindAsync(p => 
            permissionIds.Contains(p.Id) && p.IsActive && !p.IsDeleted);
        
        return permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
    }

    public async Task<bool> HasPermissionAsync(int employeeId, string permissionName)
    {
        var permissions = await GetEmployeePermissionsAsync(employeeId);
        return permissions.Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> HasRoleAsync(int employeeId, string roleName)
    {
        var roles = await GetEmployeeRolesAsync(employeeId);
        return roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsUserInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        // Convert string userId to int for internal use
        if (int.TryParse(userId, out int userIdInt))
        {
            return await HasRoleAsync(userIdInt, roleName);
        }
        return false;
    }

    public async Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        // Get the role
        var role = await GetRoleByNameAsync(roleName);
        if (role == null)
        {
            return new List<User>();
        }

        // Get employee roles for this role
        var employeeRoles = await _employeeRoleRepository.FindAsync(er => 
            er.RoleId == role.Id && er.IsActive && !er.IsDeleted, cancellationToken);
        
        var employeeIds = employeeRoles.Select(er => er.EmployeeId).ToList();
        
        // Get users for these employees
        var users = await _userRepository.FindAsync(u => 
            employeeIds.Contains(u.EmployeeId) && !u.IsDeleted, cancellationToken);
        
        return users.ToList();
    }

    public async Task InitializeDefaultRolesAndPermissionsAsync()
    {
        _logger.LogInformation("Initializing default roles and permissions");

        // Create default permissions
        await CreateDefaultPermissionsAsync();
        
        // Create default roles
        await CreateDefaultRolesAsync();
        
        _logger.LogInformation("Default roles and permissions initialized");
    }

    private async Task CreateDefaultPermissionsAsync()
    {
        var defaultPermissions = new[]
        {
            // Employee permissions
            new { Name = "Employee.View", Module = "Employee", Action = "View", Description = "View employee information" },
            new { Name = "Employee.Create", Module = "Employee", Action = "Create", Description = "Create new employees" },
            new { Name = "Employee.Update", Module = "Employee", Action = "Update", Description = "Update employee information" },
            new { Name = "Employee.Delete", Module = "Employee", Action = "Delete", Description = "Delete employees" },
            
            // Attendance permissions
            new { Name = "Attendance.View", Module = "Attendance", Action = "View", Description = "View attendance records" },
            new { Name = "Attendance.Manage", Module = "Attendance", Action = "Manage", Description = "Manage attendance records" },
            
            // Payroll permissions
            new { Name = "Payroll.View", Module = "Payroll", Action = "View", Description = "View payroll information" },
            new { Name = "Payroll.Create", Module = "Payroll", Action = "Create", Description = "Create payroll records" },
            new { Name = "Payroll.Update", Module = "Payroll", Action = "Update", Description = "Update payroll records" },
            new { Name = "Payroll.Process", Module = "Payroll", Action = "Process", Description = "Process payroll" },
            
            // Reports permissions
            new { Name = "Reports.View", Module = "Reports", Action = "View", Description = "View reports" },
            new { Name = "Reports.Create", Module = "Reports", Action = "Create", Description = "Create custom reports" },
            
            // System permissions
            new { Name = "System.Admin", Module = "System", Action = "Admin", Description = "System administration" },
            new { Name = "System.Config", Module = "System", Action = "Config", Description = "System configuration" }
        };

        foreach (var permissionData in defaultPermissions)
        {
            var existingPermission = await _permissionRepository.FirstOrDefaultAsync(p => 
                p.Name == permissionData.Name && !p.IsDeleted);
            
            if (existingPermission == null)
            {
                var permission = new Permission
                {
                    Name = permissionData.Name,
                    Module = permissionData.Module,
                    Action = permissionData.Action,
                    Description = permissionData.Description,
                    IsActive = true,
                    IsSystemPermission = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _permissionRepository.AddAsync(permission);
            }
        }

        await _permissionRepository.SaveChangesAsync();
    }

    private async Task CreateDefaultRolesAsync()
    {
        var defaultRoles = new[]
        {
            new { Name = "SuperAdmin", Description = "Super Administrator with full access", HierarchyLevel = 1 },
            new { Name = "Admin", Description = "Administrator with system access", HierarchyLevel = 2 },
            new { Name = "HR", Description = "Human Resources Manager", HierarchyLevel = 3 },
            new { Name = "Manager", Description = "Department Manager", HierarchyLevel = 4 },
            new { Name = "Employee", Description = "Regular Employee", HierarchyLevel = 5 }
        };

        foreach (var roleData in defaultRoles)
        {
            var existingRole = await GetRoleByNameAsync(roleData.Name);
            if (existingRole == null)
            {
                var role = new Role
                {
                    Name = roleData.Name,
                    Description = roleData.Description,
                    HierarchyLevel = roleData.HierarchyLevel,
                    IsActive = true,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _roleRepository.AddAsync(role);
            }
        }

        await _roleRepository.SaveChangesAsync();

        // Assign permissions to roles
        await AssignDefaultPermissionsToRolesAsync();
    }

    private async Task AssignDefaultPermissionsToRolesAsync()
    {
        // SuperAdmin gets all permissions
        var superAdminRole = await GetRoleByNameAsync("SuperAdmin");
        var allPermissions = await GetAllPermissionsAsync();
        
        if (superAdminRole != null)
        {
            foreach (var permission in allPermissions)
            {
                await AssignPermissionToRoleAsync(superAdminRole.Id, permission.Id);
            }
        }

        // Admin gets most permissions except system admin
        var adminRole = await GetRoleByNameAsync("Admin");
        if (adminRole != null)
        {
            var adminPermissions = allPermissions.Where(p => p.Name != "System.Admin").ToList();
            foreach (var permission in adminPermissions)
            {
                await AssignPermissionToRoleAsync(adminRole.Id, permission.Id);
            }
        }

        // HR gets employee and attendance permissions
        var hrRole = await GetRoleByNameAsync("HR");
        if (hrRole != null)
        {
            var hrPermissions = allPermissions.Where(p => 
                p.Module == "Employee" || p.Module == "Attendance" || p.Module == "Reports").ToList();
            foreach (var permission in hrPermissions)
            {
                await AssignPermissionToRoleAsync(hrRole.Id, permission.Id);
            }
        }

        // Manager gets view permissions
        var managerRole = await GetRoleByNameAsync("Manager");
        if (managerRole != null)
        {
            var managerPermissions = allPermissions.Where(p => 
                p.Action == "View" || p.Name == "Attendance.Manage").ToList();
            foreach (var permission in managerPermissions)
            {
                await AssignPermissionToRoleAsync(managerRole.Id, permission.Id);
            }
        }

        // Employee gets basic view permissions
        var employeeRole = await GetRoleByNameAsync("Employee");
        if (employeeRole != null)
        {
            var employeePermissions = allPermissions.Where(p => 
                p.Name == "Employee.View" || p.Name == "Attendance.View").ToList();
            foreach (var permission in employeePermissions)
            {
                await AssignPermissionToRoleAsync(employeeRole.Id, permission.Id);
            }
        }
    }
}