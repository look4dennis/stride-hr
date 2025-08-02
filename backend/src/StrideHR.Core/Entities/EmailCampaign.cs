using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class EmailCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmailTemplateId { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public int BouncedCount { get; set; }
    public int FailedCount { get; set; }
    public EmailCampaignType Type { get; set; }
    public string? TargetAudience { get; set; }
    public List<int>? TargetUserIds { get; set; }
    public List<int>? TargetBranchIds { get; set; }
    public List<string>? TargetRoles { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public new int CreatedBy { get; set; }
    
    // Navigation Properties
    public virtual EmailTemplate EmailTemplate { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}