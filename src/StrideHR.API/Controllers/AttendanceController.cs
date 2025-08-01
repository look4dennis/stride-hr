using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.DTOs.Attendance;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

/// <summary>
/// Controller for attendance tracking operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Check in employee
    /// </summary>
    [HttpPost("checkin")]
    public async Task<ActionResult<AttendanceDto>> CheckIn([FromBody] CheckInDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var request = new CheckInRequest
            {
                Location = dto.Location,
                IpAddress = dto.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                DeviceInfo = dto.DeviceInfo ?? HttpContext.Request.Headers.UserAgent.ToString(),
                Notes = dto.Notes,
                WeatherInfo = dto.WeatherInfo
            };

            var result = await _attendanceService.CheckInAsync(employeeId, request);
            var response = MapToAttendanceDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Check out employee
    /// </summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<AttendanceDto>> CheckOut([FromBody] CheckOutDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var request = new CheckOutRequest
            {
                Location = dto.Location,
                IpAddress = dto.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                DeviceInfo = dto.DeviceInfo ?? HttpContext.Request.Headers.UserAgent.ToString(),
                Notes = dto.Notes
            };

            var result = await _attendanceService.CheckOutAsync(employeeId, request);
            var response = MapToAttendanceDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Start a break
    /// </summary>
    [HttpPost("break/start")]
    public async Task<ActionResult<BreakRecordDto>> StartBreak([FromBody] StartBreakDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var request = new StartBreakRequest
            {
                Type = dto.Type,
                Location = dto.Location,
                Reason = dto.Reason
            };

            var result = await _attendanceService.StartBreakAsync(employeeId, request);
            var response = MapToBreakRecordDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// End current break
    /// </summary>
    [HttpPost("break/end")]
    public async Task<ActionResult<BreakRecordDto>> EndBreak()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _attendanceService.EndBreakAsync(employeeId);
            var response = MapToBreakRecordDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Get current attendance status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<AttendanceStatus>> GetCurrentStatus()
    {
        var employeeId = GetCurrentEmployeeId();
        var status = await _attendanceService.GetCurrentStatusAsync(employeeId);
        return Ok(status);
    }

    /// <summary>
    /// Get today's attendance record
    /// </summary>
    [HttpGet("today")]
    public async Task<ActionResult<AttendanceDto?>> GetTodayAttendance()
    {
        var employeeId = GetCurrentEmployeeId();
        var record = await _attendanceService.GetTodayAttendanceAsync(employeeId);
        
        if (record == null)
            return Ok(null);

        var response = MapToAttendanceDto(record);
        return Ok(response);
    }

    /// <summary>
    /// Get employee attendance history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<AttendanceDto>>> GetAttendanceHistory(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var employeeId = GetCurrentEmployeeId();
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var records = await _attendanceService.GetEmployeeAttendanceAsync(employeeId, from, to);
        var response = records.Select(MapToAttendanceDto).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get active breaks for current employee
    /// </summary>
    [HttpGet("breaks/active")]
    public async Task<ActionResult<List<BreakRecordDto>>> GetActiveBreaks()
    {
        var employeeId = GetCurrentEmployeeId();
        var breaks = await _attendanceService.GetActiveBreaksAsync(employeeId);
        var response = breaks.Select(MapToBreakRecordDto).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Request attendance correction
    /// </summary>
    [HttpPost("{attendanceId}/corrections")]
    public async Task<ActionResult<AttendanceCorrectionDto>> RequestCorrection(
        int attendanceId, 
        [FromBody] AttendanceCorrectionRequestDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var request = new CorrectionRequest
            {
                RequestedBy = employeeId,
                Type = dto.Type,
                OriginalValue = dto.OriginalValue,
                CorrectedValue = dto.CorrectedValue,
                Reason = dto.Reason
            };

            var result = await _attendanceService.RequestCorrectionAsync(attendanceId, request);
            var response = MapToAttendanceCorrectionDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // HR/Manager endpoints

    /// <summary>
    /// Get branch attendance summary (HR/Manager only)
    /// </summary>
    [HttpGet("branch/{branchId}/summary")]
    [Authorize(Policy = "HROrManager")]
    public async Task<ActionResult<AttendanceStatusSummaryDto>> GetBranchAttendanceSummary(
        int branchId, 
        [FromQuery] DateTime? date = null)
    {
        var summary = await _attendanceService.GetBranchAttendanceSummaryAsync(branchId, date);
        var response = MapToAttendanceStatusSummaryDto(summary);

        return Ok(response);
    }

    /// <summary>
    /// Get today's branch attendance (HR/Manager only)
    /// </summary>
    [HttpGet("branch/{branchId}/today")]
    [Authorize(Policy = "HROrManager")]
    public async Task<ActionResult<List<AttendanceDto>>> GetTodayBranchAttendance(int branchId)
    {
        var records = await _attendanceService.GetTodayBranchAttendanceAsync(branchId);
        var response = records.Select(MapToAttendanceDto).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Search attendance records (HR/Manager only)
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "HROrManager")]
    public async Task<ActionResult<object>> SearchAttendance(
        [FromQuery] int? employeeId = null,
        [FromQuery] int? branchId = null,
        [FromQuery] string? department = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] AttendanceStatus? status = null,
        [FromQuery] bool? isLate = null,
        [FromQuery] bool? hasOvertime = null,
        [FromQuery] bool? isManualEntry = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var criteria = new AttendanceSearchCriteria
        {
            EmployeeId = employeeId,
            BranchId = branchId,
            Department = department,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            IsLate = isLate,
            HasOvertime = hasOvertime,
            IsManualEntry = isManualEntry,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var (records, totalCount) = await _attendanceService.SearchAttendanceAsync(criteria);
        var response = records.Select(MapToAttendanceDto).ToList();

        return Ok(new
        {
            Records = response,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>
    /// Get pending corrections (HR only)
    /// </summary>
    [HttpGet("corrections/pending")]
    [Authorize(Policy = "HR")]
    public async Task<ActionResult<List<AttendanceCorrectionDto>>> GetPendingCorrections(
        [FromQuery] int? branchId = null)
    {
        var corrections = await _attendanceService.GetPendingCorrectionsAsync(branchId);
        var response = corrections.Select(MapToAttendanceCorrectionDto).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Approve correction (HR only)
    /// </summary>
    [HttpPost("corrections/{correctionId}/approve")]
    [Authorize(Policy = "HR")]
    public async Task<ActionResult<AttendanceCorrectionDto>> ApproveCorrection(
        int correctionId, 
        [FromBody] CorrectionApprovalDto dto)
    {
        try
        {
            var approvedBy = GetCurrentEmployeeId();
            var result = await _attendanceService.ApproveCorrectionAsync(correctionId, approvedBy, dto.Comments);
            var response = MapToAttendanceCorrectionDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Reject correction (HR only)
    /// </summary>
    [HttpPost("corrections/{correctionId}/reject")]
    [Authorize(Policy = "HR")]
    public async Task<ActionResult<AttendanceCorrectionDto>> RejectCorrection(
        int correctionId, 
        [FromBody] CorrectionApprovalDto dto)
    {
        try
        {
            var rejectedBy = GetCurrentEmployeeId();
            var reason = dto.Comments ?? "No reason provided";
            var result = await _attendanceService.RejectCorrectionAsync(correctionId, rejectedBy, reason);
            var response = MapToAttendanceCorrectionDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Create manual attendance entry (HR only)
    /// </summary>
    [HttpPost("manual")]
    [Authorize(Policy = "HR")]
    public async Task<ActionResult<AttendanceDto>> CreateManualEntry([FromBody] ManualAttendanceDto dto)
    {
        try
        {
            var enteredBy = GetCurrentEmployeeId();
            var request = new ManualAttendanceRequest
            {
                EmployeeId = dto.EmployeeId,
                Date = dto.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status,
                Location = dto.Location,
                Reason = dto.Reason,
                EnteredBy = enteredBy,
                Notes = dto.Notes
            };

            var result = await _attendanceService.CreateManualEntryAsync(request);
            var response = MapToAttendanceDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Update manual attendance entry (HR only)
    /// </summary>
    [HttpPut("manual/{attendanceId}")]
    [Authorize(Policy = "HR")]
    public async Task<ActionResult<AttendanceDto>> UpdateManualEntry(
        int attendanceId, 
        [FromBody] ManualAttendanceDto dto)
    {
        try
        {
            var request = new ManualAttendanceRequest
            {
                EmployeeId = dto.EmployeeId,
                Date = dto.Date,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                Status = dto.Status,
                Location = dto.Location,
                Reason = dto.Reason,
                EnteredBy = GetCurrentEmployeeId(),
                Notes = dto.Notes
            };

            var result = await _attendanceService.UpdateManualEntryAsync(attendanceId, request);
            var response = MapToAttendanceDto(result);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Validate location
    /// </summary>
    [HttpPost("validate-location")]
    public async Task<ActionResult<object>> ValidateLocation([FromBody] LocationValidationRequest request)
    {
        var branchId = GetCurrentBranchId();
        var result = await _attendanceService.GetLocationValidationAsync(request.Location, branchId);

        return Ok(result);
    }

    // Private helper methods

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(employeeIdClaim, out var employeeId))
            return employeeId;

        throw new UnauthorizedAccessException("Employee ID not found in token");
    }

    private int GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId")?.Value;
        if (int.TryParse(branchIdClaim, out var branchId))
            return branchId;

        throw new UnauthorizedAccessException("Branch ID not found in token");
    }

    private static AttendanceDto MapToAttendanceDto(AttendanceRecord record)
    {
        return new AttendanceDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = record.Employee?.FullName ?? string.Empty,
            EmployeeCode = record.Employee?.EmployeeId ?? string.Empty,
            Date = record.Date,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            CheckInTimeLocal = record.CheckInTimeLocal,
            CheckOutTimeLocal = record.CheckOutTimeLocal,
            TotalWorkingHours = record.TotalWorkingHours,
            BreakDuration = record.BreakDuration,
            OvertimeHours = record.OvertimeHours,
            ProductiveHours = record.ProductiveHours,
            IdleTime = record.IdleTime,
            Status = record.Status,
            CheckInLocation = record.CheckInLocation,
            CheckOutLocation = record.CheckOutLocation,
            LateArrivalDuration = record.LateArrivalDuration,
            EarlyDepartureDuration = record.EarlyDepartureDuration,
            IsManualEntry = record.IsManualEntry,
            ManualEntryReason = record.ManualEntryReason,
            Notes = record.Notes,
            BreakRecords = record.BreakRecords?.Select(MapToBreakRecordDto).ToList() ?? new(),
            Corrections = record.AttendanceCorrections?.Select(MapToAttendanceCorrectionDto).ToList() ?? new()
        };
    }

    private static BreakRecordDto MapToBreakRecordDto(BreakRecord breakRecord)
    {
        return new BreakRecordDto
        {
            Id = breakRecord.Id,
            Type = breakRecord.Type,
            StartTime = breakRecord.StartTime,
            EndTime = breakRecord.EndTime,
            StartTimeLocal = breakRecord.StartTimeLocal,
            EndTimeLocal = breakRecord.EndTimeLocal,
            Duration = breakRecord.Duration,
            Location = breakRecord.Location,
            Reason = breakRecord.Reason,
            IsPaid = breakRecord.IsPaid,
            MaxAllowedMinutes = breakRecord.MaxAllowedMinutes,
            IsExceeding = breakRecord.IsExceeding,
            ExceededDuration = breakRecord.ExceededDuration,
            ApprovalStatus = breakRecord.ApprovalStatus,
            IsActive = breakRecord.IsActive
        };
    }

    private static AttendanceCorrectionDto MapToAttendanceCorrectionDto(AttendanceCorrection correction)
    {
#pragma warning disable CS8601 // Possible null reference assignment - both properties are int? so this is safe
        return new AttendanceCorrectionDto
        {
            Id = correction.Id,
            AttendanceRecordId = correction.AttendanceRecordId,
            RequestedBy = correction.RequestedBy,
            RequestedByName = correction.RequestedByEmployee?.FullName ?? string.Empty,
            Type = correction.Type,
            OriginalValue = correction.OriginalValue,
            CorrectedValue = correction.CorrectedValue,
            Reason = correction.Reason,
            Status = correction.Status,
            ApprovedBy = correction.ApprovedBy,
            ApprovedByName = correction.ApprovedByEmployee?.FullName ?? string.Empty,
            ApprovedAt = correction.ApprovedAt,
            ApprovalComments = correction.ApprovalComments,
            CreatedAt = correction.CreatedAt
        };
#pragma warning restore CS8601
    }

    private static AttendanceStatusSummaryDto MapToAttendanceStatusSummaryDto(AttendanceStatusSummary summary)
    {
        return new AttendanceStatusSummaryDto
        {
            Date = summary.Date,
            BranchId = summary.BranchId,
            BranchName = summary.BranchName,
            TotalEmployees = summary.TotalEmployees,
            PresentCount = summary.PresentCount,
            AbsentCount = summary.AbsentCount,
            LateCount = summary.LateCount,
            OnBreakCount = summary.OnBreakCount,
            OnLeaveCount = summary.OnLeaveCount,
            AttendancePercentage = summary.AttendancePercentage,
            EmployeeStatuses = summary.EmployeeStatuses.Select(es => new EmployeeAttendanceStatusDto
            {
                EmployeeId = es.EmployeeId,
                EmployeeName = es.EmployeeName,
                EmployeeCode = es.EmployeeCode,
                Status = es.Status,
                CheckInTime = es.CheckInTime,
                CheckOutTime = es.CheckOutTime,
                WorkingHours = es.WorkingHours,
                CurrentBreakType = es.CurrentBreakType,
                BreakDuration = es.BreakDuration,
                IsLate = es.IsLate,
                LateBy = es.LateBy
            }).ToList()
        };
    }
}

/// <summary>
/// Request model for location validation
/// </summary>
public class LocationValidationRequest
{
    public string Location { get; set; } = string.Empty;
}