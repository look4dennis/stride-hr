using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmailLogRepository : IRepository<EmailLog>
{
    Task<EmailLog?> GetByExternalIdAsync(string externalId);
    Task<List<EmailLog>> GetByUserIdAsync(int userId);
    Task<List<EmailLog>> GetByBranchIdAsync(int branchId);
    Task<List<EmailLog>> GetByTemplateIdAsync(int templateId);
    Task<List<EmailLog>> GetByCampaignIdAsync(int? campaignId);
    Task<List<EmailLog>> GetByStatusAsync(EmailStatus status);
    Task<List<EmailLog>> GetPendingEmailsAsync(int limit = 100);
    Task<List<EmailLog>> GetFailedEmailsAsync(int maxRetries = 3);
    Task<List<EmailLog>> GetEmailsForRetryAsync();
    Task<Dictionary<EmailStatus, int>> GetStatusCountsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<EmailLog>> GetEmailsByDateRangeAsync(DateTime fromDate, DateTime toDate, int? branchId = null);
    Task<int> GetTotalSentCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetDeliveryRateAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetOpenRateAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetClickRateAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetBounceRateAsync(DateTime? fromDate = null, DateTime? toDate = null);
}