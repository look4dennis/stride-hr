using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class GrievanceFollowUpRepository : Repository<GrievanceFollowUp>, IGrievanceFollowUpRepository
{
    public GrievanceFollowUpRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<GrievanceFollowUp>> GetByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceFollowUps
            .Include(f => f.ScheduledBy)
            .Include(f => f.CompletedBy)
            .Where(f => f.GrievanceId == grievanceId && !f.IsDeleted)
            .OrderBy(f => f.ScheduledDate)
            .ToListAsync();
    }

    public async Task<List<GrievanceFollowUp>> GetPendingFollowUpsAsync()
    {
        return await _context.GrievanceFollowUps
            .Include(f => f.Grievance)
            .Include(f => f.ScheduledBy)
            .Where(f => !f.IsCompleted && !f.IsDeleted)
            .OrderBy(f => f.ScheduledDate)
            .ToListAsync();
    }

    public async Task<List<GrievanceFollowUp>> GetOverdueFollowUpsAsync()
    {
        return await _context.GrievanceFollowUps
            .Include(f => f.Grievance)
            .Include(f => f.ScheduledBy)
            .Where(f => !f.IsCompleted && 
                       f.ScheduledDate < DateTime.UtcNow && 
                       !f.IsDeleted)
            .OrderBy(f => f.ScheduledDate)
            .ToListAsync();
    }

    public async Task<int> GetFollowUpsCountAsync(int grievanceId)
    {
        return await _context.GrievanceFollowUps
            .CountAsync(f => f.GrievanceId == grievanceId && !f.IsDeleted);
    }
}