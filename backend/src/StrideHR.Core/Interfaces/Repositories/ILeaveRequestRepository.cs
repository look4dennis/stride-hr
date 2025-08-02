using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync(int branchId);
    Task<IEnumerable<LeaveRequest>> GetRequestsForApprovalAsync(int approverId);
    Task<IEnumerable<LeaveRequest>> GetRequestsByDateRangeAsync(DateTime startDate, DateTime endDate, int branchId);
    Task<IEnumerable<LeaveRequest>> GetConflictingRequestsAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LeaveRequest>> GetTeamRequestsAsync(int managerId, DateTime startDate, DateTime endDate);
    Task<LeaveRequest?> GetWithDetailsAsync(int id);
    Task<bool> HasOverlappingRequestsAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null);
    Task<IEnumerable<LeaveRequest>> GetApprovedRequestsByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year);
}