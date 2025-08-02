using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class EmailLog : BaseEntity
{
    public int? EmailTemplateId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExternalId { get; set; }
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public int? CampaignId { get; set; }
    
    // Navigation Properties
    public virtual EmailTemplate? EmailTemplate { get; set; }
    public virtual User? User { get; set; }
    public virtual Branch? Branch { get; set; }
}