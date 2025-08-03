using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IWebhookRepository : IRepository<WebhookSubscription>
{
    Task<List<WebhookSubscription>> GetByOrganizationIdAsync(int organizationId);
    Task<List<WebhookSubscription>> GetActiveSubscriptionsAsync(int organizationId);
    Task<List<WebhookSubscription>> GetSubscriptionsByEventAsync(string eventType);
}

public interface IWebhookDeliveryRepository : IRepository<WebhookDelivery>
{
    Task<List<WebhookDelivery>> GetBySubscriptionIdAsync(int subscriptionId, int page = 1, int pageSize = 50);
    Task<List<WebhookDelivery>> GetFailedDeliveriesAsync();
    Task<List<WebhookDelivery>> GetPendingDeliveriesAsync();
}