using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Email;

public class EmailLogDto
{
    public int Id { get; set; }
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
    public EmailPriority Priority { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public int? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TemplateName { get; set; }
    public string? UserName { get; set; }
    public string? BranchName { get; set; }
}

public class SendEmailDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string? CcEmails { get; set; }
    public string? BccEmails { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public Dictionary<string, object>? Metadata { get; set; }
    public int? CampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class BulkEmailDto
{
    public List<EmailRecipientDto> Recipients { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public Dictionary<string, object>? Metadata { get; set; }
    public int? CampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? BranchId { get; set; }
}

public class EmailRecipientDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? UserId { get; set; }
    public Dictionary<string, object>? PersonalizedData { get; set; }
}

public class SendTemplateEmailDto
{
    public int TemplateId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public int? CampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class BulkTemplateEmailDto
{
    public int TemplateId { get; set; }
    public List<EmailRecipientDto> Recipients { get; set; } = new();
    public Dictionary<string, object> GlobalParameters { get; set; } = new();
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    public int? CampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int? BranchId { get; set; }
}

public class EmailLogFilterDto
{
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public int? TemplateId { get; set; }
    public EmailStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CampaignId { get; set; }
    public EmailPriority? Priority { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}