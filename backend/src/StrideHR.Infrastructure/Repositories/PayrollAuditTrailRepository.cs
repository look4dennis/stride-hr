using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class PayrollAuditTrailRepository : Repository<PayrollAuditTrail>, IPayrollAuditTrailRepository
{
    public PayrollAuditTrailRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<PayrollAuditTrail>> GetByPayrollRecordIdAsync(int payrollRecordId)
    {
        return await _context.Set<PayrollAuditTrail>()
            .Where(at => at.PayrollRecordId == payrollRecordId)
            .Include(at => at.Employee)
            .Include(at => at.User)
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync();
    }

    public async Task<List<PayrollAuditTrail>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<PayrollAuditTrail>()
            .Where(at => at.EmployeeId == employeeId)
            .Include(at => at.PayrollRecord)
            .Include(at => at.User)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(at => at.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(at => at.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync();
    }

    public async Task<List<PayrollAuditTrail>> GetByActionAsync(PayrollAuditAction action, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<PayrollAuditTrail>()
            .Where(at => at.Action == action)
            .Include(at => at.Employee)
            .Include(at => at.User)
            .Include(at => at.PayrollRecord)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(at => at.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(at => at.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync();
    }

    public async Task<List<PayrollAuditTrail>> GetByUserIdAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<PayrollAuditTrail>()
            .Where(at => at.UserId == userId)
            .Include(at => at.Employee)
            .Include(at => at.PayrollRecord)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(at => at.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(at => at.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync();
    }

    public async Task<(List<PayrollAuditTrail> items, int totalCount)> GetPagedAsync(PayrollAuditTrailRequest request)
    {
        var query = _context.Set<PayrollAuditTrail>()
            .Include(at => at.Employee)
            .Include(at => at.User)
            .Include(at => at.PayrollRecord)
            .AsQueryable();

        // Apply filters
        if (request.PayrollRecordId.HasValue)
        {
            query = query.Where(at => at.PayrollRecordId == request.PayrollRecordId.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(at => at.EmployeeId == request.EmployeeId.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(at => at.Employee.BranchId == request.BranchId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(at => at.Timestamp >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(at => at.Timestamp <= request.EndDate.Value);
        }

        if (request.Action.HasValue)
        {
            query = query.Where(at => at.Action == request.Action.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(at => at.UserId == request.UserId.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(at => at.Timestamp)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}