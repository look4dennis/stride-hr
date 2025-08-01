namespace StrideHR.Core.Entities;

public class AuditLog : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; } // JSON serialized data
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSecurityEvent { get; set; } = false;
    
    // Navigation Properties
    public virtual User? User { get; set; }
}