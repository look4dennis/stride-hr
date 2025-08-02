using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Email;

namespace StrideHR.Core.Interfaces.Services;

public interface IEmailService
{
    // Basic email operations
    Task<EmailLogDto> SendEmailAsync(SendEmailDto dto);
    Task<List<EmailLogDto>> SendBulkEmailAsync(BulkEmailDto dto);
    Task<EmailLogDto?> GetEmailLogAsync(int id);
    Task<List<EmailLogDto>> GetEmailLogsAsync(EmailLogFilterDto filter);
    
    // Template-based email operations
    Task<EmailLogDto> SendTemplateEmailAsync(SendTemplateEmailDto dto);
    Task<List<EmailLogDto>> SendBulkTemplateEmailAsync(BulkTemplateEmailDto dto);
    
    // Email template management
    Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto);
    Task<EmailTemplateDto> UpdateTemplateAsync(int id, UpdateEmailTemplateDto dto);
    Task<EmailTemplateDto?> GetTemplateAsync(int id);
    Task<EmailTemplateDto?> GetTemplateByNameAsync(string name);
    Task<List<EmailTemplateDto>> GetTemplatesAsync(EmailTemplateFilterDto filter);
    Task<bool> DeleteTemplateAsync(int id);
    Task<string> PreviewTemplateAsync(int templateId, Dictionary<string, object> parameters);
    
    // Email campaign management
    Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto);
    Task<EmailCampaignDto> UpdateCampaignAsync(int id, UpdateEmailCampaignDto dto);
    Task<EmailCampaignDto?> GetCampaignAsync(int id);
    Task<List<EmailCampaignDto>> GetCampaignsAsync(EmailCampaignFilterDto filter);
    Task<bool> StartCampaignAsync(int id);
    Task<bool> PauseCampaignAsync(int id);
    Task<bool> CancelCampaignAsync(int id);
    Task<EmailCampaignStatsDto> GetCampaignStatsAsync(int id);
    
    // Email delivery tracking
    Task<bool> UpdateDeliveryStatusAsync(string externalId, EmailStatus status, DateTime timestamp);
    Task<bool> TrackEmailOpenAsync(string externalId);
    Task<bool> TrackEmailClickAsync(string externalId, string url);
    Task<bool> HandleBounceAsync(string externalId, string reason);
    
    // Email analytics
    Task<EmailAnalyticsDto> GetEmailAnalyticsAsync(EmailAnalyticsFilterDto filter);
    Task<List<EmailDeliveryStatsDto>> GetDeliveryStatsAsync(DateTime fromDate, DateTime toDate, int? branchId = null);
    Task<List<EmailTemplateStatsDto>> GetTemplateUsageStatsAsync(DateTime fromDate, DateTime toDate);
    Task<List<EmailCampaignStatsDto>> GetCampaignPerformanceAsync(DateTime fromDate, DateTime toDate);
    
    // Email queue management
    Task<bool> RetryFailedEmailsAsync(int maxRetries = 3);
    Task<int> ProcessEmailQueueAsync(int batchSize = 100);
    Task<List<EmailLogDto>> GetPendingEmailsAsync(int limit = 100);
    
    // Email validation and utilities
    Task<bool> ValidateEmailAddressAsync(string email);
    Task<List<string>> ValidateBulkEmailAddressesAsync(List<string> emails);
    Task<string> GenerateUnsubscribeTokenAsync(int userId);
    Task<bool> ProcessUnsubscribeAsync(string token);
    
    // System email templates
    Task SeedDefaultTemplatesAsync();
    Task<List<EmailTemplateDto>> GetSystemTemplatesAsync();
}