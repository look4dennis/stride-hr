using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Add client information to request
        request.IpAddress = GetClientIpAddress();
        request.UserAgent = Request.Headers.UserAgent.ToString();

        var result = await _authenticationService.AuthenticateAsync(request);

        if (!result.Success)
        {
            return Unauthorized(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                user = result.User
            }
        });
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.IpAddress = GetClientIpAddress();

        var result = await _authenticationService.RefreshTokenAsync(request);

        if (!result.Success)
        {
            return Unauthorized(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt
            }
        });
    }

    /// <summary>
    /// Logout user and revoke all tokens
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var result = await _authenticationService.LogoutAsync(userId.Value, GetClientIpAddress());

        if (!result)
        {
            return BadRequest(new { success = false, message = "Logout failed" });
        }

        return Ok(new { success = true, message = "Logged out successfully" });
    }

    /// <summary>
    /// Logout from all sessions
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var result = await _authenticationService.LogoutAllSessionsAsync(userId.Value);

        if (!result)
        {
            return BadRequest(new { success = false, message = "Logout failed" });
        }

        return Ok(new { success = true, message = "Logged out from all sessions successfully" });
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
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

        var result = await _authenticationService.ChangePasswordAsync(userId.Value, request);

        if (!result)
        {
            return BadRequest(new { success = false, message = "Password change failed" });
        }

        return Ok(new { success = true, message = "Password changed successfully" });
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.ResetPasswordAsync(request);

        if (!result)
        {
            return BadRequest(new { success = false, message = "Password reset request failed" });
        }

        return Ok(new { success = true, message = "Password reset instructions sent to your email" });
    }

    /// <summary>
    /// Confirm password reset with token
    /// </summary>
    [HttpPost("reset-password-confirm")]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.ResetPasswordConfirmAsync(request);

        if (!result)
        {
            return BadRequest(new { success = false, message = "Password reset failed" });
        }

        return Ok(new { success = true, message = "Password reset successfully" });
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { success = false, message = "Token is required" });
        }

        var isValid = await _authenticationService.ValidateTokenAsync(request.Token);

        if (!isValid)
        {
            return Unauthorized(new { success = false, message = "Invalid token" });
        }

        var userInfo = await _authenticationService.GetUserFromTokenAsync(request.Token);

        return Ok(new
        {
            success = true,
            message = "Token is valid",
            data = new { user = userInfo }
        });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userInfo = HttpContext.Items["User"] as UserInfo;
        if (userInfo == null)
        {
            return Unauthorized(new { success = false, message = "User information not available" });
        }

        return Ok(new
        {
            success = true,
            data = new { user = userInfo }
        });
    }

    /// <summary>
    /// Get active sessions for current user
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var sessions = await _authenticationService.GetActiveSessionsAsync(userId.Value);

        return Ok(new
        {
            success = true,
            data = new { sessions }
        });
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "Invalid user" });
        }

        var result = await _authenticationService.RevokeSessionAsync(userId.Value, sessionId);

        if (!result)
        {
            return BadRequest(new { success = false, message = "Failed to revoke session" });
        }

        return Ok(new { success = true, message = "Session revoked successfully" });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP (when behind proxy/load balancer)
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }
        else if (Request.Headers.ContainsKey("X-Real-IP"))
        {
            ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        return ipAddress ?? "Unknown";
    }
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}