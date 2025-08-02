using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ReportExecutionRepository : Repository<ReportExecution>, IReportExecutionRepository
{
    public ReportExecutionRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReportExecution>> GetExecutionsByReportAsync(int reportId, int limit = 50)
    {
        return await _context.ReportExecutions
            .Include(re => re.ExecutedByEmployee)
            .Where(re => re.ReportId == reportId)
            .OrderByDescending(re => re.ExecutedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetExecutionsByUserAsync(int userId, int limit = 50)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.ExecutedByEmployee)
            .Where(re => re.ExecutedBy == userId)
            .OrderByDescending(re => re.ExecutedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetFailedExecutionsAsync()
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.ExecutedByEmployee)
            .Where(re => re.Status == ReportExecutionStatus.Failed)
            .OrderByDescending(re => re.ExecutedAt)
            .ToListAsync();
    }

    public async Task<ReportExecution?> GetLatestExecutionAsync(int reportId)
    {
        return await _context.ReportExecutions
            .Include(re => re.ExecutedByEmployee)
            .Where(re => re.ReportId == reportId)
            .OrderByDescending(re => re.ExecutedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ReportExecution>> GetExecutionsByStatusAsync(ReportExecutionStatus status)
    {
        return await _context.ReportExecutions
            .Include(re => re.Report)
            .Include(re => re.ExecutedByEmployee)
            .Where(re => re.Status == status)
            .OrderByDescending(re => re.ExecutedAt)
            .ToListAsync();
    }
}