using StrideHR.Core.Models.Integrations;

namespace StrideHR.Core.Interfaces.Services;

public interface IExternalIntegrationService
{
    // Payroll System Integrations
    Task<PayrollIntegrationResult> ConnectPayrollSystemAsync(int organizationId, PayrollSystemType systemType, PayrollSystemConfig config);
    Task<PayrollIntegrationResult> DisconnectPayrollSystemAsync(int organizationId, PayrollSystemType systemType);
    Task<PayrollExportResult> ExportPayrollDataAsync(int organizationId, PayrollSystemType systemType, PayrollExportRequest request);
    Task<PayrollImportResult> ImportPayrollDataAsync(int organizationId, PayrollSystemType systemType, PayrollImportRequest request);
    Task<bool> ValidatePayrollConnectionAsync(int organizationId, PayrollSystemType systemType);
    
    // Accounting System Integrations
    Task<AccountingIntegrationResult> ConnectAccountingSystemAsync(int organizationId, AccountingSystemType systemType, AccountingSystemConfig config);
    Task<AccountingIntegrationResult> DisconnectAccountingSystemAsync(int organizationId, AccountingSystemType systemType);
    Task<AccountingExportResult> ExportAccountingDataAsync(int organizationId, AccountingSystemType systemType, AccountingExportRequest request);
    Task<AccountingImportResult> ImportAccountingDataAsync(int organizationId, AccountingSystemType systemType, AccountingImportRequest request);
    Task<bool> ValidateAccountingConnectionAsync(int organizationId, AccountingSystemType systemType);
    
    // Generic Integration Management
    Task<List<ExternalIntegration>> GetOrganizationIntegrationsAsync(int organizationId);
    Task<ExternalIntegration> GetIntegrationAsync(int integrationId);
    Task<ExternalIntegration> UpdateIntegrationConfigAsync(int integrationId, object config);
    Task<bool> TestIntegrationConnectionAsync(int integrationId);
    Task<IntegrationSyncResult> SyncIntegrationDataAsync(int integrationId, SyncDirection direction);
    
    // Integration Logs and Monitoring
    Task<List<IntegrationLog>> GetIntegrationLogsAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IntegrationHealthStatus> GetIntegrationHealthAsync(int integrationId);
    Task<IntegrationMetrics> GetIntegrationMetricsAsync(int integrationId, DateTime startDate, DateTime endDate);
}