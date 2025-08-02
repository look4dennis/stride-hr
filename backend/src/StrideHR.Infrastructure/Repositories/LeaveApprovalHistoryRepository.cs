using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveApprovalHistoryRepository : Repository<LeaveApprovalHistory>, ILeaveApprovalHistoryRepository
{
    public LeaveApprovalHistoryRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveApprovalHistory>> GetByLeaveRequestIdAsync(int leaveRequestId)
    {
        return await _dbSet
            .Include(lah => lah.LeaveRequest)
            .Include(lah => lah.Approver)
            .Include(lah => lah.EscalatedTo)
            .Where(lah => lah.LeaveRequestId == leaveRequestId && !lah.IsDeleted)
            .OrderBy(lah => lah.ActionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveApprovalHistory>> GetByApproverIdAsync(int approverId)
    {
        return await _dbSet
            .Include(lah => lah.LeaveRequest)
                .ThenInclude(lr => lr.Employee)
            .Include(lah => lah.Approver)
            .Include(lah => lah.EscalatedTo)
            .Where(lah => lah.ApproverId == approverId && !lah.IsDeleted)
            .OrderByDescending(lah => lah.ActionDate)
            .ToListAsync();
    }
}