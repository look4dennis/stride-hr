using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AssetHandoverRepository : Repository<AssetHandover>, IAssetHandoverRepository
{
    public AssetHandoverRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AssetHandover>> GetHandoversByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Include(h => h.CompletedByEmployee)
            .Include(h => h.ApprovedByEmployee)
            .Where(h => h.EmployeeId == employeeId && !h.IsDeleted)
            .OrderByDescending(h => h.InitiatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHandover>> GetHandoversByAssetIdAsync(int assetId)
    {
        return await _dbSet
            .Include(h => h.Employee)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Include(h => h.CompletedByEmployee)
            .Include(h => h.ApprovedByEmployee)
            .Where(h => h.AssetId == assetId && !h.IsDeleted)
            .OrderByDescending(h => h.InitiatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHandover>> GetHandoversByStatusAsync(HandoverStatus status)
    {
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.Employee)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Include(h => h.CompletedByEmployee)
            .Include(h => h.ApprovedByEmployee)
            .Where(h => h.Status == status && !h.IsDeleted)
            .OrderByDescending(h => h.InitiatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHandover>> GetPendingHandoversAsync()
    {
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.Employee)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Where(h => h.Status == HandoverStatus.Pending && !h.IsDeleted)
            .OrderBy(h => h.DueDate ?? DateTime.MaxValue)
            .ThenBy(h => h.InitiatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHandover>> GetOverdueHandoversAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.Employee)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Where(h => h.Status == HandoverStatus.Pending && 
                       h.DueDate.HasValue && 
                       h.DueDate.Value < today && 
                       !h.IsDeleted)
            .OrderBy(h => h.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetHandover>> GetHandoversByEmployeeExitAsync(int employeeExitId)
    {
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.Employee)
            .Include(h => h.InitiatedByEmployee)
            .Include(h => h.CompletedByEmployee)
            .Include(h => h.ApprovedByEmployee)
            .Where(h => h.EmployeeExitId == employeeExitId && !h.IsDeleted)
            .OrderByDescending(h => h.InitiatedDate)
            .ToListAsync();
    }

    public async Task<bool> HasPendingHandoverAsync(int assetId)
    {
        return await _dbSet
            .AnyAsync(h => h.AssetId == assetId && 
                          h.Status == HandoverStatus.Pending && 
                          !h.IsDeleted);
    }

    public async Task<IEnumerable<AssetHandover>> GetHandoversRequiringApprovalAsync()
    {
        return await _dbSet
            .Include(h => h.Asset)
            .Include(h => h.Employee)
            .Include(h => h.EmployeeExit)
            .Include(h => h.InitiatedByEmployee)
            .Include(h => h.CompletedByEmployee)
            .Where(h => h.Status == HandoverStatus.Completed && 
                       !h.IsApproved && 
                       !h.IsDeleted)
            .OrderBy(h => h.CompletedDate)
            .ToListAsync();
    }
}