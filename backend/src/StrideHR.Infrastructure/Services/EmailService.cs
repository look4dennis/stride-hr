using System.Net.Mail;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;

namespace StrideHR.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly IEmailCampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IEmailTemplateService _templateService;
    private readonly IMapper _mapper;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(
        IEmailLogRepository emailLogRepository,
        IEmailTemplateRepository templateRepository,
        IEmailCampaignRepository campaignRepository,
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IEmailTemplateService templateService,
        IMapper mapper,
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _emailLogRepository = emailLogRepository;
        _templateRepository = templateRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _templateService = templateService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<EmailLogDto> SendEmailAsync(SendEmailDto dto)
    {
        try
        {
            var emailLog = new EmailLog
            {
                ToEmail = dto.ToEmail,
                ToName = dto.ToName,
                CcEmails = dto.CcEmails,
                BccEmails = dto.BccEmails,
                Subject = dto.Subject,
                HtmlBody = dto.HtmlBody,
                TextBody = dto.TextBody,
                UserId = dto.UserId,
                BranchId = dto.BranchId,
                Priority = dto.Priority,
                Metadata = dto.Metadata,
                CampaignId = dto.CampaignId,
                Status = dto.ScheduledAt.HasValue && dto.ScheduledAt > DateTime.UtcNow 
                    ? EmailStatus.Queued 
                    : EmailStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _emailLogRepository.AddAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();

            // If not scheduled, send immediately
            if (!dto.ScheduledAt.HasValue || dto.ScheduledAt <= DateTime.UtcNow)
            {
                await SendEmailImmediatelyAsync(emailLog);
            }

            return _mapper.Map<EmailLogDto>(emailLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", dto.ToEmail);
            throw;
        }
    }

    public async Task<List<EmailLogDto>> SendBulkEmailAsync(BulkEmailDto dto)
    {
        var emailLogs = new List<EmailLog>();

        try
        {
            foreach (var recipient in dto.Recipients)
            {
                var personalizedBody = dto.HtmlBody;
                var personalizedSubject = dto.Subject;

                // Apply personalization if provided
                if (recipient.PersonalizedData != null)
                {
                    foreach (var data in recipient.PersonalizedData)
                    {
                        var placeholder = $"{{{{{data.Key}}}}}";
                        personalizedBody = personalizedBody.Replace(placeholder, data.Value?.ToString() ?? "");
                        personalizedSubject = personalizedSubject.Replace(placeholder, data.Value?.ToString() ?? "");
                    }
                }

                var emailLog = new EmailLog
                {
                    ToEmail = recipient.Email,
                    ToName = recipient.Name,
                    Subject = personalizedSubject,
                    HtmlBody = personalizedBody,
                    TextBody = dto.TextBody,
                    UserId = recipient.UserId,
                    BranchId = dto.BranchId,
                    Priority = dto.Priority,
                    Metadata = dto.Metadata,
                    CampaignId = dto.CampaignId,
                    Status = dto.ScheduledAt.HasValue && dto.ScheduledAt > DateTime.UtcNow 
                        ? EmailStatus.Queued 
                        : EmailStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                emailLogs.Add(emailLog);
            }

            await _emailLogRepository.AddRangeAsync(emailLogs);
            await _emailLogRepository.SaveChangesAsync();

            // Send immediately if not scheduled
            if (!dto.ScheduledAt.HasValue || dto.ScheduledAt <= DateTime.UtcNow)
            {
                foreach (var emailLog in emailLogs)
                {
                    await SendEmailImmediatelyAsync(emailLog);
                }
            }

            return _mapper.Map<List<EmailLogDto>>(emailLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email to {RecipientCount} recipients", dto.Recipients.Count);
            throw;
        }
    }

    public async Task<EmailLogDto> SendEmailWithAttachmentAsync(List<string> recipients, string subject, string body, byte[] attachmentData, string attachmentFileName, string attachmentMimeType)
    {
        try
        {
            var emailLog = new EmailLog
            {
                ToEmail = string.Join(";", recipients),
                Subject = subject,
                HtmlBody = body,
                Status = EmailStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _emailLogRepository.AddAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();

            // Send email with attachment immediately
            await SendEmailWithAttachmentImmediatelyAsync(emailLog, attachmentData, attachmentFileName, attachmentMimeType);

            return _mapper.Map<EmailLogDto>(emailLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachment to {Recipients}", string.Join(", ", recipients));
            throw;
        }
    }

    public async Task<EmailLogDto?> GetEmailLogAsync(int id)
    {
        var emailLog = await _emailLogRepository.GetByIdAsync(id);
        return emailLog != null ? _mapper.Map<EmailLogDto>(emailLog) : null;
    }

    public async Task<List<EmailLogDto>> GetEmailLogsAsync(EmailLogFilterDto filter)
    {
        var query = await _emailLogRepository.GetAllAsync();
        
        // Apply filters
        if (filter.UserId.HasValue)
            query = query.Where(l => l.UserId == filter.UserId.Value);
            
        if (filter.BranchId.HasValue)
            query = query.Where(l => l.BranchId == filter.BranchId.Value);
            
        if (filter.TemplateId.HasValue)
            query = query.Where(l => l.EmailTemplateId == filter.TemplateId.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(l => l.Status == filter.Status.Value);
            
        if (filter.FromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.FromDate.Value);
            
        if (filter.ToDate.HasValue)
            query = query.Where(l => l.CreatedAt <= filter.ToDate.Value);
            
        if (filter.CampaignId.HasValue)
            query = query.Where(l => l.CampaignId == filter.CampaignId);
            
        if (filter.Priority.HasValue)
            query = query.Where(l => l.Priority == filter.Priority.Value);
            
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(l => 
                l.ToEmail.Contains(filter.SearchTerm) ||
                l.Subject.Contains(filter.SearchTerm) ||
                (l.ToName != null && l.ToName.Contains(filter.SearchTerm)));
        }

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "email" => filter.SortDescending ? query.OrderByDescending(l => l.ToEmail) : query.OrderBy(l => l.ToEmail),
            "subject" => filter.SortDescending ? query.OrderByDescending(l => l.Subject) : query.OrderBy(l => l.Subject),
            "status" => filter.SortDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            _ => filter.SortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt)
        };

        // Apply pagination
        var pagedResults = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return _mapper.Map<List<EmailLogDto>>(pagedResults);
    }

    public async Task<EmailLogDto> SendTemplateEmailAsync(SendTemplateEmailDto dto)
    {
        try
        {
            var renderResult = await _templateService.RenderEmailTemplateAsync(dto.TemplateId, dto.Parameters);
            
            if (!renderResult.IsValid)
            {
                throw new ArgumentException($"Template rendering failed: {string.Join(", ", renderResult.Errors)}");
            }

            var sendDto = new SendEmailDto
            {
                ToEmail = dto.ToEmail,
                ToName = dto.ToName,
                Subject = renderResult.Subject,
                HtmlBody = renderResult.HtmlBody,
                TextBody = renderResult.TextBody,
                UserId = dto.UserId,
                BranchId = dto.BranchId,
                Priority = dto.Priority,
                CampaignId = dto.CampaignId,
                ScheduledAt = dto.ScheduledAt,
                Metadata = new Dictionary<string, object>
                {
                    ["TemplateId"] = dto.TemplateId,
                    ["Parameters"] = dto.Parameters
                }
            };

            var result = await SendEmailAsync(sendDto);
            
            // Update the email log with template reference
            var emailLog = await _emailLogRepository.GetByIdAsync(result.Id);
            if (emailLog != null)
            {
                emailLog.EmailTemplateId = dto.TemplateId;
                await _emailLogRepository.UpdateAsync(emailLog);
                await _emailLogRepository.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email {TemplateId} to {Email}", dto.TemplateId, dto.ToEmail);
            throw;
        }
    }

    public async Task<List<EmailLogDto>> SendBulkTemplateEmailAsync(BulkTemplateEmailDto dto)
    {
        var results = new List<EmailLogDto>();

        try
        {
            foreach (var recipient in dto.Recipients)
            {
                // Merge global and personal parameters
                var parameters = new Dictionary<string, object>(dto.GlobalParameters);
                if (recipient.PersonalizedData != null)
                {
                    foreach (var data in recipient.PersonalizedData)
                    {
                        parameters[data.Key] = data.Value;
                    }
                }

                var templateEmailDto = new SendTemplateEmailDto
                {
                    TemplateId = dto.TemplateId,
                    ToEmail = recipient.Email,
                    ToName = recipient.Name,
                    UserId = recipient.UserId,
                    BranchId = dto.BranchId,
                    Parameters = parameters,
                    Priority = dto.Priority,
                    CampaignId = dto.CampaignId,
                    ScheduledAt = dto.ScheduledAt
                };

                var result = await SendTemplateEmailAsync(templateEmailDto);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk template email {TemplateId} to {RecipientCount} recipients", 
                dto.TemplateId, dto.Recipients.Count);
            throw;
        }
    }

    // Template management methods
    public async Task<EmailTemplateDto> CreateTemplateAsync(CreateEmailTemplateDto dto)
    {
        try
        {
            // Validate template name uniqueness
            if (await _templateRepository.ExistsAsync(dto.Name))
            {
                throw new ArgumentException($"Template with name '{dto.Name}' already exists");
            }

            // Validate template content
            var validation = await _templateService.ValidateTemplateAsync(dto.HtmlBody, dto.RequiredParameters);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join(", ", validation.Errors)}");
            }

            var template = _mapper.Map<EmailTemplate>(dto);
            template.CreatedAt = DateTime.UtcNow;

            await _templateRepository.AddAsync(template);
            await _templateRepository.SaveChangesAsync();

            return _mapper.Map<EmailTemplateDto>(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template {Name}", dto.Name);
            throw;
        }
    }

    public async Task<EmailTemplateDto> UpdateTemplateAsync(int id, UpdateEmailTemplateDto dto)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                throw new ArgumentException("Template not found");
            }

            // Validate template name uniqueness (excluding current template)
            if (await _templateRepository.ExistsAsync(dto.Name, id))
            {
                throw new ArgumentException($"Template with name '{dto.Name}' already exists");
            }

            // Validate template content
            var validation = await _templateService.ValidateTemplateAsync(dto.HtmlBody, dto.RequiredParameters);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join(", ", validation.Errors)}");
            }

            _mapper.Map(dto, template);
            template.UpdatedAt = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template);
            await _templateRepository.SaveChangesAsync();

            return _mapper.Map<EmailTemplateDto>(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template {Id}", id);
            throw;
        }
    }

    public async Task<EmailTemplateDto?> GetTemplateAsync(int id)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
    }

    public async Task<EmailTemplateDto?> GetTemplateByNameAsync(string name)
    {
        var template = await _templateRepository.GetByNameAsync(name);
        return template != null ? _mapper.Map<EmailTemplateDto>(template) : null;
    }

    public async Task<List<EmailTemplateDto>> GetTemplatesAsync(EmailTemplateFilterDto filter)
    {
        var query = await _templateRepository.GetAllAsync();

        // Apply filters
        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);
            
        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(t => t.Category == filter.Category);
            
        if (filter.IsActive.HasValue)
            query = query.Where(t => t.IsActive == filter.IsActive.Value);
            
        if (filter.BranchId.HasValue)
            query = query.Where(t => t.BranchId == filter.BranchId.Value);
            
        if (filter.IsGlobal.HasValue)
            query = query.Where(t => t.IsGlobal == filter.IsGlobal.Value);
            
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(t => 
                t.Name.Contains(filter.SearchTerm) ||
                t.Subject.Contains(filter.SearchTerm) ||
                (t.Description != null && t.Description.Contains(filter.SearchTerm)));
        }

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "subject" => filter.SortDescending ? query.OrderByDescending(t => t.Subject) : query.OrderBy(t => t.Subject),
            "type" => filter.SortDescending ? query.OrderByDescending(t => t.Type) : query.OrderBy(t => t.Type),
            "category" => filter.SortDescending ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
            "createdat" => filter.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => filter.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name)
        };

        // Apply pagination
        var pagedResults = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return _mapper.Map<List<EmailTemplateDto>>(pagedResults);
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null) return false;

            // Check if template is being used
            var usageCount = await _templateRepository.GetUsageCountAsync(id);
            if (usageCount > 0)
            {
                // Soft delete by deactivating
                template.IsActive = false;
                template.UpdatedAt = DateTime.UtcNow;
                await _templateRepository.UpdateAsync(template);
            }
            else
            {
                // Hard delete if not used
                await _templateRepository.DeleteAsync(template);
            }

            await _templateRepository.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template {Id}", id);
            return false;
        }
    }

    public async Task<string> PreviewTemplateAsync(int templateId, Dictionary<string, object> parameters)
    {
        var renderResult = await _templateService.RenderEmailTemplateAsync(templateId, parameters);
        
        if (!renderResult.IsValid)
        {
            throw new ArgumentException($"Template rendering failed: {string.Join(", ", renderResult.Errors)}");
        }

        return renderResult.HtmlBody;
    }

    // Campaign management methods (simplified implementation)
    public async Task<EmailCampaignDto> CreateCampaignAsync(CreateEmailCampaignDto dto)
    {
        var campaign = _mapper.Map<EmailCampaign>(dto);
        campaign.Status = EmailCampaignStatus.Draft;
        campaign.CreatedAt = DateTime.UtcNow;

        await _campaignRepository.AddAsync(campaign);
        await _campaignRepository.SaveChangesAsync();

        return _mapper.Map<EmailCampaignDto>(campaign);
    }

    public async Task<EmailCampaignDto> UpdateCampaignAsync(int id, UpdateEmailCampaignDto dto)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        _mapper.Map(dto, campaign);
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign);
        await _campaignRepository.SaveChangesAsync();

        return _mapper.Map<EmailCampaignDto>(campaign);
    }

    public async Task<EmailCampaignDto?> GetCampaignAsync(int id)
    {
        var campaign = await _campaignRepository.GetByIdAsync(id);
        return campaign != null ? _mapper.Map<EmailCampaignDto>(campaign) : null;
    }

    public async Task<List<EmailCampaignDto>> GetCampaignsAsync(EmailCampaignFilterDto filter)
    {
        var campaigns = await _campaignRepository.GetAllAsync();
        return _mapper.Map<List<EmailCampaignDto>>(campaigns);
    }

    public async Task<bool> StartCampaignAsync(int id)
    {
        return await _campaignRepository.UpdateStatusAsync(id, EmailCampaignStatus.InProgress);
    }

    public async Task<bool> PauseCampaignAsync(int id)
    {
        return await _campaignRepository.UpdateStatusAsync(id, EmailCampaignStatus.Paused);
    }

    public async Task<bool> CancelCampaignAsync(int id)
    {
        return await _campaignRepository.UpdateStatusAsync(id, EmailCampaignStatus.Cancelled);
    }

    public async Task<EmailCampaignStatsDto> GetCampaignStatsAsync(int id)
    {
        var campaign = await _campaignRepository.GetWithEmailLogsAsync(id);
        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        return _mapper.Map<EmailCampaignStatsDto>(campaign);
    }

    // Email delivery tracking methods
    public async Task<bool> UpdateDeliveryStatusAsync(string externalId, EmailStatus status, DateTime timestamp)
    {
        var emailLog = await _emailLogRepository.GetByExternalIdAsync(externalId);
        if (emailLog == null) return false;

        emailLog.Status = status;
        
        switch (status)
        {
            case EmailStatus.Sent:
                emailLog.SentAt = timestamp;
                break;
            case EmailStatus.Delivered:
                emailLog.DeliveredAt = timestamp;
                break;
            case EmailStatus.Opened:
                emailLog.OpenedAt = timestamp;
                break;
            case EmailStatus.Clicked:
                emailLog.ClickedAt = timestamp;
                break;
            case EmailStatus.Bounced:
                emailLog.BouncedAt = timestamp;
                break;
        }

        await _emailLogRepository.UpdateAsync(emailLog);
        await _emailLogRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TrackEmailOpenAsync(string externalId)
    {
        return await UpdateDeliveryStatusAsync(externalId, EmailStatus.Opened, DateTime.UtcNow);
    }

    public async Task<bool> TrackEmailClickAsync(string externalId, string url)
    {
        var emailLog = await _emailLogRepository.GetByExternalIdAsync(externalId);
        if (emailLog == null) return false;

        emailLog.Status = EmailStatus.Clicked;
        emailLog.ClickedAt = DateTime.UtcNow;
        
        if (emailLog.Metadata == null)
            emailLog.Metadata = new Dictionary<string, object>();
            
        emailLog.Metadata["ClickedUrl"] = url;

        await _emailLogRepository.UpdateAsync(emailLog);
        await _emailLogRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HandleBounceAsync(string externalId, string reason)
    {
        var emailLog = await _emailLogRepository.GetByExternalIdAsync(externalId);
        if (emailLog == null) return false;

        emailLog.Status = EmailStatus.Bounced;
        emailLog.BouncedAt = DateTime.UtcNow;
        emailLog.ErrorMessage = reason;

        await _emailLogRepository.UpdateAsync(emailLog);
        await _emailLogRepository.SaveChangesAsync();
        return true;
    }

    // Analytics methods (simplified implementations)
    public async Task<EmailAnalyticsDto> GetEmailAnalyticsAsync(EmailAnalyticsFilterDto filter)
    {
        var emails = await _emailLogRepository.GetEmailsByDateRangeAsync(filter.FromDate, filter.ToDate, filter.BranchId);
        
        return new EmailAnalyticsDto
        {
            TotalEmailsSent = emails.Count,
            TotalEmailsDelivered = emails.Count(e => e.Status == EmailStatus.Delivered || e.Status == EmailStatus.Opened || e.Status == EmailStatus.Clicked),
            TotalEmailsOpened = emails.Count(e => e.Status == EmailStatus.Opened || e.Status == EmailStatus.Clicked),
            TotalEmailsClicked = emails.Count(e => e.Status == EmailStatus.Clicked),
            TotalEmailsBounced = emails.Count(e => e.Status == EmailStatus.Bounced),
            TotalEmailsFailed = emails.Count(e => e.Status == EmailStatus.Failed),
            FromDate = filter.FromDate,
            ToDate = filter.ToDate
        };
    }

    public async Task<List<EmailDeliveryStatsDto>> GetDeliveryStatsAsync(DateTime fromDate, DateTime toDate, int? branchId = null)
    {
        var emails = await _emailLogRepository.GetEmailsByDateRangeAsync(fromDate, toDate, branchId);
        
        return emails
            .GroupBy(e => e.CreatedAt.Date)
            .Select(g => new EmailDeliveryStatsDto
            {
                Date = g.Key,
                TotalSent = g.Count(),
                TotalDelivered = g.Count(e => e.Status == EmailStatus.Delivered || e.Status == EmailStatus.Opened || e.Status == EmailStatus.Clicked),
                TotalBounced = g.Count(e => e.Status == EmailStatus.Bounced),
                TotalFailed = g.Count(e => e.Status == EmailStatus.Failed)
            })
            .ToList();
    }

    public async Task<List<EmailTemplateStatsDto>> GetTemplateUsageStatsAsync(DateTime fromDate, DateTime toDate)
    {
        var emails = await _emailLogRepository.GetEmailsByDateRangeAsync(fromDate, toDate);
        
        return emails
            .Where(e => e.EmailTemplateId.HasValue)
            .GroupBy(e => e.EmailTemplateId)
            .Select(g => new EmailTemplateStatsDto
            {
                TemplateId = g.Key!.Value,
                TotalSent = g.Count(),
                TotalDelivered = g.Count(e => e.Status == EmailStatus.Delivered || e.Status == EmailStatus.Opened || e.Status == EmailStatus.Clicked),
                TotalOpened = g.Count(e => e.Status == EmailStatus.Opened || e.Status == EmailStatus.Clicked),
                TotalClicked = g.Count(e => e.Status == EmailStatus.Clicked),
                TotalBounced = g.Count(e => e.Status == EmailStatus.Bounced),
                TotalFailed = g.Count(e => e.Status == EmailStatus.Failed),
                LastUsed = g.Max(e => e.CreatedAt)
            })
            .ToList();
    }

    public async Task<List<EmailCampaignStatsDto>> GetCampaignPerformanceAsync(DateTime fromDate, DateTime toDate)
    {
        var campaigns = await _campaignRepository.GetCampaignsByDateRangeAsync(fromDate, toDate);
        return _mapper.Map<List<EmailCampaignStatsDto>>(campaigns);
    }

    // Queue management methods
    public async Task<bool> RetryFailedEmailsAsync(int maxRetries = 3)
    {
        var failedEmails = await _emailLogRepository.GetEmailsForRetryAsync();
        
        foreach (var email in failedEmails.Take(100)) // Process in batches
        {
            if (email.RetryCount < maxRetries)
            {
                await SendEmailImmediatelyAsync(email);
            }
        }

        return true;
    }

    public async Task<int> ProcessEmailQueueAsync(int batchSize = 100)
    {
        var pendingEmails = await _emailLogRepository.GetPendingEmailsAsync(batchSize);
        
        foreach (var email in pendingEmails)
        {
            await SendEmailImmediatelyAsync(email);
        }

        return pendingEmails.Count;
    }

    public async Task<List<EmailLogDto>> GetPendingEmailsAsync(int limit = 100)
    {
        var pendingEmails = await _emailLogRepository.GetPendingEmailsAsync(limit);
        return _mapper.Map<List<EmailLogDto>>(pendingEmails);
    }

    // Utility methods
    public async Task<bool> ValidateEmailAddressAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> ValidateBulkEmailAddressesAsync(List<string> emails)
    {
        var validEmails = new List<string>();
        
        foreach (var email in emails)
        {
            if (await ValidateEmailAddressAsync(email))
            {
                validEmails.Add(email);
            }
        }

        return validEmails;
    }

    public async Task<string> GenerateUnsubscribeTokenAsync(int userId)
    {
        // Generate a secure token for unsubscribe functionality
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow.Ticks}"));
    }

    public async Task<bool> ProcessUnsubscribeAsync(string token)
    {
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            
            if (parts.Length == 2 && int.TryParse(parts[0], out var userId))
            {
                // Update user notification preferences to disable email notifications
                // This would typically update the UserNotificationPreference table
                _logger.LogInformation("User {UserId} unsubscribed from email notifications", userId);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing unsubscribe token {Token}", token);
        }

        return false;
    }

    public async Task SeedDefaultTemplatesAsync()
    {
        var defaultTemplates = GetDefaultEmailTemplates();
        
        foreach (var template in defaultTemplates)
        {
            if (!await _templateRepository.ExistsAsync(template.Name))
            {
                await _templateRepository.AddAsync(template);
            }
        }

        await _templateRepository.SaveChangesAsync();
    }

    public async Task<List<EmailTemplateDto>> GetSystemTemplatesAsync()
    {
        var systemTemplates = await _templateRepository.GetGlobalTemplatesAsync();
        return _mapper.Map<List<EmailTemplateDto>>(systemTemplates);
    }

    private async Task SendEmailImmediatelyAsync(EmailLog emailLog)
    {
        try
        {
            emailLog.Status = EmailStatus.Sending;
            emailLog.ExternalId = Guid.NewGuid().ToString();
            
            // Here you would integrate with your email service provider (SendGrid, AWS SES, etc.)
            // For now, we'll simulate sending
            await Task.Delay(100); // Simulate network delay
            
            emailLog.Status = EmailStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            
            await _emailLogRepository.UpdateAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();
            
            _logger.LogInformation("Email sent successfully to {Email} with external ID {ExternalId}", 
                emailLog.ToEmail, emailLog.ExternalId);
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
            emailLog.RetryCount++;
            emailLog.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, emailLog.RetryCount)); // Exponential backoff
            
            await _emailLogRepository.UpdateAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();
            
            _logger.LogError(ex, "Failed to send email to {Email}", emailLog.ToEmail);
        }
    }

    private async Task SendEmailWithAttachmentImmediatelyAsync(EmailLog emailLog, byte[] attachmentData, string attachmentFileName, string attachmentMimeType)
    {
        try
        {
            emailLog.Status = EmailStatus.Sending;
            emailLog.ExternalId = Guid.NewGuid().ToString();
            
            // Here you would integrate with your email service provider to send with attachment
            // For now, we'll simulate sending with attachment
            await Task.Delay(150); // Simulate network delay (slightly longer for attachment)
            
            emailLog.Status = EmailStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            
            await _emailLogRepository.UpdateAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();
            
            _logger.LogInformation("Email with attachment sent successfully to {Email} with external ID {ExternalId}, attachment: {FileName}", 
                emailLog.ToEmail, emailLog.ExternalId, attachmentFileName);
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
            emailLog.RetryCount++;
            emailLog.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, emailLog.RetryCount)); // Exponential backoff
            
            await _emailLogRepository.UpdateAsync(emailLog);
            await _emailLogRepository.SaveChangesAsync();
            
            _logger.LogError(ex, "Failed to send email with attachment to {Email}", emailLog.ToEmail);
        }
    }

    private List<EmailTemplate> GetDefaultEmailTemplates()
    {
        return new List<EmailTemplate>
        {
            new EmailTemplate
            {
                Name = "Welcome",
                Subject = "Welcome to {{CompanyName}}, {{EmployeeName}}!",
                HtmlBody = @"
                    <h1>Welcome to {{CompanyName}}!</h1>
                    <p>Dear {{EmployeeName}},</p>
                    <p>We are excited to have you join our team. Your journey with us begins on {{StartDate}}.</p>
                    <p>Best regards,<br>HR Team</p>",
                Type = EmailTemplateType.Welcome,
                Category = "Onboarding",
                IsActive = true,
                IsGlobal = true,
                RequiredParameters = new List<string> { "CompanyName", "EmployeeName", "StartDate" },
                CreatedAt = DateTime.UtcNow
            },
            new EmailTemplate
            {
                Name = "Leave Approved",
                Subject = "Leave Request Approved - {{LeaveType}}",
                HtmlBody = @"
                    <h2>Leave Request Approved</h2>
                    <p>Dear {{EmployeeName}},</p>
                    <p>Your {{LeaveType}} request from {{StartDate}} to {{EndDate}} has been approved.</p>
                    <p>Enjoy your time off!</p>
                    <p>HR Team</p>",
                Type = EmailTemplateType.LeaveRequestApproved,
                Category = "Leave Management",
                IsActive = true,
                IsGlobal = true,
                RequiredParameters = new List<string> { "EmployeeName", "LeaveType", "StartDate", "EndDate" },
                CreatedAt = DateTime.UtcNow
            },
            new EmailTemplate
            {
                Name = "Payslip Generated",
                Subject = "Your Payslip for {{Month}} {{Year}}",
                HtmlBody = @"
                    <h2>Payslip Available</h2>
                    <p>Dear {{EmployeeName}},</p>
                    <p>Your payslip for {{Month}} {{Year}} is now available.</p>
                    <p>Net Salary: {{Currency}} {{NetSalary}}</p>
                    <p>Please log in to the system to download your payslip.</p>
                    <p>HR Team</p>",
                Type = EmailTemplateType.PayslipGenerated,
                Category = "Payroll",
                IsActive = true,
                IsGlobal = true,
                RequiredParameters = new List<string> { "EmployeeName", "Month", "Year", "Currency", "NetSalary" },
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}