using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayslipTemplateServiceTests
{
    private readonly Mock<IPayslipTemplateRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<PayslipTemplateService>> _mockLogger;
    private readonly PayslipTemplateService _service;

    public PayslipTemplateServiceTests()
    {
        _mockRepository = new Mock<IPayslipTemplateRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<PayslipTemplateService>>();

        _service = new PayslipTemplateService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTemplateAsync_ValidTemplate_ReturnsCreatedTemplate()
    {
        // Arrange
        var templateDto = new PayslipTemplateDto
        {
            Name = "Test Template",
            Description = "Test Description",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>
                {
                    new PayslipSection { Id = "header", Name = "Header", Type = "header", Order = 1 }
                }
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontSize = 12
            }
        };

        _mockRepository.Setup(x => x.GetActiveTemplateByNameAsync(1, "Test Template"))
            .ReturnsAsync((PayslipTemplate?)null);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<PayslipTemplate>()))
            .Returns(Task.FromResult(new PayslipTemplate()));

        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        var createdTemplate = new PayslipTemplate
        {
            Id = 1,
            Name = "Test Template",
            OrganizationId = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedByEmployee = new Employee { FirstName = "Test", LastName = "User" },
            TemplateConfig = """{"Sections":[{"Id":"header","Name":"Header","Type":"header","Order":1}]}""",
            VisibleFields = """["field1","field2"]""",
            FieldLabels = """{"field1":"Field 1","field2":"Field 2"}""",
            ShowOrganizationLogo = true,
            HeaderText = "Test Header"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(createdTemplate);

        // Act
        var result = await _service.CreateTemplateAsync(templateDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Template", result.Name);
        Assert.Equal(1, result.OrganizationId);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<PayslipTemplate>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTemplateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var templateDto = new PayslipTemplateDto
        {
            Name = "Existing Template",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>
                {
                    new PayslipSection { Id = "header", Name = "Header", Type = "header", Order = 1 }
                }
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontSize = 12
            }
        };

        var existingTemplate = new PayslipTemplate
        {
            Id = 1,
            Name = "Existing Template",
            OrganizationId = 1
        };

        _mockRepository.Setup(x => x.GetActiveTemplateByNameAsync(1, "Existing Template"))
            .ReturnsAsync(existingTemplate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateTemplateAsync(templateDto, 1));

        Assert.Contains("Template with name 'Existing Template' already exists", exception.Message);
    }

    [Fact]
    public async Task ValidateTemplateAsync_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateDto = new PayslipTemplateDto
        {
            Name = "Valid Template",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>
                {
                    new PayslipSection { Id = "header", Name = "Header", Type = "header", Order = 1 },
                    new PayslipSection { Id = "summary", Name = "Summary", Type = "summary", Order = 2 }
                }
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontSize = 12
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateAsync(templateDto);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateTemplateAsync_InvalidTemplate_ReturnsFalseWithErrors()
    {
        // Arrange
        var templateDto = new PayslipTemplateDto
        {
            Name = "", // Invalid: empty name
            OrganizationId = 0, // Invalid: zero organization ID
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>() // Invalid: no sections
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "invalid-color", // Invalid: not hex color
                SecondaryColor = "#6b7280",
                FontSize = 50 // Invalid: font size too large
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateAsync(templateDto);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Template name is required", errors);
        Assert.Contains("Valid organization ID is required", errors);
        Assert.Contains("Template must have at least one section", errors);
        Assert.Contains("Primary color must be a valid hex color", errors);
        Assert.Contains("Font size must be between 8 and 24", errors);
    }

    [Fact]
    public async Task UpdateTemplateAsync_ValidUpdate_ReturnsUpdatedTemplate()
    {
        // Arrange
        var templateId = 1;
        var templateDto = new PayslipTemplateDto
        {
            Name = "Updated Template",
            Description = "Updated Description",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>
                {
                    new PayslipSection { Id = "header", Name = "Header", Type = "header", Order = 1 }
                }
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontSize = 12
            }
        };

        var existingTemplate = new PayslipTemplate
        {
            Id = 1,
            Name = "Original Template",
            OrganizationId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedByEmployee = new Employee { FirstName = "Creator", LastName = "User" }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(existingTemplate);

        _mockRepository.Setup(x => x.GetActiveTemplateByNameAsync(1, "Updated Template"))
            .ReturnsAsync((PayslipTemplate?)null);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipTemplate>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.UpdateTemplateAsync(templateId, templateDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Template", result.Name);
        Assert.Equal("Updated Description", result.Description);

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<PayslipTemplate>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTemplateAsync_TemplateNotFound_ThrowsArgumentException()
    {
        // Arrange
        var templateId = 999;
        var templateDto = new PayslipTemplateDto
        {
            Name = "Updated Template",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Sections = new List<PayslipSection>
                {
                    new PayslipSection { Id = "header", Name = "Header", Type = "header", Order = 1 }
                }
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontSize = 12
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync((PayslipTemplate?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateTemplateAsync(templateId, templateDto, 1));

        Assert.Contains("Template with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetDefaultTemplateAsync_BranchSpecificExists_ReturnsBranchTemplate()
    {
        // Arrange
        var organizationId = 1;
        var branchId = 1;

        var branchTemplate = new PayslipTemplate
        {
            Id = 1,
            Name = "Branch Template",
            OrganizationId = organizationId,
            BranchId = branchId,
            IsDefault = true,
            CreatedByEmployee = new Employee { FirstName = "Creator", LastName = "User" },
            TemplateConfig = """{"Sections":[{"Id":"header","Name":"Header","Type":"header","Order":1}]}""",
            VisibleFields = """["field1","field2"]""",
            FieldLabels = """{"field1":"Field 1","field2":"Field 2"}""",
            ShowOrganizationLogo = true,
            HeaderText = "Branch Header"
        };

        _mockRepository.Setup(x => x.GetDefaultTemplateAsync(organizationId, branchId))
            .ReturnsAsync(branchTemplate);

        // Act
        var result = await _service.GetDefaultTemplateAsync(organizationId, branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Branch Template", result.Name);
        Assert.Equal(branchId, result.BranchId);
    }

    [Fact]
    public async Task SetAsDefaultAsync_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateId = 1;
        var organizationId = 1;
        var branchId = 1;

        _mockRepository.Setup(x => x.SetAsDefaultAsync(templateId, organizationId, branchId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.SetAsDefaultAsync(templateId, organizationId, branchId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.SetAsDefaultAsync(templateId, organizationId, branchId), Times.Once);
    }

    [Fact]
    public async Task DeactivateTemplateAsync_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateId = 1;
        var template = new PayslipTemplate
        {
            Id = templateId,
            Name = "Test Template",
            IsActive = true
        };

        _mockRepository.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync(template);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipTemplate>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.DeactivateTemplateAsync(templateId);

        // Assert
        Assert.True(result);
        Assert.False(template.IsActive);
        Assert.NotNull(template.LastModifiedAt);

        _mockRepository.Verify(x => x.UpdateAsync(template), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateTemplateAsync_TemplateNotFound_ReturnsFalse()
    {
        // Arrange
        var templateId = 999;

        _mockRepository.Setup(x => x.GetByIdAsync(templateId))
            .ReturnsAsync((PayslipTemplate?)null);

        // Act
        var result = await _service.DeactivateTemplateAsync(templateId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<PayslipTemplate>()), Times.Never);
    }
}