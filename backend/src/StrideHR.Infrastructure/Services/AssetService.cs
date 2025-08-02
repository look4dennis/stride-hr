using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Infrastructure.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;

    public AssetService(
        IAssetRepository assetRepository,
        IAssetAssignmentRepository assignmentRepository,
        IMapper mapper)
    {
        _assetRepository = assetRepository;
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetDto createAssetDto)
    {
        // Validate asset tag uniqueness
        if (!await _assetRepository.IsAssetTagUniqueAsync(createAssetDto.AssetTag))
        {
            throw new InvalidOperationException($"Asset tag '{createAssetDto.AssetTag}' already exists.");
        }

        var asset = _mapper.Map<Asset>(createAssetDto);
        asset.CreatedAt = DateTime.UtcNow;
        
        // Calculate current value based on depreciation if applicable
        if (asset.PurchasePrice.HasValue && asset.DepreciationRate.HasValue && asset.PurchaseDate.HasValue)
        {
            asset.CurrentValue = CalculateDepreciatedValue(
                asset.PurchasePrice.Value, 
                asset.DepreciationRate.Value, 
                asset.PurchaseDate.Value);
        }
        else
        {
            asset.CurrentValue = asset.PurchasePrice;
        }

        await _assetRepository.AddAsync(asset);
        await _assetRepository.SaveChangesAsync();

        return await GetAssetByIdAsync(asset.Id) ?? throw new InvalidOperationException("Failed to retrieve created asset.");
    }

    public async Task<AssetDto> UpdateAssetAsync(int id, UpdateAssetDto updateAssetDto)
    {
        var existingAsset = await _assetRepository.GetByIdAsync(id);
        if (existingAsset == null || existingAsset.IsDeleted)
        {
            throw new InvalidOperationException($"Asset with ID {id} not found.");
        }

        _mapper.Map(updateAssetDto, existingAsset);
        existingAsset.UpdatedAt = DateTime.UtcNow;

        // Recalculate current value if depreciation parameters changed
        if (existingAsset.PurchasePrice.HasValue && existingAsset.DepreciationRate.HasValue && existingAsset.PurchaseDate.HasValue)
        {
            existingAsset.CurrentValue = CalculateDepreciatedValue(
                existingAsset.PurchasePrice.Value, 
                existingAsset.DepreciationRate.Value, 
                existingAsset.PurchaseDate.Value);
        }

        await _assetRepository.UpdateAsync(existingAsset);
        await _assetRepository.SaveChangesAsync();

        return await GetAssetByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated asset.");
    }

    public async Task<AssetDto?> GetAssetByIdAsync(int id)
    {
        var asset = await _assetRepository.GetAssetWithDetailsAsync(id);
        return asset != null ? _mapper.Map<AssetDto>(asset) : null;
    }

    public async Task<AssetDto?> GetAssetByTagAsync(string assetTag)
    {
        var asset = await _assetRepository.GetByAssetTagAsync(assetTag);
        return asset != null ? _mapper.Map<AssetDto>(asset) : null;
    }

    public async Task<IEnumerable<AssetDto>> SearchAssetsAsync(AssetSearchCriteria criteria)
    {
        var assets = await _assetRepository.SearchAssetsAsync(criteria);
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }

    public async Task<int> GetTotalAssetsCountAsync(AssetSearchCriteria criteria)
    {
        return await _assetRepository.GetTotalAssetsCountAsync(criteria);
    }

    public async Task<IEnumerable<AssetDto>> GetAssetsByBranchAsync(int branchId)
    {
        var assets = await _assetRepository.GetAssetsByBranchAsync(branchId);
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null || asset.IsDeleted)
        {
            return false;
        }

        // Check if asset has active assignments
        if (await _assignmentRepository.HasActiveAssignmentAsync(id))
        {
            throw new InvalidOperationException("Cannot delete asset that has active assignments.");
        }

        // Soft delete
        asset.IsDeleted = true;
        asset.DeletedAt = DateTime.UtcNow;
        asset.Status = AssetStatus.Retired;

        await _assetRepository.UpdateAsync(asset);
        return await _assetRepository.SaveChangesAsync();
    }

    public async Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null)
    {
        return await _assetRepository.IsAssetTagUniqueAsync(assetTag, excludeId);
    }

    public async Task<AssetReportDto> GenerateAssetReportAsync(int? branchId = null)
    {
        return await _assetRepository.GenerateAssetReportAsync(branchId);
    }

    public async Task<IEnumerable<AssetDto>> GetAssetsRequiringMaintenanceAsync()
    {
        var assets = await _assetRepository.GetAssetsRequiringMaintenanceAsync();
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }

    public async Task<IEnumerable<AssetDto>> GetAssetsUnderWarrantyAsync()
    {
        var assets = await _assetRepository.GetAssetsUnderWarrantyAsync();
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }

    public async Task<IEnumerable<AssetDto>> GetExpiredWarrantyAssetsAsync()
    {
        var assets = await _assetRepository.GetExpiredWarrantyAssetsAsync();
        return _mapper.Map<IEnumerable<AssetDto>>(assets);
    }

    public async Task<decimal> CalculateCurrentValueAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null || !asset.PurchasePrice.HasValue)
        {
            return 0;
        }

        if (asset.DepreciationRate.HasValue && asset.PurchaseDate.HasValue)
        {
            return CalculateDepreciatedValue(
                asset.PurchasePrice.Value, 
                asset.DepreciationRate.Value, 
                asset.PurchaseDate.Value);
        }

        return asset.PurchasePrice.Value;
    }

    private decimal CalculateDepreciatedValue(decimal purchasePrice, decimal depreciationRate, DateTime purchaseDate)
    {
        var yearsOwned = (decimal)(DateTime.UtcNow - purchaseDate).TotalDays / 365.25m;
        var depreciationAmount = purchasePrice * (depreciationRate / 100) * yearsOwned;
        var currentValue = purchasePrice - depreciationAmount;
        
        return Math.Max(0, currentValue); // Ensure value doesn't go below zero
    }
}