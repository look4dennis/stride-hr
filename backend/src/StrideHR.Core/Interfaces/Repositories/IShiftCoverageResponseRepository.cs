using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftCoverageResponseRepository : IRepository<ShiftCoverageResponse>
{
    Task<IEnumerable<ShiftCoverageResponse>> GetByShiftCoverageRequestIdAsync(int shiftCoverageRequestId);
    Task<IEnumerable<ShiftCoverageResponse>> GetByResponderIdAsync(int responderId);
}