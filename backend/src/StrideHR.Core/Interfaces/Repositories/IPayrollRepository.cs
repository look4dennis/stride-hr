using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayrollRepository : IRepository<PayrollRecord>
{
    Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int year, int month);
    Task<List<PayrollRecord>> GetByBranchAndPeriodAsync(int branchId, int year, int month);
    Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId, int year, int? month = null);
    Task<bool> ExistsForPeriodAsync(int employeeId, int year, int month);
}