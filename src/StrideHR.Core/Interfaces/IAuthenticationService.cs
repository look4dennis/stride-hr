using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<AuthenticationResult> AuthenticateAsync(string email, string password, string ipAddress, string userAgent);
    
    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    
    /// <summary>
    /// Revoke refresh token
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress, string reason);
    
    /// <summary>
    /// Revoke all tokens for a user
    /// </summary>
    Task<bool> RevokeAllTokensAsync(int userId, string ipAddress, string reason);
    
    /// <summary>
    /// Validate JWT token
    /// </summary>
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Generate password reset token
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(string email);
    
    /// <summary>
    /// Reset password using reset token
    /// </summary>
    Task<bool> ResetPasswordAsync(string resetToken, string newPassword);
    
    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    
    /// <summary>
    /// Force password change for user
    /// </summary>
    Task<bool> ForcePasswordChangeAsync(int userId, string newPassword);
    
    /// <summary>
    /// Lock user account
    /// </summary>
    Task<bool> LockUserAccountAsync(int userId, TimeSpan lockDuration, string reason);
    
    /// <summary>
    /// Unlock user account
    /// </summary>
    Task<bool> UnlockUserAccountAsync(int userId);
    
    /// <summary>
    /// Get active sessions for user
    /// </summary>
    Task<List<UserSession>> GetActiveSessionsAsync(int userId);
    
    /// <summary>
    /// Terminate user session
    /// </summary>
    Task<bool> TerminateSessionAsync(string sessionToken, string reason);
}

/// <summary>
/// Authentication result model
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public bool RequiresTwoFactor { get; set; }
}

/// <summary>
/// Token validation result model
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime? ExpiresAt { get; set; }
}