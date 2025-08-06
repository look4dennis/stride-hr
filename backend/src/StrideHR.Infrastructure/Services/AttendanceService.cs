using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Attendance;
using System.Linq.Expressions;
using System.Text.Json;

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
            Latitude = latitude.HasValue ? (decimal?)latitude.Value : null,
            Longitude = longitude.HasValue ? (decimal?)longitude.Value : null,
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

    public async Task<AttendanceReportResponse> GenerateAttendanceReportAsync(AttendanceReportRequest request)
    {
        // Build the predicate for filtering
        Expression<Func<AttendanceRecord, bool>> predicate = a => a.Date >= request.StartDate && a.Date <= request.EndDate;

        if (request.EmployeeId.HasValue)
        {
            var employeeId = request.EmployeeId.Value;
            predicate = a => a.Date >= request.StartDate && a.Date <= request.EndDate && a.EmployeeId == employeeId;
        }

        if (request.BranchId.HasValue)
        {
            var branchId = request.BranchId.Value;
            predicate = a => a.Date >= request.StartDate && a.Date <= request.EndDate && a.Employee.BranchId == branchId;
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentId = request.DepartmentId.Value.ToString();
            predicate = a => a.Date >= request.StartDate && a.Date <= request.EndDate && a.Employee.Department == departmentId;
        }

        var records = await _unitOfWork.AttendanceRecords.FindAsync(
            predicate,
            a => a.Employee, a => a.BreakRecords
        );

        var groupedRecords = records.GroupBy(r => r.EmployeeId);
        var reportItems = new List<AttendanceReportItem>();

        foreach (var group in groupedRecords)
        {
            var employee = group.First().Employee;
            var employeeRecords = group.ToList();
            
            var totalWorkingDays = (request.EndDate - request.StartDate).Days + 1;
            var presentDays = employeeRecords.Count(r => r.Status != AttendanceStatus.Absent);
            var absentDays = totalWorkingDays - presentDays;
            var lateDays = employeeRecords.Count(r => r.IsLate);
            var earlyDepartures = employeeRecords.Count(r => r.IsEarlyOut);
            
            var totalWorkingHours = TimeSpan.FromTicks(
                employeeRecords.Where(r => r.TotalWorkingHours.HasValue)
                              .Sum(r => r.TotalWorkingHours!.Value.Ticks)
            );
            
            var totalOvertimeHours = TimeSpan.FromTicks(
                employeeRecords.Where(r => r.OvertimeHours.HasValue)
                              .Sum(r => r.OvertimeHours!.Value.Ticks)
            );
            
            var totalBreakTime = TimeSpan.FromTicks(
                employeeRecords.Where(r => r.BreakDuration.HasValue)
                              .Sum(r => r.BreakDuration!.Value.Ticks)
            );

            var attendancePercentage = totalWorkingDays > 0 ? (double)presentDays / totalWorkingDays * 100 : 0;

            var reportItem = new AttendanceReportItem
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                EmployeeCode = employee.EmployeeId,
                Department = employee.Department ?? "N/A",
                TotalWorkingDays = totalWorkingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = lateDays,
                EarlyDepartures = earlyDepartures,
                TotalWorkingHours = totalWorkingHours,
                TotalOvertimeHours = totalOvertimeHours,
                TotalBreakTime = totalBreakTime,
                AttendancePercentage = Math.Round(attendancePercentage, 2)
            };

            // Add detailed records if requested
            if (request.ReportType == "detailed")
            {
                reportItem.Details = employeeRecords.Select(r => new AttendanceDetailItem
                {
                    Date = r.Date,
                    CheckInTime = r.CheckInTime,
                    CheckOutTime = r.CheckOutTime,
                    WorkingHours = r.TotalWorkingHours,
                    BreakDuration = r.BreakDuration,
                    OvertimeHours = r.OvertimeHours,
                    Status = r.Status.ToString(),
                    IsLate = r.IsLate,
                    LateBy = r.LateBy,
                    IsEarlyOut = r.IsEarlyOut,
                    EarlyOutBy = r.EarlyOutBy,
                    Notes = r.Notes
                }).ToList();
            }

            reportItems.Add(reportItem);
        }

        // Calculate summary
        var summary = new AttendanceReportSummary
        {
            TotalEmployees = reportItems.Count,
            TotalWorkingDays = reportItems.Sum(r => r.TotalWorkingDays),
            AverageAttendancePercentage = reportItems.Any() ? Math.Round(reportItems.Average(r => r.AttendancePercentage), 2) : 0,
            TotalPresentDays = reportItems.Sum(r => r.PresentDays),
            TotalAbsentDays = reportItems.Sum(r => r.AbsentDays),
            TotalLateDays = reportItems.Sum(r => r.LateDays),
            TotalEarlyDepartures = reportItems.Sum(r => r.EarlyDepartures),
            TotalWorkingHours = TimeSpan.FromTicks(reportItems.Sum(r => r.TotalWorkingHours.Ticks)),
            TotalOvertimeHours = TimeSpan.FromTicks(reportItems.Sum(r => r.TotalOvertimeHours.Ticks)),
            AverageWorkingHoursPerDay = reportItems.Any() && reportItems.Sum(r => r.PresentDays) > 0 
                ? TimeSpan.FromTicks(reportItems.Sum(r => r.TotalWorkingHours.Ticks) / reportItems.Sum(r => r.PresentDays))
                : TimeSpan.Zero,
            AverageOvertimePerDay = reportItems.Any() && reportItems.Sum(r => r.PresentDays) > 0
                ? TimeSpan.FromTicks(reportItems.Sum(r => r.TotalOvertimeHours.Ticks) / reportItems.Sum(r => r.PresentDays))
                : TimeSpan.Zero
        };

        return new AttendanceReportResponse
        {
            ReportType = request.ReportType ?? "summary",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            GeneratedAt = DateTime.Now,
            TotalEmployees = reportItems.Count,
            Items = reportItems,
            Summary = summary
        };
    }

    public async Task<AttendanceCalendarResponse> GetAttendanceCalendarAsync(int employeeId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var records = await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate,
            a => a.BreakRecords
        );

        var calendarDays = new List<AttendanceCalendarDay>();
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var record = records.FirstOrDefault(r => r.Date.Date == date.Date);
            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            
            var calendarDay = new AttendanceCalendarDay
            {
                Date = date,
                Status = record?.Status ?? AttendanceStatus.Absent,
                CheckInTime = record?.CheckInTime,
                CheckOutTime = record?.CheckOutTime,
                WorkingHours = record?.TotalWorkingHours,
                BreakDuration = record?.BreakDuration,
                OvertimeHours = record?.OvertimeHours,
                IsLate = record?.IsLate ?? false,
                LateBy = record?.LateBy,
                IsEarlyOut = record?.IsEarlyOut ?? false,
                EarlyOutBy = record?.EarlyOutBy,
                IsWeekend = isWeekend,
                IsHoliday = false, // TODO: Implement holiday checking
                Notes = record?.Notes,
                Breaks = record?.BreakRecords?.Select(b => new AttendanceCalendarBreak
                {
                    Type = b.Type,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Duration = b.Duration
                }).ToList() ?? new List<AttendanceCalendarBreak>()
            };

            calendarDays.Add(calendarDay);
        }

        // Calculate summary
        var workingDays = calendarDays.Count(d => !d.IsWeekend && !d.IsHoliday);
        var presentDays = calendarDays.Count(d => d.Status != AttendanceStatus.Absent && !d.IsWeekend && !d.IsHoliday);
        var absentDays = workingDays - presentDays;
        var lateDays = calendarDays.Count(d => d.IsLate);
        var earlyDepartures = calendarDays.Count(d => d.IsEarlyOut);
        var weekends = calendarDays.Count(d => d.IsWeekend);
        var holidays = calendarDays.Count(d => d.IsHoliday);

        var totalWorkingHours = TimeSpan.FromTicks(
            calendarDays.Where(d => d.WorkingHours.HasValue).Sum(d => d.WorkingHours!.Value.Ticks)
        );
        
        var totalOvertimeHours = TimeSpan.FromTicks(
            calendarDays.Where(d => d.OvertimeHours.HasValue).Sum(d => d.OvertimeHours!.Value.Ticks)
        );

        var summary = new AttendanceCalendarSummary
        {
            TotalWorkingDays = workingDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            EarlyDepartures = earlyDepartures,
            Weekends = weekends,
            Holidays = holidays,
            TotalWorkingHours = totalWorkingHours,
            TotalOvertimeHours = totalOvertimeHours,
            AttendancePercentage = workingDays > 0 ? Math.Round((double)presentDays / workingDays * 100, 2) : 0
        };

        return new AttendanceCalendarResponse
        {
            Year = year,
            Month = month,
            Days = calendarDays,
            Summary = summary
        };
    }

    public async Task<IEnumerable<AttendanceAlertResponse>> GetAttendanceAlertsAsync(int? branchId = null, bool unreadOnly = false)
    {
        // Build the predicate for filtering
        Expression<Func<AttendanceAlert, bool>> predicate = a => true;

        if (branchId.HasValue)
        {
            var branchIdValue = branchId.Value;
            predicate = a => a.BranchId == branchIdValue;
        }

        if (unreadOnly)
        {
            if (branchId.HasValue)
            {
                var branchIdValue = branchId.Value;
                predicate = a => a.BranchId == branchIdValue && !a.IsRead;
            }
            else
            {
                predicate = a => !a.IsRead;
            }
        }

        var alerts = await _unitOfWork.AttendanceAlerts.FindAsync(
            predicate,
            a => a.Employee, a => a.Branch
        );

        return alerts.Select(a => new AttendanceAlertResponse
        {
            Id = a.Id,
            AlertType = a.AlertType,
            AlertMessage = a.AlertMessage,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : null,
            BranchId = a.BranchId,
            BranchName = a.Branch?.Name,
            CreatedAt = a.CreatedAt,
            IsRead = a.IsRead,
            Severity = a.Severity,
            Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(a.Metadata) ?? new Dictionary<string, object>()
        }).OrderByDescending(a => a.CreatedAt);
    }

    public async Task<AttendanceAlertResponse> CreateAttendanceAlertAsync(AttendanceAlertRequest request)
    {
        var alert = new AttendanceAlert
        {
            AlertType = request.AlertType,
            AlertMessage = GenerateAlertMessage(request.AlertType, request.EmployeeId),
            EmployeeId = request.EmployeeId ?? 0,
            BranchId = request.BranchId,
            CreatedAt = DateTime.Now,
            IsRead = false,
            Severity = DetermineAlertSeverity(request.AlertType),
            Metadata = JsonSerializer.Serialize(new
            {
                ThresholdMinutes = request.ThresholdMinutes,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            })
        };

        await _unitOfWork.AttendanceAlerts.AddAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        return new AttendanceAlertResponse
        {
            Id = alert.Id,
            AlertType = alert.AlertType,
            AlertMessage = alert.AlertMessage,
            EmployeeId = alert.EmployeeId,
            BranchId = alert.BranchId,
            CreatedAt = alert.CreatedAt,
            IsRead = alert.IsRead,
            Severity = alert.Severity,
            Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(alert.Metadata) ?? new Dictionary<string, object>()
        };
    }

    public async Task<bool> MarkAlertAsReadAsync(int alertId)
    {
        var alert = await _unitOfWork.AttendanceAlerts.GetByIdAsync(alertId);
        if (alert == null)
        {
            return false;
        }

        alert.IsRead = true;
        alert.ReadAt = DateTime.Now;

        await _unitOfWork.AttendanceAlerts.UpdateAsync(alert);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AttendanceRecord>> GetAttendanceRecordsForCorrectionAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        return await _unitOfWork.AttendanceRecords.FindAsync(
            a => a.Employee.BranchId == branchId && 
                 a.Date >= start && 
                 a.Date <= end &&
                 (a.CheckInTime == null || a.CheckOutTime == null || a.IsLate || a.IsEarlyOut),
            a => a.Employee, a => a.BreakRecords
        );
    }

    public async Task<byte[]> ExportAttendanceReportAsync(AttendanceReportRequest request, string format = "excel")
    {
        var report = await GenerateAttendanceReportAsync(request);
        
        // TODO: Implement actual export functionality based on format
        // For now, return a placeholder
        var jsonData = JsonSerializer.Serialize(report);
        return System.Text.Encoding.UTF8.GetBytes(jsonData);
    }

    private string GenerateAlertMessage(AttendanceAlertType alertType, int? employeeId)
    {
        return alertType switch
        {
            AttendanceAlertType.LateArrival => $"Employee {employeeId} arrived late",
            AttendanceAlertType.EarlyDeparture => $"Employee {employeeId} left early",
            AttendanceAlertType.MissedCheckIn => $"Employee {employeeId} missed check-in",
            AttendanceAlertType.MissedCheckOut => $"Employee {employeeId} missed check-out",
            AttendanceAlertType.ExcessiveBreakTime => $"Employee {employeeId} exceeded break time limit",
            AttendanceAlertType.ConsecutiveAbsences => $"Employee {employeeId} has consecutive absences",
            AttendanceAlertType.LowAttendancePercentage => $"Employee {employeeId} has low attendance percentage",
            AttendanceAlertType.OvertimeThreshold => $"Employee {employeeId} exceeded overtime threshold",
            AttendanceAlertType.UnusualWorkingHours => $"Employee {employeeId} has unusual working hours",
            _ => $"Attendance alert for employee {employeeId}"
        };
    }

    private AlertSeverity DetermineAlertSeverity(AttendanceAlertType alertType)
    {
        return alertType switch
        {
            AttendanceAlertType.MissedCheckIn or AttendanceAlertType.MissedCheckOut => AlertSeverity.High,
            AttendanceAlertType.ConsecutiveAbsences or AttendanceAlertType.LowAttendancePercentage => AlertSeverity.Critical,
            AttendanceAlertType.LateArrival or AttendanceAlertType.EarlyDeparture => AlertSeverity.Medium,
            AttendanceAlertType.ExcessiveBreakTime or AttendanceAlertType.UnusualWorkingHours => AlertSeverity.Medium,
            AttendanceAlertType.OvertimeThreshold => AlertSeverity.Low,
            _ => AlertSeverity.Medium
        };
    }
}