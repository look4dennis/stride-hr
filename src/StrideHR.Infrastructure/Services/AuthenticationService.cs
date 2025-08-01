using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SecuritySettings _securitySettings;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        IAuditService auditService,
        ILogger<AuthenticationService> logger,
        IOptions<SecuritySettings> securitySettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
        _securitySettings = securitySettings.Value;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password, string ipAddress, string userAgent)
    {
        try
        {
            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                await _auditService.LogAuthenticationEventAsync(null, "Login Failed - User Not Found", false, 
                    $"Login attempt with email: {email}", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Check if account is locked
            if (user.Status == UserStatus.Locked && user.LockedUntil > DateTime.UtcNow)
            {
                await _auditService.LogAuthenticationEventAsync(user.Id, "Login Failed - Account Locked", false, 
                    $"Account locked until: {user.LockedUntil}", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = $"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC"
                };
            }

            // Check if account is inactive
            if (user.Status == UserStatus.Inactive || user.Status == UserStatus.Suspended)
            {
                await _auditService.LogAuthenticationEventAsync(user.Id, "Login Failed - Account Inactive", false, 
                    $"Account status: {user.Status}", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Account is not active"
                };
            }

            // Verify password
            if (!_passwordService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                // Increment failed login attempts
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id);
                
                // Check if we should lock the account
                if (user.FailedLoginAttempts + 1 >= _securitySettings.MaxFailedLoginAttempts)
                {
                    var lockUntil = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutDurationMinutes);
                    await _userRepository.LockUserAsync(user.Id, lockUntil);
                    
                    await _auditService.LogSecurityEventAsync(user.Id, "Account Locked - Too Many Failed Attempts", 
                        AuditSeverity.Warning, $"Account locked until: {lockUntil}", ipAddress, userAgent);
                }

                await _userRepository.SaveChangesAsync();
                
                await _auditService.LogAuthenticationEventAsync(user.Id, "Login Failed - Invalid Password", false, 
                    $"Failed attempts: {user.FailedLoginAttempts + 1}", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid email or password"
                };
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
            {
                await _auditService.LogAuthenticationEventAsync(user.Id, "Login Failed - Email Not Verified", false, 
                    "Email verification required", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Please verify your email address before logging in"
                };
            }

            // Get user with roles and permissions
            var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id);
            if (userWithRoles == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Unable to load user permissions"
                };
            }

            // Extract roles and permissions
            var roles = userWithRoles.Employee?.EmployeeRoles?.Select(er => er.Role.Name).ToList() ?? new List<string>();
            var permissions = userWithRoles.Employee?.EmployeeRoles?
                .SelectMany(er => er.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList() ?? new List<string>();

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(30), // 30 days
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceInfo = ExtractDeviceInfo(userAgent),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            // Update user login information
            await _userRepository.UpdateLastLoginAsync(user.Id, ipAddress);
            await _userRepository.SaveChangesAsync();
            await _refreshTokenRepository.SaveChangesAsync();

            // Log successful authentication
            await _auditService.LogAuthenticationEventAsync(user.Id, "Login Successful", true, 
                "User authenticated successfully", ipAddress, userAgent);

            return new AuthenticationResult
            {
                Success = true,
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetTokenExpiration(accessToken),
                User = user,
                RequiresPasswordChange = user.ForcePasswordChange
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", email);
            
            await _auditService.LogAuthenticationEventAsync(null, "Login Error", false, 
                $"Authentication error: {ex.Message}", ipAddress, userAgent);
            
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during authentication"
            };
        }
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent)
    {
        try
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            
            if (tokenEntity == null || !tokenEntity.IsActive)
            {
                await _auditService.LogSecurityEventAsync(null, "Refresh Token Invalid", AuditSeverity.Warning, 
                    "Invalid or expired refresh token used", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired refresh token"
                };
            }

            var user = tokenEntity.User;
            if (user.Status != UserStatus.Active)
            {
                await _auditService.LogSecurityEventAsync(user.Id, "Refresh Token Failed - Inactive User", AuditSeverity.Warning, 
                    $"User status: {user.Status}", ipAddress, userAgent);
                
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User account is not active"
                };
            }

            // Get user with roles and permissions
            var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id);
            if (userWithRoles == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Unable to load user permissions"
                };
            }

            // Extract roles and permissions
            var roles = userWithRoles.Employee?.EmployeeRoles?.Select(er => er.Role.Name).ToList() ?? new List<string>();
            var permissions = userWithRoles.Employee?.EmployeeRoles?
                .SelectMany(er => er.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList() ?? new List<string>();

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Replace the old refresh token
            await _refreshTokenRepository.ReplaceTokenAsync(refreshToken, newRefreshToken, user.Id, ipAddress, userAgent);
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogAuthenticationEventAsync(user.Id, "Token Refreshed", true, 
                "Access token refreshed successfully", ipAddress, userAgent);

            return new AuthenticationResult
            {
                Success = true,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _jwtService.GetTokenExpiration(newAccessToken),
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            
            await _auditService.LogSecurityEventAsync(null, "Token Refresh Error", AuditSeverity.Error, 
                $"Token refresh error: {ex.Message}", ipAddress, userAgent);
            
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "An error occurred during token refresh"
            };
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, string reason)
    {
        try
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null)
                return false;

            await _refreshTokenRepository.RevokeTokenAsync(refreshToken, tokenEntity.UserId, reason);
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(tokenEntity.UserId, "Token Revoked", AuditSeverity.Information, 
                $"Reason: {reason}", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensAsync(int userId, string ipAddress, string reason)
    {
        try
        {
            await _refreshTokenRepository.RevokeAllTokensForUserAsync(userId, userId, reason);
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "All Tokens Revoked", AuditSeverity.Information, 
                $"Reason: {reason}", ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            return false;
        }
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token"
                };
            }

            var userIdClaim = principal.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid user ID in token"
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Status != UserStatus.Active)
            {
                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "User not found or inactive"
                };
            }

            var roles = principal.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = principal.FindAll("permission").Select(c => c.Value).ToList();

            return new TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions,
                ExpiresAt = _jwtService.GetTokenExpiration(token)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token validation error"
            };
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new ArgumentException("User not found");

        var resetToken = _passwordService.GenerateSecureToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(_securitySettings.PasswordResetTokenExpirationHours);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(user.Id, "Password Reset Token Generated", AuditSeverity.Information, 
            "Password reset token generated");

        return resetToken;
    }

    public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(resetToken);
            if (user == null)
                return false;

            // Validate password strength
            var passwordValidation = _passwordService.ValidatePasswordStrength(newPassword);
            if (!passwordValidation.IsValid)
                throw new ArgumentException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");

            // Hash new password
            var (hash, salt) = _passwordService.HashPassword(newPassword);
            
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.ForcePasswordChange = false;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            // Unlock account if it was locked
            if (user.Status == UserStatus.Locked)
            {
                user.Status = UserStatus.Active;
                user.LockedUntil = null;
            }

            await _userRepository.UpdateAsync(user);
            
            // Revoke all existing refresh tokens for security
            await _refreshTokenRepository.RevokeAllTokensForUserAsync(user.Id, user.Id, "Password reset");
            
            await _userRepository.SaveChangesAsync();
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(user.Id, "Password Reset Successful", AuditSeverity.Information, 
                "Password reset completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Verify current password
            if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                await _auditService.LogSecurityEventAsync(userId, "Password Change Failed - Invalid Current Password", 
                    AuditSeverity.Warning, "Invalid current password provided");
                return false;
            }

            // Validate new password strength
            var passwordValidation = _passwordService.ValidatePasswordStrength(newPassword);
            if (!passwordValidation.IsValid)
                throw new ArgumentException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");

            // Hash new password
            var (hash, salt) = _passwordService.HashPassword(newPassword);
            
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.ForcePasswordChange = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "Password Changed", AuditSeverity.Information, 
                "Password changed successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ForcePasswordChangeAsync(int userId, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Validate password strength
            var passwordValidation = _passwordService.ValidatePasswordStrength(newPassword);
            if (!passwordValidation.IsValid)
                throw new ArgumentException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");

            // Hash new password
            var (hash, salt) = _passwordService.HashPassword(newPassword);
            
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.ForcePasswordChange = true; // User must change on next login
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "Password Force Changed", AuditSeverity.Information, 
                "Password force changed by administrator");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error force changing password for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> LockUserAccountAsync(int userId, TimeSpan lockDuration, string reason)
    {
        try
        {
            var lockUntil = DateTime.UtcNow.Add(lockDuration);
            await _userRepository.LockUserAsync(userId, lockUntil);
            
            // Revoke all refresh tokens
            await _refreshTokenRepository.RevokeAllTokensForUserAsync(userId, userId, "Account locked");
            
            await _userRepository.SaveChangesAsync();
            await _refreshTokenRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "Account Locked", AuditSeverity.Warning, 
                $"Reason: {reason}, Locked until: {lockUntil}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user account {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UnlockUserAccountAsync(int userId)
    {
        try
        {
            await _userRepository.UnlockUserAsync(userId);
            await _userRepository.SaveChangesAsync();

            await _auditService.LogSecurityEventAsync(userId, "Account Unlocked", AuditSeverity.Information, 
                "Account unlocked by administrator");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user account {UserId}", userId);
            return false;
        }
    }

    public Task<List<UserSession>> GetActiveSessionsAsync(int userId)
    {
        // This would require implementing UserSession repository
        // For now, return empty list
        return Task.FromResult(new List<UserSession>());
    }

    public Task<bool> TerminateSessionAsync(string sessionToken, string reason)
    {
        // This would require implementing UserSession repository
        // For now, return true
        return Task.FromResult(true);
    }

    private string ExtractDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        // Simple device detection - in production, use a proper library
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            return "Mobile";
        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            return "Tablet";
        return "Desktop";
    }
}