namespace StrideHR.Core.Entities;

public class DocumentAuditLog : BaseEntity
{
    public int GeneratedDocumentId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Generated, Viewed, Downloaded, Signed, Approved, etc.
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Navigation Properties
    public virtual GeneratedDocument GeneratedDocument { get; set; } = null!;
    public virtual Employee User { get; set; } = null!;
}