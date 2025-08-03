using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class WebhookRepository : Repository<WebhookSubscription>, IWebhookRepository
{
    public WebhookRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<WebhookSubscription>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _context.WebhookSubscriptions
            .Where(w => w.OrganizationId == organizationId)
            .Include(w => w.Deliveries)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WebhookSubscription>> GetActiveSubscriptionsAsync(int organizationId)
    {
        return await _context.WebhookSubscriptions
            .Where(w => w.OrganizationId == organizationId && w.IsActive)
            .ToListAsync();
    }

    public async Task<List<WebhookSubscription>> GetSubscriptionsByEventAsync(string eventType)
    {
        return await _context.WebhookSubscriptions
            .Where(w => w.IsActive && w.Events.Contains(eventType))
            .ToListAsync();
    }
}

public class WebhookDeliveryRepository : Repository<WebhookDelivery>, IWebhookDeliveryRepository
{
    public WebhookDeliveryRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<WebhookDelivery>> GetBySubscriptionIdAsync(int subscriptionId, int page = 1, int pageSize = 50)
    {
        return await _context.WebhookDeliveries
            .Where(d => d.WebhookSubscriptionId == subscriptionId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<WebhookDelivery>> GetFailedDeliveriesAsync()
    {
        return await _context.WebhookDeliveries
            .Where(d => d.Status == Core.Enums.WebhookDeliveryStatus.Failed)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WebhookDelivery>> GetPendingDeliveriesAsync()
    {
        return await _context.WebhookDeliveries
            .Where(d => d.Status == Core.Enums.WebhookDeliveryStatus.Pending)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();
    }
}