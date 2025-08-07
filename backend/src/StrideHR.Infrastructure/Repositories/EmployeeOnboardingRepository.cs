using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class EmployeeOnboardingRepository : Repository<EmployeeOnboarding>, IEmployeeOnboardingRepository
{
    public EmployeeOnboardingRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<EmployeeOnboarding?> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeOnboardings
            .Where(eo => eo.EmployeeId == employeeId)
            .Include(eo => eo.Employee)
            .Include(eo => eo.Tasks)
            .OrderByDescending(eo => eo.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeOnboarding?> GetActiveOnboardingByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeOnboardings
            .Where(eo => eo.EmployeeId == employeeId && !eo.IsCompleted)
            .Include(eo => eo.Employee)
            .Include(eo => eo.Tasks)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<EmployeeOnboarding>> GetPendingOnboardingsAsync()
    {
        return await _context.EmployeeOnboardings
            .Where(eo => !eo.IsCompleted)
            .Include(eo => eo.Employee)
            .Include(eo => eo.Tasks)
            .OrderBy(eo => eo.OnboardingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmployeeOnboarding>> GetOverdueOnboardingsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.EmployeeOnboardings
            .Where(eo => !eo.IsCompleted && eo.OnboardingDate < today)
            .Include(eo => eo.Employee)
            .Include(eo => eo.Tasks)
            .OrderBy(eo => eo.OnboardingDate)
            .ToListAsync();
    }
}