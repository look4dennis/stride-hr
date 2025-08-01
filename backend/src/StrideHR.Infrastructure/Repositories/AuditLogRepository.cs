using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (filter.FromDate.HasValue)
        {
            query = query.Where(al => al.Timestamp >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(al => al.Timestamp <= filter.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.EventType))
        {
            query = query.Where(al => al.EventType.Contains(filter.EventType));
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(al => al.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrEmpty(filter.IpAddress))
        {
            query = query.Where(al => al.IpAddress == filter.IpAddress);
        }

        return await query
            .Include(al => al.User)
            .OrderByDescending(al => al.Timestamp)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AuditLogs
            .Where(al => al.UserId == userId);

        if (fromDate.HasValue)
        {
            query = query.Where(al => al.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(al => al.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(al => al.Timestamp)
            .Take(100) // Limit to last 100 entries
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetSecurityAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AuditLogs
            .Where(al => al.IsSecurityEvent);

        if (fromDate.HasValue)
        {
            query = query.Where(al => al.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(al => al.Timestamp <= toDate.Value);
        }

        return await query
            .Include(al => al.User)
            .OrderByDescending(al => al.Timestamp)
            .Take(500) // Limit to last 500 security events
            .ToListAsync();
    }

    public async Task<int> GetAuditLogCountAsync(AuditLogFilter filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (filter.FromDate.HasValue)
        {
            query = query.Where(al => al.Timestamp >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(al => al.Timestamp <= filter.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.EventType))
        {
            query = query.Where(al => al.EventType.Contains(filter.EventType));
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(al => al.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrEmpty(filter.IpAddress))
        {
            query = query.Where(al => al.IpAddress == filter.IpAddress);
        }

        return await query.CountAsync();
    }
}