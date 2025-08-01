using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUserRepository userRepository,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _roleRepository.GetByIdAsync(id);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _roleRepository.GetByNameAsync(name);
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return (await _roleRepository.GetAllAsync()).ToList();
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        return await _roleRepository.GetActiveRolesAsync();
    }

    public async Task<Role> CreateRoleAsync(string name, string description, int hierarchyLevel)
    {
        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{name}' already exists");
        }

        var role = new Role
        {
            Name = name,
            Description = description,
            HierarchyLevel = hierarchyLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role);
        await _roleRepository.SaveChangesAsync();

        _logger.LogInformation("Role '{RoleName}' created successfully", name);
        return role;
    }

    public async Task<bool> UpdateRoleAsync(int id, string name, string description, int hierarchyLevel)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", id);
            return false;
        }

        // Check if name is already taken by another role
        var existingRole = await _roleRepository.GetByNameAsync(name);
        if (existingRole != null && existingRole.Id != id)
        {
            throw new InvalidOperationException($"Role with name '{name}' already exists");
        }

        role.Name = name;
        role.Description = description;
        role.HierarchyLevel = hierarchyLevel;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        var result = await _roleRepository.SaveChangesAsync();

        if (result)
        {
            _logger.LogInformation("Role '{RoleName}' updated successfully", name);
        }

        return result;
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", id);
            return false;
        }

        // Check if role is assigned to any employees
        var employeeRoles = await _roleRepository.GetEmployeeRolesAsync(id);
        if (employeeRoles.Any())
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to employees");
        }

        role.IsActive = false;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        var result = await _roleRepository.SaveChangesAsync();

        if (result)
        {
            _logger.LogInformation("Role '{RoleName}' deactivated successfully", role.Name);
        }

        return result;
    }

    public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", roleId);
            return false;
        }

        // Validate that all permission IDs exist
        var permissions = await _permissionRepository.GetByIdsAsync(permissionIds);
        if (permissions.Count != permissionIds.Count)
        {
            throw new InvalidOperationException("One or more permission IDs are invalid");
        }

        var result = await _roleRepository.AssignPermissionsAsync(roleId, permissionIds);

        if (result)
        {
            _logger.LogInformation("Permissions assigned to role '{RoleName}' successfully", role.Name);
        }

        return result;
    }

    public async Task<bool> RemovePermissionsFromRoleAsync(int roleId, List<int> permissionIds)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", roleId);
            return false;
        }

        var result = await _roleRepository.RemovePermissionsAsync(roleId, permissionIds);

        if (result)
        {
            _logger.LogInformation("Permissions removed from role '{RoleName}' successfully", role.Name);
        }

        return result;
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _roleRepository.GetRolePermissionsAsync(roleId);
    }

    public async Task<List<string>> GetRolePermissionNamesAsync(int roleId)
    {
        var permissions = await GetRolePermissionsAsync(roleId);
        return permissions.Select(p => $"{p.Module}.{p.Action}.{p.Resource}").ToList();
    }

    public async Task<int> GetMaxHierarchyLevelAsync(List<string> roleNames)
    {
        if (!roleNames.Any())
            return 0;

        var roles = await _roleRepository.GetRolesByNamesAsync(roleNames);
        return roles.Any() ? roles.Max(r => r.HierarchyLevel) : 0;
    }

    public async Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, DateTime? expiryDate = null)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null || !role.IsActive)
        {
            _logger.LogWarning("Role with ID {RoleId} not found or inactive", roleId);
            return false;
        }

        var result = await _roleRepository.AssignRoleToEmployeeAsync(employeeId, roleId, expiryDate);

        if (result)
        {
            _logger.LogInformation("Role '{RoleName}' assigned to employee {EmployeeId}", role.Name, employeeId);
        }

        return result;
    }

    public async Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", roleId);
            return false;
        }

        var result = await _roleRepository.RemoveRoleFromEmployeeAsync(employeeId, roleId);

        if (result)
        {
            _logger.LogInformation("Role '{RoleName}' removed from employee {EmployeeId}", role.Name, employeeId);
        }

        return result;
    }

    public async Task<List<Role>> GetEmployeeRolesAsync(int employeeId)
    {
        return await _roleRepository.GetEmployeeRolesAsync(employeeId);
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var permissions = await _userRepository.GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }

    public async Task<bool> CanAccessBranchAsync(int userId, int branchId)
    {
        var user = await _userRepository.GetWithEmployeeAsync(userId);
        if (user?.Employee == null)
            return false;

        // SuperAdmin can access all branches
        var roles = await _userRepository.GetUserRolesAsync(userId);
        if (roles.Contains("SuperAdmin"))
            return true;

        // User can access their own branch
        return user.Employee.BranchId == branchId;
    }
}