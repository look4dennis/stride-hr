using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmailCampaignRepository : IRepository<EmailCampaign>
{
    Task<List<EmailCampaign>> GetByStatusAsync(EmailCampaignStatus status);
    Task<List<EmailCampaign>> GetByTypeAsync(EmailCampaignType type);
    Task<List<EmailCampaign>> GetByCreatedByAsync(int userId);
    Task<List<EmailCampaign>> GetScheduledCampaignsAsync();
    Task<List<EmailCampaign>> GetActiveCampaignsAsync();
    Task<EmailCampaign?> GetWithEmailLogsAsync(int id);
    Task<List<EmailCampaign>> GetCampaignsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<bool> UpdateStatusAsync(int id, EmailCampaignStatus status);
    Task<bool> UpdateCountersAsync(int id, int sentCount, int deliveredCount, int openedCount, int clickedCount, int bouncedCount, int failedCount);
}