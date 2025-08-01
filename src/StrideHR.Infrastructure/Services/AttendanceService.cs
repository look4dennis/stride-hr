using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for attendance tracking operations
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IBreakRecordRepository _breakRecordRepository;
    private readonly IAttendanceCorrectionRepository _correctionRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<AttendanceService> _logger;
    private readonly IAuditService _auditService;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IBreakRecordRepository breakRecordRepository,
        IAttendanceCorrectionRepository correctionRepository,
        IEmployeeRepository employeeRepository,
        IBranchRepository branchRepository,
        ILogger<AttendanceService> logger,
        IAuditService auditService)
    {
        _attendanceRepository = attendanceRepository;
        _breakRecordRepository = breakRecordRepository;
        _correctionRepository = correctionRepository;
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<AttendanceRecord> CheckInAsync(int employeeId, CheckInRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing check-in for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        // Check if already checked in today
        var existingRecord = await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
        if (existingRecord?.CheckInTime != null)
        {
            throw new InvalidOperationException("Employee has already checked in today");
        }

        var now = DateTime.UtcNow;
        var localTime = await ConvertToLocalTimeAsync(now, employee.Branch.TimeZone);

        // Get expected check-in time (from shift or default working hours)
        var shift = await GetEmployeeCurrentShiftAsync(employeeId, now.Date, cancellationToken);
        var expectedCheckInTime = shift?.StartTime ?? TimeSpan.FromHours(9); // Default 9 AM

        // Calculate if late
        var expectedDateTime = now.Date.Add(expectedCheckInTime);
        var isLate = now > expectedDateTime;
        var lateBy = isLate ? now - expectedDateTime : TimeSpan.Zero;

        AttendanceRecord record;
        if (existingRecord != null)
        {
            // Update existing record
            existingRecord.CheckInTime = now;
            existingRecord.CheckInTimeLocal = localTime;
            existingRecord.CheckInLocation = request.Location;
            existingRecord.CheckInIpAddress = request.IpAddress;
            existingRecord.CheckInDevice = request.DeviceInfo;
            existingRecord.Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present;
            existingRecord.ExpectedCheckInTime = expectedDateTime;
            existingRecord.LateArrivalDuration = lateBy;
            existingRecord.WeatherInfo = request.WeatherInfo != null ? JsonSerializer.Serialize(request.WeatherInfo) : null;
            existingRecord.Notes = request.Notes;
            existingRecord.UpdatedAt = now;

            record = await _attendanceRepository.UpdateAsync(existingRecord, cancellationToken);
        }
        else
        {
            // Create new record
            record = new AttendanceRecord
            {
                EmployeeId = employeeId,
                Date = now.Date,
                CheckInTime = now,
                CheckInTimeLocal = localTime,
                CheckInLocation = request.Location,
                CheckInIpAddress = request.IpAddress,
                CheckInDevice = request.DeviceInfo,
                Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
                ExpectedCheckInTime = expectedDateTime,
                LateArrivalDuration = lateBy,
                WeatherInfo = request.WeatherInfo != null ? JsonSerializer.Serialize(request.WeatherInfo) : null,
                Notes = request.Notes,
                ShiftId = shift?.Id,
                CreatedAt = now
            };

            record = await _attendanceRepository.AddAsync(record, cancellationToken);
        }

        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(employeeId, "Attendance", record.Id, "CHECK_IN", 
            null, $"Employee {employee.EmployeeId} checked in at {localTime:HH:mm}");

        _logger.LogInformation("Check-in completed for employee: {EmployeeId} at {Time}", 
            employee.EmployeeId, localTime);

        return record;
    }

    public async Task<AttendanceRecord> CheckOutAsync(int employeeId, CheckOutRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing check-out for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        var record = await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
        if (record == null || record.CheckInTime == null)
        {
            throw new InvalidOperationException("Employee has not checked in today");
        }

        if (record.CheckOutTime != null)
        {
            throw new InvalidOperationException("Employee has already checked out today");
        }

        // End any active breaks
        var activeBreaks = await _breakRecordRepository.GetActiveBreaksByEmployeeAsync(employeeId, cancellationToken);
        foreach (var activeBreak in activeBreaks)
        {
            await EndBreakInternal(activeBreak, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var localTime = await ConvertToLocalTimeAsync(now, employee.Branch.TimeZone);

        // Calculate working hours and overtime
        var totalTime = now - record.CheckInTime.Value;
        var breakDuration = record.BreakRecords.Sum(b => b.Duration?.TotalMinutes ?? 0);
        var workingHours = totalTime - TimeSpan.FromMinutes(breakDuration);
        
        var normalWorkingHours = TimeSpan.FromHours((double)employee.Branch.Organization.NormalWorkingHours);
        var overtimeHours = workingHours > normalWorkingHours ? workingHours - normalWorkingHours : TimeSpan.Zero;

        // Update record
        record.CheckOutTime = now;
        record.CheckOutTimeLocal = localTime;
        record.CheckOutLocation = request.Location;
        record.CheckOutIpAddress = request.IpAddress;
        record.CheckOutDevice = request.DeviceInfo;
        record.TotalWorkingHours = workingHours;
        record.BreakDuration = TimeSpan.FromMinutes(breakDuration);
        record.OvertimeHours = overtimeHours;
        record.UpdatedAt = now;

        if (!string.IsNullOrEmpty(request.Notes))
        {
            record.Notes = string.IsNullOrEmpty(record.Notes) ? request.Notes : $"{record.Notes}; {request.Notes}";
        }

        var updatedRecord = await _attendanceRepository.UpdateAsync(record, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(employeeId, "Attendance", record.Id, "CHECK_OUT", 
            null, $"Employee {employee.EmployeeId} checked out at {localTime:HH:mm}");

        _logger.LogInformation("Check-out completed for employee: {EmployeeId} at {Time}, Working hours: {Hours}", 
            employee.EmployeeId, localTime, workingHours);

        return updatedRecord;
    }

    public async Task<BreakRecord> StartBreakAsync(int employeeId, StartBreakRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting break for employee: {EmployeeId}, Type: {BreakType}", employeeId, request.Type);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        var attendanceRecord = await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
        if (attendanceRecord == null || attendanceRecord.CheckInTime == null)
        {
            throw new InvalidOperationException("Employee must check in before taking a break");
        }

        if (attendanceRecord.CheckOutTime != null)
        {
            throw new InvalidOperationException("Employee has already checked out today");
        }

        // Check if already on break
        var activeBreak = await _breakRecordRepository.GetActiveBreakAsync(attendanceRecord.Id, cancellationToken);
        if (activeBreak != null)
        {
            throw new InvalidOperationException("Employee is already on a break");
        }

        var now = DateTime.UtcNow;
        var localTime = await ConvertToLocalTimeAsync(now, employee.Branch.TimeZone);

        // Get break limits based on type
        var (maxMinutes, isPaid) = GetBreakLimits(request.Type);

        var breakRecord = new BreakRecord
        {
            AttendanceRecordId = attendanceRecord.Id,
            Type = request.Type,
            StartTime = now,
            StartTimeLocal = localTime,
            Location = request.Location,
            Reason = request.Reason,
            IsPaid = isPaid,
            MaxAllowedMinutes = maxMinutes,
            CreatedAt = now
        };

        var createdBreak = await _breakRecordRepository.AddAsync(breakRecord, cancellationToken);
        await _breakRecordRepository.SaveChangesAsync(cancellationToken);

        // Update attendance status
        attendanceRecord.Status = AttendanceStatus.OnBreak;
        await _attendanceRepository.UpdateAsync(attendanceRecord, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(employeeId, "Break", createdBreak.Id, "START", 
            null, $"Employee {employee.EmployeeId} started {request.Type} break");

        _logger.LogInformation("Break started for employee: {EmployeeId}, Type: {BreakType}", 
            employee.EmployeeId, request.Type);

        return createdBreak;
    }

    public async Task<BreakRecord> EndBreakAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ending break for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        var attendanceRecord = await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
        if (attendanceRecord == null)
        {
            throw new InvalidOperationException("No attendance record found for today");
        }

        var activeBreak = await _breakRecordRepository.GetActiveBreakAsync(attendanceRecord.Id, cancellationToken);
        if (activeBreak == null)
        {
            throw new InvalidOperationException("Employee is not currently on a break");
        }

        var updatedBreak = await EndBreakInternal(activeBreak, cancellationToken);

        // Update attendance status back to present
        attendanceRecord.Status = AttendanceStatus.Present;
        await _attendanceRepository.UpdateAsync(attendanceRecord, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(employeeId, "Break", updatedBreak.Id, "END", 
            null, $"Employee {employee.EmployeeId} ended {updatedBreak.Type} break");

        _logger.LogInformation("Break ended for employee: {EmployeeId}, Duration: {Duration}", 
            employee.EmployeeId, updatedBreak.Duration);

        return updatedBreak;
    }

    public async Task<IEnumerable<BreakRecord>> GetActiveBreaksAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _breakRecordRepository.GetActiveBreaksByEmployeeAsync(employeeId, cancellationToken);
    }

    public async Task<AttendanceStatus> GetCurrentStatusAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var record = await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
        return record?.Status ?? AttendanceStatus.Absent;
    }

    public async Task<AttendanceRecord?> GetTodayAttendanceAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _attendanceRepository.GetTodayRecordAsync(employeeId, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetTodayBranchAttendanceAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _attendanceRepository.GetTodayBranchRecordsAsync(branchId, cancellationToken);
    }

    public async Task<AttendanceStatusSummary> GetBranchAttendanceSummaryAsync(int branchId, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.Today;
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {branchId} not found");
        }

        var attendanceRecords = await _attendanceRepository.GetBranchAttendanceByDateAsync(branchId, targetDate, cancellationToken);
        var totalEmployees = await _employeeRepository.CountAsync(e => e.BranchId == branchId && e.Status == EmployeeStatus.Active, cancellationToken);

        var presentCount = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late || r.Status == AttendanceStatus.OnBreak);
        var absentCount = totalEmployees - presentCount;
        var lateCount = attendanceRecords.Count(r => r.Status == AttendanceStatus.Late);
        var onBreakCount = attendanceRecords.Count(r => r.Status == AttendanceStatus.OnBreak);
        var onLeaveCount = attendanceRecords.Count(r => r.Status == AttendanceStatus.OnLeave);

        var attendancePercentage = totalEmployees > 0 ? (decimal)presentCount / totalEmployees * 100 : 0;

        var employeeStatuses = attendanceRecords.Select(r => new EmployeeAttendanceStatus
        {
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee.FullName,
            EmployeeCode = r.Employee.EmployeeId,
            Status = r.Status,
            CheckInTime = r.CheckInTimeLocal,
            CheckOutTime = r.CheckOutTimeLocal,
            WorkingHours = r.TotalWorkingHours,
            CurrentBreakType = r.CurrentBreakType(),
            BreakDuration = r.BreakDuration,
            IsLate = r.LateArrivalDuration > TimeSpan.Zero,
            LateBy = r.LateArrivalDuration
        }).ToList();

        return new AttendanceStatusSummary
        {
            Date = targetDate,
            BranchId = branchId,
            BranchName = branch.Name,
            TotalEmployees = totalEmployees,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            OnBreakCount = onBreakCount,
            OnLeaveCount = onLeaveCount,
            AttendancePercentage = attendancePercentage,
            EmployeeStatuses = employeeStatuses
        };
    }

    public async Task<AttendanceRecord?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _attendanceRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _attendanceRepository.GetEmployeeAttendanceAsync(employeeId, fromDate, toDate, cancellationToken);
    }

    public async Task<(IEnumerable<AttendanceRecord> Records, int TotalCount)> SearchAttendanceAsync(AttendanceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        return await _attendanceRepository.SearchAsync(criteria, cancellationToken);
    }

    public async Task<AttendanceCorrection> RequestCorrectionAsync(int attendanceId, CorrectionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing correction request for attendance: {AttendanceId}", attendanceId);

        var attendanceRecord = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (attendanceRecord == null)
        {
            throw new ArgumentException($"Attendance record with ID {attendanceId} not found");
        }

        var correction = new AttendanceCorrection
        {
            AttendanceRecordId = attendanceId,
            RequestedBy = request.RequestedBy,
            Type = request.Type,
            OriginalValue = request.OriginalValue,
            CorrectedValue = request.CorrectedValue,
            Reason = request.Reason,
            Status = CorrectionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdCorrection = await _correctionRepository.AddAsync(correction, cancellationToken);
        await _correctionRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(request.RequestedBy, "AttendanceCorrection", createdCorrection.Id, "REQUEST", 
            null, $"Correction requested for attendance {attendanceId}");

        _logger.LogInformation("Correction request created: {CorrectionId}", createdCorrection.Id);
        return createdCorrection;
    }

    public async Task<AttendanceCorrection> ApproveCorrectionAsync(int correctionId, int approvedBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        var correction = await _correctionRepository.GetByIdAsync(correctionId, cancellationToken);
        if (correction == null)
        {
            throw new ArgumentException($"Correction with ID {correctionId} not found");
        }

        if (correction.Status != CorrectionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending corrections can be approved");
        }

        correction.Status = CorrectionStatus.Approved;
        correction.ApprovedBy = approvedBy;
        correction.ApprovedAt = DateTime.UtcNow;
        correction.ApprovalComments = comments;

        // Apply the correction to the attendance record
        await ApplyCorrectionAsync(correction, cancellationToken);

        var updatedCorrection = await _correctionRepository.UpdateAsync(correction, cancellationToken);
        await _correctionRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(approvedBy, "AttendanceCorrection", correctionId, "APPROVE", 
            null, $"Correction approved by {approvedBy}");

        return updatedCorrection;
    }

    public async Task<AttendanceCorrection> RejectCorrectionAsync(int correctionId, int rejectedBy, string reason, CancellationToken cancellationToken = default)
    {
        var correction = await _correctionRepository.GetByIdAsync(correctionId, cancellationToken);
        if (correction == null)
        {
            throw new ArgumentException($"Correction with ID {correctionId} not found");
        }

        if (correction.Status != CorrectionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending corrections can be rejected");
        }

        correction.Status = CorrectionStatus.Rejected;
        correction.ApprovedBy = rejectedBy;
        correction.ApprovedAt = DateTime.UtcNow;
        correction.ApprovalComments = reason;

        var updatedCorrection = await _correctionRepository.UpdateAsync(correction, cancellationToken);
        await _correctionRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(rejectedBy, "AttendanceCorrection", correctionId, "REJECT", 
            null, $"Correction rejected by {rejectedBy}");

        return updatedCorrection;
    }

    public async Task<IEnumerable<AttendanceCorrection>> GetPendingCorrectionsAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        return await _correctionRepository.GetPendingCorrectionsAsync(branchId, cancellationToken);
    }

    public async Task<AttendanceRecord> CreateManualEntryAsync(ManualAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating manual attendance entry for employee: {EmployeeId}", request.EmployeeId);

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {request.EmployeeId} not found");
        }

        // Check if record already exists for the date
        var existingRecord = await _attendanceRepository.GetEmployeeAttendanceByDateAsync(request.EmployeeId, request.Date, cancellationToken);
        if (existingRecord != null)
        {
            throw new InvalidOperationException($"Attendance record already exists for {request.Date:yyyy-MM-dd}");
        }

        var localCheckInTime = request.CheckInTime.HasValue ? 
            await ConvertToLocalTimeAsync(request.CheckInTime.Value, employee.Branch.TimeZone) : (DateTime?)null;
        var localCheckOutTime = request.CheckOutTime.HasValue ? 
            await ConvertToLocalTimeAsync(request.CheckOutTime.Value, employee.Branch.TimeZone) : (DateTime?)null;

        var workingHours = (request.CheckInTime.HasValue && request.CheckOutTime.HasValue) ?
            request.CheckOutTime.Value - request.CheckInTime.Value : (TimeSpan?)null;

        var record = new AttendanceRecord
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            CheckInTimeLocal = localCheckInTime,
            CheckOutTimeLocal = localCheckOutTime,
            TotalWorkingHours = workingHours,
            Status = request.Status,
            CheckInLocation = request.Location,
            CheckOutLocation = request.Location,
            IsManualEntry = true,
            ManualEntryReason = request.Reason,
            ManualEntryBy = request.EnteredBy,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var createdRecord = await _attendanceRepository.AddAsync(record, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(request.EnteredBy, "Attendance", createdRecord.Id, "MANUAL_CREATE", 
            null, $"Manual entry created for employee {employee.EmployeeId} by {request.EnteredBy}");

        _logger.LogInformation("Manual attendance entry created: {RecordId}", createdRecord.Id);
        return createdRecord;
    }

    public async Task<AttendanceRecord> UpdateManualEntryAsync(int attendanceId, ManualAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var record = await _attendanceRepository.GetByIdAsync(attendanceId, cancellationToken);
        if (record == null)
        {
            throw new ArgumentException($"Attendance record with ID {attendanceId} not found");
        }

        if (!record.IsManualEntry)
        {
            throw new InvalidOperationException("Only manual entries can be updated using this method");
        }

        var employee = await _employeeRepository.GetByIdAsync(record.EmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {record.EmployeeId} not found");
        }

        var localCheckInTime = request.CheckInTime.HasValue ? 
            await ConvertToLocalTimeAsync(request.CheckInTime.Value, employee.Branch.TimeZone) : (DateTime?)null;
        var localCheckOutTime = request.CheckOutTime.HasValue ? 
            await ConvertToLocalTimeAsync(request.CheckOutTime.Value, employee.Branch.TimeZone) : (DateTime?)null;

        var workingHours = (request.CheckInTime.HasValue && request.CheckOutTime.HasValue) ?
            request.CheckOutTime.Value - request.CheckInTime.Value : (TimeSpan?)null;

        record.CheckInTime = request.CheckInTime;
        record.CheckOutTime = request.CheckOutTime;
        record.CheckInTimeLocal = localCheckInTime;
        record.CheckOutTimeLocal = localCheckOutTime;
        record.TotalWorkingHours = workingHours;
        record.Status = request.Status;
        record.CheckInLocation = request.Location;
        record.CheckOutLocation = request.Location;
        record.ManualEntryReason = request.Reason;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        var updatedRecord = await _attendanceRepository.UpdateAsync(record, cancellationToken);
        await _attendanceRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogDataModificationAsync(request.EnteredBy, "Attendance", attendanceId, "MANUAL_UPDATE", 
            null, $"Manual entry updated for employee {employee.EmployeeId}");

        return updatedRecord;
    }

    public async Task<AttendanceReport> GenerateAttendanceReportAsync(AttendanceReportCriteria criteria, CancellationToken cancellationToken = default)
    {
        var searchCriteria = new AttendanceSearchCriteria
        {
            BranchId = criteria.BranchId,
            EmployeeId = criteria.EmployeeId,
            Department = criteria.Department,
            FromDate = criteria.FromDate,
            ToDate = criteria.ToDate,
            PageSize = int.MaxValue // Get all records for report
        };

        var (records, totalCount) = await _attendanceRepository.SearchAsync(searchCriteria, cancellationToken);

        var reportItems = records.Select(r => new AttendanceReportItem
        {
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee.FullName,
            EmployeeCode = r.Employee.EmployeeId,
            Department = r.Employee.Department ?? string.Empty,
            Date = r.Date,
            Status = r.Status,
            CheckInTime = r.CheckInTimeLocal,
            CheckOutTime = r.CheckOutTimeLocal,
            WorkingHours = r.TotalWorkingHours,
            BreakDuration = r.BreakDuration,
            OvertimeHours = r.OvertimeHours,
            IsLate = r.LateArrivalDuration > TimeSpan.Zero,
            LateBy = r.LateArrivalDuration,
            IsManualEntry = r.IsManualEntry,
            BreakDetails = criteria.IncludeBreakDetails ? r.BreakRecords.ToList() : null
        }).ToList();

        var summary = new AttendanceReportSummary
        {
            TotalWorkingDays = (criteria.ToDate - criteria.FromDate).Days + 1,
            TotalPresentDays = reportItems.Count(i => i.Status == AttendanceStatus.Present || i.Status == AttendanceStatus.Late),
            TotalAbsentDays = reportItems.Count(i => i.Status == AttendanceStatus.Absent),
            TotalLateDays = reportItems.Count(i => i.IsLate),
            TotalWorkingHours = TimeSpan.FromTicks(reportItems.Sum(i => i.WorkingHours?.Ticks ?? 0)),
            TotalOvertimeHours = TimeSpan.FromTicks(reportItems.Sum(i => i.OvertimeHours?.Ticks ?? 0))
        };

        summary.AttendancePercentage = summary.TotalWorkingDays > 0 ? 
            (decimal)summary.TotalPresentDays / summary.TotalWorkingDays * 100 : 0;
        summary.AverageWorkingHours = summary.TotalPresentDays > 0 ? 
            TimeSpan.FromTicks(summary.TotalWorkingHours.Ticks / summary.TotalPresentDays) : TimeSpan.Zero;

        return new AttendanceReport
        {
            Criteria = criteria,
            GeneratedAt = DateTime.UtcNow,
            TotalRecords = totalCount,
            Items = reportItems,
            Summary = summary
        };
    }

    public async Task<ProductivityReport> GenerateProductivityReportAsync(ProductivityReportCriteria criteria, CancellationToken cancellationToken = default)
    {
        var searchCriteria = new AttendanceSearchCriteria
        {
            BranchId = criteria.BranchId,
            EmployeeId = criteria.EmployeeId,
            FromDate = criteria.FromDate,
            ToDate = criteria.ToDate,
            PageSize = int.MaxValue
        };

        var (records, _) = await _attendanceRepository.SearchAsync(searchCriteria, cancellationToken);

        var reportItems = records.Where(r => r.TotalWorkingHours.HasValue).Select(r =>
        {
            var workingHours = r.TotalWorkingHours!.Value;
            var productiveHours = r.ProductiveHours ?? workingHours; // Default to working hours if not set
            var idleTime = r.IdleTime ?? TimeSpan.Zero;
            var productivityPercentage = workingHours.TotalMinutes > 0 ? 
                (decimal)(productiveHours.TotalMinutes / workingHours.TotalMinutes * 100) : 0;

            return new ProductivityReportItem
            {
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee.FullName,
                Date = r.Date,
                WorkingHours = workingHours,
                ProductiveHours = productiveHours,
                IdleTime = idleTime,
                ProductivityPercentage = productivityPercentage,
                MetProductivityThreshold = productiveHours.TotalHours >= r.Employee.Branch.Organization.ProductiveHoursThreshold
            };
        }).ToList();

        var summary = new ProductivityReportSummary
        {
            TotalWorkingHours = TimeSpan.FromTicks(reportItems.Sum(i => i.WorkingHours.Ticks)),
            TotalProductiveHours = TimeSpan.FromTicks(reportItems.Sum(i => i.ProductiveHours.Ticks)),
            TotalIdleTime = TimeSpan.FromTicks(reportItems.Sum(i => i.IdleTime.Ticks)),
            DaysMetThreshold = reportItems.Count(i => i.MetProductivityThreshold),
            TotalDays = reportItems.Count
        };

        summary.AverageProductivityPercentage = reportItems.Count > 0 ? 
            reportItems.Average(i => i.ProductivityPercentage) : 0;

        return new ProductivityReport
        {
            Criteria = criteria,
            GeneratedAt = DateTime.UtcNow,
            Items = reportItems,
            Summary = summary
        };
    }

    public async Task<IEnumerable<LateArrivalRecord>> GetLateArrivalsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var lateRecords = await _attendanceRepository.GetLateArrivalsAsync(branchId, fromDate, toDate, cancellationToken);
        
        return lateRecords.Where(r => r.LateArrivalDuration > TimeSpan.Zero).Select(r => new LateArrivalRecord
        {
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee.FullName,
            Date = r.Date,
            ExpectedTime = r.ExpectedCheckInTime ?? r.Date.AddHours(9), // Default 9 AM
            ActualTime = r.CheckInTime ?? DateTime.MinValue,
            LateBy = r.LateArrivalDuration ?? TimeSpan.Zero
        });
    }

    public async Task<IEnumerable<OvertimeRecord>> GetOvertimeRecordsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var overtimeRecords = await _attendanceRepository.GetOvertimeRecordsAsync(branchId, fromDate, toDate, cancellationToken);
        
        return overtimeRecords.Where(r => r.OvertimeHours > TimeSpan.Zero).Select(r => new OvertimeRecord
        {
            EmployeeId = r.EmployeeId,
            EmployeeName = r.Employee.FullName,
            Date = r.Date,
            RegularHours = r.TotalWorkingHours ?? TimeSpan.Zero,
            OvertimeHours = r.OvertimeHours ?? TimeSpan.Zero,
            OvertimeRate = r.Employee.Branch.Organization.OvertimeRate
        });
    }

    public async Task<bool> ValidateLocationAsync(string location, int branchId, CancellationToken cancellationToken = default)
    {
        // Simple validation - in production, this would use GPS coordinates and geofencing
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        return branch != null && !string.IsNullOrEmpty(location);
    }

    public async Task<LocationValidationResult> GetLocationValidationAsync(string location, int branchId, CancellationToken cancellationToken = default)
    {
        var isValid = await ValidateLocationAsync(location, branchId, cancellationToken);
        
        return new LocationValidationResult
        {
            IsValid = isValid,
            Message = isValid ? "Location is valid" : "Location validation failed",
            DistanceFromOffice = null, // Would calculate actual distance in production
            NearestOfficeLocation = null
        };
    }

    public Task<Shift?> GetEmployeeCurrentShiftAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        // This would be implemented based on shift assignment logic
        // For now, return null (no specific shift assigned)
        return Task.FromResult<Shift?>(null);
    }

    public async Task<bool> IsWithinShiftHoursAsync(int employeeId, DateTime time, CancellationToken cancellationToken = default)
    {
        var shift = await GetEmployeeCurrentShiftAsync(employeeId, time.Date, cancellationToken);
        if (shift == null)
        {
            // Default working hours: 9 AM to 6 PM
            var timeOfDay = time.TimeOfDay;
            return timeOfDay >= TimeSpan.FromHours(9) && timeOfDay <= TimeSpan.FromHours(18);
        }

        return time.TimeOfDay >= shift.StartTime && time.TimeOfDay <= shift.EndTime;
    }

    // Private helper methods

    private async Task<BreakRecord> EndBreakInternal(BreakRecord breakRecord, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var employee = await _employeeRepository.GetByIdAsync(breakRecord.AttendanceRecord.EmployeeId, cancellationToken);
        var localTime = await ConvertToLocalTimeAsync(now, employee!.Branch.TimeZone);

        breakRecord.EndTime = now;
        breakRecord.EndTimeLocal = localTime;
        breakRecord.Duration = breakRecord.CalculateDuration();

        // Check if break exceeded allowed time
        if (breakRecord.MaxAllowedMinutes.HasValue && 
            breakRecord.Duration.Value.TotalMinutes > breakRecord.MaxAllowedMinutes.Value)
        {
            breakRecord.IsExceeding = true;
            breakRecord.ExceededDuration = breakRecord.Duration.Value - TimeSpan.FromMinutes(breakRecord.MaxAllowedMinutes.Value);
            breakRecord.ApprovalStatus = BreakApprovalStatus.Pending;
        }

        return await _breakRecordRepository.UpdateAsync(breakRecord, cancellationToken);
    }

    private async Task ApplyCorrectionAsync(AttendanceCorrection correction, CancellationToken cancellationToken)
    {
        var record = await _attendanceRepository.GetByIdAsync(correction.AttendanceRecordId, cancellationToken);
        if (record == null) return;

        switch (correction.Type)
        {
            case CorrectionType.CheckInTime:
                if (DateTime.TryParse(correction.CorrectedValue, out var checkInTime))
                {
                    record.CheckInTime = checkInTime;
                    var employee = await _employeeRepository.GetByIdAsync(record.EmployeeId, cancellationToken);
                    record.CheckInTimeLocal = await ConvertToLocalTimeAsync(checkInTime, employee!.Branch.TimeZone);
                }
                break;
            case CorrectionType.CheckOutTime:
                if (DateTime.TryParse(correction.CorrectedValue, out var checkOutTime))
                {
                    record.CheckOutTime = checkOutTime;
                    var employee = await _employeeRepository.GetByIdAsync(record.EmployeeId, cancellationToken);
                    record.CheckOutTimeLocal = await ConvertToLocalTimeAsync(checkOutTime, employee!.Branch.TimeZone);
                }
                break;
            case CorrectionType.AttendanceStatus:
                if (Enum.TryParse<AttendanceStatus>(correction.CorrectedValue, out var status))
                {
                    record.Status = status;
                }
                break;
            // Add other correction types as needed
        }

        record.UpdatedAt = DateTime.UtcNow;
        await _attendanceRepository.UpdateAsync(record, cancellationToken);
    }

    private Task<DateTime> ConvertToLocalTimeAsync(DateTime utcTime, string timeZone)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return Task.FromResult(TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo));
        }
        catch (TimeZoneNotFoundException)
        {
            return Task.FromResult(utcTime); // Return UTC if timezone not found
        }
    }

    private static (int? maxMinutes, bool isPaid) GetBreakLimits(BreakType breakType)
    {
        return breakType switch
        {
            BreakType.Tea => (15, true),
            BreakType.Lunch => (60, false),
            BreakType.Personal => (10, true),
            BreakType.Meeting => (null, true),
            BreakType.Prayer => (15, true),
            BreakType.Medical => (30, true),
            BreakType.Emergency => (null, true),
            _ => (15, true)
        };
    }
}