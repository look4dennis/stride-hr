using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        IAuditLogService auditLogService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [RequirePermission("User.View")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManagementService.GetAllUsersAsync();
        
        var userList = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.IsActive,
            u.IsFirstLogin,
            u.ForcePasswordChange,
            u.LastLoginAt,
            Employee = u.Employee != null ? new
            {
                u.Employee.Id,
                u.Employee.FullName,
                u.Employee.Department,
                u.Employee.Designation,
                u.Employee.BranchId
            } : null
        });

        return Ok(new
        {
            success = true,
            data = new { users = userList }
        });
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("User.View")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new
        {
            success = true,
            data = new { user }
        });
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [RequirePermission("User.Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userManagementService.CreateUserAsync(request);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                success = true,
                message = "User created successfully",
                data = new { user }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("User.Update")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _userManagementService.UpdateUserAsync(id, request);
            if (!result)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, message = "User updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a user
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [RequirePermission("User.Deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var result = await _userManagementService.DeactivateUserAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new { success = true, message = "User deactivated successfully" });
    }

    /// <summary>
    /// Activate a user
    /// </summary>
    [HttpPost("{id}/activate")]
    [RequirePermission("User.Activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var result = await _userManagementService.ActivateUserAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new { success = true, message = "User activated successfully" });
    }

    /// <summary>
    /// Force password change for a user
    /// </summary>
    [HttpPost("{id}/force-password-change")]
    [RequirePermission("User.ForcePasswordChange")]
    public async Task<IActionResult> ForcePasswordChange(int id)
    {
        var result = await _userManagementService.ForcePasswordChangeAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new { success = true, message = "Password change forced successfully" });
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    [HttpPost("{id}/unlock")]
    [RequirePermission("User.Unlock")]
    public async Task<IActionResult> UnlockUser(int id)
    {
        var result = await _userManagementService.UnlockUserAccountAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new { success = true, message = "User account unlocked successfully" });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var profile = await _userManagementService.GetUserProfileAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { success = false, message = "Profile not found" });
        }

        return Ok(new
        {
            success = true,
            data = new { profile }
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var result = await _userManagementService.UpdateUserProfileAsync(userId.Value, request);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to update profile" });
        }

        return Ok(new { success = true, message = "Profile updated successfully" });
    }

    /// <summary>
    /// Get user sessions
    /// </summary>
    [HttpGet("{id}/sessions")]
    [RequirePermission("User.ViewSessions")]
    public async Task<IActionResult> GetUserSessions(int id)
    {
        var sessions = await _userManagementService.GetUserSessionsAsync(id);
        return Ok(new
        {
            success = true,
            data = new { sessions }
        });
    }

    /// <summary>
    /// Get current user sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var sessions = await _userManagementService.GetUserSessionsAsync(userId.Value);
        return Ok(new
        {
            success = true,
            data = new { sessions }
        });
    }

    /// <summary>
    /// Terminate a specific user session
    /// </summary>
    [HttpDelete("{id}/sessions/{sessionId}")]
    [RequirePermission("User.TerminateSession")]
    public async Task<IActionResult> TerminateUserSession(int id, string sessionId)
    {
        var result = await _userManagementService.TerminateUserSessionAsync(id, sessionId);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to terminate session" });
        }

        return Ok(new { success = true, message = "Session terminated successfully" });
    }

    /// <summary>
    /// Terminate all user sessions
    /// </summary>
    [HttpDelete("{id}/sessions")]
    [RequirePermission("User.TerminateSession")]
    public async Task<IActionResult> TerminateAllUserSessions(int id)
    {
        var result = await _userManagementService.TerminateAllUserSessionsAsync(id);
        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to terminate sessions" });
        }

        return Ok(new { success = true, message = "All sessions terminated successfully" });
    }

    /// <summary>
    /// Get audit logs for a user
    /// </summary>
    [HttpGet("{id}/audit-logs")]
    [RequirePermission("AuditLog.View")]
    public async Task<IActionResult> GetUserAuditLogs(int id, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var auditLogs = await _auditLogService.GetUserAuditLogsAsync(id, fromDate, toDate);
        return Ok(new
        {
            success = true,
            data = new { auditLogs }
        });
    }

    /// <summary>
    /// Get security audit logs
    /// </summary>
    [HttpGet("security-audit-logs")]
    [RequirePermission("AuditLog.ViewSecurity")]
    public async Task<IActionResult> GetSecurityAuditLogs([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var auditLogs = await _auditLogService.GetSecurityAuditLogsAsync(fromDate, toDate);
        return Ok(new
        {
            success = true,
            data = new { auditLogs }
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}