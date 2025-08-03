using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;
using System.Text;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class DataImportExportControllerTests
{
    private readonly Mock<IDataImportExportService> _mockDataImportExportService;
    private readonly Mock<ILogger<DataImportExportController>> _mockLogger;
    private readonly DataImportExportController _controller;

    public DataImportExportControllerTests()
    {
        _mockDataImportExportService = new Mock<IDataImportExportService>();
        _mockLogger = new Mock<ILogger<DataImportExportController>>();
        _controller = new DataImportExportController(_mockDataImportExportService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateImportData_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.xlsx", "test content");
        var apiRequest = new ImportRequestApiDto
        {
            EntityType = "Employee",
            File = mockFile,
            BranchId = 1
        };

        var validationResult = new ValidationResultDto
        {
            IsValid = true,
            TotalRecords = 10,
            ValidRecords = 10,
            InvalidRecords = 0
        };

        _mockDataImportExportService
            .Setup(x => x.ValidateImportDataAsync(It.IsAny<ImportRequestDto>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.ValidateImportData(apiRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ValidationResultDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(validationResult, response.Data);
    }

    [Fact]
    public async Task ValidateImportData_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var apiRequest = new ImportRequestApiDto
        {
            EntityType = "Employee",
            File = null!
        };

        // Act
        var result = await _controller.ValidateImportData(apiRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ValidationResultDto>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("No file provided", response.Message);
    }

    [Fact]
    public async Task ImportData_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.xlsx", "test content");
        var apiRequest = new ImportRequestApiDto
        {
            EntityType = "Employee",
            File = mockFile,
            BranchId = 1
        };

        var importResult = new ImportResultDto
        {
            Success = true,
            TotalRecords = 10,
            SuccessfulRecords = 10,
            FailedRecords = 0,
            Message = "Import completed successfully"
        };

        _mockDataImportExportService
            .Setup(x => x.ImportDataAsync(It.IsAny<ImportRequestDto>()))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportData(apiRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ImportResultDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(importResult, response.Data);
    }

    [Fact]
    public async Task ImportEmployees_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var mockFile = CreateMockFormFile("employees.xlsx", "test content");
        var apiRequest = new ImportRequestApiDto
        {
            File = mockFile,
            BranchId = 1
        };

        var importResult = new ImportResultDto
        {
            Success = true,
            TotalRecords = 5,
            SuccessfulRecords = 5,
            FailedRecords = 0,
            Message = "Employee import completed successfully"
        };

        _mockDataImportExportService
            .Setup(x => x.ImportEmployeesAsync(It.IsAny<ImportRequestDto>()))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.ImportEmployees(apiRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ImportResultDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(importResult, response.Data);
    }

    [Fact]
    public async Task ExportData_WithValidRequest_ReturnsFile()
    {
        // Arrange
        var exportRequest = new ExportRequestDto
        {
            EntityType = "Employee",
            Format = Core.Enums.ReportExportFormat.Excel
        };

        var exportResult = new ExportResultDto
        {
            Success = true,
            FileContent = Encoding.UTF8.GetBytes("test file content"),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = "employees.xlsx"
        };

        _mockDataImportExportService
            .Setup(x => x.ExportDataAsync(It.IsAny<ExportRequestDto>()))
            .ReturnsAsync(exportResult);

        // Act
        var result = await _controller.ExportData(exportRequest);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(exportResult.ContentType, fileResult.ContentType);
        Assert.Equal(exportResult.FileName, fileResult.FileDownloadName);
        Assert.Equal(exportResult.FileContent, fileResult.FileContents);
    }

    [Fact]
    public async Task GenerateImportTemplate_WithValidEntityType_ReturnsFile()
    {
        // Arrange
        var entityType = "Employee";
        var templateContent = Encoding.UTF8.GetBytes("template content");

        _mockDataImportExportService
            .Setup(x => x.GenerateImportTemplateAsync(entityType))
            .ReturnsAsync(templateContent);

        // Act
        var result = await _controller.GenerateImportTemplate(entityType);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal($"{entityType}_Import_Template.xlsx", fileResult.FileDownloadName);
        Assert.Equal(templateContent, fileResult.FileContents);
    }

    [Fact]
    public async Task GetSupportedEntityTypes_ReturnsSuccess()
    {
        // Arrange
        var entityTypes = new List<string> { "Employee", "AttendanceRecord", "LeaveRequest" };

        _mockDataImportExportService
            .Setup(x => x.GetSupportedEntityTypesAsync())
            .ReturnsAsync(entityTypes);

        // Act
        var result = await _controller.GetSupportedEntityTypes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<string>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(entityTypes, response.Data);
    }

    [Fact]
    public async Task GetEntityFieldMappings_WithValidEntityType_ReturnsSuccess()
    {
        // Arrange
        var entityType = "Employee";
        var fieldMappings = new Dictionary<string, string>
        {
            { "FirstName", "FirstName" },
            { "LastName", "LastName" },
            { "Email", "Email" }
        };

        _mockDataImportExportService
            .Setup(x => x.GetEntityFieldMappingsAsync(entityType))
            .ReturnsAsync(fieldMappings);

        // Act
        var result = await _controller.GetEntityFieldMappings(entityType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Dictionary<string, string>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(fieldMappings, response.Data);
    }

    private static IFormFile CreateMockFormFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(bytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        
        return mockFile.Object;
    }
}