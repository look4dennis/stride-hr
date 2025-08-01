using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for role management service
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get all roles
    /// </summary>
    Task<List<Role>> GetAllRolesAsync();
    
    /// <summary>
    /// Get role by ID
    /// </summary>
    Task<Role?> GetRoleByIdAsync(int roleId);
    
    /// <summary>
    /// Get role by name
    /// </summary>
    Task<Role?> GetRoleByNameAsync(string roleName);
    
    /// <summary>
    /// Create new role
    /// </summary>
    Task<Role> CreateRoleAsync(string name, string? description = null, int hierarchyLevel = 1);
    
    /// <summary>
    /// Update role
    /// </summary>
    Task<Role> UpdateRoleAsync(int roleId, string name, string? description = null, int? hierarchyLevel = null);
    
    /// <summary>
    /// Delete role
    /// </summary>
    Task<bool> DeleteRoleAsync(int roleId);
    
    /// <summary>
    /// Get all permissions
    /// </summary>
    Task<List<Permission>> GetAllPermissionsAsync();
    
    /// <summary>
    /// Get permissions for role
    /// </summary>
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);
    
    /// <summary>
    /// Assign permission to role
    /// </summary>
    Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
    
    /// <summary>
    /// Remove permission from role
    /// </summary>
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
    
    /// <summary>
    /// Assign role to employee
    /// </summary>
    Task<bool> AssignRoleToEmployeeAsync(int employeeId, int roleId, int? assignedBy = null);
    
    /// <summary>
    /// Remove role from employee
    /// </summary>
    Task<bool> RemoveRoleFromEmployeeAsync(int employeeId, int roleId);
    
    /// <summary>
    /// Get employee roles
    /// </summary>
    Task<List<Role>> GetEmployeeRolesAsync(int employeeId);
    
    /// <summary>
    /// Get employee permissions (from all assigned roles)
    /// </summary>
    Task<List<Permission>> GetEmployeePermissionsAsync(int employeeId);
    
    /// <summary>
    /// Check if employee has permission
    /// </summary>
    Task<bool> HasPermissionAsync(int employeeId, string permissionName);
    
    /// <summary>
    /// Check if employee has role
    /// </summary>
    Task<bool> HasRoleAsync(int employeeId, string roleName);
    
    /// <summary>
    /// Check if user has role (alias for HasRoleAsync for compatibility)
    /// </summary>
    Task<bool> IsUserInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get users in role
    /// </summary>
    Task<List<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Initialize default roles and permissions
    /// </summary>
    Task InitializeDefaultRolesAndPermissionsAsync();
}