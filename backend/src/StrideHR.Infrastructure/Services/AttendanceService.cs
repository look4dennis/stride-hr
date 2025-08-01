using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttendanceService> _logger;
    private readonly IAuditLogService _auditLogService;

    public AttendanceService(
        IUnitOfWork unitOfWork, 
        ILogger<AttendanceService> logger,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<AttendanceRecord?> GetTodayAttendanceAsync(int employeeId)
    {
        var today = DateTime.Today;
        return await _unitOfWork.AttendanceRecords.FirstOrDefaultAsync(
            a => a.EmployeeId == employeeId && a.Date == today,
            a => a.BreakRecords
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate,
            a => a.BreakRecords
        );
    }

    public async Task<AttendanceRecord> CheckInAsync(int employeeId, string? location = null, double? latitude = null, double? longitude = null, string? deviceInfo = null, string? ipAddress = null)
    {
        var today = DateTime.Today;
        var existingRecord = await GetTodayAttendanceAsync(employeeId);

        if (existingRecord != null && existingRecord.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Employee has already checked in today.");
        }

        var checkInTime = DateTime.Now;
        var record = existingRecord ?? new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = today
        };

        record.CheckInTime = checkInTime;
        record.CheckInLocation = location;
        record.CheckInLatitude = latitude;
        record.CheckInLongitude = longitude;
        record.CheckInTimeZone = TimeZoneInfo.Local.Id;
        record.DeviceInfo = deviceInfo;
        record.IpAddress = ipAddress;
        record.Status = AttendanceStatus.Present;

        // Check if employee is late
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee?.Branch?.Organization != null)
        {
            var expectedStartTime = DateTime.Today.Add(employee.Branch.Organization.NormalWorkingHours);
            if (checkInTime > expectedStartTime)
            {
                record.IsLate = true;
                record.LateBy = checkInTime - expectedStartTime;
                record.Status = AttendanceStatus.Late;
            }
        }

        if (existingRecord == null)
        {
            await _unitOfWork.AttendanceRecords.AddAsync(record);
        }
        else
        {
            await _unitOfWork.AttendanceRecords.UpdateAsync(record);
        }

        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Employee {EmployeeId} checked in at {CheckInTime} from location {Location}", 
            employeeId, checkInTime, location);

        return record;
    }

    public async Task<AttendanceRecord> CheckOutAsync(int employeeId, string? location = null, double? latitude = null, double? longitude = null)
    {
        var record = await GetTodayAttendanceAsync(employeeId);
        
        if (record == null || !record.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Employee has not checked in today.");
        }

        if (record.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Employee has already checked out today.");
        }

        // End any active break
        var activeBreak = record.BreakRecords.FirstOrDefault(b => !b.EndTime.HasValue);
        if (activeBreak != null)
        {
            activeBreak.EndTime = DateTime.Now;
            activeBreak.Duration = activeBreak.EndTime.Value - activeBreak.StartTime;
            await _unitOfWork.BreakRecords.UpdateAsync(activeBreak);
        }

        var checkOutTime = DateTime.Now;
        record.CheckOutTime = checkOutTime;
        record.CheckOutLocation = location;
        record.CheckOutLatitude = latitude;
        record.CheckOutLongitude = longitude;
        record.CheckOutTimeZone = TimeZoneInfo.Local.Id;
        record.TotalWorkingHours = checkOutTime - record.CheckInTime.Value;

        // Calculate break duration
        var totalBreakDuration = record.BreakRecords
            .Where(b => b.Duration.HasValue)
            .Sum(b => b.Duration!.Value.Ticks);
        
        record.BreakDuration = new TimeSpan(totalBreakDuration);
        
        // Calculate overtime if applicable
        var workingHours = record.TotalWorkingHours.Value - record.BreakDuration.Value;
        var standardHours = TimeSpan.FromHours(8); // This should come from organization settings
        
        if (workingHours > standardHours)
        {
            record.OvertimeHours = workingHours - standardHours;
        }

        // Check for early checkout
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee?.Branch?.Organization != null)
        {
            var expectedEndTime = DateTime.Today.Add(employee.Branch.Organization.NormalWorkingHours).AddHours(8);
            if (checkOutTime < expectedEndTime)
            {
                record.IsEarlyOut = true;
                record.EarlyOutBy = expectedEndTime - checkOutTime;
            }
        }

        await _unitOfWork.AttendanceRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Employee {EmployeeId} checked out at {CheckOutTime} from location {Location}", 
            employeeId, checkOutTime, location);
        
        return record;
    }

    public async Task<BreakRecord> StartBreakAsync(int employeeId, BreakType breakType, string? location = null, double? latitude = null, double? longitude = null)
    {
        var attendanceRecord = await GetTodayAttendanceAsync(employeeId);
        
        if (attendanceRecord == null || !attendanceRecord.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Employee must check in before taking a break.");
        }

        if (attendanceRecord.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Employee has already checked out today.");
        }

        // Check if employee is already on break
        var activeBreak = attendanceRecord.BreakRecords
            .FirstOrDefault(b => !b.EndTime.HasValue);
            
        if (activeBreak != null)
        {
            throw new InvalidOperationException("Employee is already on break.");
        }

        var breakRecord = new BreakRecord
        {
            AttendanceRecordId = attendanceRecord.Id,
            Type = breakType,
            StartTime = DateTime.Now,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            TimeZone = TimeZoneInfo.Local.Id
        };

        await _unitOfWork.BreakRecords.AddAsync(breakRecord);
        
        // Update attendance status
        attendanceRecord.Status = AttendanceStatus.OnBreak;
        await _unitOfWork.AttendanceRecords.UpdateAsync(attendanceRecord);
        
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Employee {EmployeeId} started {BreakType} break at {StartTime}", 
            employeeId, breakType, breakRecord.StartTime);
        
        return breakRecord;
    }

    public async Task<BreakRecord> EndBreakAsync(int employeeId)
    {
        var attendanceRecord = await GetTodayAttendanceAsync(employeeId);
        
        if (attendanceRecord == null)
        {
            throw new InvalidOperationException("No attendance record found for today.");
        }

        var activeBreak = attendanceRecord.BreakRecords
            .FirstOrDefault(b => !b.EndTime.HasValue);
            
        if (activeBreak == null)
        {
            throw new InvalidOperationException("Employee is not currently on break.");
        }

        activeBreak.EndTime = DateTime.Now;
        activeBreak.Duration = activeBreak.EndTime.Value - activeBreak.StartTime;

        await _unitOfWork.BreakRecords.UpdateAsync(activeBreak);
        
        // Update attendance status back to present
        attendanceRecord.Status = AttendanceStatus.Present;
        await _unitOfWork.AttendanceRecords.UpdateAsync(attendanceRecord);
        
        await _unitOfWork.SaveChangesAsync();
        
        return activeBreak;
    }

    public async Task<bool> IsEmployeeCheckedInAsync(int employeeId)
    {
        var record = await GetTodayAttendanceAsync(employeeId);
        return record?.CheckInTime.HasValue == true && !record.CheckOutTime.HasValue;
    }

    public async Task<bool> IsEmployeeOnBreakAsync(int employeeId)
    {
        var record = await GetTodayAttendanceAsync(employeeId);
        return record?.Status == AttendanceStatus.OnBreak;
    }

    public async Task<AttendanceStatus> GetEmployeeCurrentStatusAsync(int employeeId)
    {
        var record = await GetTodayAttendanceAsync(employeeId);
        return record?.Status ?? AttendanceStatus.Absent;
    }

    public async Task<IEnumerable<AttendanceRecord>> GetTodayBranchAttendanceAsync(int branchId)
    {
        var today = DateTime.Today;
        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.Employee.BranchId == branchId && a.Date == today,
            a => a.Employee, a => a.BreakRecords
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetCurrentlyPresentEmployeesAsync(int branchId)
    {
        var today = DateTime.Today;
        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.Employee.BranchId == branchId && 
                 a.Date == today && 
                 a.CheckInTime.HasValue && 
                 !a.CheckOutTime.HasValue &&
                 (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.OnBreak),
            a => a.Employee, a => a.BreakRecords
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetEmployeesOnBreakAsync(int branchId)
    {
        var today = DateTime.Today;
        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.Employee.BranchId == branchId && 
                 a.Date == today && 
                 a.Status == AttendanceStatus.OnBreak,
            a => a.Employee, a => a.BreakRecords
        );
    }

    public async Task<IEnumerable<AttendanceRecord>> GetLateEmployeesTodayAsync(int branchId)
    {
        var today = DateTime.Today;
        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.Employee.BranchId == branchId && 
                 a.Date == today && 
                 a.IsLate,
            a => a.Employee
        );
    }

    public async Task<AttendanceRecord> CorrectAttendanceAsync(int attendanceRecordId, int correctedBy, DateTime? checkInTime = null, DateTime? checkOutTime = null, string? reason = null)
    {
        var record = await _unitOfWork.AttendanceRecords.GetByIdAsync(attendanceRecordId);
        if (record == null)
        {
            throw new InvalidOperationException("Attendance record not found.");
        }

        var originalCheckIn = record.CheckInTime;
        var originalCheckOut = record.CheckOutTime;

        if (checkInTime.HasValue)
        {
            record.CheckInTime = checkInTime.Value;
        }

        if (checkOutTime.HasValue)
        {
            record.CheckOutTime = checkOutTime.Value;
        }

        // Recalculate working hours if both times are available
        if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
        {
            record.TotalWorkingHours = record.CheckOutTime.Value - record.CheckInTime.Value;
            
            // Recalculate break duration
            var totalBreakDuration = record.BreakRecords
                .Where(b => b.Duration.HasValue)
                .Sum(b => b.Duration!.Value.Ticks);
            
            record.BreakDuration = new TimeSpan(totalBreakDuration);
            
            // Recalculate overtime
            var workingHours = record.TotalWorkingHours.Value - record.BreakDuration.Value;
            var standardHours = TimeSpan.FromHours(8);
            
            if (workingHours > standardHours)
            {
                record.OvertimeHours = workingHours - standardHours;
            }
            else
            {
                record.OvertimeHours = null;
            }
        }

        record.CorrectionReason = reason;
        record.CorrectedBy = correctedBy;
        record.CorrectedAt = DateTime.Now;

        await _unitOfWork.AttendanceRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        // Log the correction
        await _auditLogService.LogEventAsync(
            "AttendanceCorrection",
            $"Attendance corrected for employee {record.EmployeeId}. Original CheckIn: {originalCheckIn}, New CheckIn: {record.CheckInTime}. Original CheckOut: {originalCheckOut}, New CheckOut: {record.CheckOutTime}. Reason: {reason}",
            correctedBy
        );

        _logger.LogInformation("Attendance record {RecordId} corrected by user {CorrectedBy}. Reason: {Reason}", 
            attendanceRecordId, correctedBy, reason);

        return record;
    }

    public async Task<AttendanceRecord> AddMissingAttendanceAsync(int employeeId, DateTime date, DateTime checkInTime, DateTime? checkOutTime, int addedBy, string reason)
    {
        // Check if attendance record already exists for this date
        var existingRecord = await _unitOfWork.AttendanceRecords.FirstOrDefaultAsync(
            a => a.EmployeeId == employeeId && a.Date.Date == date.Date
        );

        if (existingRecord != null)
        {
            throw new InvalidOperationException("Attendance record already exists for this date.");
        }

        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = date.Date,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            Status = AttendanceStatus.Present,
            CorrectionReason = reason,
            CorrectedBy = addedBy,
            CorrectedAt = DateTime.Now
        };

        // Calculate working hours if checkout time is provided
        if (checkOutTime.HasValue)
        {
            record.TotalWorkingHours = checkOutTime.Value - checkInTime;
            
            // Calculate overtime
            var workingHours = record.TotalWorkingHours.Value;
            var standardHours = TimeSpan.FromHours(8);
            
            if (workingHours > standardHours)
            {
                record.OvertimeHours = workingHours - standardHours;
            }
        }

        await _unitOfWork.AttendanceRecords.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        // Log the addition
        await _auditLogService.LogEventAsync(
            "AttendanceAdded",
            $"Missing attendance added for employee {employeeId} for date {date:yyyy-MM-dd}. CheckIn: {checkInTime}, CheckOut: {checkOutTime}. Reason: {reason}",
            addedBy
        );

        _logger.LogInformation("Missing attendance record added for employee {EmployeeId} for date {Date} by user {AddedBy}", 
            employeeId, date, addedBy);

        return record;
    }

    public async Task<bool> DeleteAttendanceRecordAsync(int attendanceRecordId, int deletedBy, string reason)
    {
        var record = await _unitOfWork.AttendanceRecords.GetByIdAsync(attendanceRecordId);
        if (record == null)
        {
            return false;
        }

        // Log the deletion before removing
        await _auditLogService.LogEventAsync(
            "AttendanceDeleted",
            $"Attendance record deleted for employee {record.EmployeeId} for date {record.Date:yyyy-MM-dd}. CheckIn: {record.CheckInTime}, CheckOut: {record.CheckOutTime}. Reason: {reason}",
            deletedBy
        );

        await _unitOfWork.AttendanceRecords.DeleteAsync(record);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Attendance record {RecordId} deleted by user {DeletedBy}. Reason: {Reason}", 
            attendanceRecordId, deletedBy, reason);

        return true;
    }

    public async Task<TimeSpan> GetAverageWorkingHoursAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var records = await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.EmployeeId == employeeId && 
                 a.Date >= startDate && 
                 a.Date <= endDate && 
                 a.TotalWorkingHours.HasValue
        );

        if (!records.Any())
        {
            return TimeSpan.Zero;
        }

        var totalTicks = records.Sum(r => r.TotalWorkingHours!.Value.Ticks);
        var averageTicks = totalTicks / records.Count();
        
        return new TimeSpan(averageTicks);
    }

    public async Task<int> GetLateCountAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var lateRecords = await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.EmployeeId == employeeId && 
                 a.Date >= startDate && 
                 a.Date <= endDate && 
                 a.IsLate
        );

        return lateRecords.Count();
    }

    public async Task<TimeSpan> GetTotalOvertimeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var records = await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.EmployeeId == employeeId && 
                 a.Date >= startDate && 
                 a.Date <= endDate && 
                 a.OvertimeHours.HasValue
        );

        var totalTicks = records.Sum(r => r.OvertimeHours!.Value.Ticks);
        return new TimeSpan(totalTicks);
    }
}