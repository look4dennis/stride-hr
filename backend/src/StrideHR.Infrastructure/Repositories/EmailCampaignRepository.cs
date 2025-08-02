using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmailCampaignRepository : Repository<EmailCampaign>, IEmailCampaignRepository
{
    public EmailCampaignRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<EmailCampaign>> GetByStatusAsync(EmailCampaignStatus status)
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailCampaign>> GetByTypeAsync(EmailCampaignType type)
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.Type == type)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailCampaign>> GetByCreatedByAsync(int userId)
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CreatedBy == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailCampaign>> GetScheduledCampaignsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.Status == EmailCampaignStatus.Scheduled && 
                       c.ScheduledAt.HasValue && 
                       c.ScheduledAt <= now)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync();
    }

    public async Task<List<EmailCampaign>> GetActiveCampaignsAsync()
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.Status == EmailCampaignStatus.InProgress)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();
    }

    public async Task<EmailCampaign?> GetWithEmailLogsAsync(int id)
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Include(c => c.EmailLogs)
                .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<EmailCampaign>> GetCampaignsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.EmailCampaigns
            .Include(c => c.EmailTemplate)
            .Include(c => c.CreatedByUser)
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, EmailCampaignStatus status)
    {
        var campaign = await _context.EmailCampaigns.FindAsync(id);
        if (campaign == null) return false;

        campaign.Status = status;
        
        switch (status)
        {
            case EmailCampaignStatus.InProgress:
                campaign.StartedAt = DateTime.UtcNow;
                break;
            case EmailCampaignStatus.Completed:
                campaign.CompletedAt = DateTime.UtcNow;
                break;
        }

        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCountersAsync(int id, int sentCount, int deliveredCount, int openedCount, int clickedCount, int bouncedCount, int failedCount)
    {
        var campaign = await _context.EmailCampaigns.FindAsync(id);
        if (campaign == null) return false;

        campaign.SentCount = sentCount;
        campaign.DeliveredCount = deliveredCount;
        campaign.OpenedCount = openedCount;
        campaign.ClickedCount = clickedCount;
        campaign.BouncedCount = bouncedCount;
        campaign.FailedCount = failedCount;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}