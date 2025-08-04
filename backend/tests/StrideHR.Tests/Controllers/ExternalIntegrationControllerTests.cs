using Microsoft.AspNetCore.Mvc;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Integrations;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ExternalIntegrationControllerTests
{
    private readonly Mock<IExternalIntegrationService> _mockIntegrationService;
    private readonly ExternalIntegrationController _controller;

    public ExternalIntegrationControllerTests()
    {
        _mockIntegrationService = new Mock<IExternalIntegrationService>();
        _controller = new ExternalIntegrationController(_mockIntegrationService.Object);
    }

    #region Payroll System Integration Tests

    [Fact]
    public async Task ConnectPayrollSystem_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;
        var config = new PayrollSystemConfig
        {
            ApiUrl = "https://api.adp.com",
            ApiKey = "test_api_key",
            Username = "test_user",
            Password = "test_password"
        };

        var result = new PayrollIntegrationResult
        {
            Success = true,
            Message = "ADP payroll system connected successfully",
            Integration = new ExternalIntegration
            {
                Id = 1,
                OrganizationId = organizationId,
                Name = "ADP Payroll Integration",
                Type = IntegrationType.Payroll,
                SystemType = systemType.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockIntegrationService.Setup(s => s.ConnectPayrollSystemAsync(organizationId, systemType, config))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ConnectPayrollSystem(organizationId, systemType, config);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task ConnectPayrollSystem_FailedConnection_ReturnsBadRequest()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;
        var config = new PayrollSystemConfig
        {
            ApiUrl = "invalid_url",
            ApiKey = "invalid_key"
        };

        var result = new PayrollIntegrationResult
        {
            Success = false,
            Message = "Failed to validate payroll system connection",
            ErrorCode = "CONNECTION_VALIDATION_FAILED"
        };

        _mockIntegrationService.Setup(s => s.ConnectPayrollSystemAsync(organizationId, systemType, config))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ConnectPayrollSystem(organizationId, systemType, config);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        var responseData = badRequestResult.Value as ApiResponse<object>;
        Assert.False(responseData?.Success);
        Assert.Equal("Failed to validate payroll system connection", responseData?.Message);
    }

    [Fact]
    public async Task DisconnectPayrollSystem_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;

        var result = new PayrollIntegrationResult
        {
            Success = true,
            Message = "ADP payroll system disconnected successfully"
        };

        _mockIntegrationService.Setup(s => s.DisconnectPayrollSystemAsync(organizationId, systemType))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DisconnectPayrollSystem(organizationId, systemType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.Equal("ADP payroll system disconnected successfully", responseData?.Message);
    }

    [Fact]
    public async Task ExportPayrollData_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;
        var request = new PayrollExportRequest
        {
            PayrollPeriodStart = DateTime.UtcNow.AddDays(-30),
            PayrollPeriodEnd = DateTime.UtcNow,
            Format = PayrollExportFormat.Json,
            EmployeeIds = new List<int> { 1, 2, 3 }
        };

        var result = new PayrollExportResult
        {
            Success = true,
            Message = "Payroll data exported successfully",
            ExportedData = "{\"employees\":[{\"id\":1,\"salary\":5000}]}",
            FileName = "payroll_export_20241201_120000.json",
            RecordsExported = 100
        };

        _mockIntegrationService.Setup(s => s.ExportPayrollDataAsync(organizationId, systemType, request))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ExportPayrollData(organizationId, systemType, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task ImportPayrollData_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;
        var request = new PayrollImportRequest
        {
            Data = "{\"employees\":[{\"id\":1,\"salary\":5000}]}",
            Format = PayrollImportFormat.Json,
            ValidateOnly = false
        };

        var result = new PayrollImportResult
        {
            Success = true,
            Message = "Payroll data imported successfully",
            RecordsProcessed = 100,
            RecordsImported = 95,
            RecordsSkipped = 5,
            Warnings = new List<string> { "5 records skipped due to validation errors" }
        };

        _mockIntegrationService.Setup(s => s.ImportPayrollDataAsync(organizationId, systemType, request))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ImportPayrollData(organizationId, systemType, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task ValidatePayrollConnection_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = PayrollSystemType.ADP;

        _mockIntegrationService.Setup(s => s.ValidatePayrollConnectionAsync(organizationId, systemType))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.ValidatePayrollConnection(organizationId, systemType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        // For ValidatePayrollConnection, we need to check the nested valid property
        var data = responseData?.Data as dynamic;
        Assert.True(data?.valid);
    }

    #endregion

    #region Accounting System Integration Tests

    [Fact]
    public async Task ConnectAccountingSystem_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = AccountingSystemType.QuickBooks;
        var config = new AccountingSystemConfig
        {
            ApiUrl = "https://api.quickbooks.com",
            ApiKey = "test_api_key",
            CompanyId = "company123",
            Username = "test_user",
            Password = "test_password"
        };

        var result = new AccountingIntegrationResult
        {
            Success = true,
            Message = "QuickBooks accounting system connected successfully",
            Integration = new ExternalIntegration
            {
                Id = 1,
                OrganizationId = organizationId,
                Name = "QuickBooks Accounting Integration",
                Type = IntegrationType.Accounting,
                SystemType = systemType.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockIntegrationService.Setup(s => s.ConnectAccountingSystemAsync(organizationId, systemType, config))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ConnectAccountingSystem(organizationId, systemType, config);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task ExportAccountingData_ValidData_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var systemType = AccountingSystemType.QuickBooks;
        var request = new AccountingExportRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            DataType = AccountingDataType.Payroll,
            Format = AccountingExportFormat.Json
        };

        var result = new AccountingExportResult
        {
            Success = true,
            Message = "Accounting data exported successfully",
            ExportedData = "{\"accounts\":[{\"name\":\"Salaries\",\"amount\":50000}]}",
            FileName = "accounting_export_20241201_120000.json",
            RecordsExported = 150
        };

        _mockIntegrationService.Setup(s => s.ExportAccountingDataAsync(organizationId, systemType, request))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ExportAccountingData(organizationId, systemType, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    #endregion

    #region Generic Integration Management Tests

    [Fact]
    public async Task GetOrganizationIntegrations_ValidOrganizationId_ReturnsOkResult()
    {
        // Arrange
        var organizationId = 1;
        var integrations = new List<ExternalIntegration>
        {
            new ExternalIntegration
            {
                Id = 1,
                OrganizationId = organizationId,
                Name = "ADP Payroll Integration",
                Type = IntegrationType.Payroll,
                SystemType = "ADP",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ExternalIntegration
            {
                Id = 2,
                OrganizationId = organizationId,
                Name = "QuickBooks Accounting Integration",
                Type = IntegrationType.Accounting,
                SystemType = "QuickBooks",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockIntegrationService.Setup(s => s.GetOrganizationIntegrationsAsync(organizationId))
            .ReturnsAsync(integrations);

        // Act
        var response = await _controller.GetOrganizationIntegrations(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task GetIntegration_ValidId_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var integration = new ExternalIntegration
        {
            Id = integrationId,
            OrganizationId = 1,
            Name = "ADP Payroll Integration",
            Type = IntegrationType.Payroll,
            SystemType = "ADP",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockIntegrationService.Setup(s => s.GetIntegrationAsync(integrationId))
            .ReturnsAsync(integration);

        // Act
        var response = await _controller.GetIntegration(integrationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task GetIntegration_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var integrationId = 999;
        _mockIntegrationService.Setup(s => s.GetIntegrationAsync(integrationId))
            .ThrowsAsync(new ArgumentException($"Integration with ID {integrationId} not found"));

        // Act
        var response = await _controller.GetIntegration(integrationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        var responseData = notFoundResult.Value as ApiResponse<object>;
        Assert.False(responseData?.Success);
    }

    [Fact]
    public async Task UpdateIntegrationConfig_ValidData_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var config = new { apiUrl = "https://new-api.example.com", apiKey = "new_key" };
        var updatedIntegration = new ExternalIntegration
        {
            Id = integrationId,
            OrganizationId = 1,
            Name = "Updated Integration",
            Type = IntegrationType.Payroll,
            SystemType = "ADP",
            Configuration = "{\"apiUrl\":\"https://new-api.example.com\",\"apiKey\":\"new_key\"}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockIntegrationService.Setup(s => s.UpdateIntegrationConfigAsync(integrationId, config))
            .ReturnsAsync(updatedIntegration);

        // Act
        var response = await _controller.UpdateIntegrationConfig(integrationId, config);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task TestIntegrationConnection_ValidId_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        _mockIntegrationService.Setup(s => s.TestIntegrationConnectionAsync(integrationId))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.TestIntegrationConnection(integrationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        // For TestIntegrationConnection, we need to check the nested valid property
        var data = responseData?.Data as dynamic;
        Assert.True(data?.valid);
    }

    [Fact]
    public async Task SyncIntegrationData_ValidData_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var direction = SyncDirection.Export;
        var result = new IntegrationSyncResult
        {
            Success = true,
            Message = "Data synced successfully in Export direction",
            RecordsSynced = 50,
            SyncedAt = DateTime.UtcNow,
            Direction = direction
        };

        _mockIntegrationService.Setup(s => s.SyncIntegrationDataAsync(integrationId, direction))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.SyncIntegrationData(integrationId, direction);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task GetIntegrationLogs_ValidData_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var logs = new List<IntegrationLog>
        {
            new IntegrationLog
            {
                Id = 1,
                ExternalIntegrationId = integrationId,
                Operation = "Export",
                Status = "Success",
                Duration = TimeSpan.FromSeconds(2),
                CreatedAt = DateTime.UtcNow
            },
            new IntegrationLog
            {
                Id = 2,
                ExternalIntegrationId = integrationId,
                Operation = "Import",
                Status = "Failed",
                ErrorMessage = "Connection timeout",
                Duration = TimeSpan.FromSeconds(30),
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockIntegrationService.Setup(s => s.GetIntegrationLogsAsync(integrationId, startDate, endDate))
            .ReturnsAsync(logs);

        // Act
        var response = await _controller.GetIntegrationLogs(integrationId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task GetIntegrationHealth_ValidId_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var health = new IntegrationHealthStatus
        {
            IsHealthy = true,
            Status = "Active",
            LastChecked = DateTime.UtcNow,
            LastSuccessfulSync = DateTime.UtcNow.AddHours(-1),
            ConsecutiveFailures = 0,
            Issues = new List<string>()
        };

        _mockIntegrationService.Setup(s => s.GetIntegrationHealthAsync(integrationId))
            .ReturnsAsync(health);

        // Act
        var response = await _controller.GetIntegrationHealth(integrationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var responseData = okResult.Value as ApiResponse<object>;
        Assert.True(responseData?.Success);
        Assert.NotNull(responseData?.Data);
    }

    [Fact]
    public async Task GetIntegrationMetrics_ValidData_ReturnsOkResult()
    {
        // Arrange
        var integrationId = 1;
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var metrics = new IntegrationMetrics
        {
            TotalOperations = 100,
            SuccessfulOperations = 95,
            FailedOperations = 5,
            SuccessRate = 95.0,
            AverageResponseTime = TimeSpan.FromSeconds(2.5),
            StartDate = startDate,
            EndDate = endDate,
            OperationCounts = new Dictionary<string, int>
            {
                { "Export", 60 },
                { "Import", 40 }
            }
        };

        _mockIntegrationService.Setup(s => s.GetIntegrationMetricsAsync(integrationId, startDate, endDate))
            .ReturnsAsync(metrics);

        // Act
        var response = await _controller.GetIntegrationMetrics(integrationId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.NotNull(okResult.Value);
        
        // Use dynamic to handle the generic ApiResponse type
        dynamic responseData = okResult.Value;
        Assert.True(responseData.Success);
        Assert.NotNull(responseData.Data);
    }

    #endregion
}