using StrideHR.Core.Entities;
using StrideHR.Core.Models.Shift;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftSwapRequestRepository : IRepository<ShiftSwapRequest>
{
    Task<IEnumerable<ShiftSwapRequest>> GetByRequesterIdAsync(int requesterId);
    Task<IEnumerable<ShiftSwapRequest>> GetByTargetEmployeeIdAsync(int targetEmployeeId);
    Task<IEnumerable<ShiftSwapRequest>> GetPendingApprovalsAsync(int managerId);
    Task<(IEnumerable<ShiftSwapRequest> Requests, int TotalCount)> SearchAsync(ShiftSwapSearchCriteria criteria);
    Task<IEnumerable<ShiftSwapRequest>> GetByBranchIdAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ShiftSwapRequest>> GetEmergencyRequestsAsync(int branchId);
    Task<ShiftSwapRequest?> GetWithDetailsAsync(int id);
}