using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftSwapResponseRepository : Repository<ShiftSwapResponse>, IShiftSwapResponseRepository
{
    public ShiftSwapResponseRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShiftSwapResponse>> GetByShiftSwapRequestIdAsync(int shiftSwapRequestId)
    {
        return await _context.ShiftSwapResponses
            .Include(r => r.Responder)
            .Include(r => r.ResponderShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Where(r => r.ShiftSwapRequestId == shiftSwapRequestId)
            .OrderByDescending(r => r.RespondedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftSwapResponse>> GetByResponderIdAsync(int responderId)
    {
        return await _context.ShiftSwapResponses
            .Include(r => r.ShiftSwapRequest)
                .ThenInclude(sr => sr.Requester)
            .Include(r => r.ResponderShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Where(r => r.ResponderId == responderId)
            .OrderByDescending(r => r.RespondedAt)
            .ToListAsync();
    }
}