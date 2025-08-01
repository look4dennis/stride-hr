using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// User entity for authentication and system access
/// </summary>
public class User : AuditableEntity
{
    public int EmployeeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed password
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Salt used for password hashing
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PasswordSalt { get; set; } = string.Empty;
    
    /// <summary>
    /// User status (Active, Inactive, Locked, etc.)
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Last login IP address
    /// </summary>
    [MaxLength(45)]
    public string? LastLoginIp { get; set; }
    
    /// <summary>
    /// Failed login attempts count
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;
    
    /// <summary>
    /// Account locked until this timestamp
    /// </summary>
    public DateTime? LockedUntil { get; set; }
    
    /// <summary>
    /// Password reset token
    /// </summary>
    [MaxLength(500)]
    public string? PasswordResetToken { get; set; }
    
    /// <summary>
    /// Password reset token expiry
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    /// <summary>
    /// Email verification token
    /// </summary>
    [MaxLength(500)]
    public string? EmailVerificationToken { get; set; }
    
    /// <summary>
    /// Is email verified
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;
    
    /// <summary>
    /// Email verified at timestamp
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }
    
    /// <summary>
    /// Two-factor authentication enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// Two-factor authentication secret key
    /// </summary>
    [MaxLength(500)]
    public string? TwoFactorSecretKey { get; set; }
    
    /// <summary>
    /// Force password change on next login
    /// </summary>
    public bool ForcePasswordChange { get; set; } = false;
    
    /// <summary>
    /// Password last changed timestamp
    /// </summary>
    public DateTime? PasswordChangedAt { get; set; }
    
    /// <summary>
    /// User preferences and settings (stored as JSON)
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

/// <summary>
/// User status enumeration
/// </summary>
public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Locked = 3,
    Suspended = 4,
    PendingVerification = 5
}