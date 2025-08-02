using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Email;

public class EmailCampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmailTemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
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
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmailCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmailTemplateId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public EmailCampaignType Type { get; set; }
    public string? TargetAudience { get; set; }
    public List<int>? TargetUserIds { get; set; }
    public List<int>? TargetBranchIds { get; set; }
    public List<string>? TargetRoles { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class UpdateEmailCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmailTemplateId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public EmailCampaignType Type { get; set; }
    public string? TargetAudience { get; set; }
    public List<int>? TargetUserIds { get; set; }
    public List<int>? TargetBranchIds { get; set; }
    public List<string>? TargetRoles { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class EmailCampaignFilterDto
{
    public EmailCampaignStatus? Status { get; set; }
    public EmailCampaignType? Type { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class EmailCampaignStatsDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public EmailCampaignType Type { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public int BouncedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal DeliveryRate { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public decimal FailureRate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}