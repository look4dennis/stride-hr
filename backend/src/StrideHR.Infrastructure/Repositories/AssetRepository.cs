using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Asset;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AssetRepository : Repository<Asset>, IAssetRepository
{
    public AssetRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Asset?> GetByAssetTagAsync(string assetTag)
    {
        return await _dbSet
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.AssetTag == assetTag && !a.IsDeleted);
    }

    public async Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null)
    {
        var query = _dbSet.Where(a => a.AssetTag == assetTag && !a.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Asset>> SearchAssetsAsync(AssetSearchCriteria criteria)
    {
        var query = BuildSearchQuery(criteria);
        
        // Apply sorting
        if (!string.IsNullOrEmpty(criteria.SortBy))
        {
            query = criteria.SortBy.ToLower() switch
            {
                "name" => criteria.SortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
                "assettag" => criteria.SortDescending ? query.OrderByDescending(a => a.AssetTag) : query.OrderBy(a => a.AssetTag),
                "type" => criteria.SortDescending ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
                "status" => criteria.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "purchasedate" => criteria.SortDescending ? query.OrderByDescending(a => a.PurchaseDate) : query.OrderBy(a => a.PurchaseDate),
                "purchaseprice" => criteria.SortDescending ? query.OrderByDescending(a => a.PurchasePrice) : query.OrderBy(a => a.PurchasePrice),
                _ => query.OrderBy(a => a.Name)
            };
        }
        else
        {
            query = query.OrderBy(a => a.Name);
        }
        
        // Apply pagination
        var skip = (criteria.Page - 1) * criteria.PageSize;
        query = query.Skip(skip).Take(criteria.PageSize);
        
        return await query
            .Include(a => a.Branch)
            .Include(a => a.AssetAssignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Employee)
            .Include(a => a.AssetAssignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Project)
            .ToListAsync();
    }

    public async Task<int> GetTotalAssetsCountAsync(AssetSearchCriteria criteria)
    {
        var query = BuildSearchQuery(criteria);
        return await query.CountAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsByBranchAsync(int branchId)
    {
        return await _dbSet
            .Where(a => a.BranchId == branchId && !a.IsDeleted)
            .Include(a => a.Branch)
            .Include(a => a.AssetAssignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Employee)
            .Include(a => a.AssetAssignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Project)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsRequiringMaintenanceAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(a => !a.IsDeleted && 
                       (a.NextMaintenanceDate.HasValue && a.NextMaintenanceDate.Value <= today.AddDays(30)))
            .Include(a => a.Branch)
            .OrderBy(a => a.NextMaintenanceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsUnderWarrantyAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(a => !a.IsDeleted && 
                       a.WarrantyEndDate.HasValue && 
                       a.WarrantyEndDate.Value > today)
            .Include(a => a.Branch)
            .OrderBy(a => a.WarrantyEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(a => !a.IsDeleted && 
                       a.WarrantyEndDate.HasValue && 
                       a.WarrantyEndDate.Value <= today)
            .Include(a => a.Branch)
            .OrderByDescending(a => a.WarrantyEndDate)
            .ToListAsync();
    }

    public async Task<AssetReportDto> GenerateAssetReportAsync(int? branchId = null)
    {
        var query = _dbSet.Where(a => !a.IsDeleted);
        
        if (branchId.HasValue)
        {
            query = query.Where(a => a.BranchId == branchId.Value);
        }

        var assets = await query
            .Include(a => a.Branch)
            .Include(a => a.MaintenanceRecords)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var totalAssets = assets.Count;
        var availableAssets = assets.Count(a => a.Status == AssetStatus.Available);
        var assignedAssets = assets.Count(a => a.Status == AssetStatus.Assigned);
        var assetsInMaintenance = assets.Count(a => a.Status == AssetStatus.InMaintenance);
        var retiredAssets = assets.Count(a => a.Status == AssetStatus.Retired);
        var totalAssetValue = assets.Where(a => a.CurrentValue.HasValue).Sum(a => a.CurrentValue!.Value);
        var totalMaintenanceCost = assets.SelectMany(a => a.MaintenanceRecords).Sum(m => m.Cost ?? 0);
        var assetsUnderWarranty = assets.Count(a => a.WarrantyEndDate.HasValue && a.WarrantyEndDate.Value > today);
        var assetsRequiringMaintenance = assets.Count(a => a.NextMaintenanceDate.HasValue && a.NextMaintenanceDate.Value <= today.AddDays(30));

        // Get maintenance records for overdue calculation
        var overdueMaintenanceCount = await _context.Set<AssetMaintenance>()
            .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < today)
            .CountAsync();

        // Get handover records for pending/overdue calculation
        var pendingHandovers = await _context.Set<AssetHandover>()
            .Where(h => h.Status == HandoverStatus.Pending)
            .CountAsync();

        var overdueHandovers = await _context.Set<AssetHandover>()
            .Where(h => h.Status == HandoverStatus.Pending && h.DueDate.HasValue && h.DueDate.Value < today)
            .CountAsync();

        return new AssetReportDto
        {
            TotalAssets = totalAssets,
            AvailableAssets = availableAssets,
            AssignedAssets = assignedAssets,
            AssetsInMaintenance = assetsInMaintenance,
            RetiredAssets = retiredAssets,
            TotalAssetValue = totalAssetValue,
            TotalMaintenanceCost = totalMaintenanceCost,
            AssetsUnderWarranty = assetsUnderWarranty,
            AssetsRequiringMaintenance = assetsRequiringMaintenance,
            OverdueMaintenanceCount = overdueMaintenanceCount,
            PendingHandovers = pendingHandovers,
            OverdueHandovers = overdueHandovers,
            AssetTypeBreakdown = GetAssetTypeBreakdown(assets),
            AssetStatusBreakdown = GetAssetStatusBreakdown(assets),
            BranchBreakdown = GetBranchBreakdown(assets),
            MaintenanceCostTrends = await GetMaintenanceCostTrendsAsync(branchId)
        };
    }

    public async Task<Asset?> GetAssetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Branch)
            .Include(a => a.AssetAssignments)
                .ThenInclude(aa => aa.Employee)
            .Include(a => a.AssetAssignments)
                .ThenInclude(aa => aa.Project)
            .Include(a => a.MaintenanceRecords)
                .ThenInclude(m => m.Technician)
            .Include(a => a.HandoverRecords)
                .ThenInclude(h => h.Employee)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    private IQueryable<Asset> BuildSearchQuery(AssetSearchCriteria criteria)
    {
        var query = _dbSet.Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(a => 
                a.Name.ToLower().Contains(searchTerm) ||
                a.AssetTag.ToLower().Contains(searchTerm) ||
                (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) ||
                (a.Model != null && a.Model.ToLower().Contains(searchTerm)) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)));
        }

        if (criteria.Type.HasValue)
        {
            query = query.Where(a => a.Type == criteria.Type.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(a => a.Status == criteria.Status.Value);
        }

        if (criteria.Condition.HasValue)
        {
            query = query.Where(a => a.Condition == criteria.Condition.Value);
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(a => a.BranchId == criteria.BranchId.Value);
        }

        if (criteria.AssignedToEmployeeId.HasValue)
        {
            query = query.Where(a => a.AssetAssignments.Any(aa => aa.IsActive && aa.EmployeeId == criteria.AssignedToEmployeeId.Value));
        }

        if (criteria.AssignedToProjectId.HasValue)
        {
            query = query.Where(a => a.AssetAssignments.Any(aa => aa.IsActive && aa.ProjectId == criteria.AssignedToProjectId.Value));
        }

        if (criteria.IsUnderWarranty.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            if (criteria.IsUnderWarranty.Value)
            {
                query = query.Where(a => a.WarrantyEndDate.HasValue && a.WarrantyEndDate.Value > today);
            }
            else
            {
                query = query.Where(a => !a.WarrantyEndDate.HasValue || a.WarrantyEndDate.Value <= today);
            }
        }

        if (criteria.RequiresMaintenance.HasValue && criteria.RequiresMaintenance.Value)
        {
            var maintenanceDate = DateTime.UtcNow.Date.AddDays(30);
            query = query.Where(a => a.NextMaintenanceDate.HasValue && a.NextMaintenanceDate.Value <= maintenanceDate);
        }

        if (criteria.PurchaseDateFrom.HasValue)
        {
            query = query.Where(a => a.PurchaseDate.HasValue && a.PurchaseDate.Value >= criteria.PurchaseDateFrom.Value);
        }

        if (criteria.PurchaseDateTo.HasValue)
        {
            query = query.Where(a => a.PurchaseDate.HasValue && a.PurchaseDate.Value <= criteria.PurchaseDateTo.Value);
        }

        if (criteria.PurchasePriceFrom.HasValue)
        {
            query = query.Where(a => a.PurchasePrice.HasValue && a.PurchasePrice.Value >= criteria.PurchasePriceFrom.Value);
        }

        if (criteria.PurchasePriceTo.HasValue)
        {
            query = query.Where(a => a.PurchasePrice.HasValue && a.PurchasePrice.Value <= criteria.PurchasePriceTo.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Vendor))
        {
            query = query.Where(a => a.Vendor != null && a.Vendor.ToLower().Contains(criteria.Vendor.ToLower()));
        }

        if (!string.IsNullOrEmpty(criteria.Brand))
        {
            query = query.Where(a => a.Brand != null && a.Brand.ToLower().Contains(criteria.Brand.ToLower()));
        }

        if (!string.IsNullOrEmpty(criteria.Location))
        {
            query = query.Where(a => a.Location != null && a.Location.ToLower().Contains(criteria.Location.ToLower()));
        }

        return query;
    }

    private List<AssetTypeStatistics> GetAssetTypeBreakdown(List<Asset> assets)
    {
        return assets
            .GroupBy(a => a.Type)
            .Select(g => new AssetTypeStatistics
            {
                Type = g.Key,
                Count = g.Count(),
                TotalValue = g.Where(a => a.CurrentValue.HasValue).Sum(a => a.CurrentValue!.Value),
                Available = g.Count(a => a.Status == AssetStatus.Available),
                Assigned = g.Count(a => a.Status == AssetStatus.Assigned),
                InMaintenance = g.Count(a => a.Status == AssetStatus.InMaintenance)
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    private List<AssetStatusStatistics> GetAssetStatusBreakdown(List<Asset> assets)
    {
        var totalAssets = assets.Count;
        return assets
            .GroupBy(a => a.Status)
            .Select(g => new AssetStatusStatistics
            {
                Status = g.Key,
                Count = g.Count(),
                Percentage = totalAssets > 0 ? (decimal)g.Count() / totalAssets * 100 : 0
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    private List<BranchAssetStatistics> GetBranchBreakdown(List<Asset> assets)
    {
        return assets
            .GroupBy(a => new { a.BranchId, a.Branch.Name })
            .Select(g => new BranchAssetStatistics
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.Name,
                TotalAssets = g.Count(),
                TotalValue = g.Where(a => a.CurrentValue.HasValue).Sum(a => a.CurrentValue!.Value),
                AssignedAssets = g.Count(a => a.Status == AssetStatus.Assigned),
                AvailableAssets = g.Count(a => a.Status == AssetStatus.Available)
            })
            .OrderByDescending(s => s.TotalAssets)
            .ToList();
    }

    private async Task<List<MaintenanceCostTrend>> GetMaintenanceCostTrendsAsync(int? branchId)
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        
        var query = _context.Set<AssetMaintenance>()
            .Where(m => m.CompletedDate.HasValue && m.CompletedDate.Value >= sixMonthsAgo);

        if (branchId.HasValue)
        {
            query = query.Where(m => m.Asset.BranchId == branchId.Value);
        }

        var maintenanceRecords = await query
            .Include(m => m.Asset)
            .ToListAsync();

        return maintenanceRecords
            .GroupBy(m => new { Year = m.CompletedDate!.Value.Year, Month = m.CompletedDate!.Value.Month })
            .Select(g => new MaintenanceCostTrend
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                Cost = g.Sum(m => m.Cost ?? 0),
                MaintenanceCount = g.Count()
            })
            .OrderBy(t => t.Month)
            .ToList();
    }
}