using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveEncashmentRepository : IRepository<LeaveEncashment>
{
    Task<IEnumerable<LeaveEncashment>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveEncashment>> GetByEmployeeAndYearAsync(int employeeId, int year);
    Task<IEnumerable<LeaveEncashment>> GetPendingEncashmentsAsync(int branchId);
    Task<IEnumerable<LeaveEncashment>> GetByStatusAsync(EncashmentStatus status);
    Task<decimal> GetTotalEncashedDaysAsync(int employeeId, int leavePolicyId, int year);
    Task<decimal> GetTotalEncashmentAmountAsync(int employeeId, int year);
    Task<bool> HasPendingEncashmentAsync(int employeeId, int leavePolicyId, int year);
}