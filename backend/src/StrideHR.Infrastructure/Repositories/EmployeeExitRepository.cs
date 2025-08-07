using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeExitRepository : Repository<EmployeeExit>, IEmployeeExitRepository
{
    public EmployeeExitRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<EmployeeExit?> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeExits
            .Where(ee => ee.EmployeeId == employeeId)
            .Include(ee => ee.Employee)
            .Include(ee => ee.Tasks)
            .OrderByDescending(ee => ee.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeExit?> GetActiveExitByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeExits
            .Where(ee => ee.EmployeeId == employeeId && !ee.IsCompleted)
            .Include(ee => ee.Employee)
            .Include(ee => ee.Tasks)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<EmployeeExit>> GetPendingExitsAsync()
    {
        return await _context.EmployeeExits
            .Where(ee => !ee.IsCompleted)
            .Include(ee => ee.Employee)
            .Include(ee => ee.Tasks)
            .OrderBy(ee => ee.ExitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeExit>> GetOverdueExitsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.EmployeeExits
            .Where(ee => !ee.IsCompleted && ee.ExitDate < today)
            .Include(ee => ee.Employee)
            .Include(ee => ee.Tasks)
            .OrderBy(ee => ee.ExitDate)
            .ToListAsync();
    }
}