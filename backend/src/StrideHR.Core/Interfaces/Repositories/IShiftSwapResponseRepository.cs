using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftSwapResponseRepository : IRepository<ShiftSwapResponse>
{
    Task<IEnumerable<ShiftSwapResponse>> GetByShiftSwapRequestIdAsync(int shiftSwapRequestId);
    Task<IEnumerable<ShiftSwapResponse>> GetByResponderIdAsync(int responderId);
}