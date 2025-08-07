using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeOnboardingTaskRepository : Repository<EmployeeOnboardingTask>, IEmployeeOnboardingTaskRepository
{
    public EmployeeOnboardingTaskRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeeOnboardingTask>> GetByOnboardingIdAsync(int onboardingId)
    {
        return await _context.EmployeeOnboardingTasks
            .Where(eot => eot.EmployeeOnboardingId == onboardingId)
            .Include(eot => eot.AssignedToEmployee)
            .OrderBy(eot => eot.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeOnboardingTask>> GetPendingTasksByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeOnboardingTasks
            .Where(eot => eot.EmployeeOnboarding.EmployeeId == employeeId && !eot.IsCompleted)
            .Include(eot => eot.EmployeeOnboarding)
            .Include(eot => eot.AssignedToEmployee)
            .OrderBy(eot => eot.CreatedAt)
            .ThenBy(eot => eot.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeOnboardingTask>> GetOverdueTasksAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.EmployeeOnboardingTasks
            .Where(eot => !eot.IsCompleted && eot.DueDate.HasValue && eot.DueDate.Value < today)
            .Include(eot => eot.EmployeeOnboarding)
                .ThenInclude(eo => eo.Employee)
            .Include(eot => eot.AssignedToEmployee)
            .OrderBy(eot => eot.DueDate)
            .ToListAsync();
    }
}