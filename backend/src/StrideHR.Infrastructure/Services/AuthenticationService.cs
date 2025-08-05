using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace StrideHR.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // Get user with employee and roles
            var user = await _userRepository.GetWithEmployeeByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for email {Email}", request.Email);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: User account is inactive for {Email}", request.Email);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Account is inactive",
                    Errors = new List<string> { "Your account has been deactivated" }
                };
            }

            // Check if account is locked
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Authentication failed: Account is locked for {Email}", request.Email);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Account is temporarily locked",
                    Errors = new List<string> { $"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss}" }
                };
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                user.UpdatedAt = DateTime.UtcNow;

                // Lock account after 5 failed attempts
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                    _logger.LogWarning("Account locked due to multiple failed login attempts for {Email}", request.Email);
                }

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogWarning("Authentication failed: Invalid password for {Email}", request.Email);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }

            // Reset failed login attempts on successful authentication
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Get user roles and permissions
            var roles = await _userRepository.GetUserRolesAsync(user.Id);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id);

            // Generate JWT token
            var token = _jwtService.GenerateToken(user, user.Employee, roles, permissions);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Create refresh token entity
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
                CreatedByIp = request.IpAddress ?? string.Empty
            };

            await _userRepository.AddRefreshTokenAsync(refreshTokenEntity);

            // Create user session
            var session = new UserSession
            {
                UserId = user.Id,
                SessionId = Guid.NewGuid().ToString(),
                IpAddress = request.IpAddress ?? string.Empty,
                UserAgent = request.UserAgent ?? string.Empty,
                DeviceInfo = request.DeviceInfo,
                LoginTime = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.CreateUserSessionAsync(session);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User {Email} authenticated successfully", request.Email);

            return new AuthenticationResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserInfo
                {
                    Id = user.Id,
                    EmployeeId = user.Employee.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.Employee.FullName,
                    ProfilePhoto = user.Employee.ProfilePhoto ?? string.Empty,
                    BranchId = user.Employee.BranchId,
                    OrganizationId = user.Employee.Branch?.OrganizationId ?? 0,
                    BranchName = user.Employee.Branch?.Name ?? string.Empty,
                    Roles = roles,
                    Permissions = permissions,
                    IsFirstLogin = user.IsFirstLogin,
                    ForcePasswordChange = user.ForcePasswordChange,
                    IsTwoFactorEnabled = user.IsTwoFactorEnabled
                },
                Message = "Authentication successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for {Email}", request.Email);
            return new AuthenticationResult
            {
                Success = false,
                Message = "An error occurred during authentication",
                Errors = new List<string> { "Internal server error" }
            };
        }
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Invalid token",
                    Errors = new List<string> { "Invalid or malformed token" }
                };
            }

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Invalid token",
                    Errors = new List<string> { "Invalid user ID in token" }
                };
            }

            var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive || refreshToken.UserId != userId)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "Invalid refresh token",
                    Errors = new List<string> { "Refresh token is invalid or expired" }
                };
            }

            // Get user with employee and roles
            var user = await _userRepository.GetWithEmployeeAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new RefreshTokenResult
                {
                    Success = false,
                    Message = "User not found or inactive",
                    Errors = new List<string> { "User account is not available" }
                };
            }

            // Revoke old refresh token
            await _userRepository.RevokeRefreshTokenAsync(request.RefreshToken, request.IpAddress);

            // Get user roles and permissions
            var roles = await _userRepository.GetUserRolesAsync(user.Id);
            var permissions = await _userRepository.GetUserPermissionsAsync(user.Id);

            // Generate new tokens
            var newToken = _jwtService.GenerateToken(user, user.Employee, roles, permissions);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Create new refresh token entity
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = request.IpAddress ?? string.Empty,
                ReplacedByToken = refreshToken.Token
            };

            await _userRepository.AddRefreshTokenAsync(newRefreshTokenEntity);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

            return new RefreshTokenResult
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Message = "Token refreshed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new RefreshTokenResult
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { "Internal server error" }
            };
        }
    }

    public async Task<bool> LogoutAsync(int userId, string? ipAddress = null)
    {
        try
        {
            // Revoke all refresh tokens
            await _userRepository.RevokeAllRefreshTokensAsync(userId);

            // End all active sessions
            await _userRepository.EndAllUserSessionsAsync(userId);

            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> LogoutAllSessionsAsync(int userId)
    {
        return await LogoutAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Password change failed: User {UserId} not found", userId);
                return false;
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed: Invalid current password for user {UserId}", userId);
                return false;
            }

            // Check if new password is strong
            if (!_passwordService.IsPasswordStrong(request.NewPassword))
            {
                _logger.LogWarning("Password change failed: New password is not strong enough for user {UserId}", userId);
                return false;
            }

            // Hash new password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.LastPasswordChangeAt = DateTime.UtcNow;
            user.ForcePasswordChange = false;
            user.IsFirstLogin = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Revoke all refresh tokens to force re-login
            await _userRepository.RevokeAllRefreshTokensAsync(userId);

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists or not for security
                _logger.LogInformation("Password reset requested for non-existent email {Email}", request.Email);
                return true;
            }

            // Generate reset token (in real implementation, this would be sent via email)
            var resetToken = _passwordService.GeneratePasswordResetToken();
            
            // In a real implementation, you would:
            // 1. Store the reset token in database with expiration
            // 2. Send email with reset link
            // For now, we'll just log it
            
            _logger.LogInformation("Password reset token generated for user {Email}: {Token}", request.Email, resetToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request)
    {
        try
        {
            // In a real implementation, you would validate the reset token from database
            if (!_passwordService.ValidatePasswordResetToken(request.Token))
            {
                _logger.LogWarning("Invalid password reset token for email {Email}", request.Email);
                return false;
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset confirmation failed: User not found for email {Email}", request.Email);
                return false;
            }

            // Check if new password is strong
            if (!_passwordService.IsPasswordStrong(request.NewPassword))
            {
                _logger.LogWarning("Password reset failed: New password is not strong enough for user {Email}", request.Email);
                return false;
            }

            // Hash new password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.LastPasswordChangeAt = DateTime.UtcNow;
            user.ForcePasswordChange = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Revoke all refresh tokens
            await _userRepository.RevokeAllRefreshTokensAsync(user.Id);

            _logger.LogInformation("Password reset successfully for user {Email}", request.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset confirmation for email {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = _jwtService.ValidateToken(token);
            return principal != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return false;
        }
    }

    public async Task<UserInfo?> GetUserFromTokenAsync(string token)
    {
        try
        {
            return await _jwtService.GetUserInfoFromTokenAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from token");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        try
        {
            return await _userRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return false;
        }
    }

    public async Task<List<UserSession>> GetActiveSessionsAsync(int userId)
    {
        try
        {
            return await _userRepository.GetActiveUserSessionsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user {UserId}", userId);
            return new List<UserSession>();
        }
    }

    public async Task<bool> RevokeSessionAsync(int userId, string sessionId)
    {
        try
        {
            return await _userRepository.EndUserSessionAsync(userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId} for user {UserId}", sessionId, userId);
            return false;
        }
    }
}