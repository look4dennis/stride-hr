using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Webhooks;
using Xunit;
using EntityWebhookSubscription = StrideHR.Core.Entities.WebhookSubscription;
using EntityWebhookDelivery = StrideHR.Core.Entities.WebhookDelivery;

namespace StrideHR.Tests.Controllers;

public class WebhookControllerTests
{
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly WebhookController _controller;

    public WebhookControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookService>();
        _controller = new WebhookController(_mockWebhookService.Object);
    }

    [Fact]
    public async Task CreateSubscription_ValidDto_ReturnsOkResult()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Test Webhook",
            Url = "https://example.com/webhook",
            Secret = "secret123",
            Events = new List<string> { "employee.created" },
            IsActive = true
        };

        var subscription = new WebhookSubscription
        {
            Id = 1,
            Name = dto.Name,
            Url = dto.Url,
            Secret = dto.Secret,
            Events = dto.Events,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(dto))
            .ReturnsAsync(subscription);

        // Act
        var result = await _controller.CreateSubscription(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task CreateSubscription_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Test Webhook",
            Url = "invalid-url",
            Secret = "secret123",
            Events = new List<string> { "employee.created" },
            IsActive = true
        };

        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(dto))
            .ThrowsAsync(new ArgumentException("Invalid URL"));

        // Act
        var result = await _controller.CreateSubscription(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value as dynamic;
        Assert.False(response?.success);
        Assert.Equal("Invalid URL", response?.message?.ToString());
    }

    [Fact]
    public async Task GetSubscription_ValidId_ReturnsOkResult()
    {
        // Arrange
        var subscriptionId = 1;
        var subscription = new WebhookSubscription
        {
            Id = subscriptionId,
            Name = "Test Webhook",
            Url = "https://example.com/webhook",
            Secret = "secret123",
            Events = new List<string> { "employee.created" },
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockWebhookService.Setup(s => s.GetSubscriptionAsync(subscriptionId))
            .ReturnsAsync(subscription);

        // Act
        var result = await _controller.GetSubscription(subscriptionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task GetSubscription_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var subscriptionId = 999;
        _mockWebhookService.Setup(s => s.GetSubscriptionAsync(subscriptionId))
            .ThrowsAsync(new ArgumentException($"Webhook subscription with ID {subscriptionId} not found"));

        // Act
        var result = await _controller.GetSubscription(subscriptionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value as dynamic;
        Assert.False(response?.success);
    }

    [Fact]
    public async Task GetSubscriptions_ValidOrganizationId_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var subscriptions = new List<WebhookSubscription>
        {
            new WebhookSubscription
            {
                Id = 1,
                OrganizationId = organizationId,
                Name = "Test Webhook 1",
                Url = "https://example.com/webhook1",
                Secret = "secret123",
                Events = new List<string> { "employee.created" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new WebhookSubscription
            {
                Id = 2,
                OrganizationId = organizationId,
                Name = "Test Webhook 2",
                Url = "https://example.com/webhook2",
                Secret = "secret456",
                Events = new List<string> { "employee.updated" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockWebhookService.Setup(s => s.GetSubscriptionsAsync(organizationId))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _controller.GetSubscriptions(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task UpdateSubscription_ValidData_ReturnsOkResult()
    {
        // Arrange
        var subscriptionId = 1;
        var dto = new UpdateWebhookSubscriptionDto
        {
            Name = "Updated Webhook",
            IsActive = false
        };

        var updatedSubscription = new WebhookSubscription
        {
            Id = subscriptionId,
            Name = dto.Name,
            Url = "https://example.com/webhook",
            Secret = "secret123",
            Events = new List<string> { "employee.created" },
            IsActive = dto.IsActive.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockWebhookService.Setup(s => s.UpdateSubscriptionAsync(subscriptionId, dto))
            .ReturnsAsync(updatedSubscription);

        // Act
        var result = await _controller.UpdateSubscription(subscriptionId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task DeleteSubscription_ValidId_ReturnsOkResult()
    {
        // Arrange
        var subscriptionId = 1;
        _mockWebhookService.Setup(s => s.DeleteSubscriptionAsync(subscriptionId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSubscription(subscriptionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.Equal("Webhook subscription deleted successfully", response?.message?.ToString());
    }

    [Fact]
    public async Task DeleteSubscription_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var subscriptionId = 999;
        _mockWebhookService.Setup(s => s.DeleteSubscriptionAsync(subscriptionId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteSubscription(subscriptionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value as dynamic;
        Assert.False(response?.success);
        Assert.Equal("Webhook subscription not found", response?.message?.ToString());
    }

    [Fact]
    public async Task ToggleSubscription_ValidData_ReturnsOkResult()
    {
        // Arrange
        var subscriptionId = 1;
        var isActive = false;
        _mockWebhookService.Setup(s => s.ToggleSubscriptionAsync(subscriptionId, isActive))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleSubscription(subscriptionId, isActive);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.Equal("Webhook subscription deactivated successfully", response?.message?.ToString());
    }

    [Fact]
    public async Task SendWebhook_ValidData_ReturnsOkResult()
    {
        // Arrange
        var eventType = "employee.created";
        var payload = new { employeeId = 1, name = "John Doe" };
        var delivery = new WebhookDelivery
        {
            Id = 1,
            EventType = eventType,
            Payload = "{\"employeeId\":1,\"name\":\"John Doe\"}",
            Status = WebhookDeliveryStatus.Delivered,
            CreatedAt = DateTime.UtcNow,
            DeliveredAt = DateTime.UtcNow
        };

        _mockWebhookService.Setup(s => s.SendWebhookAsync(eventType, payload, null))
            .ReturnsAsync(delivery);

        // Act
        var result = await _controller.SendWebhook(eventType, payload);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task TestWebhook_ValidData_ReturnsOkResult()
    {
        // Arrange
        var subscriptionId = 1;
        var eventType = "test.event";
        var testResult = new WebhookTestResult
        {
            Success = true,
            HttpStatusCode = 200,
            ResponseBody = "OK",
            ResponseTime = TimeSpan.FromMilliseconds(150)
        };

        _mockWebhookService.Setup(s => s.TestWebhookAsync(subscriptionId, eventType))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestWebhook(subscriptionId, eventType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public void GetAvailableEvents_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetAvailableEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.NotNull(response?.data);
    }

    [Fact]
    public async Task ValidateSignature_ValidData_ReturnsOkResult()
    {
        // Arrange
        var payload = "{\"test\":\"data\"}";
        var signature = "sha256=abc123";
        var secret = "secret123";

        _mockWebhookService.Setup(s => s.ValidateWebhookSignatureAsync(payload, signature, secret))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateSignature(payload, signature, secret);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.True(response?.success);
        Assert.True(response?.valid);
    }
}