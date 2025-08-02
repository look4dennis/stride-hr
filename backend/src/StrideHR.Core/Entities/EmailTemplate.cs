using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class EmailTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailTemplateType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public List<string> RequiredParameters { get; set; } = new();
    public Dictionary<string, object>? DefaultParameters { get; set; }
    public string? PreviewData { get; set; }
    public int? BranchId { get; set; }
    public bool IsGlobal { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}