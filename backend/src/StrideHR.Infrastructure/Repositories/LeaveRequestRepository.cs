using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.ApprovedByEmployee)
            .Include(lr => lr.ApprovalHistory)
                .ThenInclude(ah => ah.Approver)
            .Where(lr => lr.EmployeeId == employeeId && !lr.IsDeleted)
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync(int branchId)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.ApprovalHistory)
                .ThenInclude(ah => ah.Approver)
            .Where(lr => lr.Employee.BranchId == branchId && 
                        lr.Status == LeaveStatus.Pending && 
                        !lr.IsDeleted)
            .OrderBy(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetRequestsForApprovalAsync(int approverId)
    {
        // Get requests where the approver is the reporting manager or HR
        return await _dbSet
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.ReportingManager)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.ApprovalHistory)
                .ThenInclude(ah => ah.Approver)
            .Where(lr => (lr.Employee.ReportingManagerId == approverId || 
                         lr.ApprovalHistory.Any(ah => ah.ApproverId == approverId && ah.Action == ApprovalAction.Pending)) &&
                        lr.Status == LeaveStatus.Pending && 
                        !lr.IsDeleted)
            .OrderBy(lr => lr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetRequestsByDateRangeAsync(DateTime startDate, DateTime endDate, int branchId)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Where(lr => lr.Employee.BranchId == branchId &&
                        lr.StartDate <= endDate &&
                        lr.EndDate >= startDate &&
                        lr.Status == LeaveStatus.Approved &&
                        !lr.IsDeleted)
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetConflictingRequestsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Where(lr => lr.EmployeeId == employeeId &&
                        lr.StartDate <= endDate &&
                        lr.EndDate >= startDate &&
                        (lr.Status == LeaveStatus.Approved || lr.Status == LeaveStatus.Pending) &&
                        !lr.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetTeamRequestsAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Where(lr => lr.Employee.ReportingManagerId == managerId &&
                        lr.StartDate <= endDate &&
                        lr.EndDate >= startDate &&
                        lr.Status == LeaveStatus.Approved &&
                        !lr.IsDeleted)
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();
    }

    public async Task<LeaveRequest?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
                .ThenInclude(e => e.ReportingManager)
            .Include(lr => lr.LeavePolicy)
            .Include(lr => lr.ApprovedByEmployee)
            .Include(lr => lr.ApprovalHistory)
                .ThenInclude(ah => ah.Approver)
            .Include(lr => lr.ApprovalHistory)
                .ThenInclude(ah => ah.EscalatedTo)
            .FirstOrDefaultAsync(lr => lr.Id == id && !lr.IsDeleted);
    }

    public async Task<bool> HasOverlappingRequestsAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null)
    {
        var query = _dbSet.Where(lr => lr.EmployeeId == employeeId &&
                                      lr.StartDate <= endDate &&
                                      lr.EndDate >= startDate &&
                                      (lr.Status == LeaveStatus.Approved || lr.Status == LeaveStatus.Pending) &&
                                      !lr.IsDeleted);

        if (excludeRequestId.HasValue)
        {
            query = query.Where(lr => lr.Id != excludeRequestId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetApprovedRequestsByEmployeeAndPolicyAsync(int employeeId, int leavePolicyId, int year)
    {
        return await _dbSet
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeavePolicy)
            .Where(lr => lr.EmployeeId == employeeId &&
                        lr.LeavePolicyId == leavePolicyId &&
                        lr.StartDate.Year == year &&
                        lr.Status == LeaveStatus.Approved &&
                        !lr.IsDeleted)
            .OrderBy(lr => lr.StartDate)
            .ToListAsync();
    }
}