using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IAttendanceService
{
    // Basic attendance operations
    Task<AttendanceRecord?> GetTodayAttendanceAsync(int employeeId);
    Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<AttendanceRecord> CheckInAsync(int employeeId, string? location = null, double? latitude = null, double? longitude = null, string? deviceInfo = null, string? ipAddress = null);
    Task<AttendanceRecord> CheckOutAsync(int employeeId, string? location = null, double? latitude = null, double? longitude = null);
    
    // Break management
    Task<BreakRecord> StartBreakAsync(int employeeId, BreakType breakType, string? location = null, double? latitude = null, double? longitude = null);
    Task<BreakRecord> EndBreakAsync(int employeeId);
    
    // Status checking
    Task<bool> IsEmployeeCheckedInAsync(int employeeId);
    Task<bool> IsEmployeeOnBreakAsync(int employeeId);
    Task<AttendanceStatus> GetEmployeeCurrentStatusAsync(int employeeId);
    
    // Real-time attendance tracking
    Task<IEnumerable<AttendanceRecord>> GetTodayBranchAttendanceAsync(int branchId);
    Task<IEnumerable<AttendanceRecord>> GetCurrentlyPresentEmployeesAsync(int branchId);
    Task<IEnumerable<AttendanceRecord>> GetEmployeesOnBreakAsync(int branchId);
    Task<IEnumerable<AttendanceRecord>> GetLateEmployeesTodayAsync(int branchId);
    
    // HR correction workflows
    Task<AttendanceRecord> CorrectAttendanceAsync(int attendanceRecordId, int correctedBy, DateTime? checkInTime = null, DateTime? checkOutTime = null, string? reason = null);
    Task<AttendanceRecord> AddMissingAttendanceAsync(int employeeId, DateTime date, DateTime checkInTime, DateTime? checkOutTime, int addedBy, string reason);
    Task<bool> DeleteAttendanceRecordAsync(int attendanceRecordId, int deletedBy, string reason);
    
    // Attendance analytics
    Task<TimeSpan> GetAverageWorkingHoursAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<int> GetLateCountAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<TimeSpan> GetTotalOvertimeAsync(int employeeId, DateTime startDate, DateTime endDate);
}