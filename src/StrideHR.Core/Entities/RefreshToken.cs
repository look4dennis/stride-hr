using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Refresh token entity for JWT token management
/// </summary>
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    
    /// <summary>
    /// Refresh token value
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiry date
    /// </summary>
    public DateTime ExpiryDate { get; set; }
    
    /// <summary>
    /// Is token revoked
    /// </summary>
    public bool IsRevoked { get; set; } = false;
    
    /// <summary>
    /// Token revoked at timestamp
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Revoked by user ID
    /// </summary>
    public int? RevokedBy { get; set; }
    
    /// <summary>
    /// Reason for revocation
    /// </summary>
    [MaxLength(200)]
    public string? RevocationReason { get; set; }
    
    /// <summary>
    /// Replaced by token (when token is refreshed)
    /// </summary>
    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }
    
    /// <summary>
    /// Device information
    /// </summary>
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// IP address when token was created
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent information
    /// </summary>
    [MaxLength(1000)]
    public string? UserAgent { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Check if token is active (not expired and not revoked)
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiryDate;
    
    /// <summary>
    /// Check if token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
}

/// <summary>
/// User session entity for tracking active sessions
/// </summary>
public class UserSession : BaseEntity
{
    public int UserId { get; set; }
    
    /// <summary>
    /// Session token/identifier
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string SessionToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Session end time (null for active sessions)
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// IP address
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent information
    /// </summary>
    [MaxLength(1000)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Device information
    /// </summary>
    [MaxLength(500)]
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// Session status
    /// </summary>
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    
    /// <summary>
    /// Session termination reason
    /// </summary>
    [MaxLength(200)]
    public string? TerminationReason { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Check if session is active
    /// </summary>
    public bool IsActive => Status == SessionStatus.Active && EndTime == null;
    
    /// <summary>
    /// Session duration
    /// </summary>
    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
}

/// <summary>
/// Session status enumeration
/// </summary>
public enum SessionStatus
{
    Active = 1,
    Expired = 2,
    Terminated = 3,
    Revoked = 4
}