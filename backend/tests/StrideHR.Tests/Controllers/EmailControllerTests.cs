using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class EmailControllerTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly EmailController _controller;

    public EmailControllerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _controller = new EmailController(_mockEmailService.Object);
    }

    [Fact]
    public async Task SendEmail_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var sendEmailDto = new SendEmailDto
        {
            ToEmail = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<p>Test Body</p>"
        };

        var emailLogDto = new EmailLogDto
        {
            Id = 1,
            ToEmail = sendEmailDto.ToEmail,
            Subject = sendEmailDto.Subject,
            Status = EmailStatus.Pending
        };

        _mockEmailService.Setup(s => s.SendEmailAsync(sendEmailDto))
            .ReturnsAsync(emailLogDto);

        // Act
        var result = await _controller.SendEmail(sendEmailDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.SendEmailAsync(sendEmailDto), Times.Once);
    }

    [Fact]
    public async Task SendEmail_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var sendEmailDto = new SendEmailDto
        {
            ToEmail = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<p>Test Body</p>"
        };

        _mockEmailService.Setup(s => s.SendEmailAsync(sendEmailDto))
            .ThrowsAsync(new Exception("Email service error"));

        // Act
        var result = await _controller.SendEmail(sendEmailDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task SendBulkEmail_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var bulkEmailDto = new BulkEmailDto
        {
            Recipients = new List<EmailRecipientDto>
            {
                new() { Email = "user1@example.com", Name = "User 1" },
                new() { Email = "user2@example.com", Name = "User 2" }
            },
            Subject = "Bulk Test",
            HtmlBody = "<p>Bulk Body</p>"
        };

        var emailLogDtos = new List<EmailLogDto>
        {
            new() { Id = 1, ToEmail = "user1@example.com", Status = EmailStatus.Pending },
            new() { Id = 2, ToEmail = "user2@example.com", Status = EmailStatus.Pending }
        };

        _mockEmailService.Setup(s => s.SendBulkEmailAsync(bulkEmailDto))
            .ReturnsAsync(emailLogDtos);

        // Act
        var result = await _controller.SendBulkEmail(bulkEmailDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.SendBulkEmailAsync(bulkEmailDto), Times.Once);
    }

    [Fact]
    public async Task SendTemplateEmail_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var sendTemplateEmailDto = new SendTemplateEmailDto
        {
            TemplateId = 1,
            ToEmail = "test@example.com",
            Parameters = new Dictionary<string, object> { ["Name"] = "John" }
        };

        var emailLogDto = new EmailLogDto
        {
            Id = 1,
            ToEmail = sendTemplateEmailDto.ToEmail,
            Status = EmailStatus.Pending
        };

        _mockEmailService.Setup(s => s.SendTemplateEmailAsync(sendTemplateEmailDto))
            .ReturnsAsync(emailLogDto);

        // Act
        var result = await _controller.SendTemplateEmail(sendTemplateEmailDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.SendTemplateEmailAsync(sendTemplateEmailDto), Times.Once);
    }

    [Fact]
    public async Task GetEmailLogs_ValidFilter_ReturnsEmailLogs()
    {
        // Arrange
        var filter = new EmailLogFilterDto
        {
            Page = 1,
            PageSize = 10
        };

        var emailLogs = new List<EmailLogDto>
        {
            new() { Id = 1, ToEmail = "test1@example.com", Status = EmailStatus.Sent },
            new() { Id = 2, ToEmail = "test2@example.com", Status = EmailStatus.Delivered }
        };

        _mockEmailService.Setup(s => s.GetEmailLogsAsync(filter))
            .ReturnsAsync(emailLogs);

        // Act
        var result = await _controller.GetEmailLogs(filter);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.GetEmailLogsAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetEmailLog_ExistingId_ReturnsEmailLog()
    {
        // Arrange
        var emailId = 1;
        var emailLog = new EmailLogDto
        {
            Id = emailId,
            ToEmail = "test@example.com",
            Status = EmailStatus.Sent
        };

        _mockEmailService.Setup(s => s.GetEmailLogAsync(emailId))
            .ReturnsAsync(emailLog);

        // Act
        var result = await _controller.GetEmailLog(emailId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.GetEmailLogAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task GetEmailLog_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var emailId = 999;

        _mockEmailService.Setup(s => s.GetEmailLogAsync(emailId))
            .ReturnsAsync((EmailLogDto?)null);

        // Act
        var result = await _controller.GetEmailLog(emailId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockEmailService.Verify(s => s.GetEmailLogAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task ValidateEmail_ValidEmail_ReturnsValidResult()
    {
        // Arrange
        var request = new ValidateEmailRequest { Email = "test@example.com" };

        _mockEmailService.Setup(s => s.ValidateEmailAddressAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateEmail(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.ValidateEmailAddressAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task ValidateEmail_InvalidEmail_ReturnsInvalidResult()
    {
        // Arrange
        var request = new ValidateEmailRequest { Email = "invalid-email" };

        _mockEmailService.Setup(s => s.ValidateEmailAddressAsync(request.Email))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateEmail(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.ValidateEmailAddressAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task TrackEmailOpen_ValidExternalId_ReturnsSuccess()
    {
        // Arrange
        var externalId = "test-external-id";

        _mockEmailService.Setup(s => s.TrackEmailOpenAsync(externalId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TrackEmailOpen(externalId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.TrackEmailOpenAsync(externalId), Times.Once);
    }

    [Fact]
    public async Task TrackEmailOpen_InvalidExternalId_ReturnsNotFound()
    {
        // Arrange
        var externalId = "invalid-external-id";

        _mockEmailService.Setup(s => s.TrackEmailOpenAsync(externalId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.TrackEmailOpen(externalId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockEmailService.Verify(s => s.TrackEmailOpenAsync(externalId), Times.Once);
    }

    [Fact]
    public async Task TrackEmailClick_ValidExternalId_ReturnsSuccess()
    {
        // Arrange
        var externalId = "test-external-id";
        var request = new TrackClickRequest { Url = "https://example.com" };

        _mockEmailService.Setup(s => s.TrackEmailClickAsync(externalId, request.Url))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TrackEmailClick(externalId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.TrackEmailClickAsync(externalId, request.Url), Times.Once);
    }

    [Fact]
    public async Task ProcessUnsubscribe_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = "valid-unsubscribe-token";

        _mockEmailService.Setup(s => s.ProcessUnsubscribeAsync(token))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ProcessUnsubscribe(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.ProcessUnsubscribeAsync(token), Times.Once);
    }

    [Fact]
    public async Task ProcessUnsubscribe_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var token = "invalid-token";

        _mockEmailService.Setup(s => s.ProcessUnsubscribeAsync(token))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ProcessUnsubscribe(token);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockEmailService.Verify(s => s.ProcessUnsubscribeAsync(token), Times.Once);
    }

    [Fact]
    public async Task GetEmailAnalytics_ValidFilter_ReturnsAnalytics()
    {
        // Arrange
        var filter = new EmailAnalyticsFilterDto
        {
            FromDate = DateTime.Now.AddDays(-30),
            ToDate = DateTime.Now
        };

        var analytics = new EmailAnalyticsDto
        {
            TotalEmailsSent = 100,
            TotalEmailsDelivered = 95,
            DeliveryRate = 95.0m
        };

        _mockEmailService.Setup(s => s.GetEmailAnalyticsAsync(filter))
            .ReturnsAsync(analytics);

        // Act
        var result = await _controller.GetEmailAnalytics(filter);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.GetEmailAnalyticsAsync(filter), Times.Once);
    }

    [Fact]
    public async Task RetryFailedEmails_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var maxRetries = 3;

        _mockEmailService.Setup(s => s.RetryFailedEmailsAsync(maxRetries))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RetryFailedEmails(maxRetries);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.RetryFailedEmailsAsync(maxRetries), Times.Once);
    }

    [Fact]
    public async Task ProcessEmailQueue_ValidRequest_ReturnsProcessedCount()
    {
        // Arrange
        var batchSize = 50;
        var processedCount = 25;

        _mockEmailService.Setup(s => s.ProcessEmailQueueAsync(batchSize))
            .ReturnsAsync(processedCount);

        // Act
        var result = await _controller.ProcessEmailQueue(batchSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockEmailService.Verify(s => s.ProcessEmailQueueAsync(batchSize), Times.Once);
    }
}