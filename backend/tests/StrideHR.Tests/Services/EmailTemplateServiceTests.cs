using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Email;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly Mock<IEmailTemplateRepository> _mockTemplateRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<EmailTemplateService>> _mockLogger;
    private readonly EmailTemplateService _templateService;

    public EmailTemplateServiceTests()
    {
        _mockTemplateRepository = new Mock<IEmailTemplateRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<EmailTemplateService>>();

        _templateService = new EmailTemplateService(
            _mockTemplateRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task RenderTemplateAsync_SimpleTemplate_ReturnsRenderedContent()
    {
        // Arrange
        var templateContent = "Hello {{Name}}, welcome to {{CompanyName}}!";
        var parameters = new Dictionary<string, object>
        {
            ["Name"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        // Act
        var result = await _templateService.RenderTemplateAsync(templateContent, parameters);

        // Assert
        Assert.Equal("Hello John Doe, welcome to Test Company!", result);
    }

    [Fact]
    public async Task RenderTemplateAsync_MissingParameter_ReplacesWithEmpty()
    {
        // Arrange
        var templateContent = "Hello {{Name}}, welcome to {{CompanyName}}!";
        var parameters = new Dictionary<string, object>
        {
            ["Name"] = "John Doe"
            // CompanyName is missing
        };

        // Act
        var result = await _templateService.RenderTemplateAsync(templateContent, parameters);

        // Assert
        Assert.Equal("Hello John Doe, welcome to !", result);
    }

    [Fact]
    public async Task RenderSubjectAsync_ValidTemplate_ReturnsRenderedSubject()
    {
        // Arrange
        var subjectTemplate = "Welcome {{Name}} to {{CompanyName}}";
        var parameters = new Dictionary<string, object>
        {
            ["Name"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        // Act
        var result = await _templateService.RenderSubjectAsync(subjectTemplate, parameters);

        // Assert
        Assert.Equal("Welcome John Doe to Test Company", result);
    }

    [Fact]
    public async Task RenderEmailTemplateAsync_ValidTemplate_ReturnsRenderResult()
    {
        // Arrange
        var templateId = 1;
        var template = new EmailTemplate
        {
            Id = templateId,
            Name = "Welcome Template",
            Subject = "Welcome {{Name}}!",
            HtmlBody = "<p>Hello {{Name}}, welcome to {{CompanyName}}!</p>",
            RequiredParameters = new List<string> { "Name", "CompanyName" },
            DefaultParameters = new Dictionary<string, object>
            {
                ["CompanyName"] = "Default Company"
            }
        };

        var parameters = new Dictionary<string, object>
        {
            ["Name"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _templateService.RenderEmailTemplateAsync(templateId, parameters);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Welcome John Doe!", result.Subject);
        Assert.Equal("<p>Hello John Doe, welcome to Test Company!</p>", result.HtmlBody);
        Assert.Empty(result.Errors);
        Assert.Empty(result.MissingParameters);
    }

    [Fact]
    public async Task RenderEmailTemplateAsync_MissingRequiredParameters_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = 1;
        var template = new EmailTemplate
        {
            Id = templateId,
            Name = "Welcome Template",
            Subject = "Welcome {{Name}}!",
            HtmlBody = "<p>Hello {{Name}}, welcome to {{CompanyName}}!</p>",
            RequiredParameters = new List<string> { "Name", "CompanyName" }
        };

        var parameters = new Dictionary<string, object>
        {
            ["Name"] = "John Doe"
            // CompanyName is missing
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _templateService.RenderEmailTemplateAsync(templateId, parameters);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Missing required parameters", result.Errors);
        Assert.Contains("CompanyName", result.MissingParameters);
    }

    [Fact]
    public async Task RenderEmailTemplateAsync_TemplateNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var templateId = 999;
        var parameters = new Dictionary<string, object>();

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((EmailTemplate?)null);

        // Act
        var result = await _templateService.RenderEmailTemplateAsync(templateId, parameters);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Template not found", result.Errors);
    }

    [Fact]
    public async Task ValidateTemplateAsync_ValidTemplate_ReturnsValidResult()
    {
        // Arrange
        var templateContent = "<p>Hello {{Name}}, welcome to {{CompanyName}}!</p>";
        var requiredParameters = new List<string> { "Name", "CompanyName" };

        // Act
        var result = await _templateService.ValidateTemplateAsync(templateContent, requiredParameters);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Contains("Name", result.ExtractedParameters);
        Assert.Contains("CompanyName", result.ExtractedParameters);
        Assert.Empty(result.MissingRequiredParameters);
    }

    [Fact]
    public async Task ValidateTemplateAsync_MissingRequiredParameter_ReturnsInvalidResult()
    {
        // Arrange
        var templateContent = "<p>Hello {{Name}}!</p>";
        var requiredParameters = new List<string> { "Name", "CompanyName" };

        // Act
        var result = await _templateService.ValidateTemplateAsync(templateContent, requiredParameters);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("CompanyName", result.MissingRequiredParameters);
        Assert.Contains("Name", result.ExtractedParameters);
        Assert.DoesNotContain("CompanyName", result.ExtractedParameters);
    }

    [Fact]
    public async Task ExtractParametersFromTemplateAsync_ValidTemplate_ReturnsParameters()
    {
        // Arrange
        var templateContent = "Hello {{Name}}, your order {{OrderId}} is ready. Total: {{Amount}}";

        // Act
        var result = await _templateService.ExtractParametersFromTemplateAsync(templateContent);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("Name", result);
        Assert.Contains("OrderId", result);
        Assert.Contains("Amount", result);
    }

    [Fact]
    public async Task ExtractParametersFromTemplateAsync_NoParameters_ReturnsEmptyList()
    {
        // Arrange
        var templateContent = "Hello, this is a static template with no parameters.";

        // Act
        var result = await _templateService.ExtractParametersFromTemplateAsync(templateContent);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ConvertToHtmlAsync_PlainText_ReturnsHtmlDocument()
    {
        // Arrange
        var textContent = "Hello World!\nThis is a test.\n\nNew paragraph.";

        // Act
        var result = await _templateService.ConvertToHtmlAsync(textContent);

        // Assert
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("<html>", result);
        Assert.Contains("<body>", result);
        Assert.Contains("<p>Hello World!</p>", result);
        Assert.Contains("<p>This is a test.</p>", result);
        Assert.Contains("<p>New paragraph.</p>", result);
        Assert.Contains("<br>", result); // For empty lines
    }

    [Fact]
    public async Task ConvertToTextAsync_HtmlContent_ReturnsPlainText()
    {
        // Arrange
        var htmlContent = "<html><body><h1>Title</h1><p>Hello <strong>World</strong>!</p></body></html>";

        // Act
        var result = await _templateService.ConvertToTextAsync(htmlContent);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.Contains("Title", result);
        Assert.Contains("Hello World!", result);
    }

    [Fact]
    public async Task SanitizeHtmlAsync_DangerousHtml_RemovesDangerousElements()
    {
        // Arrange
        var dangerousHtml = @"
            <p>Safe content</p>
            <script>alert('dangerous');</script>
            <iframe src='http://evil.com'></iframe>
            <a href='javascript:alert()'>Link</a>
            <div onclick='alert()'>Click me</div>";

        // Act
        var result = await _templateService.SanitizeHtmlAsync(dangerousHtml);

        // Assert
        Assert.Contains("<p>Safe content</p>", result);
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("<iframe>", result);
        Assert.DoesNotContain("javascript:", result);
        Assert.DoesNotContain("onclick", result);
    }

    [Fact]
    public async Task GeneratePreviewAsync_ValidTemplate_ReturnsPreview()
    {
        // Arrange
        var templateId = 1;
        var template = new EmailTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Subject = "Hello {{Name}}!",
            HtmlBody = "<p>Welcome {{Name}} to {{CompanyName}}!</p>",
            RequiredParameters = new List<string> { "Name", "CompanyName" }
        };

        var sampleData = new Dictionary<string, object>
        {
            ["Name"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _templateService.GeneratePreviewAsync(templateId, sampleData);

        // Assert
        Assert.Equal("Hello John Doe!", result.Subject);
        Assert.Equal("<p>Welcome John Doe to Test Company!</p>", result.HtmlBody);
        Assert.Equal(sampleData, result.SampleData);
        Assert.Contains($"/api/email-templates/{templateId}/preview", result.PreviewUrl);
    }

    [Fact]
    public async Task GeneratePreviewAsync_TemplateNotFound_ThrowsArgumentException()
    {
        // Arrange
        var templateId = 999;
        var sampleData = new Dictionary<string, object>();

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync((EmailTemplate?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _templateService.GeneratePreviewAsync(templateId, sampleData));

        Assert.Contains("Template not found", exception.Message);
    }

    [Fact]
    public async Task GeneratePreviewHtmlAsync_ValidTemplate_ReturnsRenderedHtml()
    {
        // Arrange
        var templateContent = "<h1>Hello {{Name}}!</h1><p>Welcome to {{CompanyName}}.</p>";
        var sampleData = new Dictionary<string, object>
        {
            ["Name"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        // Act
        var result = await _templateService.GeneratePreviewHtmlAsync(templateContent, sampleData);

        // Assert
        Assert.Equal("<h1>Hello John Doe!</h1><p>Welcome to Test Company.</p>", result);
    }
}