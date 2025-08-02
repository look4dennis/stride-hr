using StrideHR.Core.Entities;
using StrideHR.Core.Models;

namespace StrideHR.Core.Interfaces.Services;

public interface IReportSchedulingService
{
    Task<ReportSchedule> CreateScheduleAsync(ReportScheduleRequest request, int userId);
    Task<ReportSchedule> UpdateScheduleAsync(int scheduleId, ReportScheduleRequest request, int userId);
    Task<bool> DeleteScheduleAsync(int scheduleId, int userId);
    Task<ReportSchedule?> GetScheduleAsync(int scheduleId, int userId);
    Task<IEnumerable<ReportSchedule>> GetReportSchedulesAsync(int reportId, int userId);
    Task<IEnumerable<ReportSchedule>> GetUserSchedulesAsync(int userId);
    Task<bool> ActivateScheduleAsync(int scheduleId, int userId);
    Task<bool> DeactivateScheduleAsync(int scheduleId, int userId);
    Task ExecuteScheduledReportsAsync();
    Task<DateTime?> GetNextRunTimeAsync(string cronExpression);
    Task<bool> ValidateCronExpressionAsync(string cronExpression);
}