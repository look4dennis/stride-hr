using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.API.Models;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DocumentTemplate;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class DocumentTemplateControllerTests
{
    private readonly Mock<IDocumentTemplateService> _mockService;
    private readonly DocumentTemplateController _controller;

    public DocumentTemplateControllerTests()
    {
        _mockService = new Mock<IDocumentTemplateService>();
        _controller = new DocumentTemplateController(_mockService.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetActiveTemplates_ReturnsOkResult()
    {
        // Arrange
        var templates = new List<DocumentTemplateDto>
        {
            new DocumentTemplateDto
            {
                Id = 1,
                Name = "Test Template",
                Type = DocumentType.OfferLetter,
                IsActive = true
            }
        };

        _mockService.Setup(s => s.GetActiveTemplatesAsync())
            .ReturnsAsync(templates);

        // Act
        var result = await _controller.GetActiveTemplates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<DocumentTemplateDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetTemplateById_ExistingId_ReturnsTemplate()
    {
        // Arrange
        var templateId = 1;
        var template = new DocumentTemplateDto
        {
            Id = templateId,
            Name = "Test Template",
            Type = DocumentType.OfferLetter
        };

        _mockService.Setup(s => s.GetTemplateByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _controller.GetTemplateById(templateId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(templateId, response.Data.Id);
    }

    [Fact]
    public async Task GetTemplateById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var templateId = 999;
        _mockService.Setup(s => s.GetTemplateByIdAsync(templateId))
            .ReturnsAsync((DocumentTemplateDto?)null);

        // Act
        var result = await _controller.GetTemplateById(templateId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(notFoundResult.Value);
        Assert.False(response.Success);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task CreateTemplate_ValidDto_ReturnsCreatedResult()
    {
        // Arrange
        var dto = new CreateDocumentTemplateDto
        {
            Name = "New Template",
            Type = DocumentType.OfferLetter,
            Content = "<html><body>Test</body></html>"
        };

        var createdTemplate = new DocumentTemplateDto
        {
            Id = 1,
            Name = dto.Name,
            Type = dto.Type,
            Content = dto.Content
        };

        _mockService.Setup(s => s.CreateTemplateAsync(dto, It.IsAny<int>()))
            .ReturnsAsync(createdTemplate);

        // Act
        var result = await _controller.CreateTemplate(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.Equal(dto.Name, response.Data.Name);
    }

    [Fact]
    public async Task CreateTemplate_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateDocumentTemplateDto
        {
            Name = "Duplicate Template",
            Type = DocumentType.OfferLetter
        };

        _mockService.Setup(s => s.CreateTemplateAsync(dto, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Template already exists"));

        // Act
        var result = await _controller.CreateTemplate(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task UpdateTemplate_ValidDto_ReturnsOkResult()
    {
        // Arrange
        var templateId = 1;
        var dto = new UpdateDocumentTemplateDto
        {
            Name = "Updated Template",
            Content = "<html><body>Updated</body></html>"
        };

        var updatedTemplate = new DocumentTemplateDto
        {
            Id = templateId,
            Name = dto.Name,
            Content = dto.Content
        };

        _mockService.Setup(s => s.UpdateTemplateAsync(templateId, dto, It.IsAny<int>()))
            .ReturnsAsync(updatedTemplate);

        // Act
        var result = await _controller.UpdateTemplate(templateId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(dto.Name, response.Data.Name);
    }

    [Fact]
    public async Task DeleteTemplate_ExistingTemplate_ReturnsOkResult()
    {
        // Arrange
        var templateId = 1;
        _mockService.Setup(s => s.DeleteTemplateAsync(templateId, It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTemplate(templateId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
    }

    [Fact]
    public async Task DeleteTemplate_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        var templateId = 999;
        _mockService.Setup(s => s.DeleteTemplateAsync(templateId, It.IsAny<int>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTemplate(templateId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task PreviewTemplate_ValidId_ReturnsPreview()
    {
        // Arrange
        var templateId = 1;
        var preview = new DocumentTemplatePreviewDto
        {
            PreviewHtml = "<html><body>Preview content</body></html>",
            SampleData = new Dictionary<string, object> { ["EmployeeName"] = "John Doe" }
        };

        _mockService.Setup(s => s.PreviewTemplateAsync(templateId, It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(preview);

        // Act
        var result = await _controller.PreviewTemplate(templateId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplatePreviewDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Contains("Preview content", response.Data.PreviewHtml);
    }

    [Fact]
    public async Task GetAvailableCategories_ReturnsCategories()
    {
        // Arrange
        var categories = new List<string> { "HR", "Legal", "Finance" };
        _mockService.Setup(s => s.GetAvailableCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAvailableCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<string>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(3, response.Data.Count());
    }

    [Fact]
    public async Task GetAvailableMergeFields_ValidType_ReturnsMergeFields()
    {
        // Arrange
        var documentType = DocumentType.OfferLetter;
        var mergeFields = new Dictionary<string, object>
        {
            ["EmployeeName"] = "Employee's full name",
            ["Position"] = "Job position",
            ["Salary"] = "Offered salary"
        };

        _mockService.Setup(s => s.GetAvailableMergeFieldsAsync(documentType))
            .ReturnsAsync(mergeFields);

        // Act
        var result = await _controller.GetAvailableMergeFields(documentType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Dictionary<string, object>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(3, response.Data.Count);
        Assert.True(response.Data.ContainsKey("EmployeeName"));
    }

    [Fact]
    public async Task ValidateTemplate_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateId = 1;
        _mockService.Setup(s => s.ValidateTemplateAsync(templateId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateTemplate(templateId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);
    }

    [Fact]
    public async Task CloneTemplate_ValidRequest_ReturnsClonedTemplate()
    {
        // Arrange
        var templateId = 1;
        var newName = "Cloned Template";
        var clonedTemplate = new DocumentTemplateDto
        {
            Id = 2,
            Name = newName,
            Type = DocumentType.OfferLetter
        };

        _mockService.Setup(s => s.CloneTemplateAsync(templateId, newName, It.IsAny<int>()))
            .ReturnsAsync(clonedTemplate);

        // Act
        var result = await _controller.CloneTemplate(templateId, newName);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DocumentTemplateDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.Equal(newName, response.Data.Name);
    }
}