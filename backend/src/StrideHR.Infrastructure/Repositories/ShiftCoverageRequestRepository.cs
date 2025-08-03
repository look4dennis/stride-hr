using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ShiftCoverageRequestRepository : Repository<ShiftCoverageRequest>, IShiftCoverageRequestRepository
{
    public ShiftCoverageRequestRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShiftCoverageRequest>> GetByRequesterIdAsync(int requesterId)
    {
        return await _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.AcceptedByEmployee)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.CoverageResponses)
                .ThenInclude(cr => cr.Responder)
            .Where(r => r.RequesterId == requesterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftCoverageRequest>> GetPendingApprovalsAsync(int managerId)
    {
        // Get employees managed by this manager
        var managedEmployeeIds = await _context.Employees
            .Where(e => e.ReportingManagerId == managerId)
            .Select(e => e.Id)
            .ToListAsync();

        return await _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.AcceptedByEmployee)
            .Include(r => r.CoverageResponses)
                .ThenInclude(cr => cr.Responder)
            .Where(r => r.Status == Core.Enums.ShiftCoverageRequestStatus.Accepted &&
                       r.ApprovedBy == null &&
                       (managedEmployeeIds.Contains(r.RequesterId) || 
                        (r.AcceptedBy.HasValue && managedEmployeeIds.Contains(r.AcceptedBy.Value))))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ShiftCoverageRequest> Requests, int TotalCount)> SearchAsync(ShiftCoverageSearchCriteria criteria)
    {
        var query = _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.AcceptedByEmployee)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.CoverageResponses)
                .ThenInclude(cr => cr.Responder)
            .AsQueryable();

        if (criteria.RequesterId.HasValue)
            query = query.Where(r => r.RequesterId == criteria.RequesterId.Value);

        if (criteria.AcceptedBy.HasValue)
            query = query.Where(r => r.AcceptedBy == criteria.AcceptedBy.Value);

        if (criteria.BranchId.HasValue)
            query = query.Where(r => r.Requester.BranchId == criteria.BranchId.Value);

        if (criteria.Status.HasValue)
            query = query.Where(r => r.Status == criteria.Status.Value);

        if (criteria.IsEmergency.HasValue)
            query = query.Where(r => r.IsEmergency == criteria.IsEmergency.Value);

        if (criteria.StartDate.HasValue)
            query = query.Where(r => r.ShiftDate >= criteria.StartDate.Value);

        if (criteria.EndDate.HasValue)
            query = query.Where(r => r.ShiftDate <= criteria.EndDate.Value);

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

    public async Task<IEnumerable<ShiftCoverageRequest>> GetByBranchIdAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.AcceptedByEmployee)
            .Include(r => r.CoverageResponses)
            .Where(r => r.Requester.BranchId == branchId);

        if (startDate.HasValue)
            query = query.Where(r => r.ShiftDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.ShiftDate <= endDate.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftCoverageRequest>> GetEmergencyRequestsAsync(int branchId)
    {
        return await _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.CoverageResponses)
            .Where(r => r.Requester.BranchId == branchId && 
                       r.IsEmergency && 
                       r.Status == Core.Enums.ShiftCoverageRequestStatus.Open)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShiftCoverageRequest>> GetOpenRequestsAsync(int branchId)
    {
        return await _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.CoverageResponses)
            .Where(r => r.Requester.BranchId == branchId && 
                       r.Status == Core.Enums.ShiftCoverageRequestStatus.Open &&
                       (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShiftCoverageRequest?> GetWithDetailsAsync(int id)
    {
        return await _context.ShiftCoverageRequests
            .Include(r => r.Requester)
            .Include(r => r.ShiftAssignment)
                .ThenInclude(sa => sa.Shift)
            .Include(r => r.AcceptedByEmployee)
            .Include(r => r.ApprovedByEmployee)
            .Include(r => r.CoverageResponses)
                .ThenInclude(cr => cr.Responder)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ShiftCoverageRequest>> GetExpiredRequestsAsync()
    {
        return await _context.ShiftCoverageRequests
            .Where(r => r.Status == Core.Enums.ShiftCoverageRequestStatus.Open &&
                       r.ExpiresAt.HasValue &&
                       r.ExpiresAt.Value <= DateTime.UtcNow)
            .ToListAsync();
    }
}