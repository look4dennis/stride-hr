using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Email;

public class EmailAnalyticsDto
{
    public int TotalEmailsSent { get; set; }
    public int TotalEmailsDelivered { get; set; }
    public int TotalEmailsOpened { get; set; }
    public int TotalEmailsClicked { get; set; }
    public int TotalEmailsBounced { get; set; }
    public int TotalEmailsFailed { get; set; }
    public decimal DeliveryRate { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal BounceRate { get; set; }
    public decimal FailureRate { get; set; }
    public List<EmailVolumeByDateDto> VolumeByDate { get; set; } = new();
    public List<EmailStatsByTemplateDto> StatsByTemplate { get; set; } = new();
    public List<EmailStatsByBranchDto> StatsByBranch { get; set; } = new();
    public List<EmailStatsByCampaignDto> StatsByCampaign { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class EmailAnalyticsFilterDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? BranchId { get; set; }
    public int? TemplateId { get; set; }
    public int? CampaignId { get; set; }
    public EmailTemplateType? TemplateType { get; set; }
    public EmailCampaignType? CampaignType { get; set; }
    public bool IncludeVolumeByDate { get; set; } = true;
    public bool IncludeStatsByTemplate { get; set; } = true;
    public bool IncludeStatsByBranch { get; set; } = true;
    public bool IncludeStatsByCampaign { get; set; } = true;
}

public class EmailVolumeByDateDto
{
    public DateTime Date { get; set; }
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalBounced { get; set; }
    public int TotalFailed { get; set; }
}

public class EmailStatsByTemplateDto
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
}

public class EmailStatsByBranchDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
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
}

public class EmailStatsByCampaignDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public EmailCampaignType Type { get; set; }
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
}

public class EmailDeliveryStatsDto
{
    public DateTime Date { get; set; }
    public int TotalSent { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalBounced { get; set; }
    public int TotalFailed { get; set; }
    public decimal DeliveryRate { get; set; }
    public decimal BounceRate { get; set; }
    public decimal FailureRate { get; set; }
    public string? BranchName { get; set; }
}