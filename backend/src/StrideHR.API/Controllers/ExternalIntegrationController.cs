using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Integrations;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for managing external system integrations (Payroll, Accounting)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExternalIntegrationController : BaseController
{
    private readonly IExternalIntegrationService _integrationService;

    public ExternalIntegrationController(IExternalIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    #region Payroll System Integrations

    /// <summary>
    /// Connect payroll system
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Payroll system type</param>
    /// <param name="config">System configuration</param>
    /// <returns>Integration result</returns>
    [HttpPost("payroll/connect")]
    [Authorize(Policy = "Permission:Integration.Payroll.Connect")]
    public async Task<IActionResult> ConnectPayrollSystem([FromQuery] int organizationId, [FromQuery] PayrollSystemType systemType, [FromBody] PayrollSystemConfig config)
    {
        try
        {
            var result = await _integrationService.ConnectPayrollSystemAsync(organizationId, systemType, config);
            if (result.Success)
                return Success(result, "Payroll system connected successfully");
            else
                return Error(result.Message, new List<string> { result.ErrorCode });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Disconnect payroll system
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Payroll system type</param>
    /// <returns>Integration result</returns>
    [HttpPost("payroll/disconnect")]
    [Authorize(Policy = "Permission:Integration.Payroll.Disconnect")]
    public async Task<IActionResult> DisconnectPayrollSystem([FromQuery] int organizationId, [FromQuery] PayrollSystemType systemType)
    {
        try
        {
            var result = await _integrationService.DisconnectPayrollSystemAsync(organizationId, systemType);
            if (result.Success)
                return Success(result.Message);
            else
                return Error(result.Message, new List<string> { result.ErrorCode });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Export payroll data
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Payroll system type</param>
    /// <param name="request">Export request</param>
    /// <returns>Export result</returns>
    [HttpPost("payroll/export")]
    [Authorize(Policy = "Permission:Integration.Payroll.Export")]
    public async Task<IActionResult> ExportPayrollData([FromQuery] int organizationId, [FromQuery] PayrollSystemType systemType, [FromBody] PayrollExportRequest request)
    {
        try
        {
            var result = await _integrationService.ExportPayrollDataAsync(organizationId, systemType, request);
            if (result.Success)
                return Success(result, "Payroll data exported successfully");
            else
                return Error(result.Message, result.Errors);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Import payroll data
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Payroll system type</param>
    /// <param name="request">Import request</param>
    /// <returns>Import result</returns>
    [HttpPost("payroll/import")]
    [Authorize(Policy = "Permission:Integration.Payroll.Import")]
    public async Task<IActionResult> ImportPayrollData([FromQuery] int organizationId, [FromQuery] PayrollSystemType systemType, [FromBody] PayrollImportRequest request)
    {
        try
        {
            var result = await _integrationService.ImportPayrollDataAsync(organizationId, systemType, request);
            if (result.Success)
                return Success(result, "Payroll data imported successfully");
            else
                return Error(result.Message, result.Errors);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Validate payroll system connection
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Payroll system type</param>
    /// <returns>Validation result</returns>
    [HttpPost("payroll/validate")]
    [Authorize(Policy = "Permission:Integration.Payroll.Validate")]
    public async Task<IActionResult> ValidatePayrollConnection([FromQuery] int organizationId, [FromQuery] PayrollSystemType systemType)
    {
        try
        {
            var isValid = await _integrationService.ValidatePayrollConnectionAsync(organizationId, systemType);
            return Success(new { valid = isValid }, "Payroll connection validation completed");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Accounting System Integrations

    /// <summary>
    /// Connect accounting system
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Accounting system type</param>
    /// <param name="config">System configuration</param>
    /// <returns>Integration result</returns>
    [HttpPost("accounting/connect")]
    [Authorize(Policy = "Permission:Integration.Accounting.Connect")]
    public async Task<IActionResult> ConnectAccountingSystem([FromQuery] int organizationId, [FromQuery] AccountingSystemType systemType, [FromBody] AccountingSystemConfig config)
    {
        try
        {
            var result = await _integrationService.ConnectAccountingSystemAsync(organizationId, systemType, config);
            if (result.Success)
                return Success(result, "Accounting system connected successfully");
            else
                return Error(result.Message, new List<string> { result.ErrorCode });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Disconnect accounting system
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Accounting system type</param>
    /// <returns>Integration result</returns>
    [HttpPost("accounting/disconnect")]
    [Authorize(Policy = "Permission:Integration.Accounting.Disconnect")]
    public async Task<IActionResult> DisconnectAccountingSystem([FromQuery] int organizationId, [FromQuery] AccountingSystemType systemType)
    {
        try
        {
            var result = await _integrationService.DisconnectAccountingSystemAsync(organizationId, systemType);
            if (result.Success)
                return Success(result.Message);
            else
                return Error(result.Message, new List<string> { result.ErrorCode });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Export accounting data
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Accounting system type</param>
    /// <param name="request">Export request</param>
    /// <returns>Export result</returns>
    [HttpPost("accounting/export")]
    [Authorize(Policy = "Permission:Integration.Accounting.Export")]
    public async Task<IActionResult> ExportAccountingData([FromQuery] int organizationId, [FromQuery] AccountingSystemType systemType, [FromBody] AccountingExportRequest request)
    {
        try
        {
            var result = await _integrationService.ExportAccountingDataAsync(organizationId, systemType, request);
            if (result.Success)
                return Success(result, "Accounting data exported successfully");
            else
                return Error(result.Message, result.Errors);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Import accounting data
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Accounting system type</param>
    /// <param name="request">Import request</param>
    /// <returns>Import result</returns>
    [HttpPost("accounting/import")]
    [Authorize(Policy = "Permission:Integration.Accounting.Import")]
    public async Task<IActionResult> ImportAccountingData([FromQuery] int organizationId, [FromQuery] AccountingSystemType systemType, [FromBody] AccountingImportRequest request)
    {
        try
        {
            var result = await _integrationService.ImportAccountingDataAsync(organizationId, systemType, request);
            if (result.Success)
                return Success(result, "Accounting data imported successfully");
            else
                return Error(result.Message, result.Errors);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Validate accounting system connection
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="systemType">Accounting system type</param>
    /// <returns>Validation result</returns>
    [HttpPost("accounting/validate")]
    [Authorize(Policy = "Permission:Integration.Accounting.Validate")]
    public async Task<IActionResult> ValidateAccountingConnection([FromQuery] int organizationId, [FromQuery] AccountingSystemType systemType)
    {
        try
        {
            var isValid = await _integrationService.ValidateAccountingConnectionAsync(organizationId, systemType);
            return Success(new { valid = isValid }, "Accounting connection validation completed");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Generic Integration Management

    /// <summary>
    /// Get all integrations for organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <returns>List of integrations</returns>
    [HttpGet("organization/{organizationId}")]
    [Authorize(Policy = "Permission:Integration.View")]
    public async Task<IActionResult> GetOrganizationIntegrations(int organizationId)
    {
        try
        {
            var integrations = await _integrationService.GetOrganizationIntegrationsAsync(organizationId);
            return Success(integrations);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get integration by ID
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <returns>Integration details</returns>
    [HttpGet("{integrationId}")]
    [Authorize(Policy = "Permission:Integration.View")]
    public async Task<IActionResult> GetIntegration(int integrationId)
    {
        try
        {
            var integration = await _integrationService.GetIntegrationAsync(integrationId);
            return Success(integration);
        }
        catch (ArgumentException ex)
        {
            return NotFoundError(ex.Message);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Update integration configuration
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="config">New configuration</param>
    /// <returns>Updated integration</returns>
    [HttpPut("{integrationId}/config")]
    [Authorize(Policy = "Permission:Integration.Update")]
    public async Task<IActionResult> UpdateIntegrationConfig(int integrationId, [FromBody] object config)
    {
        try
        {
            var integration = await _integrationService.UpdateIntegrationConfigAsync(integrationId, config);
            return Success(integration, "Integration configuration updated successfully");
        }
        catch (ArgumentException ex)
        {
            return NotFoundError(ex.Message);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Test integration connection
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <returns>Test result</returns>
    [HttpPost("{integrationId}/test")]
    [Authorize(Policy = "Permission:Integration.Test")]
    public async Task<IActionResult> TestIntegrationConnection(int integrationId)
    {
        try
        {
            var isValid = await _integrationService.TestIntegrationConnectionAsync(integrationId);
            return Success(new { valid = isValid }, "Integration connection test completed");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Sync integration data
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="direction">Sync direction</param>
    /// <returns>Sync result</returns>
    [HttpPost("{integrationId}/sync")]
    [Authorize(Policy = "Permission:Integration.Sync")]
    public async Task<IActionResult> SyncIntegrationData(int integrationId, [FromQuery] SyncDirection direction)
    {
        try
        {
            var result = await _integrationService.SyncIntegrationDataAsync(integrationId, direction);
            if (result.Success)
                return Success(result, "Integration data synchronized successfully");
            else
                return Error(result.Message, result.Errors);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Integration Logs and Monitoring

    /// <summary>
    /// Get integration logs
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <returns>List of integration logs</returns>
    [HttpGet("{integrationId}/logs")]
    [Authorize(Policy = "Permission:Integration.ViewLogs")]
    public async Task<IActionResult> GetIntegrationLogs(int integrationId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var logs = await _integrationService.GetIntegrationLogsAsync(integrationId, startDate, endDate);
            return Success(logs);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get integration health status
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <returns>Health status</returns>
    [HttpGet("{integrationId}/health")]
    [Authorize(Policy = "Permission:Integration.ViewHealth")]
    public async Task<IActionResult> GetIntegrationHealth(int integrationId)
    {
        try
        {
            var health = await _integrationService.GetIntegrationHealthAsync(integrationId);
            return Success(health);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    /// <summary>
    /// Get integration metrics
    /// </summary>
    /// <param name="integrationId">Integration ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Integration metrics</returns>
    [HttpGet("{integrationId}/metrics")]
    [Authorize(Policy = "Permission:Integration.ViewMetrics")]
    public async Task<IActionResult> GetIntegrationMetrics(int integrationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var metrics = await _integrationService.GetIntegrationMetricsAsync(integrationId, startDate, endDate);
            return Success(metrics);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion
}