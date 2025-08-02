using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveApprovalHistoryRepository : IRepository<LeaveApprovalHistory>
{
    Task<IEnumerable<LeaveApprovalHistory>> GetByLeaveRequestIdAsync(int leaveRequestId);
    Task<IEnumerable<LeaveApprovalHistory>> GetByApproverIdAsync(int approverId);
}