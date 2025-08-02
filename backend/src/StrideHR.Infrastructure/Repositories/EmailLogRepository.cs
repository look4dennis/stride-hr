using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmailLogRepository : Repository<EmailLog>, IEmailLogRepository
{
    public EmailLogRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<EmailLog?> GetByExternalIdAsync(string externalId)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .FirstOrDefaultAsync(l => l.ExternalId == externalId);
    }

    public async Task<List<EmailLog>> GetByUserIdAsync(int userId)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.Branch)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetByBranchIdAsync(int branchId)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Where(l => l.BranchId == branchId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetByTemplateIdAsync(int templateId)
    {
        return await _context.EmailLogs
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.EmailTemplateId == templateId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetByCampaignIdAsync(string campaignId)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.CampaignId == campaignId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetByStatusAsync(EmailStatus status)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetPendingEmailsAsync(int limit = 100)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.Status == EmailStatus.Pending || l.Status == EmailStatus.Queued)
            .OrderBy(l => l.Priority)
            .ThenBy(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetFailedEmailsAsync(int maxRetries = 3)
    {
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.Status == EmailStatus.Failed && l.RetryCount < maxRetries)
            .OrderBy(l => l.NextRetryAt)
            .ToListAsync();
    }

    public async Task<List<EmailLog>> GetEmailsForRetryAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.Status == EmailStatus.Failed && 
                       l.NextRetryAt.HasValue && 
                       l.NextRetryAt <= now)
            .OrderBy(l => l.NextRetryAt)
            .ToListAsync();
    }

    public async Task<Dictionary<EmailStatus, int>> GetStatusCountsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<List<EmailLog>> GetEmailsByDateRangeAsync(DateTime fromDate, DateTime toDate, int? branchId = null)
    {
        var query = _context.EmailLogs
            .Include(l => l.EmailTemplate)
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Where(l => l.CreatedAt >= fromDate && l.CreatedAt <= toDate);

        if (branchId.HasValue)
            query = query.Where(l => l.BranchId == branchId);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalSentCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs
            .Where(l => l.Status == EmailStatus.Sent || 
                       l.Status == EmailStatus.Delivered || 
                       l.Status == EmailStatus.Opened || 
                       l.Status == EmailStatus.Clicked);

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        return await query.CountAsync();
    }

    public async Task<decimal> GetDeliveryRateAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        var totalSent = await query.CountAsync();
        if (totalSent == 0) return 0;

        var delivered = await query
            .Where(l => l.Status == EmailStatus.Delivered || 
                       l.Status == EmailStatus.Opened || 
                       l.Status == EmailStatus.Clicked)
            .CountAsync();

        return (decimal)delivered / totalSent * 100;
    }

    public async Task<decimal> GetOpenRateAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        var totalDelivered = await query
            .Where(l => l.Status == EmailStatus.Delivered || 
                       l.Status == EmailStatus.Opened || 
                       l.Status == EmailStatus.Clicked)
            .CountAsync();

        if (totalDelivered == 0) return 0;

        var opened = await query
            .Where(l => l.Status == EmailStatus.Opened || l.Status == EmailStatus.Clicked)
            .CountAsync();

        return (decimal)opened / totalDelivered * 100;
    }

    public async Task<decimal> GetClickRateAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        var totalOpened = await query
            .Where(l => l.Status == EmailStatus.Opened || l.Status == EmailStatus.Clicked)
            .CountAsync();

        if (totalOpened == 0) return 0;

        var clicked = await query
            .Where(l => l.Status == EmailStatus.Clicked)
            .CountAsync();

        return (decimal)clicked / totalOpened * 100;
    }

    public async Task<decimal> GetBounceRateAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value);

        var totalSent = await query.CountAsync();
        if (totalSent == 0) return 0;

        var bounced = await query
            .Where(l => l.Status == EmailStatus.Bounced)
            .CountAsync();

        return (decimal)bounced / totalSent * 100;
    }
}