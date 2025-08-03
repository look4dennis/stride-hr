using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(int branchId)
    {
        return await _context.Employees
            .Where(e => e.BranchId == branchId && e.Status == Core.Enums.EmployeeStatus.Active)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId)
    {
        return await _context.Employees
            .Where(e => e.ReportingManagerId == managerId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync();
    }
}