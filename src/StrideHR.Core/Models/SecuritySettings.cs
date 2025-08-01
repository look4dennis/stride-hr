namespace StrideHR.Core.Models;

/// <summary>
/// Security configuration settings
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "Security";
    
    /// <summary>
    /// Maximum failed login attempts before account lockout
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;
    
    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;
    
    /// <summary>
    /// Password minimum length
    /// </summary>
    public int PasswordMinLength { get; set; } = 8;
    
    /// <summary>
    /// Password maximum length
    /// </summary>
    public int PasswordMaxLength { get; set; } = 128;
    
    /// <summary>
    /// Require uppercase letters in password
    /// </summary>
    public bool RequireUppercase { get; set; } = true;
    
    /// <summary>
    /// Require lowercase letters in password
    /// </summary>
    public bool RequireLowercase { get; set; } = true;
    
    /// <summary>
    /// Require digits in password
    /// </summary>
    public bool RequireDigits { get; set; } = true;
    
    /// <summary>
    /// Require special characters in password
    /// </summary>
    public bool RequireSpecialCharacters { get; set; } = true;
    
    /// <summary>
    /// Number of previous passwords to remember (prevent reuse)
    /// </summary>
    public int PasswordHistoryCount { get; set; } = 5;
    
    /// <summary>
    /// Password reset token expiration in hours
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// Email verification token expiration in hours
    /// </summary>
    public int EmailVerificationTokenExpirationHours { get; set; } = 72;
    
    /// <summary>
    /// Session timeout in minutes
    /// </summary>
    public int SessionTimeoutMinutes { get; set; } = 480; // 8 hours
    
    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    public bool EnableTwoFactorAuthentication { get; set; } = false;
    
    /// <summary>
    /// Enable IP address validation
    /// </summary>
    public bool EnableIpValidation { get; set; } = false;
    
    /// <summary>
    /// Enable device fingerprinting
    /// </summary>
    public bool EnableDeviceFingerprinting { get; set; } = false;
    
    /// <summary>
    /// Maximum concurrent sessions per user
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 3;
    
    /// <summary>
    /// Enable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;
    
    /// <summary>
    /// Audit log retention days
    /// </summary>
    public int AuditLogRetentionDays { get; set; } = 365;
}