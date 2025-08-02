using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveAccrualRepository : IRepository<LeaveAccrual>
{
    Task<IEnumerable<LeaveAccrual>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveAccrual>> GetByEmployeeAndYearAsync(int employeeId, int year);
    Task<IEnumerable<LeaveAccrual>> GetByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year);
    Task<IEnumerable<LeaveAccrual>> GetPendingAccrualsAsync();
    Task<IEnumerable<LeaveAccrual>> GetAccrualsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalAccruedDaysAsync(int employeeId, int leavePolicyId, int year);
    Task<bool> HasAccrualForPeriodAsync(int employeeId, int leavePolicyId, int year, int month);
}