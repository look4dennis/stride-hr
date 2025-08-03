using StrideHR.Core.Entities;
using StrideHR.Core.Models.Shift;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftCoverageRequestRepository : IRepository<ShiftCoverageRequest>
{
    Task<IEnumerable<ShiftCoverageRequest>> GetByRequesterIdAsync(int requesterId);
    Task<IEnumerable<ShiftCoverageRequest>> GetPendingApprovalsAsync(int managerId);
    Task<(IEnumerable<ShiftCoverageRequest> Requests, int TotalCount)> SearchAsync(ShiftCoverageSearchCriteria criteria);
    Task<IEnumerable<ShiftCoverageRequest>> GetByBranchIdAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ShiftCoverageRequest>> GetEmergencyRequestsAsync(int branchId);
    Task<IEnumerable<ShiftCoverageRequest>> GetOpenRequestsAsync(int branchId);
    Task<ShiftCoverageRequest?> GetWithDetailsAsync(int id);
    Task<IEnumerable<ShiftCoverageRequest>> GetExpiredRequestsAsync();
}