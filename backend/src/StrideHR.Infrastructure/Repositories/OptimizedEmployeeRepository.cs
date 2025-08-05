using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// Optimized employee repository with performance enhancements
/// </summary>
public class OptimizedEmployeeRepository
{
    private readonly StrideHRDbContext _context;

    public OptimizedEmployeeRepository(StrideHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get active employees by branch with optimized query
    /// </summary>
    public async Task<List<Employee>> GetActiveEmployeesByBranchAsync(int branchId, int pageNumber = 1, int pageSize = 50)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId && e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync();
    }

    /// <summary>
    /// Search employees with optimized filtering
    /// </summary>
    public async Task<(List<Employee> Employees, int TotalCount)> SearchEmployeesAsync(
        string? searchTerm = null,
        string? department = null,
        EmployeeStatus? status = null,
        int? branchId = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = _context.Employees.AsQueryable();

        // Apply filters in order of selectivity (most selective first)
        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(e => e.Department == department);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Use indexed columns for search
            query = query.Where(e => 
                e.FirstName.Contains(searchTerm) ||
                e.LastName.Contains(searchTerm) ||
                e.Email.Contains(searchTerm) ||
                e.EmployeeId.Contains(searchTerm));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and execute query
        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (employees, totalCount);
    }

    /// <summary>
    /// Get employee hierarchy with optimized loading
    /// </summary>
    public async Task<List<Employee>> GetEmployeeHierarchyAsync(int managerId)
    {
        // Use recursive CTE for better performance than multiple queries
        var sql = @"
            WITH RECURSIVE employee_hierarchy AS (
                SELECT id, employee_id, first_name, last_name, reporting_manager_id, 0 as level
                FROM employees 
                WHERE id = {0} AND is_deleted = 0
                
                UNION ALL
                
                SELECT e.id, e.employee_id, e.first_name, e.last_name, e.reporting_manager_id, eh.level + 1
                FROM employees e
                INNER JOIN employee_hierarchy eh ON e.reporting_manager_id = eh.id
                WHERE e.is_deleted = 0
            )
            SELECT * FROM employee_hierarchy ORDER BY level, last_name, first_name";

        return await _context.Employees
            .FromSqlRaw(sql, managerId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Get employee summary statistics with optimized aggregation
    /// </summary>
    public async Task<EmployeeStatistics> GetEmployeeStatisticsAsync(int? branchId = null)
    {
        var query = _context.Employees.AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId.Value);
        }

        // Use single query with multiple aggregations
        var stats = await query
            .GroupBy(e => 1) // Group all records
            .Select(g => new EmployeeStatistics
            {
                TotalEmployees = g.Count(),
                ActiveEmployees = g.Count(e => e.Status == EmployeeStatus.Active),
                InactiveEmployees = g.Count(e => e.Status == EmployeeStatus.Inactive),
                TerminatedEmployees = g.Count(e => e.Status == EmployeeStatus.Terminated),
                AverageSalary = g.Average(e => e.BasicSalary),
                DepartmentCounts = g.GroupBy(e => e.Department)
                    .ToDictionary(dg => dg.Key ?? "Unknown", dg => dg.Count())
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return stats ?? new EmployeeStatistics();
    }

    /// <summary>
    /// Get employees with their latest attendance using optimized join
    /// </summary>
    public async Task<List<EmployeeWithLatestAttendance>> GetEmployeesWithLatestAttendanceAsync(int branchId, DateTime date)
    {
        // Use a single optimized query instead of N+1 queries
        var result = await _context.Employees
            .Where(e => e.BranchId == branchId && e.Status == EmployeeStatus.Active)
            .GroupJoin(
                _context.AttendanceRecords.Where(a => a.Date == date),
                employee => employee.Id,
                attendance => attendance.EmployeeId,
                (employee, attendances) => new EmployeeWithLatestAttendance
                {
                    Employee = employee,
                    LatestAttendance = attendances.OrderByDescending(a => a.CreatedAt).FirstOrDefault()
                })
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// Bulk update employee status with optimized batch operation
    /// </summary>
    public async Task<int> BulkUpdateEmployeeStatusAsync(List<int> employeeIds, EmployeeStatus newStatus, int updatedBy)
    {
        // Use raw SQL for better performance on bulk operations
        var sql = @"
            UPDATE employees 
            SET status = {0}, updated_at = {1}, updated_by = {2}
            WHERE id IN ({3}) AND is_deleted = 0";

        var idsParameter = string.Join(",", employeeIds);
        
        return await _context.Database.ExecuteSqlRawAsync(sql, 
            (int)newStatus, 
            DateTime.UtcNow, 
            updatedBy, 
            idsParameter);
    }
}

/// <summary>
/// Employee statistics model
/// </summary>
public class EmployeeStatistics
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public int TerminatedEmployees { get; set; }
    public decimal AverageSalary { get; set; }
    public Dictionary<string, int> DepartmentCounts { get; set; } = new();
}

/// <summary>
/// Employee with latest attendance model
/// </summary>
public class EmployeeWithLatestAttendance
{
    public Employee Employee { get; set; } = null!;
    public AttendanceRecord? LatestAttendance { get; set; }
}