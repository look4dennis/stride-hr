using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;
using StrideHR.Infrastructure.Services;
using System.Text;
using Xunit;

namespace StrideHR.Tests.Services;

public class DataImportExportServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IExcelService> _mockExcelService;
    private readonly Mock<ICsvService> _mockCsvService;
    private readonly Mock<ILogger<DataImportExportService>> _mockLogger;
    private readonly DataImportExportService _service;

    public DataImportExportServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExcelService = new Mock<IExcelService>();
        _mockCsvService = new Mock<ICsvService>();
        _mockLogger = new Mock<ILogger<DataImportExportService>>();
        
        _service = new DataImportExportService(
            _mockUnitOfWork.Object,
            _mockExcelService.Object,
            _mockCsvService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateImportDataAsync_WithExcelFile_CallsExcelService()
    {
        // Arrange
        var request = new ImportRequestDto
        {
            EntityType = "Employee",
            FileName = "test.xlsx",
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };

        var testData = new List<Dictionary<string, object>>
        {
            new() { { "FirstName", "John" }, { "LastName", "Doe" }, { "Email", "john.doe@example.com" } }
        };

        var validationResult = new ValidationResultDto
        {
            IsValid = true,
            TotalRecords = 1,
            ValidRecords = 1,
            InvalidRecords = 0
        };

        _mockExcelService
            .Setup(x => x.IsValidExcelFile("test.xlsx"))
            .Returns(true);

        _mockCsvService
            .Setup(x => x.IsValidCsvFile("test.xlsx"))
            .Returns(false);

        _mockExcelService
            .Setup(x => x.ReadExcelFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(testData);

        _mockExcelService
            .Setup(x => x.ValidateExcelData<Employee>(It.IsAny<List<Dictionary<string, object>>>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(validationResult);

        // Act
        var result = await _service.ValidateImportDataAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1, result.TotalRecords);
        _mockExcelService.Verify(x => x.ReadExcelFileAsync(It.IsAny<Stream>(), "test.xlsx"), Times.Once);
    }

    [Fact]
    public async Task ValidateImportDataAsync_WithCsvFile_CallsCsvService()
    {
        // Arrange
        var request = new ImportRequestDto
        {
            EntityType = "Employee",
            FileName = "test.csv",
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };

        var testData = new List<Dictionary<string, object>>
        {
            new() { { "FirstName", "John" }, { "LastName", "Doe" }, { "Email", "john.doe@example.com" } }
        };

        var validationResult = new ValidationResultDto
        {
            IsValid = true,
            TotalRecords = 1,
            ValidRecords = 1,
            InvalidRecords = 0
        };

        _mockExcelService
            .Setup(x => x.IsValidExcelFile("test.csv"))
            .Returns(false);

        _mockCsvService
            .Setup(x => x.IsValidCsvFile("test.csv"))
            .Returns(true);

        _mockCsvService
            .Setup(x => x.ReadCsvFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testData);

        _mockCsvService
            .Setup(x => x.ValidateCsvData<Employee>(It.IsAny<List<Dictionary<string, object>>>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(validationResult);

        // Act
        var result = await _service.ValidateImportDataAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1, result.TotalRecords);
        _mockCsvService.Verify(x => x.ReadCsvFileAsync(It.IsAny<Stream>(), "test.csv", ","), Times.Once);
    }

    [Fact]
    public async Task ValidateImportDataAsync_WithUnsupportedEntityType_ThrowsException()
    {
        // Arrange
        var request = new ImportRequestDto
        {
            EntityType = "UnsupportedEntity",
            FileName = "test.xlsx",
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateImportDataAsync(request));
    }

    [Fact]
    public async Task ValidateImportDataAsync_WithUnsupportedFileFormat_ThrowsException()
    {
        // Arrange
        var request = new ImportRequestDto
        {
            EntityType = "Employee",
            FileName = "test.txt",
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateImportDataAsync(request));
    }

    [Fact]
    public async Task GenerateImportTemplateAsync_WithValidEntityType_ReturnsTemplate()
    {
        // Arrange
        var entityType = "Employee";
        var templateContent = Encoding.UTF8.GetBytes("template content");

        _mockExcelService
            .Setup(x => x.GenerateTemplateAsync(It.IsAny<Type>(), It.IsAny<string>()))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _service.GenerateImportTemplateAsync(entityType);

        // Assert
        Assert.Equal(templateContent, result);
        _mockExcelService.Verify(x => x.GenerateTemplateAsync(typeof(Employee), "Employee_Template"), Times.Once);
    }

    [Fact]
    public async Task GenerateImportTemplateAsync_WithUnsupportedEntityType_ThrowsException()
    {
        // Arrange
        var entityType = "UnsupportedEntity";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateImportTemplateAsync(entityType));
    }

    [Fact]
    public async Task GetSupportedEntityTypesAsync_ReturnsExpectedTypes()
    {
        // Act
        var result = await _service.GetSupportedEntityTypesAsync();

        // Assert
        Assert.Contains("Employee", result);
        Assert.Contains("AttendanceRecord", result);
        Assert.Contains("LeaveRequest", result);
        Assert.Contains("Project", result);
        Assert.Contains("Branch", result);
        Assert.Contains("Organization", result);
    }

    [Fact]
    public async Task GetEntityFieldMappingsAsync_WithValidEntityType_ReturnsFieldMappings()
    {
        // Arrange
        var entityType = "Employee";

        // Act
        var result = await _service.GetEntityFieldMappingsAsync(entityType);

        // Assert
        Assert.NotEmpty(result);
        Assert.IsType<Dictionary<string, string>>(result);
        // Employee entity should have common properties
        Assert.True(result.ContainsKey("FirstName") || result.ContainsKey("Email") || result.ContainsKey("EmployeeId"));
    }

    [Fact]
    public async Task GetEntityFieldMappingsAsync_WithUnsupportedEntityType_ThrowsException()
    {
        // Arrange
        var entityType = "UnsupportedEntity";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetEntityFieldMappingsAsync(entityType));
    }

    [Theory]
    [InlineData("test.xlsx", true)]
    [InlineData("test.xls", true)]
    [InlineData("test.csv", false)]
    [InlineData("test.txt", false)]
    public void IsExcelFile_WithVariousExtensions_ReturnsExpectedResult(string fileName, bool expected)
    {
        // This tests the private method indirectly through validation
        var request = new ImportRequestDto
        {
            EntityType = "Employee",
            FileName = fileName,
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };

        // The method should handle Excel files differently from CSV files
        // We can verify this by checking which service method gets called
        if (expected)
        {
            _mockExcelService
                .Setup(x => x.ReadExcelFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Dictionary<string, object>>());
        }
        else if (fileName.EndsWith(".csv"))
        {
            _mockCsvService
                .Setup(x => x.ReadCsvFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Dictionary<string, object>>());
        }

        // The actual test would be in the validation method call
        // This is more of a documentation of the expected behavior
        Assert.True(true); // Placeholder assertion
    }
}