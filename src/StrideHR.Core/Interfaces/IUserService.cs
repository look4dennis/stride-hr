using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for user management service
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Create new user account
    /// </summary>
    Task<User> CreateUserAsync(int employeeId, string email, string username, string? password = null, bool forcePasswordChange = true);
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);
    
    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Get user by username
    /// </summary>
    Task<User?> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Update user profile
    /// </summary>
    Task<User> UpdateUserProfileAsync(int userId, string? email = null, string? username = null);
    
    /// <summary>
    /// Activate user account
    /// </summary>
    Task<bool> ActivateUserAsync(int userId);
    
    /// <summary>
    /// Deactivate user account
    /// </summary>
    Task<bool> DeactivateUserAsync(int userId, string? reason = null);
    
    /// <summary>
    /// Lock user account
    /// </summary>
    Task<bool> LockUserAsync(int userId, TimeSpan? lockDuration = null, string? reason = null);
    
    /// <summary>
    /// Unlock user account
    /// </summary>
    Task<bool> UnlockUserAsync(int userId);
    
    /// <summary>
    /// Suspend user account
    /// </summary>
    Task<bool> SuspendUserAsync(int userId, string? reason = null);
    
    /// <summary>
    /// Generate email verification token
    /// </summary>
    Task<string> GenerateEmailVerificationTokenAsync(int userId);
    
    /// <summary>
    /// Verify email address
    /// </summary>
    Task<bool> VerifyEmailAsync(string verificationToken);
    
    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    Task<string> EnableTwoFactorAuthenticationAsync(int userId);
    
    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    Task<bool> DisableTwoFactorAuthenticationAsync(int userId);
    
    /// <summary>
    /// Verify two-factor authentication code
    /// </summary>
    Task<bool> VerifyTwoFactorCodeAsync(int userId, string code);
    
    /// <summary>
    /// Get user settings
    /// </summary>
    Task<Dictionary<string, object>> GetUserSettingsAsync(int userId);
    
    /// <summary>
    /// Update user settings
    /// </summary>
    Task<bool> UpdateUserSettingsAsync(int userId, Dictionary<string, object> settings);
    
    /// <summary>
    /// Get users by status
    /// </summary>
    Task<List<User>> GetUsersByStatusAsync(UserStatus status);
    
    /// <summary>
    /// Get users by branch
    /// </summary>
    Task<List<User>> GetUsersByBranchAsync(int branchId);
    
    /// <summary>
    /// Search users
    /// </summary>
    Task<List<User>> SearchUsersAsync(string searchTerm, int? branchId = null, UserStatus? status = null);
    
    /// <summary>
    /// Get user activity summary
    /// </summary>
    Task<UserActivitySummary> GetUserActivitySummaryAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Clean up inactive users
    /// </summary>
    Task<int> CleanupInactiveUsersAsync(TimeSpan inactivityThreshold);
}

/// <summary>
/// User activity summary model
/// </summary>
public class UserActivitySummary
{
    public int UserId { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int LoginCount { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public List<string> RecentActivities { get; set; } = new();
}