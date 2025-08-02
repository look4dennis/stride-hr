using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AssetHandoverController : BaseController
{
    private readonly IAssetHandoverService _handoverService;

    public AssetHandoverController(IAssetHandoverService handoverService)
    {
        _handoverService = handoverService;
    }

    /// <summary>
    /// Initiate asset handover
    /// </summary>
    [HttpPost]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> InitiateHandover([FromBody] CreateAssetHandoverDto handoverDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var handover = await _handoverService.InitiateHandoverAsync(handoverDto);
            return Success(handover, "Asset handover initiated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to initiate asset handover: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete asset handover
    /// </summary>
    [HttpPost("{id}/complete")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> CompleteHandover(int id, [FromBody] CompleteAssetHandoverDto completionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var handover = await _handoverService.CompleteHandoverAsync(id, completionDto);
            return Success(handover, "Asset handover completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to complete asset handover: {ex.Message}");
        }
    }

    /// <summary>
    /// Approve asset handover
    /// </summary>
    [HttpPost("{id}/approve")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> ApproveHandover(int id, [FromBody] ApproveAssetHandoverDto approvalDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var handover = await _handoverService.ApproveHandoverAsync(id, approvalDto);
            return Success(handover, "Asset handover approved successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to approve asset handover: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handover by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandover(int id)
    {
        try
        {
            var handover = await _handoverService.GetHandoverByIdAsync(id);
            if (handover == null)
            {
                return NotFound(new { message = "Handover not found" });
            }

            return Success(handover, "Handover retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve handover: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handovers by employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandoversByEmployee(int employeeId)
    {
        try
        {
            var handovers = await _handoverService.GetHandoversByEmployeeIdAsync(employeeId);
            return Success(handovers, "Employee handovers retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve employee handovers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handovers by asset
    /// </summary>
    [HttpGet("asset/{assetId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandoversByAsset(int assetId)
    {
        try
        {
            var handovers = await _handoverService.GetHandoversByAssetIdAsync(assetId);
            return Success(handovers, "Asset handovers retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve asset handovers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handovers by status
    /// </summary>
    [HttpGet("status/{status}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandoversByStatus(HandoverStatus status)
    {
        try
        {
            var handovers = await _handoverService.GetHandoversByStatusAsync(status);
            return Success(handovers, $"Handovers with status {status} retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve handovers by status: {ex.Message}");
        }
    }

    /// <summary>
    /// Get pending handovers
    /// </summary>
    [HttpGet("pending")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetPendingHandovers()
    {
        try
        {
            var handovers = await _handoverService.GetPendingHandoversAsync();
            return Success(handovers, "Pending handovers retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve pending handovers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get overdue handovers
    /// </summary>
    [HttpGet("overdue")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetOverdueHandovers()
    {
        try
        {
            var handovers = await _handoverService.GetOverdueHandoversAsync();
            return Success(handovers, "Overdue handovers retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve overdue handovers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handovers by employee exit
    /// </summary>
    [HttpGet("employee-exit/{employeeExitId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandoversByEmployeeExit(int employeeExitId)
    {
        try
        {
            var handovers = await _handoverService.GetHandoversByEmployeeExitAsync(employeeExitId);
            return Success(handovers, "Employee exit handovers retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve employee exit handovers: {ex.Message}");
        }
    }

    /// <summary>
    /// Get handovers requiring approval
    /// </summary>
    [HttpGet("requiring-approval")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetHandoversRequiringApproval()
    {
        try
        {
            var handovers = await _handoverService.GetHandoversRequiringApprovalAsync();
            return Success(handovers, "Handovers requiring approval retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve handovers requiring approval: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancel handover
    /// </summary>
    [HttpPost("{id}/cancel")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> CancelHandover(int id, [FromBody] int cancelledBy)
    {
        try
        {
            var result = await _handoverService.CancelHandoverAsync(id, cancelledBy);
            if (!result)
            {
                return NotFound(new { message = "Handover not found" });
            }

            return Success("Handover cancelled successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to cancel handover: {ex.Message}");
        }
    }

    /// <summary>
    /// Initiate handovers for employee exit
    /// </summary>
    [HttpPost("employee-exit")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> InitiateEmployeeExitHandovers([FromBody] EmployeeExitHandoverRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var handovers = await _handoverService.InitiateEmployeeExitHandoversAsync(
                request.EmployeeId, 
                request.EmployeeExitId, 
                request.InitiatedBy);

            return Success(handovers, "Employee exit handovers initiated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to initiate employee exit handovers: {ex.Message}");
        }
    }
}

public class EmployeeExitHandoverRequest
{
    public int EmployeeId { get; set; }
    public int EmployeeExitId { get; set; }
    public int InitiatedBy { get; set; }
}
