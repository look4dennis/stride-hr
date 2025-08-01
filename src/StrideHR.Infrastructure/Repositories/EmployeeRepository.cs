using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;
using System.Linq.Expressions;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Employee entity
/// </summary>
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && !e.IsDeleted, cancellationToken);
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Email == email && !e.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .Where(e => e.BranchId == branchId && !e.IsDeleted)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .Where(e => e.Department == department && !e.IsDeleted)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Where(e => e.ReportingManagerId == managerId && !e.IsDeleted)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Employee> Employees, int TotalCount)> SearchAsync(
        EmployeeSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .Where(e => !e.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(e => 
                e.FirstName.ToLower().Contains(searchTerm) ||
                e.LastName.ToLower().Contains(searchTerm) ||
                e.EmployeeId.ToLower().Contains(searchTerm) ||
                e.Email.ToLower().Contains(searchTerm));
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(e => e.BranchId == criteria.BranchId.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Department))
        {
            query = query.Where(e => e.Department == criteria.Department);
        }

        if (!string.IsNullOrEmpty(criteria.Designation))
        {
            query = query.Where(e => e.Designation == criteria.Designation);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(e => e.Status == criteria.Status.Value);
        }

        if (criteria.ReportingManagerId.HasValue)
        {
            query = query.Where(e => e.ReportingManagerId == criteria.ReportingManagerId.Value);
        }

        if (criteria.JoiningDateFrom.HasValue)
        {
            query = query.Where(e => e.JoiningDate >= criteria.JoiningDateFrom.Value);
        }

        if (criteria.JoiningDateTo.HasValue)
        {
            query = query.Where(e => e.JoiningDate <= criteria.JoiningDateTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrEmpty(criteria.SortBy))
        {
            query = criteria.SortBy.ToLower() switch
            {
                "firstname" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.FirstName)
                    : query.OrderBy(e => e.FirstName),
                "lastname" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.LastName)
                    : query.OrderBy(e => e.LastName),
                "employeeid" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.EmployeeId)
                    : query.OrderBy(e => e.EmployeeId),
                "email" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.Email)
                    : query.OrderBy(e => e.Email),
                "joiningdate" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.JoiningDate)
                    : query.OrderBy(e => e.JoiningDate),
                "department" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.Department)
                    : query.OrderBy(e => e.Department),
                "designation" => criteria.SortDescending 
                    ? query.OrderByDescending(e => e.Designation)
                    : query.OrderBy(e => e.Designation),
                _ => query.OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            };
        }
        else
        {
            query = query.OrderBy(e => e.FirstName).ThenBy(e => e.LastName);
        }

        // Apply pagination
        var employees = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (employees, totalCount);
    }

    public async Task<bool> IsEmployeeIdUniqueAsync(string employeeId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.Where(e => e.EmployeeId == employeeId && !e.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.Where(e => e.Email == email && !e.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetNextEmployeeSequenceAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var currentYear = DateTime.Now.Year;
        var yearPrefix = currentYear.ToString().Substring(2); // Last 2 digits

        // Get the highest sequence number for this branch and year
        var lastEmployee = await _context.Employees
            .Where(e => e.BranchId == branchId && 
                       e.EmployeeId.Contains($"-{yearPrefix}-") && 
                       !e.IsDeleted)
            .OrderByDescending(e => e.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEmployee == null)
        {
            return 1;
        }

        // Extract sequence number from employee ID (format: XXX-EMP-YY-NNN)
        var parts = lastEmployee.EmployeeId.Split('-');
        if (parts.Length >= 4 && int.TryParse(parts[3], out var sequence))
        {
            return sequence + 1;
        }

        return 1;
    }

    public async Task<IEnumerable<Employee>> GetHierarchyAsync(int? rootEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.Subordinates)
            .Where(e => !e.IsDeleted);

        if (rootEmployeeId.HasValue)
        {
            // Get hierarchy starting from specific employee
            query = query.Where(e => e.Id == rootEmployeeId.Value || e.ReportingManagerId == rootEmployeeId.Value);
        }
        else
        {
            // Get top-level employees (no manager)
            query = query.Where(e => e.ReportingManagerId == null);
        }

        return await query
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetSubordinatesAsync(int managerId, bool includeIndirect = false, CancellationToken cancellationToken = default)
    {
        if (!includeIndirect)
        {
            // Direct subordinates only
            return await _context.Employees
                .Include(e => e.Branch)
                .Where(e => e.ReportingManagerId == managerId && !e.IsDeleted)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        // Include indirect subordinates using recursive CTE
        var subordinates = new List<Employee>();
        var directSubordinates = await GetSubordinatesAsync(managerId, false, cancellationToken);
        subordinates.AddRange(directSubordinates);

        foreach (var subordinate in directSubordinates)
        {
            var indirectSubordinates = await GetSubordinatesAsync(subordinate.Id, true, cancellationToken);
            subordinates.AddRange(indirectSubordinates);
        }

        return subordinates.DistinctBy(e => e.Id).OrderBy(e => e.FirstName).ThenBy(e => e.LastName);
    }

    public async Task<bool> IsCircularReferenceAsync(int employeeId, int? newManagerId, CancellationToken cancellationToken = default)
    {
        if (!newManagerId.HasValue)
        {
            return false;
        }

        // Check if the new manager is a subordinate of the employee
        var subordinates = await GetSubordinatesAsync(employeeId, true, cancellationToken);
        return subordinates.Any(s => s.Id == newManagerId.Value);
    }

    public async Task<Employee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .Include(e => e.Subordinates)
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<Employee?> GetWithRolesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoles)
                .ThenInclude(er => er.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<Employee?> GetWithAttendanceAsync(int id, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.AttendanceRecords.Where(ar => 
                (!fromDate.HasValue || ar.Date >= fromDate.Value) &&
                (!toDate.HasValue || ar.Date <= toDate.Value)))
            .Where(e => e.Id == id && !e.IsDeleted);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.Branch)
            .Where(e => e.Status == EmployeeStatus.Active && !e.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesWithExpiringVisasAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Where(e => e.VisaExpiryDate.HasValue && 
                       e.VisaExpiryDate.Value <= beforeDate && 
                       e.Status == EmployeeStatus.Active && 
                       !e.IsDeleted)
            .OrderBy(e => e.VisaExpiryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetBirthdayEmployeesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Branch)
            .Where(e => e.DateOfBirth.Month == date.Month && 
                       e.DateOfBirth.Day == date.Day && 
                       e.Status == EmployeeStatus.Active && 
                       !e.IsDeleted)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }
}