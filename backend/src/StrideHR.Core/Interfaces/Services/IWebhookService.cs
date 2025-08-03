using StrideHR.Core.Models.Webhooks;

namespace StrideHR.Core.Interfaces.Services;

public interface IWebhookService
{
    Task<WebhookSubscription> CreateSubscriptionAsync(CreateWebhookSubscriptionDto dto);
    Task<WebhookSubscription> GetSubscriptionAsync(int id);
    Task<List<WebhookSubscription>> GetSubscriptionsAsync(int organizationId);
    Task<WebhookSubscription> UpdateSubscriptionAsync(int id, UpdateWebhookSubscriptionDto dto);
    Task<bool> DeleteSubscriptionAsync(int id);
    Task<bool> ToggleSubscriptionAsync(int id, bool isActive);
    
    Task<WebhookDelivery> SendWebhookAsync(string eventType, object payload, int? subscriptionId = null);
    Task<List<WebhookDelivery>> GetDeliveriesAsync(int subscriptionId, int page = 1, int pageSize = 50);
    Task<WebhookDelivery> GetDeliveryAsync(int deliveryId);
    Task<bool> ResendWebhookAsync(int deliveryId);
    
    Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret);
    Task<WebhookTestResult> TestWebhookAsync(int subscriptionId, string eventType);
}