using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftCoverageResponseRepository : Repository<ShiftCoverageResponse>, IShiftCoverageResponseRepository
{
    public ShiftCoverageResponseRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShiftCoverageResponse>> GetByShiftCoverageRequestIdAsync(int shiftCoverageRequestId)
    {
        return await _context.ShiftCoverageResponses
            .Include(r => r.Responder)
            .Where(r => r.ShiftCoverageRequestId == shiftCoverageRequestId)
            .OrderByDescending(r => r.RespondedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftCoverageResponse>> GetByResponderIdAsync(int responderId)
    {
        return await _context.ShiftCoverageResponses
            .Include(r => r.ShiftCoverageRequest)
                .ThenInclude(cr => cr.Requester)
            .Include(r => r.ShiftCoverageRequest)
                .ThenInclude(cr => cr.ShiftAssignment)
                    .ThenInclude(sa => sa.Shift)
            .Where(r => r.ResponderId == responderId)
            .OrderByDescending(r => r.RespondedAt)
            .ToListAsync();
    }
}