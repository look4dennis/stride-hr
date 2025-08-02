using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayslipDesignerServiceTests
{
    private readonly Mock<ILogger<PayslipDesignerService>> _mockLogger;
    private readonly PayslipDesignerService _service;

    public PayslipDesignerServiceTests()
    {
        _mockLogger = new Mock<ILogger<PayslipDesignerService>>();
        _service = new PayslipDesignerService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateHtmlPreviewAsync_ValidInputs_ReturnsHtmlContent()
    {
        // Arrange
        var template = CreateTestTemplate();
        var payrollData = CreateTestPayrollData();

        // Act
        var result = await _service.GenerateHtmlPreviewAsync(template, payrollData);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("<html>", result);
        Assert.Contains("John Doe", result);
        Assert.Contains("USD 5,000.00", result);
        Assert.Contains("December 2024", result);
    }

    [Fact]
    public async Task GeneratePdfPayslipAsync_ValidInputs_ReturnsPdfContent()
    {
        // Arrange
        var template = CreateTestTemplate();
        var payrollData = CreateTestPayrollData();

        // Act
        var (pdfContent, fileName) = await _service.GeneratePdfPayslipAsync(template, payrollData);

        // Assert
        Assert.NotNull(pdfContent);
        Assert.NotEmpty(pdfContent);
        Assert.NotNull(fileName);
        Assert.Contains("Payslip_John_Doe", fileName);
        Assert.Contains("202412", fileName);
        Assert.EndsWith(".pdf", fileName);
    }

    [Fact]
    public async Task GetAvailableFieldMappingsAsync_ReturnsExpectedMappings()
    {
        // Act
        var result = await _service.GetAvailableFieldMappingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Check for essential field mappings
        Assert.True(result.ContainsKey("employee.name"));
        Assert.True(result.ContainsKey("employee.id"));
        Assert.True(result.ContainsKey("salary.basic"));
        Assert.True(result.ContainsKey("salary.gross"));
        Assert.True(result.ContainsKey("salary.net"));
        Assert.True(result.ContainsKey("allowances.total"));
        Assert.True(result.ContainsKey("deductions.total"));
        Assert.True(result.ContainsKey("organization.name"));
        
        // Check field descriptions
        Assert.Equal("Employee Name", result["employee.name"]);
        Assert.Equal("Basic Salary", result["salary.basic"]);
        Assert.Equal("Net Salary", result["salary.net"]);
    }

    [Fact]
    public async Task ValidateTemplateDesignAsync_ValidTemplate_ReturnsTrue()
    {
        // Arrange
        var templateConfig = new PayslipTemplateConfig
        {
            Sections = new List<PayslipSection>
            {
                new PayslipSection
                {
                    Id = "header",
                    Name = "Header",
                    Type = "header",
                    Order = 1,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField
                        {
                            Id = "emp-name",
                            Name = "Employee Name",
                            DataSource = "employee.name",
                            Format = "text"
                        }
                    }
                },
                new PayslipSection
                {
                    Id = "summary",
                    Name = "Summary",
                    Type = "summary",
                    Order = 2,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField
                        {
                            Id = "net-salary",
                            Name = "Net Salary",
                            DataSource = "salary.net",
                            Format = "currency"
                        }
                    }
                }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateDesignAsync(templateConfig);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateTemplateDesignAsync_InvalidTemplate_ReturnsFalseWithErrors()
    {
        // Arrange
        var templateConfig = new PayslipTemplateConfig
        {
            Sections = new List<PayslipSection>() // No sections
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateDesignAsync(templateConfig);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Template must have at least one section", errors);
    }

    [Fact]
    public async Task ValidateTemplateDesignAsync_MissingRequiredSections_ReturnsFalseWithErrors()
    {
        // Arrange
        var templateConfig = new PayslipTemplateConfig
        {
            Sections = new List<PayslipSection>
            {
                new PayslipSection
                {
                    Id = "earnings",
                    Name = "Earnings",
                    Type = "earnings",
                    Order = 1
                }
                // Missing header and summary sections
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateDesignAsync(templateConfig);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Template must include a header section", errors);
        Assert.Contains("Template must include a summary section", errors);
    }

    [Fact]
    public async Task ValidateTemplateDesignAsync_DuplicateOrders_ReturnsFalseWithErrors()
    {
        // Arrange
        var templateConfig = new PayslipTemplateConfig
        {
            Sections = new List<PayslipSection>
            {
                new PayslipSection
                {
                    Id = "header",
                    Name = "Header",
                    Type = "header",
                    Order = 1
                },
                new PayslipSection
                {
                    Id = "summary",
                    Name = "Summary",
                    Type = "summary",
                    Order = 1 // Duplicate order
                }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateDesignAsync(templateConfig);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Section orders must be unique", errors);
    }

    [Fact]
    public async Task ValidateTemplateDesignAsync_InvalidFieldMapping_ReturnsFalseWithErrors()
    {
        // Arrange
        var templateConfig = new PayslipTemplateConfig
        {
            Sections = new List<PayslipSection>
            {
                new PayslipSection
                {
                    Id = "header",
                    Name = "Header",
                    Type = "header",
                    Order = 1,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField
                        {
                            Id = "invalid-field",
                            Name = "Invalid Field",
                            DataSource = "invalid.field.mapping", // Invalid mapping
                            Format = "text"
                        }
                    }
                },
                new PayslipSection
                {
                    Id = "summary",
                    Name = "Summary",
                    Type = "summary",
                    Order = 2
                }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateTemplateDesignAsync(templateConfig);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Invalid field mapping: invalid.field.mapping", errors);
    }

    [Fact]
    public async Task GetDefaultTemplateConfigAsync_ReturnsValidConfiguration()
    {
        // Act
        var result = await _service.GetDefaultTemplateConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Layout);
        Assert.NotNull(result.Sections);
        Assert.NotEmpty(result.Sections);

        // Check layout defaults
        Assert.Equal("portrait", result.Layout.Orientation);
        Assert.Equal("A4", result.Layout.PageSize);
        Assert.Equal(1, result.Layout.Columns);

        // Check required sections exist
        var sectionTypes = result.Sections.Select(s => s.Type).ToList();
        Assert.Contains("header", sectionTypes);
        Assert.Contains("employee-info", sectionTypes);
        Assert.Contains("earnings", sectionTypes);
        Assert.Contains("deductions", sectionTypes);
        Assert.Contains("summary", sectionTypes);
        Assert.Contains("footer", sectionTypes);

        // Check section ordering
        var orderedSections = result.Sections.OrderBy(s => s.Order).ToList();
        Assert.Equal("header", orderedSections[0].Type);
        Assert.Equal("footer", orderedSections.Last().Type);

        // Check that sections have fields
        var earningsSection = result.Sections.First(s => s.Type == "earnings");
        Assert.NotEmpty(earningsSection.Fields);
        
        var summarySection = result.Sections.First(s => s.Type == "summary");
        Assert.NotEmpty(summarySection.Fields);
    }

    private static PayslipTemplateDto CreateTestTemplate()
    {
        return new PayslipTemplateDto
        {
            Id = 1,
            Name = "Test Template",
            OrganizationId = 1,
            TemplateConfig = new PayslipTemplateConfig
            {
                Layout = new PayslipLayout
                {
                    Orientation = "portrait",
                    PageSize = "A4"
                },
                Sections = new List<PayslipSection>
                {
                    new PayslipSection
                    {
                        Id = "header",
                        Name = "Header",
                        Type = "header",
                        Order = 1,
                        IsVisible = true,
                        Fields = new List<PayslipField>
                        {
                            new PayslipField
                            {
                                Id = "org-name",
                                Name = "Organization Name",
                                DataSource = "organization.name",
                                Format = "text",
                                IsVisible = true
                            }
                        }
                    },
                    new PayslipSection
                    {
                        Id = "employee-info",
                        Name = "Employee Information",
                        Type = "employee-info",
                        Order = 2,
                        IsVisible = true,
                        Fields = new List<PayslipField>
                        {
                            new PayslipField
                            {
                                Id = "emp-name",
                                Name = "Employee Name",
                                DataSource = "employee.name",
                                Format = "text",
                                IsVisible = true
                            }
                        }
                    },
                    new PayslipSection
                    {
                        Id = "summary",
                        Name = "Summary",
                        Type = "summary",
                        Order = 3,
                        IsVisible = true,
                        Fields = new List<PayslipField>
                        {
                            new PayslipField
                            {
                                Id = "net-salary",
                                Name = "Net Salary",
                                DataSource = "salary.net",
                                Format = "currency",
                                IsVisible = true
                            }
                        }
                    }
                }
            },
            HeaderConfig = new PayslipHeaderConfig
            {
                ShowOrganizationLogo = true,
                HeaderText = "Test Organization",
                HeaderColor = "#3b82f6"
            },
            FooterConfig = new PayslipFooterConfig
            {
                FooterText = "This is a computer-generated payslip",
                ShowDigitalSignature = true
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = "#3b82f6",
                SecondaryColor = "#6b7280",
                FontFamily = "Inter",
                FontSize = 12
            }
        };
    }

    private static PayrollCalculationResult CreateTestPayrollData()
    {
        return new PayrollCalculationResult
        {
            EmployeeId = 1,
            EmployeeName = "John Doe",
            BasicSalary = 4000m,
            GrossSalary = 5500m,
            NetSalary = 5000m,
            TotalAllowances = 1000m,
            TotalDeductions = 500m,
            OvertimeAmount = 500m,
            Currency = "USD",
            PayrollMonth = 12,
            PayrollYear = 2024,
            AllowanceBreakdown = new Dictionary<string, decimal>
            {
                ["HRA"] = 600m,
                ["Transport"] = 400m
            },
            DeductionBreakdown = new Dictionary<string, decimal>
            {
                ["Tax"] = 300m,
                ["PF"] = 200m
            }
        };
    }
}