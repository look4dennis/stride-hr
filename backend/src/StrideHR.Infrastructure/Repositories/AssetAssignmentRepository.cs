using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AssetAssignmentRepository : Repository<AssetAssignment>, IAssetAssignmentRepository
{
    public AssetAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId)
    {
        return await _dbSet
            .Include(aa => aa.Asset)
            .Include(aa => aa.Employee)
            .Include(aa => aa.Project)
            .Include(aa => aa.AssignedByEmployee)
            .Include(aa => aa.ReturnedByEmployee)
            .FirstOrDefaultAsync(aa => aa.AssetId == assetId && aa.IsActive && !aa.IsDeleted);
    }

    public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(aa => aa.Asset)
            .Include(aa => aa.Project)
            .Include(aa => aa.AssignedByEmployee)
            .Include(aa => aa.ReturnedByEmployee)
            .Where(aa => aa.EmployeeId == employeeId && !aa.IsDeleted)
            .OrderByDescending(aa => aa.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByProjectIdAsync(int projectId)
    {
        return await _dbSet
            .Include(aa => aa.Asset)
            .Include(aa => aa.Employee)
            .Include(aa => aa.AssignedByEmployee)
            .Include(aa => aa.ReturnedByEmployee)
            .Where(aa => aa.ProjectId == projectId && !aa.IsDeleted)
            .OrderByDescending(aa => aa.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetAssignmentHistoryByAssetIdAsync(int assetId)
    {
        return await _dbSet
            .Include(aa => aa.Employee)
            .Include(aa => aa.Project)
            .Include(aa => aa.AssignedByEmployee)
            .Include(aa => aa.ReturnedByEmployee)
            .Where(aa => aa.AssetId == assetId && !aa.IsDeleted)
            .OrderByDescending(aa => aa.AssignedDate)
            .ToListAsync();
    }

    public async Task<bool> HasActiveAssignmentAsync(int assetId)
    {
        return await _dbSet
            .AnyAsync(aa => aa.AssetId == assetId && aa.IsActive && !aa.IsDeleted);
    }

    public async Task<IEnumerable<AssetAssignment>> GetActiveAssignmentsAsync()
    {
        return await _dbSet
            .Include(aa => aa.Asset)
            .Include(aa => aa.Employee)
            .Include(aa => aa.Project)
            .Include(aa => aa.AssignedByEmployee)
            .Where(aa => aa.IsActive && !aa.IsDeleted)
            .OrderByDescending(aa => aa.AssignedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetOverdueReturnsAsync()
    {
        // This would typically be based on a return due date field
        // For now, we'll consider assignments older than 1 year as potentially overdue
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        
        return await _dbSet
            .Include(aa => aa.Asset)
            .Include(aa => aa.Employee)
            .Include(aa => aa.Project)
            .Include(aa => aa.AssignedByEmployee)
            .Where(aa => aa.IsActive && 
                        !aa.IsDeleted && 
                        aa.AssignedDate < oneYearAgo)
            .OrderBy(aa => aa.AssignedDate)
            .ToListAsync();
    }
}