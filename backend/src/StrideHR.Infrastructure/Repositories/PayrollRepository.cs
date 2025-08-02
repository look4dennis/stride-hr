using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayrollRepository : Repository<PayrollRecord>, IPayrollRepository
{
    public PayrollRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month)
    {
        return await _context.PayrollRecords
            .Include(p => p.Employee)
            .ThenInclude(e => e.Branch)
            .Include(p => p.ApprovedByEmployee)
            .Include(p => p.ProcessedByEmployee)
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && 
                                     p.PayrollYear == year && 
                                     p.PayrollMonth == month);
    }

    public async Task<List<PayrollRecord>> GetByBranchAndPeriodAsync(int branchId, int year, int month)
    {
        return await _context.PayrollRecords
            .Include(p => p.Employee)
            .ThenInclude(e => e.Branch)
            .Include(p => p.ApprovedByEmployee)
            .Include(p => p.ProcessedByEmployee)
            .Where(p => p.Employee.BranchId == branchId && 
                       p.PayrollYear == year && 
                       p.PayrollMonth == month)
            .OrderBy(p => p.Employee.EmployeeId)
            .ToListAsync();
    }

    public async Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId, int year, int? month = null)
    {
        var query = _context.PayrollRecords
            .Include(p => p.Employee)
            .ThenInclude(e => e.Branch)
            .Include(p => p.ApprovedByEmployee)
            .Include(p => p.ProcessedByEmployee)
            .Where(p => p.EmployeeId == employeeId && p.PayrollYear == year);

        if (month.HasValue)
        {
            query = query.Where(p => p.PayrollMonth == month.Value);
        }

        return await query
            .OrderByDescending(p => p.PayrollMonth)
            .ToListAsync();
    }

    public async Task<bool> ExistsForPeriodAsync(int employeeId, int year, int month)
    {
        return await _context.PayrollRecords
            .AnyAsync(p => p.EmployeeId == employeeId && 
                          p.PayrollYear == year && 
                          p.PayrollMonth == month);
    }
}