using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active roles
    /// </summary>
    [HttpGet]
    [RequirePermission("Role.View")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetActiveRolesAsync();
        return Ok(new
        {
            success = true,
            data = new { roles }
        });
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Role.View")]
    public async Task<IActionResult> GetRole(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound(new { success = false, message = "Role not found" });
        }

        var permissions = await _roleService.GetRolePermissionsAsync(id);

        return Ok(new
        {
            success = true,
            data = new
            {
                role = new
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    role.HierarchyLevel,
                    role.IsActive,
                    permissions = permissions.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Module,
                        p.Action,
                        p.Resource
                    })
                }
            }
        });
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [RequirePermission("Role.Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var role = await _roleService.CreateRoleAsync(request.Name, request.Description, request.HierarchyLevel);

            if (request.PermissionIds?.Any() == true)
            {
                await _roleService.AssignPermissionsToRoleAsync(role.Id, request.PermissionIds);
            }

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, new
            {
                success = true,
                message = "Role created successfully",
                data = new { role }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("Role.Update")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _roleService.UpdateRoleAsync(id, request.Name, request.Description, request.HierarchyLevel);
            if (!result)
            {
                return NotFound(new { success = false, message = "Role not found" });
            }

            if (request.PermissionIds != null)
            {
                await _roleService.AssignPermissionsToRoleAsync(id, request.PermissionIds);
            }

            return Ok(new { success = true, message = "Role updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("Role.Delete")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { success = false, message = "Role not found" });
            }

            return Ok(new { success = true, message = "Role deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Assign role to employee
    /// </summary>
    [HttpPost("{roleId}/assign")]
    [RequirePermission("Role.Assign")]
    public async Task<IActionResult> AssignRole(int roleId, [FromBody] AssignRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _roleService.AssignRoleToEmployeeAsync(request.EmployeeId, roleId, request.ExpiryDate);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to assign role" });
        }

        return Ok(new { success = true, message = "Role assigned successfully" });
    }

    /// <summary>
    /// Remove role from employee
    /// </summary>
    [HttpDelete("{roleId}/remove/{employeeId}")]
    [RequirePermission("Role.Assign")]
    public async Task<IActionResult> RemoveRole(int roleId, int employeeId)
    {
        var result = await _roleService.RemoveRoleFromEmployeeAsync(employeeId, roleId);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to remove role" });
        }

        return Ok(new { success = true, message = "Role removed successfully" });
    }

    /// <summary>
    /// Get employee roles
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [RequirePermission("Role.View")]
    public async Task<IActionResult> GetEmployeeRoles(int employeeId)
    {
        var roles = await _roleService.GetEmployeeRolesAsync(employeeId);
        return Ok(new
        {
            success = true,
            data = new { roles }
        });
    }

    /// <summary>
    /// Check if current user has specific permission
    /// </summary>
    [HttpGet("check-permission/{permission}")]
    public async Task<IActionResult> CheckPermission(string permission)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var hasPermission = await _roleService.HasPermissionAsync(userId.Value, permission);
        return Ok(new
        {
            success = true,
            data = new { hasPermission, permission }
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public List<int>? PermissionIds { get; set; }
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public List<int>? PermissionIds { get; set; }
}

public class AssignRoleRequest
{
    public int EmployeeId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}