using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IAttendanceService
{
    Task<AttendanceRecord?> GetTodayAttendanceAsync(int employeeId);
    Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<AttendanceRecord> CheckInAsync(int employeeId, string? location = null);
    Task<AttendanceRecord> CheckOutAsync(int employeeId);
    Task<BreakRecord> StartBreakAsync(int employeeId, Core.Enums.BreakType breakType);
    Task<BreakRecord> EndBreakAsync(int employeeId);
    Task<bool> IsEmployeeCheckedInAsync(int employeeId);
    Task<bool> IsEmployeeOnBreakAsync(int employeeId);
}