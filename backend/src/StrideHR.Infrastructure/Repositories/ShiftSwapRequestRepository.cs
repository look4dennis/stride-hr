using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftSwapRequestRepository : Repository<ShiftSwapRequest>, IShiftSwapRequestRepository
{
    public ShiftSwapRequestRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShiftSwapRequest>> GetByRequesterIdAsync(int requesterId)
    {
        return await _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.TargetShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.Responder)
            .Where(r => r.RequesterId == requesterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftSwapRequest>> GetByTargetEmployeeIdAsync(int targetEmployeeId)
    {
        return await _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.TargetShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.Responder)
            .Where(r => r.TargetEmployeeId == targetEmployeeId || 
                       r.SwapResponses.Any(sr => sr.ResponderId == targetEmployeeId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftSwapRequest>> GetPendingApprovalsAsync(int managerId)
    {
        // Get employees managed by this manager
        var managedEmployeeIds = await _context.Employees
            .Where(e => e.ReportingManagerId == managerId)
            .Select(e => e.Id)
            .ToListAsync();

        return await _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.TargetShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.Responder)
            .Where(r => r.Status == Core.Enums.ShiftSwapStatus.ManagerApprovalRequired &&
                       (managedEmployeeIds.Contains(r.RequesterId) || 
                        (r.TargetEmployeeId.HasValue && managedEmployeeIds.Contains(r.TargetEmployeeId.Value))))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ShiftSwapRequest> Requests, int TotalCount)> SearchAsync(ShiftSwapSearchCriteria criteria)
    {
        var query = _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.TargetShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.Responder)
            .AsQueryable();

        if (criteria.RequesterId.HasValue)
            query = query.Where(r => r.RequesterId == criteria.RequesterId.Value);

        if (criteria.TargetEmployeeId.HasValue)
            query = query.Where(r => r.TargetEmployeeId == criteria.TargetEmployeeId.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(r => r.Requester.BranchId == criteria.BranchId.Value);

        if (criteria.Status.HasValue)
            query = query.Where(r => r.Status == criteria.Status.Value);

        if (criteria.IsEmergency.HasValue)
            query = query.Where(r => r.IsEmergency == criteria.IsEmergency.Value);

        if (criteria.StartDate.HasValue)
            query = query.Where(r => r.RequestedDate >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            query = query.Where(r => r.RequestedDate <= criteria.EndDate.Value);

        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(r => r.Requester.FirstName.ToLower().Contains(searchTerm) ||
                                   r.Requester.LastName.ToLower().Contains(searchTerm) ||
                                   r.Reason.ToLower().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return (requests, totalCount);
    }

    public async Task<IEnumerable<ShiftSwapRequest>> GetByBranchIdAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.SwapResponses)
            .Where(r => r.Requester.BranchId == branchId);

        if (startDate.HasValue)
            query = query.Where(r => r.RequestedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.RequestedDate <= endDate.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftSwapRequest>> GetEmergencyRequestsAsync(int branchId)
    {
        return await _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.SwapResponses)
            .Where(r => r.Requester.BranchId == branchId && 
                       r.IsEmergency && 
                       r.Status == Core.Enums.ShiftSwapStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShiftSwapRequest?> GetWithDetailsAsync(int id)
    {
        return await _context.ShiftSwapRequests
            .Include(r => r.Requester)
            .Include(r => r.RequesterShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.TargetEmployee)
            .Include(r => r.TargetShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.Responder)
            .Include(r => r.SwapResponses)
                .ThenInclude(sr => sr.ResponderShiftAssignment)
                    .ThenInclude(sa => sa.Shift)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}