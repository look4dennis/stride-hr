using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AssetMaintenanceController : BaseController
{
    private readonly IAssetMaintenanceService _maintenanceService;

    public AssetMaintenanceController(IAssetMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>
    /// Create maintenance record
    /// </summary>
    [HttpPost]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> CreateMaintenance([FromBody] CreateAssetMaintenanceDto maintenanceDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var maintenance = await _maintenanceService.CreateMaintenanceAsync(maintenanceDto);
            return Success(maintenance, "Maintenance record created successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to create maintenance record: {ex.Message}");
        }
    }

    /// <summary>
    /// Update maintenance record
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> UpdateMaintenance(int id, [FromBody] UpdateAssetMaintenanceDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var maintenance = await _maintenanceService.UpdateMaintenanceAsync(id, updateDto);
            return Success(maintenance, "Maintenance record updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to update maintenance record: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance record by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenance(int id)
    {
        try
        {
            var maintenance = await _maintenanceService.GetMaintenanceByIdAsync(id);
            if (maintenance == null)
            {
                return NotFound(new { message = "Maintenance record not found" });
            }

            return Success(maintenance, "Maintenance record retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve maintenance record: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance records by asset
    /// </summary>
    [HttpGet("asset/{assetId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceByAsset(int assetId)
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceByAssetIdAsync(assetId);
            return Success(maintenanceRecords, "Asset maintenance records retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve asset maintenance records: {ex.Message}");
        }
    }

    /// <summary>
    /// Get scheduled maintenance
    /// </summary>
    [HttpGet("scheduled")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetScheduledMaintenance()
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetScheduledMaintenanceAsync();
            return Success(maintenanceRecords, "Scheduled maintenance retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve scheduled maintenance: {ex.Message}");
        }
    }

    /// <summary>
    /// Get overdue maintenance
    /// </summary>
    [HttpGet("overdue")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetOverdueMaintenance()
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetOverdueMaintenanceAsync();
            return Success(maintenanceRecords, "Overdue maintenance retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve overdue maintenance: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance by status
    /// </summary>
    [HttpGet("status/{status}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceByStatus(MaintenanceStatus status)
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceByStatusAsync(status);
            return Success(maintenanceRecords, $"Maintenance records with status {status} retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve maintenance records by status: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance by technician
    /// </summary>
    [HttpGet("technician/{technicianId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceByTechnician(int technicianId)
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceByTechnicianAsync(technicianId);
            return Success(maintenanceRecords, "Technician maintenance records retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve technician maintenance records: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance by date range
    /// </summary>
    [HttpGet("date-range")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceByDateRangeAsync(startDate, endDate);
            return Success(maintenanceRecords, "Maintenance records for date range retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve maintenance records by date range: {ex.Message}");
        }
    }

    /// <summary>
    /// Start maintenance
    /// </summary>
    [HttpPost("{id}/start")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> StartMaintenance(int id, [FromBody] int technicianId)
    {
        try
        {
            var maintenance = await _maintenanceService.StartMaintenanceAsync(id, technicianId);
            return Success(maintenance, "Maintenance started successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to start maintenance: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete maintenance
    /// </summary>
    [HttpPost("{id}/complete")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> CompleteMaintenance(int id, [FromBody] UpdateAssetMaintenanceDto completionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var maintenance = await _maintenanceService.CompleteMaintenanceAsync(id, completionDto);
            return Success(maintenance, "Maintenance completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to complete maintenance: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete maintenance record
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("Asset.Delete")]
    public async Task<IActionResult> DeleteMaintenance(int id)
    {
        try
        {
            var result = await _maintenanceService.DeleteMaintenanceAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Maintenance record not found" });
            }

            return Success("Maintenance record deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to delete maintenance record: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance cost by asset
    /// </summary>
    [HttpGet("asset/{assetId}/cost")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceCostByAsset(int assetId)
    {
        try
        {
            var cost = await _maintenanceService.GetMaintenanceCostByAssetAsync(assetId);
            return Success(new { TotalCost = cost }, "Asset maintenance cost retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve asset maintenance cost: {ex.Message}");
        }
    }

    /// <summary>
    /// Get maintenance cost by period
    /// </summary>
    [HttpGet("cost/period")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetMaintenanceCostByPeriod(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int? branchId = null)
    {
        try
        {
            var cost = await _maintenanceService.GetMaintenanceCostByPeriodAsync(startDate, endDate, branchId);
            return Success(new { TotalCost = cost, StartDate = startDate, EndDate = endDate, BranchId = branchId }, 
                "Maintenance cost for period retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve maintenance cost by period: {ex.Message}");
        }
    }
}
