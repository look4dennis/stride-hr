using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AssetMaintenanceRepository : Repository<AssetMaintenance>, IAssetMaintenanceRepository
{
    public AssetMaintenanceRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceByAssetIdAsync(int assetId)
    {
        return await _dbSet
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.AssetId == assetId && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync()
    {
        return await _dbSet
            .Include(m => m.Asset)
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.Status == MaintenanceStatus.Scheduled && !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetMaintenance>> GetOverdueMaintenanceAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(m => m.Asset)
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.Status == MaintenanceStatus.Scheduled && 
                       m.ScheduledDate < today && 
                       !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceByStatusAsync(MaintenanceStatus status)
    {
        return await _dbSet
            .Include(m => m.Asset)
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.Status == status && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceByTechnicianAsync(int technicianId)
    {
        return await _dbSet
            .Include(m => m.Asset)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.TechnicianId == technicianId && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(m => m.Asset)
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.ScheduledDate >= startDate && 
                       m.ScheduledDate <= endDate && 
                       !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<decimal> GetMaintenanceCostByAssetAsync(int assetId)
    {
        return await _dbSet
            .Where(m => m.AssetId == assetId && 
                       m.Status == MaintenanceStatus.Completed && 
                       m.Cost.HasValue && 
                       !m.IsDeleted)
            .SumAsync(m => m.Cost!.Value);
    }

    public async Task<decimal> GetMaintenanceCostByPeriodAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        var query = _dbSet
            .Where(m => m.Status == MaintenanceStatus.Completed && 
                       m.CompletedDate.HasValue &&
                       m.CompletedDate.Value >= startDate && 
                       m.CompletedDate.Value <= endDate && 
                       m.Cost.HasValue && 
                       !m.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(m => m.Asset.BranchId == branchId.Value);
        }

        return await query.SumAsync(m => m.Cost!.Value);
    }

    public async Task<AssetMaintenance?> GetLastMaintenanceByAssetAsync(int assetId)
    {
        return await _dbSet
            .Include(m => m.Technician)
            .Include(m => m.RequestedByEmployee)
            .Where(m => m.AssetId == assetId && 
                       m.Status == MaintenanceStatus.Completed && 
                       !m.IsDeleted)
            .OrderByDescending(m => m.CompletedDate)
            .FirstOrDefaultAsync();
    }
}