using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeExitTaskRepository : Repository<EmployeeExitTask>, IEmployeeExitTaskRepository
{
    public EmployeeExitTaskRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeeExitTask>> GetByExitIdAsync(int exitId)
    {
        return await _context.EmployeeExitTasks
            .Where(eet => eet.EmployeeExitId == exitId)
            .Include(eet => eet.CompletedByEmployee)
            .OrderBy(eet => eet.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeExitTask>> GetPendingTasksByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeExitTasks
            .Where(eet => eet.EmployeeExit.EmployeeId == employeeId && !eet.IsCompleted)
            .Include(eet => eet.EmployeeExit)
            .Include(eet => eet.CompletedByEmployee)
            .OrderBy(eet => eet.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeExitTask>> GetOverdueTasksAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.EmployeeExitTasks
            .Where(eet => !eet.IsCompleted && eet.EmployeeExit.ExitDate < today)
            .Include(eet => eet.EmployeeExit)
                .ThenInclude(ee => ee.Employee)
            .Include(eet => eet.CompletedByEmployee)
            .OrderBy(eet => eet.EmployeeExit.ExitDate)
            .ToListAsync();
    }
}