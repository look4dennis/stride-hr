using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftAssignmentRepository : Repository<ShiftAssignment>, IShiftAssignmentRepository
{
    public ShiftAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShiftAssignment>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Where(sa => sa.EmployeeId == employeeId)
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .OrderByDescending(sa => sa.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> GetByShiftIdAsync(int shiftId)
    {
        return await _dbSet
            .Where(sa => sa.ShiftId == shiftId)
            .Include(sa => sa.Employee)
            .Include(sa => sa.Shift)
            .OrderBy(sa => sa.Employee.FirstName)
            .ThenBy(sa => sa.Employee.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> GetActiveAssignmentsAsync(int employeeId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(sa => sa.EmployeeId == employeeId && 
                        sa.IsActive && 
                        sa.StartDate <= today && 
                        (sa.EndDate == null || sa.EndDate >= today))
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .OrderBy(sa => sa.Shift.StartTime)
            .ToListAsync();
    }

    public async Task<ShiftAssignment?> GetCurrentAssignmentAsync(int employeeId, DateTime date)
    {
        return await _dbSet
            .Where(sa => sa.EmployeeId == employeeId && 
                        sa.IsActive && 
                        sa.StartDate <= date && 
                        (sa.EndDate == null || sa.EndDate >= date))
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> GetAssignmentsByDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(sa => sa.EmployeeId == employeeId && 
                        sa.IsActive && 
                        sa.StartDate <= endDate && 
                        (sa.EndDate == null || sa.EndDate >= startDate))
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .OrderBy(sa => sa.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> SearchAssignmentsAsync(ShiftAssignmentSearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        if (criteria.EmployeeId.HasValue)
            query = query.Where(sa => sa.EmployeeId == criteria.EmployeeId.Value);

        if (criteria.ShiftId.HasValue)
            query = query.Where(sa => sa.ShiftId == criteria.ShiftId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(sa => sa.Shift.BranchId == criteria.BranchId.Value);

        if (criteria.StartDate.HasValue)
            query = query.Where(sa => sa.StartDate >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            query = query.Where(sa => sa.EndDate == null || sa.EndDate <= criteria.EndDate.Value);

        if (criteria.IsActive.HasValue)
            query = query.Where(sa => sa.IsActive == criteria.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(sa => sa.Employee.FirstName.ToLower().Contains(searchTerm) || 
                                    sa.Employee.LastName.ToLower().Contains(searchTerm) ||
                                    sa.Shift.Name.ToLower().Contains(searchTerm));
        }

        return await query
            .Include(sa => sa.Employee)
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .OrderByDescending(sa => sa.StartDate)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(ShiftAssignmentSearchCriteria criteria)
    {
        var query = _dbSet.AsQueryable();

        if (criteria.EmployeeId.HasValue)
            query = query.Where(sa => sa.EmployeeId == criteria.EmployeeId.Value);

        if (criteria.ShiftId.HasValue)
            query = query.Where(sa => sa.ShiftId == criteria.ShiftId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(sa => sa.Shift.BranchId == criteria.BranchId.Value);

        if (criteria.StartDate.HasValue)
            query = query.Where(sa => sa.StartDate >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            query = query.Where(sa => sa.EndDate == null || sa.EndDate <= criteria.EndDate.Value);

        if (criteria.IsActive.HasValue)
            query = query.Where(sa => sa.IsActive == criteria.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(sa => sa.Employee.FirstName.ToLower().Contains(searchTerm) || 
                                    sa.Employee.LastName.ToLower().Contains(searchTerm) ||
                                    sa.Shift.Name.ToLower().Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> GetConflictingAssignmentsAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate)
    {
        var query = _dbSet
            .Where(sa => sa.EmployeeId == employeeId && 
                        sa.IsActive && 
                        sa.ShiftId != shiftId);

        // Check for date range overlap
        if (endDate.HasValue)
        {
            query = query.Where(sa => sa.StartDate <= endDate.Value && 
                                    (sa.EndDate == null || sa.EndDate >= startDate));
        }
        else
        {
            query = query.Where(sa => sa.StartDate <= startDate && 
                                    (sa.EndDate == null || sa.EndDate >= startDate));
        }

        return await query
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .OrderBy(sa => sa.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftAssignment>> GetAssignmentsByBranchAsync(int branchId, DateTime? date = null)
    {
        var query = _dbSet
            .Where(sa => sa.Shift.BranchId == branchId && sa.IsActive);

        if (date.HasValue)
        {
            query = query.Where(sa => sa.StartDate <= date.Value && 
                                    (sa.EndDate == null || sa.EndDate >= date.Value));
        }

        return await query
            .Include(sa => sa.Employee)
            .Include(sa => sa.Shift)
            .OrderBy(sa => sa.Shift.StartTime)
            .ThenBy(sa => sa.Employee.FirstName)
            .ToListAsync();
    }

    public async Task<bool> HasActiveAssignmentAsync(int employeeId, int shiftId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .AnyAsync(sa => sa.EmployeeId == employeeId && 
                           sa.ShiftId == shiftId && 
                           sa.IsActive && 
                           sa.StartDate <= today && 
                           (sa.EndDate == null || sa.EndDate >= today));
    }

    public async Task<IEnumerable<ShiftAssignment>> GetUpcomingAssignmentsAsync(int employeeId, int days = 7)
    {
        var today = DateTime.Today;
        var futureDate = today.AddDays(days);

        return await _dbSet
            .Where(sa => sa.EmployeeId == employeeId && 
                        sa.IsActive && 
                        sa.StartDate <= futureDate && 
                        (sa.EndDate == null || sa.EndDate >= today))
            .Include(sa => sa.Shift)
                .ThenInclude(s => s.Branch)
            .Include(sa => sa.Employee)
            .OrderBy(sa => sa.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetAssignedEmployeesCountAsync(int shiftId, DateTime? date = null)
    {
        var query = _dbSet.Where(sa => sa.ShiftId == shiftId && sa.IsActive);

        if (date.HasValue)
        {
            query = query.Where(sa => sa.StartDate <= date.Value && 
                                    (sa.EndDate == null || sa.EndDate >= date.Value));
        }

        return await query.CountAsync();
    }

    public async Task<bool> HasConflictingAssignmentAsync(int employeeId, DateTime date)
    {
        return await _dbSet
            .AnyAsync(sa => sa.EmployeeId == employeeId && 
                           sa.IsActive && 
                           sa.StartDate <= date && 
                           (sa.EndDate == null || sa.EndDate >= date));
    }
}