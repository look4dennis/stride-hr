using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Webhooks;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for managing webhook subscriptions and deliveries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WebhookController : BaseController
{
    private readonly IWebhookService _webhookService;

    public WebhookController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    /// <summary>
    /// Create a new webhook subscription
    /// </summary>
    /// <param name="dto">Webhook subscription details</param>
    /// <returns>Created webhook subscription</returns>
    [HttpPost]
    [Authorize(Policy = "Permission:Webhook.Create")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateWebhookSubscriptionDto dto)
    {
        try
        {
            var subscription = await _webhookService.CreateSubscriptionAsync(dto);
            return Ok(new { success = true, data = subscription });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get webhook subscription by ID
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>Webhook subscription details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Webhook.View")]
    public async Task<IActionResult> GetSubscription(int id)
    {
        try
        {
            var subscription = await _webhookService.GetSubscriptionAsync(id);
            return Ok(new { success = true, data = subscription });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get all webhook subscriptions for organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <returns>List of webhook subscriptions</returns>
    [HttpGet("organization/{organizationId}")]
    [Authorize(Policy = "Permission:Webhook.View")]
    public async Task<IActionResult> GetSubscriptions(int organizationId)
    {
        try
        {
            var subscriptions = await _webhookService.GetSubscriptionsAsync(organizationId);
            return Ok(new { success = true, data = subscriptions });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update webhook subscription
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <param name="dto">Updated subscription details</param>
    /// <returns>Updated webhook subscription</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Webhook.Update")]
    public async Task<IActionResult> UpdateSubscription(int id, [FromBody] UpdateWebhookSubscriptionDto dto)
    {
        try
        {
            var subscription = await _webhookService.UpdateSubscriptionAsync(id, dto);
            return Ok(new { success = true, data = subscription });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Delete webhook subscription
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Webhook.Delete")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        try
        {
            var result = await _webhookService.DeleteSubscriptionAsync(id);
            if (result)
                return Ok(new { success = true, message = "Webhook subscription deleted successfully" });
            else
                return NotFound(new { success = false, message = "Webhook subscription not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Toggle webhook subscription active status
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <param name="isActive">Active status</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/toggle")]
    [Authorize(Policy = "Permission:Webhook.Update")]
    public async Task<IActionResult> ToggleSubscription(int id, [FromBody] bool isActive)
    {
        try
        {
            var result = await _webhookService.ToggleSubscriptionAsync(id, isActive);
            if (result)
                return Ok(new { success = true, message = $"Webhook subscription {(isActive ? "activated" : "deactivated")} successfully" });
            else
                return NotFound(new { success = false, message = "Webhook subscription not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Send webhook manually
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="payload">Event payload</param>
    /// <param name="subscriptionId">Optional specific subscription ID</param>
    /// <returns>Webhook delivery result</returns>
    [HttpPost("send")]
    [Authorize(Policy = "Permission:Webhook.Send")]
    public async Task<IActionResult> SendWebhook([FromQuery] string eventType, [FromBody] object payload, [FromQuery] int? subscriptionId = null)
    {
        try
        {
            var delivery = await _webhookService.SendWebhookAsync(eventType, payload, subscriptionId);
            return Ok(new { success = true, data = delivery });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get webhook deliveries for a subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of webhook deliveries</returns>
    [HttpGet("{subscriptionId}/deliveries")]
    [Authorize(Policy = "Permission:Webhook.View")]
    public async Task<IActionResult> GetDeliveries(int subscriptionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var deliveries = await _webhookService.GetDeliveriesAsync(subscriptionId, page, pageSize);
            return Ok(new { success = true, data = deliveries });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get webhook delivery by ID
    /// </summary>
    /// <param name="deliveryId">Delivery ID</param>
    /// <returns>Webhook delivery details</returns>
    [HttpGet("deliveries/{deliveryId}")]
    [Authorize(Policy = "Permission:Webhook.View")]
    public async Task<IActionResult> GetDelivery(int deliveryId)
    {
        try
        {
            var delivery = await _webhookService.GetDeliveryAsync(deliveryId);
            return Ok(new { success = true, data = delivery });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Resend failed webhook delivery
    /// </summary>
    /// <param name="deliveryId">Delivery ID</param>
    /// <returns>Success status</returns>
    [HttpPost("deliveries/{deliveryId}/resend")]
    [Authorize(Policy = "Permission:Webhook.Send")]
    public async Task<IActionResult> ResendWebhook(int deliveryId)
    {
        try
        {
            var result = await _webhookService.ResendWebhookAsync(deliveryId);
            if (result)
                return Ok(new { success = true, message = "Webhook resent successfully" });
            else
                return NotFound(new { success = false, message = "Webhook delivery not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Test webhook subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="eventType">Event type to test</param>
    /// <returns>Test result</returns>
    [HttpPost("{subscriptionId}/test")]
    [Authorize(Policy = "Permission:Webhook.Test")]
    public async Task<IActionResult> TestWebhook(int subscriptionId, [FromQuery] string eventType = "test.event")
    {
        try
        {
            var result = await _webhookService.TestWebhookAsync(subscriptionId, eventType);
            return Ok(new { success = true, data = result });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get available webhook events
    /// </summary>
    /// <returns>List of available webhook events</returns>
    [HttpGet("events")]
    [Authorize(Policy = "Permission:Webhook.View")]
    public IActionResult GetAvailableEvents()
    {
        var events = WebhookEvents.GetAllEvents();
        return Ok(new { success = true, data = events });
    }

    /// <summary>
    /// Validate webhook signature (for external webhook receivers)
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <param name="secret">Webhook secret</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate-signature")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateSignature([FromBody] string payload, [FromHeader] string signature, [FromQuery] string secret)
    {
        try
        {
            var isValid = await _webhookService.ValidateWebhookSignatureAsync(payload, signature, secret);
            return Ok(new { success = true, valid = isValid });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}