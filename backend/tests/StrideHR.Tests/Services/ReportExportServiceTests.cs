using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ReportExportServiceTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly Mock<IReportBuilderService> _mockReportBuilderService;
    private readonly Mock<ILogger<ReportExportService>> _mockLogger;
    private readonly ReportExportService _service;

    public ReportExportServiceTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _mockReportBuilderService = new Mock<IReportBuilderService>();
        _mockLogger = new Mock<ILogger<ReportExportService>>();
        
        _service = new ReportExportService(
            _mockReportRepository.Object,
            _mockReportBuilderService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetExportMimeTypeAsync_PDF_ReturnsCorrectMimeType()
    {
        // Act
        var result = await _service.GetExportMimeTypeAsync(ReportExportFormat.PDF);

        // Assert
        Assert.Equal("application/pdf", result);
    }

    [Fact]
    public async Task GetExportMimeTypeAsync_Excel_ReturnsCorrectMimeType()
    {
        // Act
        var result = await _service.GetExportMimeTypeAsync(ReportExportFormat.Excel);

        // Assert
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result);
    }

    [Fact]
    public async Task GetExportMimeTypeAsync_CSV_ReturnsCorrectMimeType()
    {
        // Act
        var result = await _service.GetExportMimeTypeAsync(ReportExportFormat.CSV);

        // Assert
        Assert.Equal("text/csv", result);
    }

    [Fact]
    public async Task GetExportMimeTypeAsync_JSON_ReturnsCorrectMimeType()
    {
        // Act
        var result = await _service.GetExportMimeTypeAsync(ReportExportFormat.JSON);

        // Assert
        Assert.Equal("application/json", result);
    }

    [Fact]
    public async Task GetExportFileExtensionAsync_PDF_ReturnsCorrectExtension()
    {
        // Act
        var result = await _service.GetExportFileExtensionAsync(ReportExportFormat.PDF);

        // Assert
        Assert.Equal(".pdf", result);
    }

    [Fact]
    public async Task GetExportFileExtensionAsync_Excel_ReturnsCorrectExtension()
    {
        // Act
        var result = await _service.GetExportFileExtensionAsync(ReportExportFormat.Excel);

        // Assert
        Assert.Equal(".xlsx", result);
    }

    [Fact]
    public async Task GetExportFileExtensionAsync_CSV_ReturnsCorrectExtension()
    {
        // Act
        var result = await _service.GetExportFileExtensionAsync(ReportExportFormat.CSV);

        // Assert
        Assert.Equal(".csv", result);
    }

    [Fact]
    public async Task ExportReportDataAsync_CSV_ReturnsValidCsvData()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>
            {
                new() { ["Id"] = 1, ["Name"] = "John Doe", ["Email"] = "john@example.com" },
                new() { ["Id"] = 2, ["Name"] = "Jane Smith", ["Email"] = "jane@example.com" }
            },
            TotalRecords = 2,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, ReportExportFormat.CSV, "Test Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var csvContent = System.Text.Encoding.UTF8.GetString(result);
        Assert.Contains("Id", csvContent);
        Assert.Contains("Name", csvContent);
        Assert.Contains("Email", csvContent);
        Assert.Contains("John Doe", csvContent);
        Assert.Contains("Jane Smith", csvContent);
    }

    [Fact]
    public async Task ExportReportDataAsync_JSON_ReturnsValidJsonData()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>
            {
                new() { ["Id"] = 1, ["Name"] = "John Doe" },
                new() { ["Id"] = 2, ["Name"] = "Jane Smith" }
            },
            TotalRecords = 2,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, ReportExportFormat.JSON, "Test Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var jsonContent = System.Text.Encoding.UTF8.GetString(result);
        Assert.Contains("Test Report", jsonContent);
        Assert.Contains("TotalRecords", jsonContent);
        Assert.Contains("ExecutionTime", jsonContent);
        Assert.Contains("John Doe", jsonContent);
        Assert.Contains("Jane Smith", jsonContent);
    }

    [Fact]
    public async Task ExportReportDataAsync_XML_ReturnsValidXmlData()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>
            {
                new() { ["Id"] = 1, ["Name"] = "John Doe" },
                new() { ["Id"] = 2, ["Name"] = "Jane Smith" }
            },
            TotalRecords = 2,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, ReportExportFormat.XML, "Test Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var xmlContent = System.Text.Encoding.UTF8.GetString(result);
        Assert.Contains("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", xmlContent);
        Assert.Contains("<Report name=\"Test Report\"", xmlContent);
        Assert.Contains("<Summary totalRecords=\"2\"", xmlContent);
        Assert.Contains("<Data>", xmlContent);
        Assert.Contains("<Record>", xmlContent);
        Assert.Contains("<Id>1</Id>", xmlContent);
        Assert.Contains("<Name>John Doe</Name>", xmlContent);
    }

    [Fact]
    public async Task ExportReportDataAsync_HTML_ReturnsValidHtmlData()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>
            {
                new() { ["Id"] = 1, ["Name"] = "John Doe" },
                new() { ["Id"] = 2, ["Name"] = "Jane Smith" }
            },
            TotalRecords = 2,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, ReportExportFormat.HTML, "Test Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var htmlContent = System.Text.Encoding.UTF8.GetString(result);
        Assert.Contains("<!DOCTYPE html>", htmlContent);
        Assert.Contains("<title>Test Report</title>", htmlContent);
        Assert.Contains("<h1>Test Report</h1>", htmlContent);
        Assert.Contains("<table>", htmlContent);
        Assert.Contains("<th>Id</th>", htmlContent);
        Assert.Contains("<th>Name</th>", htmlContent);
        Assert.Contains("<td>John Doe</td>", htmlContent);
        Assert.Contains("<td>Jane Smith</td>", htmlContent);
    }

    [Fact]
    public async Task ExportReportDataAsync_UnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>(),
            TotalRecords = 0,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ExportReportDataAsync(reportData, (ReportExportFormat)999, "Test Report"));
    }

    [Fact]
    public async Task ExportReportDataAsync_EmptyData_ReturnsValidOutput()
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>(),
            TotalRecords = 0,
            ExecutionTime = TimeSpan.FromMilliseconds(50)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, ReportExportFormat.CSV, "Empty Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var csvContent = System.Text.Encoding.UTF8.GetString(result);
        // Should still have some content even with empty data
        Assert.NotEmpty(csvContent);
    }

    [Theory]
    [InlineData(ReportExportFormat.PDF)]
    [InlineData(ReportExportFormat.Excel)]
    public async Task ExportReportDataAsync_BinaryFormats_ReturnsNonEmptyData(ReportExportFormat format)
    {
        // Arrange
        var reportData = new ReportExecutionResult
        {
            Success = true,
            Data = new List<Dictionary<string, object>>
            {
                new() { ["Id"] = 1, ["Name"] = "Test" }
            },
            TotalRecords = 1,
            ExecutionTime = TimeSpan.FromMilliseconds(100)
        };

        // Act
        var result = await _service.ExportReportDataAsync(reportData, format, "Test Report");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}