using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs.Authentication;
using StrideHR.Core.Interfaces;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

/// <summary>
/// Authentication controller for login, logout, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IPasswordService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IPasswordService passwordService,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authenticationService.AuthenticateAsync(
                request.Email, 
                request.Password, 
                ipAddress, 
                userAgent);

            if (!result.Success)
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                });
            }

            // Map user information
            var userInfo = new UserInfoDto
            {
                Id = result.User!.Id,
                EmployeeId = result.User.EmployeeId,
                Email = result.User.Email,
                Username = result.User.Username,
                FullName = result.User.Employee?.FullName ?? "Unknown",
                ProfilePhotoPath = result.User.Employee?.ProfilePhotoPath
            };

            // Add branch information if available
            if (result.User.Employee?.Branch != null)
            {
                userInfo.Branch = new BranchInfoDto
                {
                    Id = result.User.Employee.Branch.Id,
                    Name = result.User.Employee.Branch.Name,
                    Country = result.User.Employee.Branch.Country,
                    Currency = result.User.Employee.Branch.Currency,
                    TimeZone = result.User.Employee.Branch.TimeZone
                };
            }

            var response = new LoginResponseDto
            {
                Success = true,
                AccessToken = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = userInfo,
                RequiresPasswordChange = result.RequiresPasswordChange,
                RequiresTwoFactor = result.RequiresTwoFactor
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authenticationService.RefreshTokenAsync(
                request.RefreshToken, 
                ipAddress, 
                userAgent);

            if (!result.Success)
            {
                return BadRequest(new RefreshTokenResponseDto
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                });
            }

            return Ok(new RefreshTokenResponseDto
            {
                Success = true,
                AccessToken = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new RefreshTokenResponseDto
            {
                Success = false,
                ErrorMessage = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Revoke refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var reason = request.Reason ?? "Token revoked by user";

            var success = await _authenticationService.RevokeTokenAsync(
                request.RefreshToken, 
                ipAddress, 
                reason);

            if (!success)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            return Ok(new { message = "Token revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return StatusCode(500, new { message = "An error occurred while revoking token" });
        }
    }

    /// <summary>
    /// Logout user and revoke all tokens
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            var ipAddress = GetClientIpAddress();
            var success = await _authenticationService.RevokeAllTokensAsync(
                userId.Value, 
                ipAddress, 
                "User logout");

            await _auditService.LogAuthenticationEventAsync(userId.Value, "Logout", true, 
                "User logged out successfully", ipAddress, Request.Headers["User-Agent"].ToString());

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            var success = await _authenticationService.ChangePasswordAsync(
                userId.Value, 
                request.CurrentPassword, 
                request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var resetToken = await _authenticationService.GeneratePasswordResetTokenAsync(request.Email);
            
            // In a real application, you would send this token via email
            // For now, we'll just return success (don't reveal if email exists)
            
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (ArgumentException)
        {
            // Don't reveal if email doesn't exist
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred while processing the request" });
        }
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var success = await _authenticationService.ResetPasswordAsync(
                request.ResetToken, 
                request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Invalid or expired reset token" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "An error occurred while resetting password" });
        }
    }

    /// <summary>
    /// Validate password strength
    /// </summary>
    [HttpPost("validate-password")]
    [AllowAnonymous]
    public ActionResult<PasswordValidationResponseDto> ValidatePassword([FromBody] string password)
    {
        try
        {
            var result = _passwordService.ValidatePasswordStrength(password);
            
            return Ok(new PasswordValidationResponseDto
            {
                IsValid = result.IsValid,
                Errors = result.Errors,
                Strength = result.Strength.ToString(),
                StrengthScore = (int)result.Strength
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password");
            return StatusCode(500, new { message = "An error occurred while validating password" });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Task.FromResult<ActionResult<UserInfoDto>>(BadRequest(new { message = "User not found" }));
            }

            // This would typically use a user service to get full user details
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            
            if (!int.TryParse(employeeIdClaim, out var employeeId))
            {
                return Task.FromResult<ActionResult<UserInfoDto>>(BadRequest(new { message = "Invalid employee ID" }));
            }

            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

            var userInfo = new UserInfoDto
            {
                Id = userId.Value,
                EmployeeId = employeeId,
                Email = userEmail,
                Username = userName,
                FullName = userName, // This should come from employee data
                Roles = roles,
                Permissions = permissions
            };

            return Task.FromResult<ActionResult<UserInfoDto>>(Ok(userInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return Task.FromResult<ActionResult<UserInfoDto>>(StatusCode(500, new { message = "An error occurred while getting user information" }));
        }
    }

    /// <summary>
    /// Force password change (Admin only)
    /// </summary>
    [HttpPost("force-password-change")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> ForcePasswordChange([FromBody] ForcePasswordChangeRequestDto request)
    {
        try
        {
            var success = await _authenticationService.ForcePasswordChangeAsync(
                request.UserId, 
                request.NewPassword);

            if (!success)
            {
                return BadRequest(new { message = "Failed to change password" });
            }

            var currentUserId = GetCurrentUserId();
            await _auditService.LogSecurityEventAsync(currentUserId, "Force Password Change", 
                Core.Entities.AuditSeverity.Information, 
                $"Password force changed for user {request.UserId}", 
                GetClientIpAddress(), 
                Request.Headers["User-Agent"].ToString());

            return Ok(new { message = "Password changed successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force changing password for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string GetClientIpAddress()
    {
        var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress ?? "Unknown";
    }
}