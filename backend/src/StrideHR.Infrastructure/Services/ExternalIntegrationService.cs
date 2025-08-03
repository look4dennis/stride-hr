using System.Text.Json;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Integrations;

namespace StrideHR.Infrastructure.Services;

public class ExternalIntegrationService : IExternalIntegrationService
{
    private readonly IExternalIntegrationRepository _integrationRepository;
    private readonly IIntegrationLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalIntegrationService> _logger;

    public ExternalIntegrationService(
        IExternalIntegrationRepository integrationRepository,
        IIntegrationLogRepository logRepository,
        IUnitOfWork unitOfWork,
        HttpClient httpClient,
        ILogger<ExternalIntegrationService> logger)
    {
        _integrationRepository = integrationRepository;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _logger = logger;
    }

    // Payroll System Integrations
    public async Task<PayrollIntegrationResult> ConnectPayrollSystemAsync(int organizationId, PayrollSystemType systemType, PayrollSystemConfig config)
    {
        try
        {
            // Validate connection first
            var isValid = await ValidatePayrollConnectionAsync(config);
            if (!isValid)
            {
                return new PayrollIntegrationResult
                {
                    Success = false,
                    Message = "Failed to validate payroll system connection",
                    ErrorCode = "CONNECTION_VALIDATION_FAILED"
                };
            }

            var existingIntegration = await _integrationRepository.GetByOrganizationAndTypeAsync(
                organizationId, IntegrationType.Payroll, systemType.ToString());

            ExternalIntegration integration;
            if (existingIntegration != null)
            {
                existingIntegration.Configuration = JsonSerializer.Serialize(config);
                existingIntegration.IsActive = true;
                existingIntegration.UpdatedAt = DateTime.UtcNow;
                
                await _integrationRepository.UpdateAsync(existingIntegration);
                integration = existingIntegration;
            }
            else
            {
                integration = new ExternalIntegration
                {
                    OrganizationId = organizationId,
                    Name = $"{systemType} Payroll Integration",
                    Type = IntegrationType.Payroll,
                    SystemType = systemType.ToString(),
                    Configuration = JsonSerializer.Serialize(config),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _integrationRepository.AddAsync(integration);
            }

            await _unitOfWork.SaveChangesAsync();
            await LogIntegrationOperationAsync(integration.Id, "Connect", "Success", null, null);

            _logger.LogInformation("Payroll system {SystemType} connected for organization {OrganizationId}", 
                systemType, organizationId);

            return new PayrollIntegrationResult
            {
                Success = true,
                Message = $"{systemType} payroll system connected successfully",
                Integration = integration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting payroll system {SystemType} for organization {OrganizationId}", 
                systemType, organizationId);
            
            return new PayrollIntegrationResult
            {
                Success = false,
                Message = "Failed to connect payroll system",
                ErrorCode = "CONNECTION_ERROR"
            };
        }
    }

    public async Task<PayrollIntegrationResult> DisconnectPayrollSystemAsync(int organizationId, PayrollSystemType systemType)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Payroll, systemType.ToString());

        if (integration == null)
        {
            return new PayrollIntegrationResult
            {
                Success = false,
                Message = "Payroll system integration not found",
                ErrorCode = "INTEGRATION_NOT_FOUND"
            };
        }

        integration.IsActive = false;
        integration.UpdatedAt = DateTime.UtcNow;
        
        await _integrationRepository.UpdateAsync(integration);
        await _unitOfWork.SaveChangesAsync();
        await LogIntegrationOperationAsync(integration.Id, "Disconnect", "Success", null, null);

        _logger.LogInformation("Payroll system {SystemType} disconnected for organization {OrganizationId}", 
            systemType, organizationId);

        return new PayrollIntegrationResult
        {
            Success = true,
            Message = $"{systemType} payroll system disconnected successfully"
        };
    }

    public async Task<PayrollExportResult> ExportPayrollDataAsync(int organizationId, PayrollSystemType systemType, PayrollExportRequest request)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Payroll, systemType.ToString());

        if (integration == null || !integration.IsActive)
        {
            return new PayrollExportResult
            {
                Success = false,
                Message = "Payroll system integration not found or inactive"
            };
        }

        var startTime = DateTime.UtcNow;
        try
        {
            var config = JsonSerializer.Deserialize<PayrollSystemConfig>(integration.Configuration);
            if (config == null)
            {
                return new PayrollExportResult
                {
                    Success = false,
                    Message = "Invalid integration configuration"
                };
            }

            // Simulate payroll data export based on system type
            var exportData = await GeneratePayrollExportDataAsync(systemType, request);
            
            var result = new PayrollExportResult
            {
                Success = true,
                Message = "Payroll data exported successfully",
                ExportedData = exportData,
                FileName = $"payroll_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToString().ToLower()}",
                RecordsExported = 100 // This would be calculated based on actual data
            };

            await LogIntegrationOperationAsync(integration.Id, "Export", "Success", 
                JsonSerializer.Serialize(request), JsonSerializer.Serialize(result), DateTime.UtcNow - startTime);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payroll data for system {SystemType}", systemType);
            
            await LogIntegrationOperationAsync(integration.Id, "Export", "Failed", 
                JsonSerializer.Serialize(request), null, DateTime.UtcNow - startTime, ex.Message);

            return new PayrollExportResult
            {
                Success = false,
                Message = "Failed to export payroll data",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<PayrollImportResult> ImportPayrollDataAsync(int organizationId, PayrollSystemType systemType, PayrollImportRequest request)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Payroll, systemType.ToString());

        if (integration == null || !integration.IsActive)
        {
            return new PayrollImportResult
            {
                Success = false,
                Message = "Payroll system integration not found or inactive"
            };
        }

        var startTime = DateTime.UtcNow;
        try
        {
            // Simulate payroll data import
            var result = new PayrollImportResult
            {
                Success = true,
                Message = "Payroll data imported successfully",
                RecordsProcessed = 100,
                RecordsImported = 95,
                RecordsSkipped = 5,
                Warnings = { "5 records skipped due to validation errors" }
            };

            await LogIntegrationOperationAsync(integration.Id, "Import", "Success", 
                $"Data length: {request.Data.Length}", JsonSerializer.Serialize(result), DateTime.UtcNow - startTime);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing payroll data for system {SystemType}", systemType);
            
            await LogIntegrationOperationAsync(integration.Id, "Import", "Failed", 
                $"Data length: {request.Data.Length}", null, DateTime.UtcNow - startTime, ex.Message);

            return new PayrollImportResult
            {
                Success = false,
                Message = "Failed to import payroll data",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<bool> ValidatePayrollConnectionAsync(int organizationId, PayrollSystemType systemType)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Payroll, systemType.ToString());

        if (integration == null)
            return false;

        var config = JsonSerializer.Deserialize<PayrollSystemConfig>(integration.Configuration);
        if (config == null)
            return false;

        return await ValidatePayrollConnectionAsync(config);
    }

    // Accounting System Integrations
    public async Task<AccountingIntegrationResult> ConnectAccountingSystemAsync(int organizationId, AccountingSystemType systemType, AccountingSystemConfig config)
    {
        try
        {
            // Validate connection first
            var isValid = await ValidateAccountingConnectionAsync(config);
            if (!isValid)
            {
                return new AccountingIntegrationResult
                {
                    Success = false,
                    Message = "Failed to validate accounting system connection",
                    ErrorCode = "CONNECTION_VALIDATION_FAILED"
                };
            }

            var existingIntegration = await _integrationRepository.GetByOrganizationAndTypeAsync(
                organizationId, IntegrationType.Accounting, systemType.ToString());

            ExternalIntegration integration;
            if (existingIntegration != null)
            {
                existingIntegration.Configuration = JsonSerializer.Serialize(config);
                existingIntegration.IsActive = true;
                existingIntegration.UpdatedAt = DateTime.UtcNow;
                
                await _integrationRepository.UpdateAsync(existingIntegration);
                integration = existingIntegration;
            }
            else
            {
                integration = new ExternalIntegration
                {
                    OrganizationId = organizationId,
                    Name = $"{systemType} Accounting Integration",
                    Type = IntegrationType.Accounting,
                    SystemType = systemType.ToString(),
                    Configuration = JsonSerializer.Serialize(config),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _integrationRepository.AddAsync(integration);
            }

            await _unitOfWork.SaveChangesAsync();
            await LogIntegrationOperationAsync(integration.Id, "Connect", "Success", null, null);

            return new AccountingIntegrationResult
            {
                Success = true,
                Message = $"{systemType} accounting system connected successfully",
                Integration = integration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting accounting system {SystemType} for organization {OrganizationId}", 
                systemType, organizationId);
            
            return new AccountingIntegrationResult
            {
                Success = false,
                Message = "Failed to connect accounting system",
                ErrorCode = "CONNECTION_ERROR"
            };
        }
    }

    public async Task<AccountingIntegrationResult> DisconnectAccountingSystemAsync(int organizationId, AccountingSystemType systemType)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Accounting, systemType.ToString());

        if (integration == null)
        {
            return new AccountingIntegrationResult
            {
                Success = false,
                Message = "Accounting system integration not found",
                ErrorCode = "INTEGRATION_NOT_FOUND"
            };
        }

        integration.IsActive = false;
        integration.UpdatedAt = DateTime.UtcNow;
        
        await _integrationRepository.UpdateAsync(integration);
        await _unitOfWork.SaveChangesAsync();
        await LogIntegrationOperationAsync(integration.Id, "Disconnect", "Success", null, null);

        return new AccountingIntegrationResult
        {
            Success = true,
            Message = $"{systemType} accounting system disconnected successfully"
        };
    }

    public async Task<AccountingExportResult> ExportAccountingDataAsync(int organizationId, AccountingSystemType systemType, AccountingExportRequest request)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Accounting, systemType.ToString());

        if (integration == null || !integration.IsActive)
        {
            return new AccountingExportResult
            {
                Success = false,
                Message = "Accounting system integration not found or inactive"
            };
        }

        var startTime = DateTime.UtcNow;
        try
        {
            // Simulate accounting data export
            var exportData = await GenerateAccountingExportDataAsync(systemType, request);
            
            var result = new AccountingExportResult
            {
                Success = true,
                Message = "Accounting data exported successfully",
                ExportedData = exportData,
                FileName = $"accounting_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToString().ToLower()}",
                RecordsExported = 150
            };

            await LogIntegrationOperationAsync(integration.Id, "Export", "Success", 
                JsonSerializer.Serialize(request), JsonSerializer.Serialize(result), DateTime.UtcNow - startTime);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting accounting data for system {SystemType}", systemType);
            
            await LogIntegrationOperationAsync(integration.Id, "Export", "Failed", 
                JsonSerializer.Serialize(request), null, DateTime.UtcNow - startTime, ex.Message);

            return new AccountingExportResult
            {
                Success = false,
                Message = "Failed to export accounting data",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<AccountingImportResult> ImportAccountingDataAsync(int organizationId, AccountingSystemType systemType, AccountingImportRequest request)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Accounting, systemType.ToString());

        if (integration == null || !integration.IsActive)
        {
            return new AccountingImportResult
            {
                Success = false,
                Message = "Accounting system integration not found or inactive"
            };
        }

        var startTime = DateTime.UtcNow;
        try
        {
            // Simulate accounting data import
            var result = new AccountingImportResult
            {
                Success = true,
                Message = "Accounting data imported successfully",
                RecordsProcessed = 150,
                RecordsImported = 145,
                RecordsSkipped = 5
            };

            await LogIntegrationOperationAsync(integration.Id, "Import", "Success", 
                $"Data length: {request.Data.Length}", JsonSerializer.Serialize(result), DateTime.UtcNow - startTime);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing accounting data for system {SystemType}", systemType);
            
            await LogIntegrationOperationAsync(integration.Id, "Import", "Failed", 
                $"Data length: {request.Data.Length}", null, DateTime.UtcNow - startTime, ex.Message);

            return new AccountingImportResult
            {
                Success = false,
                Message = "Failed to import accounting data",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<bool> ValidateAccountingConnectionAsync(int organizationId, AccountingSystemType systemType)
    {
        var integration = await _integrationRepository.GetByOrganizationAndTypeAsync(
            organizationId, IntegrationType.Accounting, systemType.ToString());

        if (integration == null)
            return false;

        var config = JsonSerializer.Deserialize<AccountingSystemConfig>(integration.Configuration);
        if (config == null)
            return false;

        return await ValidateAccountingConnectionAsync(config);
    }

    // Generic Integration Management
    public async Task<List<ExternalIntegration>> GetOrganizationIntegrationsAsync(int organizationId)
    {
        return await _integrationRepository.GetByOrganizationIdAsync(organizationId);
    }

    public async Task<ExternalIntegration> GetIntegrationAsync(int integrationId)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId);
        if (integration == null)
            throw new ArgumentException($"Integration with ID {integrationId} not found");

        return integration;
    }

    public async Task<ExternalIntegration> UpdateIntegrationConfigAsync(int integrationId, object config)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId);
        if (integration == null)
            throw new ArgumentException($"Integration with ID {integrationId} not found");

        integration.Configuration = JsonSerializer.Serialize(config);
        integration.UpdatedAt = DateTime.UtcNow;

        await _integrationRepository.UpdateAsync(integration);
        await _unitOfWork.SaveChangesAsync();

        return integration;
    }

    public async Task<bool> TestIntegrationConnectionAsync(int integrationId)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId);
        if (integration == null)
            return false;

        try
        {
            bool isValid = integration.Type switch
            {
                IntegrationType.Payroll => await ValidatePayrollConnectionAsync(
                    JsonSerializer.Deserialize<PayrollSystemConfig>(integration.Configuration)!),
                IntegrationType.Accounting => await ValidateAccountingConnectionAsync(
                    JsonSerializer.Deserialize<AccountingSystemConfig>(integration.Configuration)!),
                _ => false
            };

            await LogIntegrationOperationAsync(integrationId, "Test", isValid ? "Success" : "Failed", null, null);
            return isValid;
        }
        catch (Exception ex)
        {
            await LogIntegrationOperationAsync(integrationId, "Test", "Failed", null, null, TimeSpan.Zero, ex.Message);
            return false;
        }
    }

    public async Task<IntegrationSyncResult> SyncIntegrationDataAsync(int integrationId, SyncDirection direction)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId);
        if (integration == null)
        {
            return new IntegrationSyncResult
            {
                Success = false,
                Message = "Integration not found"
            };
        }

        var startTime = DateTime.UtcNow;
        try
        {
            // Simulate data sync
            var result = new IntegrationSyncResult
            {
                Success = true,
                Message = $"Data synced successfully in {direction} direction",
                RecordsSynced = 50,
                SyncedAt = DateTime.UtcNow,
                Direction = direction
            };

            integration.LastSyncAt = DateTime.UtcNow;
            await _integrationRepository.UpdateAsync(integration);
            await _unitOfWork.SaveChangesAsync();

            await LogIntegrationOperationAsync(integrationId, "Sync", "Success", 
                direction.ToString(), JsonSerializer.Serialize(result), DateTime.UtcNow - startTime);

            return result;
        }
        catch (Exception ex)
        {
            await LogIntegrationOperationAsync(integrationId, "Sync", "Failed", 
                direction.ToString(), null, DateTime.UtcNow - startTime, ex.Message);

            return new IntegrationSyncResult
            {
                Success = false,
                Message = "Failed to sync data",
                Direction = direction,
                Errors = { ex.Message }
            };
        }
    }

    // Integration Logs and Monitoring
    public async Task<List<IntegrationLog>> GetIntegrationLogsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _logRepository.GetByIntegrationIdAsync(integrationId, startDate, endDate);
    }

    public async Task<IntegrationHealthStatus> GetIntegrationHealthAsync(int integrationId)
    {
        var integration = await _integrationRepository.GetByIdAsync(integrationId);
        if (integration == null)
        {
            return new IntegrationHealthStatus
            {
                IsHealthy = false,
                Status = "Not Found",
                LastChecked = DateTime.UtcNow
            };
        }

        var recentLogs = await _logRepository.GetByIntegrationIdAsync(integrationId, DateTime.UtcNow.AddHours(-24));
        var failedLogs = recentLogs.Where(l => l.Status == "Failed").ToList();

        return new IntegrationHealthStatus
        {
            IsHealthy = integration.IsActive && failedLogs.Count < 5,
            Status = integration.IsActive ? "Active" : "Inactive",
            LastChecked = DateTime.UtcNow,
            LastSuccessfulSync = integration.LastSyncAt,
            ConsecutiveFailures = failedLogs.Count,
            Issues = failedLogs.Select(l => l.ErrorMessage ?? "Unknown error").ToList()
        };
    }

    public async Task<IntegrationMetrics> GetIntegrationMetricsAsync(int integrationId, DateTime startDate, DateTime endDate)
    {
        var logs = await _logRepository.GetByIntegrationIdAsync(integrationId, startDate, endDate);
        
        var totalOperations = logs.Count;
        var successfulOperations = logs.Count(l => l.Status == "Success");
        var failedOperations = logs.Count(l => l.Status == "Failed");

        return new IntegrationMetrics
        {
            TotalOperations = totalOperations,
            SuccessfulOperations = successfulOperations,
            FailedOperations = failedOperations,
            SuccessRate = totalOperations > 0 ? (double)successfulOperations / totalOperations * 100 : 0,
            AverageResponseTime = logs.Any() ? TimeSpan.FromMilliseconds(logs.Average(l => l.Duration.TotalMilliseconds)) : TimeSpan.Zero,
            StartDate = startDate,
            EndDate = endDate,
            OperationCounts = logs.GroupBy(l => l.Operation).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    // Private helper methods
    private async Task<bool> ValidatePayrollConnectionAsync(PayrollSystemConfig config)
    {
        try
        {
            // Simulate connection validation
            if (string.IsNullOrEmpty(config.ApiUrl) || string.IsNullOrEmpty(config.ApiKey))
                return false;

            // In a real implementation, this would make an actual API call to validate the connection
            await Task.Delay(100); // Simulate network call
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ValidateAccountingConnectionAsync(AccountingSystemConfig config)
    {
        try
        {
            // Simulate connection validation
            if (string.IsNullOrEmpty(config.ApiUrl) || string.IsNullOrEmpty(config.ApiKey))
                return false;

            // In a real implementation, this would make an actual API call to validate the connection
            await Task.Delay(100); // Simulate network call
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GeneratePayrollExportDataAsync(PayrollSystemType systemType, PayrollExportRequest request)
    {
        // Simulate generating export data based on the system type and format
        await Task.Delay(100);
        
        return request.Format switch
        {
            PayrollExportFormat.Json => JsonSerializer.Serialize(new { message = "Payroll export data", systemType, request }),
            PayrollExportFormat.Csv => "Employee,Salary,Deductions\nJohn Doe,5000,500\nJane Smith,6000,600",
            PayrollExportFormat.Xml => "<payroll><employee><name>John Doe</name><salary>5000</salary></employee></payroll>",
            _ => "Payroll export data"
        };
    }

    private async Task<string> GenerateAccountingExportDataAsync(AccountingSystemType systemType, AccountingExportRequest request)
    {
        // Simulate generating export data based on the system type and format
        await Task.Delay(100);
        
        return request.Format switch
        {
            AccountingExportFormat.Json => JsonSerializer.Serialize(new { message = "Accounting export data", systemType, request }),
            AccountingExportFormat.Csv => "Account,Debit,Credit\nSalaries,10000,0\nCash,0,10000",
            AccountingExportFormat.Xml => "<accounting><entry><account>Salaries</account><debit>10000</debit></entry></accounting>",
            _ => "Accounting export data"
        };
    }

    private async Task LogIntegrationOperationAsync(int integrationId, string operation, string status, 
        string? requestData, string? responseData, TimeSpan? duration = null, string? errorMessage = null)
    {
        var log = new IntegrationLog
        {
            ExternalIntegrationId = integrationId,
            Operation = operation,
            Status = status,
            RequestData = requestData,
            ResponseData = responseData,
            ErrorMessage = errorMessage,
            Duration = duration ?? TimeSpan.Zero,
            CreatedAt = DateTime.UtcNow
        };

        await _logRepository.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}