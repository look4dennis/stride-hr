using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Shift>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _dbSet
            .Where(s => s.OrganizationId == organizationId)
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetByBranchIdAsync(int branchId)
    {
        return await _dbSet
            .Where(s => s.BranchId == branchId)
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetActiveShiftsAsync(int organizationId)
    {
        return await _dbSet
            .Where(s => s.OrganizationId == organizationId && s.IsActive)
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> SearchShiftsAsync(ShiftSearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        if (criteria.OrganizationId.HasValue)
            query = query.Where(s => s.OrganizationId == criteria.OrganizationId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(s => s.BranchId == criteria.BranchId.Value);

        if (criteria.Type.HasValue)
            query = query.Where(s => s.Type == criteria.Type.Value);

        if (criteria.IsActive.HasValue)
            query = query.Where(s => s.IsActive == criteria.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchTerm) || 
                                   s.Description.ToLower().Contains(searchTerm));
        }

        return await query
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
            .OrderBy(s => s.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(ShiftSearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        if (criteria.OrganizationId.HasValue)
            query = query.Where(s => s.OrganizationId == criteria.OrganizationId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(s => s.BranchId == criteria.BranchId.Value);

        if (criteria.Type.HasValue)
            query = query.Where(s => s.Type == criteria.Type.Value);

        if (criteria.IsActive.HasValue)
            query = query.Where(s => s.IsActive == criteria.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchTerm) || 
                                   s.Description.ToLower().Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    public async Task<Shift?> GetShiftWithAssignmentsAsync(int shiftId)
    {
        return await _dbSet
            .Where(s => s.Id == shiftId)
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
                .ThenInclude(sa => sa.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Shift>> GetShiftTemplatesAsync(int organizationId)
    {
        // For now, we'll return all shifts as potential templates
        // In the future, we might add an IsTemplate property to the Shift entity
        return await _dbSet
            .Where(s => s.OrganizationId == organizationId && s.IsActive)
            .Include(s => s.Branch)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> IsShiftNameUniqueAsync(int organizationId, string name, int? excludeId = null)
    {
        var query = _dbSet.Where(s => s.OrganizationId == organizationId && s.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Shift>> GetShiftsByTypeAsync(int organizationId, Core.Enums.ShiftType shiftType)
    {
        return await _dbSet
            .Where(s => s.OrganizationId == organizationId && s.Type == shiftType && s.IsActive)
            .Include(s => s.Branch)
            .Include(s => s.ShiftAssignments)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetOverlappingShiftsAsync(int organizationId, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null)
    {
        var query = _dbSet.Where(s => s.OrganizationId == organizationId && s.IsActive);

        if (excludeShiftId.HasValue)
            query = query.Where(s => s.Id != excludeShiftId.Value);

        // Check for time overlap
        query = query.Where(s => 
            (s.StartTime < endTime && s.EndTime > startTime) || // Standard overlap
            (startTime < s.EndTime && endTime > s.StartTime));   // Reverse overlap

        return await query
            .Include(s => s.Branch)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }
}