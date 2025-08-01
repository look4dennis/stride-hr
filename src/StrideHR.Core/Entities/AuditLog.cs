using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Audit log entity for tracking security events and user activities
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    
    /// <summary>
    /// Action performed (Login, Logout, Create, Update, Delete, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity/resource affected
    /// </summary>
    [MaxLength(100)]
    public string? EntityName { get; set; }
    
    /// <summary>
    /// Entity ID affected
    /// </summary>
    public int? EntityId { get; set; }
    
    /// <summary>
    /// Old values (before change) - stored as JSON
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// New values (after change) - stored as JSON
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Additional details about the action
    /// </summary>
    [MaxLength(1000)]
    public string? Details { get; set; }
    
    /// <summary>
    /// IP address from which action was performed
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent information
    /// </summary>
    [MaxLength(1000)]
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Audit event severity level
    /// </summary>
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
    
    /// <summary>
    /// Audit event category
    /// </summary>
    public AuditCategory Category { get; set; } = AuditCategory.General;
    
    /// <summary>
    /// Was this action successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// Error message if action failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Request ID for correlation
    /// </summary>
    [MaxLength(100)]
    public string? RequestId { get; set; }
    
    /// <summary>
    /// Session ID for correlation
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    // Navigation Properties
    public virtual User? User { get; set; }
}

/// <summary>
/// Audit severity levels
/// </summary>
public enum AuditSeverity
{
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Audit event categories
/// </summary>
public enum AuditCategory
{
    General = 1,
    Authentication = 2,
    Authorization = 3,
    DataAccess = 4,
    DataModification = 5,
    Security = 6,
    System = 7,
    Configuration = 8
}