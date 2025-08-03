using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Webhooks;

namespace StrideHR.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IWebhookDeliveryRepository _deliveryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IWebhookRepository webhookRepository,
        IWebhookDeliveryRepository deliveryRepository,
        IUnitOfWork unitOfWork,
        HttpClient httpClient,
        ILogger<WebhookService> logger)
    {
        _webhookRepository = webhookRepository;
        _deliveryRepository = deliveryRepository;
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WebhookSubscription> CreateSubscriptionAsync(CreateWebhookSubscriptionDto dto)
    {
        var subscription = new WebhookSubscription
        {
            Name = dto.Name,
            Url = dto.Url,
            Secret = dto.Secret,
            Events = JsonSerializer.Serialize(dto.Events),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _webhookRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Webhook subscription created: {Name} for URL: {Url}", dto.Name, dto.Url);
        return subscription;
    }

    public async Task<WebhookSubscription> GetSubscriptionAsync(int id)
    {
        var subscription = await _webhookRepository.GetByIdAsync(id);
        if (subscription == null)
            throw new ArgumentException($"Webhook subscription with ID {id} not found");

        return subscription;
    }

    public async Task<List<WebhookSubscription>> GetSubscriptionsAsync(int organizationId)
    {
        return await _webhookRepository.GetByOrganizationIdAsync(organizationId);
    }

    public async Task<WebhookSubscription> UpdateSubscriptionAsync(int id, UpdateWebhookSubscriptionDto dto)
    {
        var subscription = await _webhookRepository.GetByIdAsync(id);
        if (subscription == null)
            throw new ArgumentException($"Webhook subscription with ID {id} not found");

        if (!string.IsNullOrEmpty(dto.Name))
            subscription.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Url))
            subscription.Url = dto.Url;

        if (!string.IsNullOrEmpty(dto.Secret))
            subscription.Secret = dto.Secret;

        if (dto.Events != null)
            subscription.Events = JsonSerializer.Serialize(dto.Events);

        if (dto.IsActive.HasValue)
            subscription.IsActive = dto.IsActive.Value;

        subscription.UpdatedAt = DateTime.UtcNow;

        await _webhookRepository.UpdateAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Webhook subscription updated: {Id}", id);
        return subscription;
    }

    public async Task<bool> DeleteSubscriptionAsync(int id)
    {
        var subscription = await _webhookRepository.GetByIdAsync(id);
        if (subscription == null)
            return false;

        await _webhookRepository.DeleteAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Webhook subscription deleted: {Id}", id);
        return true;
    }

    public async Task<bool> ToggleSubscriptionAsync(int id, bool isActive)
    {
        var subscription = await _webhookRepository.GetByIdAsync(id);
        if (subscription == null)
            return false;

        subscription.IsActive = isActive;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _webhookRepository.UpdateAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Webhook subscription {Action}: {Id}", isActive ? "activated" : "deactivated", id);
        return true;
    }

    public async Task<WebhookDelivery> SendWebhookAsync(string eventType, object payload, int? subscriptionId = null)
    {
        List<WebhookSubscription> subscriptions;

        if (subscriptionId.HasValue)
        {
            var subscription = await _webhookRepository.GetByIdAsync(subscriptionId.Value);
            subscriptions = subscription != null ? new List<WebhookSubscription> { subscription } : new List<WebhookSubscription>();
        }
        else
        {
            subscriptions = await _webhookRepository.GetSubscriptionsByEventAsync(eventType);
        }

        WebhookDelivery? lastDelivery = null;

        foreach (var subscription in subscriptions)
        {
            lastDelivery = await DeliverWebhookAsync(subscription, eventType, payload);
        }

        return lastDelivery ?? throw new InvalidOperationException("No webhook subscriptions found for the event");
    }

    public async Task<List<WebhookDelivery>> GetDeliveriesAsync(int subscriptionId, int page = 1, int pageSize = 50)
    {
        return await _deliveryRepository.GetBySubscriptionIdAsync(subscriptionId, page, pageSize);
    }

    public async Task<WebhookDelivery> GetDeliveryAsync(int deliveryId)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            throw new ArgumentException($"Webhook delivery with ID {deliveryId} not found");

        return delivery;
    }

    public async Task<bool> ResendWebhookAsync(int deliveryId)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            return false;

        var subscription = await _webhookRepository.GetByIdAsync(delivery.WebhookSubscriptionId);
        if (subscription == null)
            return false;

        var payload = JsonSerializer.Deserialize<object>(delivery.Payload);
        await DeliverWebhookAsync(subscription, delivery.EventType, payload);

        return true;
    }

    public async Task<bool> ValidateWebhookSignatureAsync(string payload, string signature, string secret)
    {
        try
        {
            var expectedSignature = GenerateSignature(payload, secret);
            return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return false;
        }
    }

    public async Task<WebhookTestResult> TestWebhookAsync(int subscriptionId, string eventType)
    {
        var subscription = await _webhookRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
            throw new ArgumentException($"Webhook subscription with ID {subscriptionId} not found");

        var testPayload = new
        {
            eventType = eventType,
            timestamp = DateTime.UtcNow,
            data = new { test = true, message = "This is a test webhook" }
        };

        var delivery = await DeliverWebhookAsync(subscription, eventType, testPayload);

        return new WebhookTestResult
        {
            Success = delivery.Status == WebhookDeliveryStatus.Delivered,
            HttpStatusCode = delivery.HttpStatusCode,
            ResponseBody = delivery.ResponseBody,
            ErrorMessage = delivery.ErrorMessage,
            ResponseTime = TimeSpan.FromMilliseconds(100) // Approximate
        };
    }

    private async Task<WebhookDelivery> DeliverWebhookAsync(WebhookSubscription subscription, string eventType, object payload)
    {
        var delivery = new WebhookDelivery
        {
            WebhookSubscriptionId = subscription.Id,
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            Url = subscription.Url,
            Status = WebhookDeliveryStatus.Pending,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _deliveryRepository.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var signature = GenerateSignature(jsonPayload, subscription.Secret);

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Add("X-Webhook-Signature", signature);
            request.Headers.Add("X-Webhook-Event", eventType);
            request.Headers.Add("User-Agent", "StrideHR-Webhook/1.0");

            delivery.AttemptCount++;
            var response = await _httpClient.SendAsync(request);

            delivery.HttpStatusCode = (int)response.StatusCode;
            delivery.ResponseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = WebhookDeliveryStatus.Delivered;
                delivery.DeliveredAt = DateTime.UtcNow;
                _logger.LogInformation("Webhook delivered successfully to {Url} for event {EventType}", subscription.Url, eventType);
            }
            else
            {
                delivery.Status = WebhookDeliveryStatus.Failed;
                delivery.ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                _logger.LogWarning("Webhook delivery failed to {Url} for event {EventType}: {StatusCode}", 
                    subscription.Url, eventType, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            delivery.ErrorMessage = ex.Message;
            delivery.HttpStatusCode = 0;
            _logger.LogError(ex, "Webhook delivery error to {Url} for event {EventType}", subscription.Url, eventType);
        }

        await _deliveryRepository.UpdateAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        return delivery;
    }

    private static string GenerateSignature(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return "sha256=" + Convert.ToHexString(hashBytes).ToLower();
    }
}