using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    public async Task<AttendanceRecord> CheckInAsync(int employeeId, string? location = null)
    {
        var today = DateTime.Today;
        var existingRecord = await GetTodayAttendanceAsync(employeeId);

        if (existingRecord != null && existingRecord.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Employee has already checked in today.");
        }

        var record = existingRecord ?? new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = today
        };

        record.CheckInTime = DateTime.Now;
        record.Location = location;
        record.Status = AttendanceStatus.Present;

        if (existingRecord == null)
        {
            await _unitOfWork.AttendanceRecords.AddAsync(record);
        }
        else
        {
            await _unitOfWork.AttendanceRecords.UpdateAsync(record);
        }

        await _unitOfWork.SaveChangesAsync();
        return record;
    }

    public async Task<AttendanceRecord> CheckOutAsync(int employeeId)
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

        record.CheckOutTime = DateTime.Now;
        record.TotalWorkingHours = record.CheckOutTime.Value - record.CheckInTime.Value;

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

        await _unitOfWork.AttendanceRecords.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();
        
        return record;
    }

    public async Task<BreakRecord> StartBreakAsync(int employeeId, BreakType breakType)
    {
        var attendanceRecord = await GetTodayAttendanceAsync(employeeId);
        
        if (attendanceRecord == null || !attendanceRecord.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Employee must check in before taking a break.");
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
            StartTime = DateTime.Now
        };

        await _unitOfWork.BreakRecords.AddAsync(breakRecord);
        
        // Update attendance status
        attendanceRecord.Status = AttendanceStatus.OnBreak;
        await _unitOfWork.AttendanceRecords.UpdateAsync(attendanceRecord);
        
        await _unitOfWork.SaveChangesAsync();
        
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
}