using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DocumentTemplate;
using StrideHR.Infrastructure.Mapping;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class DocumentTemplateServiceTests
{
    private readonly Mock<IDocumentTemplateRepository> _mockTemplateRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly IMapper _mapper;
    private readonly DocumentTemplateService _service;

    public DocumentTemplateServiceTests()
    {
        _mockTemplateRepository = new Mock<IDocumentTemplateRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DocumentTemplateMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new DocumentTemplateService(
            _mockTemplateRepository.Object,
            _mapper,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetActiveTemplatesAsync_ReturnsActiveTemplates()
    {
        // Arrange
        var templates = new List<DocumentTemplate>
        {
            new DocumentTemplate
            {
                Id = 1,
                Name = "Offer Letter Template",
                Type = DocumentType.OfferLetter,
                IsActive = true,
                CreatedByEmployee = new Employee { FirstName = "John", LastName = "Doe" }
            },
            new DocumentTemplate
            {
                Id = 2,
                Name = "Contract Template",
                Type = DocumentType.Contract,
                IsActive = true,
                CreatedByEmployee = new Employee { FirstName = "Jane", LastName = "Smith" }
            }
        };

        _mockTemplateRepository.Setup(r => r.GetActiveTemplatesAsync())
            .ReturnsAsync(templates);
        _mockTemplateRepository.Setup(r => r.GetUsageCountAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        // Act
        var result = await _service.GetActiveTemplatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task CreateTemplateAsync_ValidDto_CreatesTemplate()
    {
        // Arrange
        var dto = new CreateDocumentTemplateDto
        {
            Name = "New Template",
            Description = "Test template",
            Type = DocumentType.OfferLetter,
            Content = "<html><body>Hello {{EmployeeName}}</body></html>",
            MergeFields = new[] { "EmployeeName" },
            Category = "HR"
        };

        _mockTemplateRepository.Setup(r => r.IsTemplateNameUniqueAsync(dto.Name, null))
            .ReturnsAsync(true);
        _mockTemplateRepository.Setup(r => r.AddAsync(It.IsAny<DocumentTemplate>()))
            .ReturnsAsync((DocumentTemplate template) => template);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateTemplateAsync(dto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Type, result.Type);
        Assert.True(result.IsActive);
        _mockTemplateRepository.Verify(r => r.AddAsync(It.IsAny<DocumentTemplate>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTemplateAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var dto = new CreateDocumentTemplateDto
        {
            Name = "Existing Template",
            Type = DocumentType.OfferLetter
        };

        _mockTemplateRepository.Setup(r => r.IsTemplateNameUniqueAsync(dto.Name, null))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTemplateAsync(dto, 1));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateTemplateAsync_ValidDto_UpdatesTemplate()
    {
        // Arrange
        var templateId = 1;
        var existingTemplate = new DocumentTemplate
        {
            Id = templateId,
            Name = "Original Template",
            Content = "<html><body>Original content</body></html>",
            IsSystemTemplate = false,
            Versions = new List<DocumentTemplateVersion>()
        };

        var dto = new UpdateDocumentTemplateDto
        {
            Name = "Updated Template",
            Content = "<html><body>Updated content</body></html>",
            ChangeLog = "Updated content"
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);
        _mockTemplateRepository.Setup(r => r.IsTemplateNameUniqueAsync(dto.Name, templateId))
            .ReturnsAsync(true);
        _mockTemplateRepository.Setup(r => r.UpdateAsync(It.IsAny<DocumentTemplate>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateTemplateAsync(templateId, dto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        _mockTemplateRepository.Verify(r => r.UpdateAsync(It.IsAny<DocumentTemplate>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTemplateAsync_SystemTemplate_ThrowsException()
    {
        // Arrange
        var templateId = 1;
        var systemTemplate = new DocumentTemplate
        {
            Id = templateId,
            Name = "System Template",
            IsSystemTemplate = true
        };

        var dto = new UpdateDocumentTemplateDto
        {
            Name = "Updated Template"
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(systemTemplate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateTemplateAsync(templateId, dto, 1));
        Assert.Contains("System templates cannot be modified", exception.Message);
    }

    [Fact]
    public async Task DeleteTemplateAsync_TemplateInUse_ThrowsException()
    {
        // Arrange
        var templateId = 1;
        var template = new DocumentTemplate
        {
            Id = templateId,
            Name = "Template In Use",
            IsSystemTemplate = false
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);
        _mockTemplateRepository.Setup(r => r.GetUsageCountAsync(templateId))
            .ReturnsAsync(5);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteTemplateAsync(templateId, 1));
        Assert.Contains("Cannot delete template that has been used", exception.Message);
    }

    [Fact]
    public async Task PreviewTemplateAsync_ValidTemplate_ReturnsPreview()
    {
        // Arrange
        var templateId = 1;
        var template = new DocumentTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Type = DocumentType.OfferLetter,
            Content = "<html><body>Hello {{EmployeeName}}, welcome to {{CompanyName}}!</body></html>"
        };

        var sampleData = new Dictionary<string, object>
        {
            ["EmployeeName"] = "John Doe",
            ["CompanyName"] = "Test Company"
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.PreviewTemplateAsync(templateId, sampleData);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("John Doe", result.PreviewHtml);
        Assert.Contains("Test Company", result.PreviewHtml);
        Assert.Equal(sampleData, result.SampleData);
    }

    [Fact]
    public async Task ValidateTemplateAsync_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateId = 1;
        var template = new DocumentTemplate
        {
            Id = templateId,
            Content = "<html><body>Hello {{EmployeeName}}!</body></html>",
            MergeFields = new[] { "EmployeeName" },
            RequiredFields = new[] { "EmployeeName" }
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateTemplateAsync(templateId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateTemplateAsync_MissingMergeField_ReturnsFalse()
    {
        // Arrange
        var templateId = 1;
        var template = new DocumentTemplate
        {
            Id = templateId,
            Content = "<html><body>Hello World!</body></html>",
            MergeFields = new[] { "EmployeeName" }, // Field not in content
            RequiredFields = new[] { "EmployeeName" }
        };

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        // Act
        var result = await _service.ValidateTemplateAsync(templateId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CloneTemplateAsync_ValidTemplate_CreatesClone()
    {
        // Arrange
        var templateId = 1;
        var originalTemplate = new DocumentTemplate
        {
            Id = templateId,
            Name = "Original Template",
            Description = "Original description",
            Type = DocumentType.OfferLetter,
            Content = "<html><body>Original content</body></html>",
            MergeFields = new[] { "EmployeeName" },
            Category = "HR"
        };

        var newName = "Cloned Template";

        _mockTemplateRepository.Setup(r => r.GetByIdAsync(templateId))
            .ReturnsAsync(originalTemplate);
        _mockTemplateRepository.Setup(r => r.IsTemplateNameUniqueAsync(newName, null))
            .ReturnsAsync(true);
        _mockTemplateRepository.Setup(r => r.AddAsync(It.IsAny<DocumentTemplate>()))
            .ReturnsAsync((DocumentTemplate template) => template);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CloneTemplateAsync(templateId, newName, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newName, result.Name);
        Assert.Equal(originalTemplate.Type, result.Type);
        Assert.Equal(originalTemplate.Category, result.Category);
        Assert.False(result.IsSystemTemplate);
        _mockTemplateRepository.Verify(r => r.AddAsync(It.IsAny<DocumentTemplate>()), Times.Once);
    }

    [Fact]
    public async Task GetAvailableMergeFieldsAsync_OfferLetterType_ReturnsCorrectFields()
    {
        // Act
        var result = await _service.GetAvailableMergeFieldsAsync(DocumentType.OfferLetter);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("EmployeeName"));
        Assert.True(result.ContainsKey("Position"));
        Assert.True(result.ContainsKey("Salary"));
        Assert.True(result.ContainsKey("StartDate"));
    }
}