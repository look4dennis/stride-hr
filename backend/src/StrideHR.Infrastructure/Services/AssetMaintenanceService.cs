using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Infrastructure.Services;

public class AssetMaintenanceService : IAssetMaintenanceService
{
    private readonly IAssetMaintenanceRepository _maintenanceRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public AssetMaintenanceService(
        IAssetMaintenanceRepository maintenanceRepository,
        IAssetRepository assetRepository,
        IMapper mapper)
    {
        _maintenanceRepository = maintenanceRepository;
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetMaintenanceDto> CreateMaintenanceAsync(CreateAssetMaintenanceDto maintenanceDto)
    {
        var asset = await _assetRepository.GetByIdAsync(maintenanceDto.AssetId);
        if (asset == null || asset.IsDeleted)
        {
            throw new InvalidOperationException($"Asset with ID {maintenanceDto.AssetId} not found.");
        }

        var maintenance = _mapper.Map<AssetMaintenance>(maintenanceDto);
        maintenance.CreatedAt = DateTime.UtcNow;

        await _maintenanceRepository.AddAsync(maintenance);
        await _maintenanceRepository.SaveChangesAsync();

        // Return the maintenance with all related data
        var createdMaintenance = await _maintenanceRepository.GetByIdAsync(maintenance.Id, 
            m => m.Asset, 
            m => m.Technician, 
            m => m.RequestedByEmployee);

        return _mapper.Map<AssetMaintenanceDto>(createdMaintenance!);
    }

    public async Task<AssetMaintenanceDto> UpdateMaintenanceAsync(int id, UpdateAssetMaintenanceDto updateDto)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(id);
        if (maintenance == null || maintenance.IsDeleted)
        {
            throw new InvalidOperationException($"Maintenance record with ID {id} not found.");
        }

        _mapper.Map(updateDto, maintenance);
        maintenance.UpdatedAt = DateTime.UtcNow;

        await _maintenanceRepository.UpdateAsync(maintenance);
        await _maintenanceRepository.SaveChangesAsync();

        return await GetMaintenanceByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated maintenance record.");
    }

    public async Task<AssetMaintenanceDto?> GetMaintenanceByIdAsync(int id)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(id, 
            m => m.Asset, 
            m => m.Technician, 
            m => m.RequestedByEmployee);

        return maintenance != null ? _mapper.Map<AssetMaintenanceDto>(maintenance) : null;
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByAssetIdAsync(int assetId)
    {
        var maintenanceRecords = await _maintenanceRepository.GetMaintenanceByAssetIdAsync(assetId);
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetScheduledMaintenanceAsync()
    {
        var maintenanceRecords = await _maintenanceRepository.GetScheduledMaintenanceAsync();
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetOverdueMaintenanceAsync()
    {
        var maintenanceRecords = await _maintenanceRepository.GetOverdueMaintenanceAsync();
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByStatusAsync(MaintenanceStatus status)
    {
        var maintenanceRecords = await _maintenanceRepository.GetMaintenanceByStatusAsync(status);
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByTechnicianAsync(int technicianId)
    {
        var maintenanceRecords = await _maintenanceRepository.GetMaintenanceByTechnicianAsync(technicianId);
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetMaintenanceByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var maintenanceRecords = await _maintenanceRepository.GetMaintenanceByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<AssetMaintenanceDto>>(maintenanceRecords);
    }

    public async Task<bool> DeleteMaintenanceAsync(int id)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(id);
        if (maintenance == null || maintenance.IsDeleted)
        {
            return false;
        }

        // Only allow deletion of scheduled maintenance
        if (maintenance.Status != MaintenanceStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled maintenance can be deleted.");
        }

        // Soft delete
        maintenance.IsDeleted = true;
        maintenance.DeletedAt = DateTime.UtcNow;
        maintenance.Status = MaintenanceStatus.Cancelled;

        await _maintenanceRepository.UpdateAsync(maintenance);
        return await _maintenanceRepository.SaveChangesAsync();
    }

    public async Task<decimal> GetMaintenanceCostByAssetAsync(int assetId)
    {
        return await _maintenanceRepository.GetMaintenanceCostByAssetAsync(assetId);
    }

    public async Task<decimal> GetMaintenanceCostByPeriodAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        return await _maintenanceRepository.GetMaintenanceCostByPeriodAsync(startDate, endDate, branchId);
    }

    public async Task<AssetMaintenanceDto> StartMaintenanceAsync(int id, int technicianId)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(id);
        if (maintenance == null || maintenance.IsDeleted)
        {
            throw new InvalidOperationException($"Maintenance record with ID {id} not found.");
        }

        if (maintenance.Status != MaintenanceStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled maintenance can be started.");
        }

        maintenance.Status = MaintenanceStatus.InProgress;
        maintenance.StartDate = DateTime.UtcNow;
        maintenance.TechnicianId = technicianId;
        maintenance.UpdatedAt = DateTime.UtcNow;

        // Update asset status
        var asset = await _assetRepository.GetByIdAsync(maintenance.AssetId);
        if (asset != null)
        {
            asset.Status = AssetStatus.InMaintenance;
            asset.UpdatedAt = DateTime.UtcNow;
            await _assetRepository.UpdateAsync(asset);
        }

        await _maintenanceRepository.UpdateAsync(maintenance);
        await _maintenanceRepository.SaveChangesAsync();

        return await GetMaintenanceByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated maintenance record.");
    }

    public async Task<AssetMaintenanceDto> CompleteMaintenanceAsync(int id, UpdateAssetMaintenanceDto completionDto)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(id);
        if (maintenance == null || maintenance.IsDeleted)
        {
            throw new InvalidOperationException($"Maintenance record with ID {id} not found.");
        }

        if (maintenance.Status != MaintenanceStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress maintenance can be completed.");
        }

        // Update maintenance record
        maintenance.Status = MaintenanceStatus.Completed;
        maintenance.CompletedDate = DateTime.UtcNow;
        maintenance.WorkPerformed = completionDto.WorkPerformed;
        maintenance.PartsReplaced = completionDto.PartsReplaced;
        maintenance.Notes = completionDto.Notes;
        maintenance.DocumentUrl = completionDto.DocumentUrl;
        maintenance.UpdatedAt = DateTime.UtcNow;

        // Update asset
        var asset = await _assetRepository.GetByIdAsync(maintenance.AssetId);
        if (asset != null)
        {
            asset.Status = AssetStatus.Available; // Assume asset is available after maintenance
            asset.LastMaintenanceDate = DateTime.UtcNow;
            
            if (completionDto.NextMaintenanceDate.HasValue)
            {
                asset.NextMaintenanceDate = completionDto.NextMaintenanceDate.Value;
            }
            
            asset.UpdatedAt = DateTime.UtcNow;
            await _assetRepository.UpdateAsync(asset);
        }

        await _maintenanceRepository.UpdateAsync(maintenance);
        await _maintenanceRepository.SaveChangesAsync();

        return await GetMaintenanceByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve completed maintenance record.");
    }
}