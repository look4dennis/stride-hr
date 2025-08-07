using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IEmployeeOnboardingRepository : IRepository<EmployeeOnboarding>
{
    Task<EmployeeOnboarding?> GetByEmployeeIdAsync(int employeeId);
    Task<EmployeeOnboarding?> GetActiveOnboardingByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeOnboarding>> GetPendingOnboardingsAsync();
    Task<IEnumerable<EmployeeOnboarding>> GetOverdueOnboardingsAsync();
}