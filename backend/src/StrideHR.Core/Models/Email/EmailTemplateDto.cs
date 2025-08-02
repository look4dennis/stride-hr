using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Email;

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailTemplateType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public List<string> RequiredParameters { get; set; } = new();
    public Dictionary<string, object>? DefaultParameters { get; set; }
    public string? PreviewData { get; set; }
    public int? BranchId { get; set; }
    public bool IsGlobal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? BranchName { get; set; }
    public int UsageCount { get; set; }
}

public class CreateEmailTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailTemplateType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> RequiredParameters { get; set; } = new();
    public Dictionary<string, object>? DefaultParameters { get; set; }
    public string? PreviewData { get; set; }
    public int? BranchId { get; set; }
    public bool IsGlobal { get; set; } = true;
}

public class UpdateEmailTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public EmailTemplateType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public List<string> RequiredParameters { get; set; } = new();
    public Dictionary<string, object>? DefaultParameters { get; set; }
    public string? PreviewData { get; set; }
    public bool IsGlobal { get; set; }
}

public class EmailTemplateFilterDto
{
    public EmailTemplateType? Type { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public int? BranchId { get; set; }
    public bool? IsGlobal { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}

public class EmailRenderResultDto
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> MissingParameters { get; set; } = new();
}

public class TemplateValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> ExtractedParameters { get; set; } = new();
    public List<string> MissingRequiredParameters { get; set; } = new();
}

public class EmailPreviewDto
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? TextBody { get; set; }
    public Dictionary<string, object> SampleData { get; set; } = new();
    public string PreviewUrl { get; set; } = string.Empty;
}

public class EmailTemplateStatsDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public EmailTemplateType Type { get; set; }
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalBounced { get; set; }
    public int TotalFailed { get; set; }
    public decimal DeliveryRate { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public DateTime LastUsed { get; set; }
}