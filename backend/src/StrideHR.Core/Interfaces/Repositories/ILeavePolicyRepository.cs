using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeavePolicyRepository : IRepository<LeavePolicy>
{
    Task<IEnumerable<LeavePolicy>> GetByBranchIdAsync(int branchId);
    Task<LeavePolicy?> GetByBranchAndTypeAsync(int branchId, LeaveType leaveType);
    Task<IEnumerable<LeavePolicy>> GetActiveByBranchIdAsync(int branchId);
}