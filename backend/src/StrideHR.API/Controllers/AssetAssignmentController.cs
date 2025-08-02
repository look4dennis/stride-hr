using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AssetAssignmentController : BaseController
{
    private readonly IAssetAssignmentService _assignmentService;

    public AssetAssignmentController(IAssetAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// Assign asset to employee
    /// </summary>
    [HttpPost("assign-to-employee")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> AssignAssetToEmployee([FromBody] CreateAssetAssignmentDto assignmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _assignmentService.AssignAssetToEmployeeAsync(assignmentDto);
            return Success(assignment, "Asset assigned to employee successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to assign asset to employee: {ex.Message}");
        }
    }

    /// <summary>
    /// Assign asset to project
    /// </summary>
    [HttpPost("assign-to-project")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> AssignAssetToProject([FromBody] CreateAssetAssignmentDto assignmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _assignmentService.AssignAssetToProjectAsync(assignmentDto);
            return Success(assignment, "Asset assigned to project successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to assign asset to project: {ex.Message}");
        }
    }

    /// <summary>
    /// Return asset
    /// </summary>
    [HttpPost("{assignmentId}/return")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> ReturnAsset(int assignmentId, [FromBody] ReturnAssetDto returnDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _assignmentService.ReturnAssetAsync(assignmentId, returnDto);
            return Success(assignment, "Asset returned successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to return asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssignment(int id)
    {
        try
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound(new { message = "Assignment not found" });
            }

            return Success(assignment, "Assignment retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assignment: {ex.Message}");
        }
    }

    /// <summary>
    /// Get active assignment by asset ID
    /// </summary>
    [HttpGet("asset/{assetId}/active")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetActiveAssignmentByAsset(int assetId)
    {
        try
        {
            var assignment = await _assignmentService.GetActiveAssignmentByAssetIdAsync(assetId);
            if (assignment == null)
            {
                return NotFound(new { message = "No active assignment found for this asset" });
            }

            return Success(assignment, "Active assignment retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve active assignment: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assignments by employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssignmentsByEmployee(int employeeId)
    {
        try
        {
            var assignments = await _assignmentService.GetAssignmentsByEmployeeIdAsync(employeeId);
            return Success(assignments, "Employee assignments retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve employee assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assignments by project
    /// </summary>
    [HttpGet("project/{projectId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssignmentsByProject(int projectId)
    {
        try
        {
            var assignments = await _assignmentService.GetAssignmentsByProjectIdAsync(projectId);
            return Success(assignments, "Project assignments retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve project assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assignment history by asset
    /// </summary>
    [HttpGet("asset/{assetId}/history")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssignmentHistoryByAsset(int assetId)
    {
        try
        {
            var assignments = await _assignmentService.GetAssignmentHistoryByAssetIdAsync(assetId);
            return Success(assignments, "Assignment history retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assignment history: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all active assignments
    /// </summary>
    [HttpGet("active")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetActiveAssignments()
    {
        try
        {
            var assignments = await _assignmentService.GetActiveAssignmentsAsync();
            return Success(assignments, "Active assignments retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve active assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Get overdue returns
    /// </summary>
    [HttpGet("overdue-returns")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetOverdueReturns()
    {
        try
        {
            var assignments = await _assignmentService.GetOverdueReturnsAsync();
            return Success(assignments, "Overdue returns retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve overdue returns: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if asset can be assigned
    /// </summary>
    [HttpGet("asset/{assetId}/can-assign")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> CanAssignAsset(int assetId)
    {
        try
        {
            var canAssign = await _assignmentService.CanAssignAssetAsync(assetId);
            return Success(new { CanAssign = canAssign }, "Asset assignment eligibility checked");
        }
        catch (Exception ex)
        {
            return Error($"Failed to check asset assignment eligibility: {ex.Message}");
        }
    }
}
