using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveBalanceRepository : IRepository<LeaveBalance>
{
    Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveBalance>> GetByEmployeeAndYearAsync(int employeeId, int year);
    Task<LeaveBalance?> GetByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year);
    Task<IEnumerable<LeaveBalance>> GetByYearAsync(int year, int branchId);
    Task<decimal> GetRemainingBalanceAsync(int employeeId, int leavePolicyId, int year);
}