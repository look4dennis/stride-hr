using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IReportScheduleRepository : IRepository<ReportSchedule>
{
    Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync();
    Task<IEnumerable<ReportSchedule>> GetSchedulesDueForExecutionAsync();
    Task<IEnumerable<ReportSchedule>> GetSchedulesByReportAsync(int reportId);
    Task UpdateNextRunTimeAsync(int scheduleId, DateTime nextRunTime);
}