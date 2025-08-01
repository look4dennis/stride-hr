using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for User repository
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Get user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Get user with employee details
    /// </summary>
    Task<User?> GetWithEmployeeAsync(int userId);
    
    /// <summary>
    /// Get user with roles and permissions
    /// </summary>
    Task<User?> GetWithRolesAsync(int userId);
    
    /// <summary>
    /// Get user by password reset token
    /// </summary>
    Task<User?> GetByPasswordResetTokenAsync(string resetToken);
    
    /// <summary>
    /// Get user by email verification token
    /// </summary>
    Task<User?> GetByEmailVerificationTokenAsync(string verificationToken);
    
    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
    
    /// <summary>
    /// Check if username exists
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
    
    /// <summary>
    /// Get users by status
    /// </summary>
    Task<List<User>> GetByStatusAsync(UserStatus status);
    
    /// <summary>
    /// Get locked users
    /// </summary>
    Task<List<User>> GetLockedUsersAsync();
    
    /// <summary>
    /// Update last login information
    /// </summary>
    Task UpdateLastLoginAsync(int userId, string ipAddress);
    
    /// <summary>
    /// Increment failed login attempts
    /// </summary>
    Task IncrementFailedLoginAttemptsAsync(int userId);
    
    /// <summary>
    /// Reset failed login attempts
    /// </summary>
    Task ResetFailedLoginAttemptsAsync(int userId);
    
    /// <summary>
    /// Lock user account
    /// </summary>
    Task LockUserAsync(int userId, DateTime lockUntil);
    
    /// <summary>
    /// Unlock user account
    /// </summary>
    Task UnlockUserAsync(int userId);
}