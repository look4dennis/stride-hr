using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class GrievanceCommentRepository : Repository<GrievanceComment>, IGrievanceCommentRepository
{
    public GrievanceCommentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<GrievanceComment>> GetByGrievanceIdAsync(int grievanceId, bool includeInternal = false)
    {
        var query = _context.GrievanceComments
            .Include(c => c.Author)
            .Where(c => c.GrievanceId == grievanceId && !c.IsDeleted);

        if (!includeInternal)
        {
            query = query.Where(c => !c.IsInternal);
        }

        return await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCommentsCountAsync(int grievanceId)
    {
        return await _context.GrievanceComments
            .CountAsync(c => c.GrievanceId == grievanceId && !c.IsDeleted);
    }
}