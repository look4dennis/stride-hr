using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ReportScheduleRepository : Repository<ReportSchedule>, IReportScheduleRepository
{
    public ReportScheduleRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync()
    {
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Include(rs => rs.CreatedByEmployee)
            .Where(rs => rs.IsActive)
            .OrderBy(rs => rs.NextRunTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetSchedulesDueForExecutionAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ReportSchedules
            .Include(rs => rs.Report)
            .Include(rs => rs.CreatedByEmployee)
            .Where(rs => rs.IsActive && rs.NextRunTime <= now)
            .OrderBy(rs => rs.NextRunTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReportSchedule>> GetSchedulesByReportAsync(int reportId)
    {
        return await _context.ReportSchedules
            .Include(rs => rs.CreatedByEmployee)
            .Where(rs => rs.ReportId == reportId)
            .OrderByDescending(rs => rs.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateNextRunTimeAsync(int scheduleId, DateTime nextRunTime)
    {
        var schedule = await _context.ReportSchedules.FindAsync(scheduleId);
        if (schedule != null)
        {
            schedule.LastRunTime = schedule.NextRunTime;
            schedule.NextRunTime = nextRunTime;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}