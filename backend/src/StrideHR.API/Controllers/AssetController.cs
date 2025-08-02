using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class AssetController : BaseController
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    /// <summary>
    /// Get all assets with search and filtering
    /// </summary>
    [HttpGet]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssets([FromQuery] AssetSearchCriteria criteria)
    {
        try
        {
            var assets = await _assetService.SearchAssetsAsync(criteria);
            var totalCount = await _assetService.GetTotalAssetsCountAsync(criteria);

            var result = new
            {
                Assets = assets,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };

            return Success(result, "Assets retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assets: {ex.Message}");
        }
    }

    /// <summary>
    /// Get asset by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAsset(int id)
    {
        try
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Success(asset, "Asset retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Get asset by asset tag
    /// </summary>
    [HttpGet("by-tag/{assetTag}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssetByTag(string assetTag)
    {
        try
        {
            var asset = await _assetService.GetAssetByTagAsync(assetTag);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Success(asset, "Asset retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assets by branch
    /// </summary>
    [HttpGet("branch/{branchId}")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssetsByBranch(int branchId)
    {
        try
        {
            var assets = await _assetService.GetAssetsByBranchAsync(branchId);
            return Success(assets, "Branch assets retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve branch assets: {ex.Message}");
        }
    }

    /// <summary>
    /// Create new asset
    /// </summary>
    [HttpPost]
    [RequirePermission("Asset.Create")]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetDto createAssetDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asset = await _assetService.CreateAssetAsync(createAssetDto);
            return Success(asset, "Asset created successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to create asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Update asset
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("Asset.Update")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetDto updateAssetDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asset = await _assetService.UpdateAssetAsync(id, updateAssetDto);
            return Success(asset, "Asset updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to update asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete asset
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("Asset.Delete")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        try
        {
            var result = await _assetService.DeleteAssetAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Success("Asset deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error($"Failed to delete asset: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if asset tag is unique
    /// </summary>
    [HttpGet("check-tag-unique")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> CheckAssetTagUnique([FromQuery] string assetTag, [FromQuery] int? excludeId = null)
    {
        try
        {
            var isUnique = await _assetService.IsAssetTagUniqueAsync(assetTag, excludeId);
            return Success(new { IsUnique = isUnique }, "Asset tag uniqueness checked");
        }
        catch (Exception ex)
        {
            return Error($"Failed to check asset tag uniqueness: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assets requiring maintenance
    /// </summary>
    [HttpGet("requiring-maintenance")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssetsRequiringMaintenance()
    {
        try
        {
            var assets = await _assetService.GetAssetsRequiringMaintenanceAsync();
            return Success(assets, "Assets requiring maintenance retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assets requiring maintenance: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assets under warranty
    /// </summary>
    [HttpGet("under-warranty")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetAssetsUnderWarranty()
    {
        try
        {
            var assets = await _assetService.GetAssetsUnderWarrantyAsync();
            return Success(assets, "Assets under warranty retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assets under warranty: {ex.Message}");
        }
    }

    /// <summary>
    /// Get assets with expired warranty
    /// </summary>
    [HttpGet("expired-warranty")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GetExpiredWarrantyAssets()
    {
        try
        {
            var assets = await _assetService.GetExpiredWarrantyAssetsAsync();
            return Success(assets, "Assets with expired warranty retrieved successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to retrieve assets with expired warranty: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate asset report
    /// </summary>
    [HttpGet("report")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> GenerateAssetReport([FromQuery] int? branchId = null)
    {
        try
        {
            var report = await _assetService.GenerateAssetReportAsync(branchId);
            return Success(report, "Asset report generated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to generate asset report: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculate current value of asset
    /// </summary>
    [HttpGet("{id}/current-value")]
    [RequirePermission("Asset.Read")]
    public async Task<IActionResult> CalculateCurrentValue(int id)
    {
        try
        {
            var currentValue = await _assetService.CalculateCurrentValueAsync(id);
            return Success(new { CurrentValue = currentValue }, "Current value calculated successfully");
        }
        catch (Exception ex)
        {
            return Error($"Failed to calculate current value: {ex.Message}");
        }
    }
}