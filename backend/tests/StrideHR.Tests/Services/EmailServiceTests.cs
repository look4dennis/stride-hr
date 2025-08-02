using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IEmailLogRepository> _mockEmailLogRepository;
    private readonly Mock<IEmailTemplateRepository> _mockTemplateRepository;
    private readonly Mock<IEmailCampaignRepository> _mockCampaignRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly Mock<IEmailTemplateService> _mockTemplateService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockEmailLogRepository = new Mock<IEmailLogRepository>();
        _mockTemplateRepository = new Mock<IEmailTemplateRepository>();
        _mockCampaignRepository = new Mock<IEmailCampaignRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockBranchRepository = new Mock<IBranchRepository>();
        _mockTemplateService = new Mock<IEmailTemplateService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _emailService = new EmailService(
            _mockEmailLogRepository.Object,
            _mockTemplateRepository.Object,
            _mockCampaignRepository.Object,
            _mockUserRepository.Object,
            _mockBranchRepository.Object,
            _mockTemplateService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockConfiguration.Object
        );
    }
    [
Fact]
    public async Task SendEmailAsync_ValidEmail_ReturnsEmailLogDto()
    {
        // Arrange
        var sendEmailDto = new SendEmailDto
        {
            ToEmail = "test@example.com",
            ToName = "Test User",
            Subject = "Test Subject",
            HtmlBody = "<p>Test Body</p>",
            Priority = EmailPriority.Normal
        };

        var emailLog = new EmailLog
        {
            Id = 1,
            ToEmail = sendEmailDto.ToEmail,
            ToName = sendEmailDto.ToName,
            Subject = sendEmailDto.Subject,
            HtmlBody = sendEmailDto.HtmlBody,
            Status = EmailStatus.Pending,
            Priority = sendEmailDto.Priority,
            CreatedAt = DateTime.UtcNow
        };

        var emailLogDto = new EmailLogDto
        {
            Id = 1,
            ToEmail = sendEmailDto.ToEmail,
            ToName = sendEmailDto.ToName,
            Subject = sendEmailDto.Subject,
            HtmlBody = sendEmailDto.HtmlBody,
            Status = EmailStatus.Pending,
            Priority = sendEmailDto.Priority,
            CreatedAt = DateTime.UtcNow
        };

        // Setup mocks with correct return types
        _mockEmailLogRepository.Setup(r => r.AddAsync(It.IsAny<EmailLog>()))
            .ReturnsAsync(emailLog);
        _mockEmailLogRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<EmailLogDto>(It.IsAny<EmailLog>()))
            .Returns(emailLogDto);

        // Act
        var result = await _emailService.SendEmailAsync(sendEmailDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sendEmailDto.ToEmail, result.ToEmail);
        Assert.Equal(sendEmailDto.Subject, result.Subject);
        Assert.Equal(EmailStatus.Pending, result.Status);
        _mockEmailLogRepository.Verify(r => r.AddAsync(It.IsAny<EmailLog>()), Times.Once);
        _mockEmailLogRepository.Verify(r => r.SaveChangesAsync(), Times.Exactly(2)); // Called once after Add and once after Update in SendEmailImmediatelyAsync
    }
   
 [Fact]
    public async Task ValidateEmailAddressAsync_ValidEmail_ReturnsTrue()
    {
        // Arrange
        var validEmail = "test@example.com";

        // Act
        var result = await _emailService.ValidateEmailAddressAsync(validEmail);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateEmailAddressAsync_InvalidEmail_ReturnsFalse()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act
        var result = await _emailService.ValidateEmailAddressAsync(invalidEmail);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateDeliveryStatusAsync_ValidExternalId_ReturnsTrue()
    {
        // Arrange
        var externalId = "ext-123";
        var status = EmailStatus.Delivered;
        var emailLog = new EmailLog { Id = 1, ExternalId = externalId, Status = EmailStatus.Sent };

        _mockEmailLogRepository.Setup(r => r.GetByExternalIdAsync(externalId))
            .ReturnsAsync(emailLog);
        _mockEmailLogRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailLog>()))
            .Returns(Task.CompletedTask);
        _mockEmailLogRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _emailService.UpdateDeliveryStatusAsync(externalId, status, DateTime.UtcNow);

        // Assert
        Assert.True(result);
        Assert.Equal(status, emailLog.Status);
        _mockEmailLogRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailLog>()), Times.Once);
        _mockEmailLogRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDeliveryStatusAsync_InvalidExternalId_ReturnsFalse()
    {
        // Arrange
        var externalId = "invalid-id";
        var status = EmailStatus.Delivered;

        _mockEmailLogRepository.Setup(r => r.GetByExternalIdAsync(externalId))
            .ReturnsAsync((EmailLog?)null);

        // Act
        var result = await _emailService.UpdateDeliveryStatusAsync(externalId, status, DateTime.UtcNow);

        // Assert
        Assert.False(result);
        _mockEmailLogRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailLog>()), Times.Never);
        _mockEmailLogRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}