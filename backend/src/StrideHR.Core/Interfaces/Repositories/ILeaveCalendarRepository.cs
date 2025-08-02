using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ILeaveCalendarRepository : IRepository<LeaveCalendar>
{
    Task<IEnumerable<LeaveCalendar>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int branchId);
    Task<IEnumerable<LeaveCalendar>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<LeaveCalendar>> GetTeamCalendarAsync(int managerId, DateTime startDate, DateTime endDate);
}