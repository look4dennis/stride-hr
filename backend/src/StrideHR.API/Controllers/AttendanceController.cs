using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Attendance;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : BaseController
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(
        IAttendanceService attendanceService,
        ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Get today's attendance record for the current employee
    /// </summary>
    [HttpGet("today")]
    public async Task<ActionResult<ApiResponse<object>>> GetTodayAttendance()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var attendance = await _attendanceService.GetTodayAttendanceAsync(employeeId);
            
            return Ok(ApiResponse<object>.CreateSuccess(attendance, "Today's attendance retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's attendance");
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve today's attendance"));
        }
    }

    /// <summary>
    /// Get attendance records for a date range
    /// </summary>
    [HttpGet("range")]
    public async Task<ActionResult<ApiResponse<object>>> GetAttendanceRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var attendance = await _attendanceService.GetEmployeeAttendanceAsync(employeeId, startDate, endDate);
            
            return Ok(ApiResponse<object>.CreateSuccess(attendance, "Attendance records retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance range");
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve attendance records"));
        }
    }

    /// <summary>
    /// Check in for the current employee
    /// </summary>
    [HttpPost("checkin")]
    public async Task<ActionResult<ApiResponse<object>>> CheckIn([FromBody] CheckInRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var attendance = await _attendanceService.CheckInAsync(
                employeeId,
                request.Location,
                request.Latitude,
                request.Longitude,
                request.DeviceInfo,
                ipAddress
            );
            
            return Ok(ApiResponse<object>.CreateSuccess(attendance, "Check-in successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-in for employee {EmployeeId}", GetCurrentEmployeeId());
            return BadRequest(ApiResponse<object>.CreateFailure("Check-in failed"));
        }
    }

    /// <summary>
    /// Check out for the current employee
    /// </summary>
    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<object>>> CheckOut([FromBody] CheckOutRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            
            var attendance = await _attendanceService.CheckOutAsync(
                employeeId,
                request.Location,
                request.Latitude,
                request.Longitude
            );
            
            return Ok(ApiResponse<object>.CreateSuccess(attendance, "Check-out successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-out for employee {EmployeeId}", GetCurrentEmployeeId());
            return BadRequest(ApiResponse<object>.CreateFailure("Check-out failed"));
        }
    }

    /// <summary>
    /// Start a break for the current employee
    /// </summary>
    [HttpPost("break/start")]
    public async Task<ActionResult<ApiResponse<object>>> StartBreak([FromBody] StartBreakRequest request)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            
            var breakRecord = await _attendanceService.StartBreakAsync(
                employeeId,
                request.BreakType,
                request.Location,
                request.Latitude,
                request.Longitude
            );
            
            return Ok(ApiResponse<object>.CreateSuccess(breakRecord, "Break started successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting break for employee {EmployeeId}", GetCurrentEmployeeId());
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to start break"));
        }
    }

    /// <summary>
    /// End the current break for the employee
    /// </summary>
    [HttpPost("break/end")]
    public async Task<ActionResult<ApiResponse<object>>> EndBreak()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var breakRecord = await _attendanceService.EndBreakAsync(employeeId);
            
            return Ok(ApiResponse<object>.CreateSuccess(breakRecord, "Break ended successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending break for employee {EmployeeId}", GetCurrentEmployeeId());
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to end break"));
        }
    }

    /// <summary>
    /// Get current attendance status for the employee
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<AttendanceStatus>>> GetCurrentStatus()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var status = await _attendanceService.GetEmployeeCurrentStatusAsync(employeeId);
            
            return Ok(ApiResponse<AttendanceStatus>.CreateSuccess(status, "Status retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status for employee {EmployeeId}", GetCurrentEmployeeId());
            return BadRequest(ApiResponse<AttendanceStatus>.CreateFailure("Failed to retrieve status"));
        }
    }

    /// <summary>
    /// Get today's branch attendance (HR/Manager access)
    /// </summary>
    [HttpGet("branch/{branchId}/today")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetBranchTodayAttendance(int branchId)
    {
        try
        {
            var attendance = await _attendanceService.GetTodayBranchAttendanceAsync(branchId);
            return Ok(ApiResponse<object>.CreateSuccess(attendance, "Branch attendance retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch attendance for branch {BranchId}", branchId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve branch attendance"));
        }
    }

    /// <summary>
    /// Get currently present employees in branch (HR/Manager access)
    /// </summary>
    [HttpGet("branch/{branchId}/present")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetCurrentlyPresentEmployees(int branchId)
    {
        try
        {
            var employees = await _attendanceService.GetCurrentlyPresentEmployeesAsync(branchId);
            return Ok(ApiResponse<object>.CreateSuccess(employees, "Present employees retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving present employees for branch {BranchId}", branchId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve present employees"));
        }
    }

    /// <summary>
    /// Get employees currently on break (HR/Manager access)
    /// </summary>
    [HttpGet("branch/{branchId}/on-break")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetEmployeesOnBreak(int branchId)
    {
        try
        {
            var employees = await _attendanceService.GetEmployeesOnBreakAsync(branchId);
            return Ok(ApiResponse<object>.CreateSuccess(employees, "Employees on break retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees on break for branch {BranchId}", branchId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve employees on break"));
        }
    }

    /// <summary>
    /// Get late employees today (HR/Manager access)
    /// </summary>
    [HttpGet("branch/{branchId}/late")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetLateEmployeesToday(int branchId)
    {
        try
        {
            var employees = await _attendanceService.GetLateEmployeesTodayAsync(branchId);
            return Ok(ApiResponse<object>.CreateSuccess(employees, "Late employees retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving late employees for branch {BranchId}", branchId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve late employees"));
        }
    }

    /// <summary>
    /// Correct attendance record (HR access only)
    /// </summary>
    [HttpPut("{attendanceRecordId}/correct")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> CorrectAttendance(
        int attendanceRecordId,
        [FromBody] AttendanceCorrectionRequest request)
    {
        try
        {
            var correctedBy = GetCurrentUserId();
            var correctedRecord = await _attendanceService.CorrectAttendanceAsync(
                attendanceRecordId,
                correctedBy,
                request.CheckInTime,
                request.CheckOutTime,
                request.Reason
            );
            
            return Ok(ApiResponse<object>.CreateSuccess(correctedRecord, "Attendance corrected successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error correcting attendance record {RecordId}", attendanceRecordId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to correct attendance"));
        }
    }

    /// <summary>
    /// Add missing attendance record (HR access only)
    /// </summary>
    [HttpPost("add-missing")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> AddMissingAttendance([FromBody] AddMissingAttendanceRequest request)
    {
        try
        {
            var addedBy = GetCurrentUserId();
            var record = await _attendanceService.AddMissingAttendanceAsync(
                request.EmployeeId,
                request.Date,
                request.CheckInTime,
                request.CheckOutTime,
                addedBy,
                request.Reason
            );
            
            return Ok(ApiResponse<object>.CreateSuccess(record, "Missing attendance added successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding missing attendance for employee {EmployeeId}", request.EmployeeId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to add missing attendance"));
        }
    }

    /// <summary>
    /// Delete attendance record (HR access only)
    /// </summary>
    [HttpDelete("{attendanceRecordId}")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAttendanceRecord(
        int attendanceRecordId,
        [FromQuery] string reason)
    {
        try
        {
            var deletedBy = GetCurrentUserId();
            var result = await _attendanceService.DeleteAttendanceRecordAsync(attendanceRecordId, deletedBy, reason);
            
            if (result)
            {
                return Ok(ApiResponse<bool>.CreateSuccess(true, "Attendance record deleted successfully"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.CreateFailure("Attendance record not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attendance record {RecordId}", attendanceRecordId);
            return BadRequest(ApiResponse<bool>.CreateFailure("Failed to delete attendance record"));
        }
    }

    /// <summary>
    /// Get attendance analytics for an employee (HR/Manager access)
    /// </summary>
    [HttpGet("analytics/{employeeId}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetAttendanceAnalytics(
        int employeeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var averageHours = await _attendanceService.GetAverageWorkingHoursAsync(employeeId, startDate, endDate);
            var lateCount = await _attendanceService.GetLateCountAsync(employeeId, startDate, endDate);
            var totalOvertime = await _attendanceService.GetTotalOvertimeAsync(employeeId, startDate, endDate);

            var analytics = new
            {
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                AverageWorkingHours = averageHours,
                LateCount = lateCount,
                TotalOvertimeHours = totalOvertime
            };

            return Ok(ApiResponse<object>.CreateSuccess(analytics, "Attendance analytics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance analytics for employee {EmployeeId}", employeeId);
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to retrieve attendance analytics"));
        }
    }

    /// <summary>
    /// Generate attendance report (HR/Manager access)
    /// </summary>
    [HttpPost("reports/generate")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<AttendanceReportResponse>>> GenerateAttendanceReport([FromBody] AttendanceReportRequest request)
    {
        try
        {
            var report = await _attendanceService.GenerateAttendanceReportAsync(request);
            return Ok(ApiResponse<AttendanceReportResponse>.CreateSuccess(report, "Attendance report generated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating attendance report");
            return BadRequest(ApiResponse<AttendanceReportResponse>.CreateFailure("Failed to generate attendance report"));
        }
    }

    /// <summary>
    /// Export attendance report (HR/Manager access)
    /// </summary>
    [HttpPost("reports/export")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<IActionResult> ExportAttendanceReport([FromBody] AttendanceReportRequest request, [FromQuery] string format = "excel")
    {
        try
        {
            var data = await _attendanceService.ExportAttendanceReportAsync(request, format);
            var contentType = format.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                _ => "application/json"
            };
            
            var fileName = $"attendance_report_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting attendance report");
            return BadRequest(ApiResponse<object>.CreateFailure("Failed to export attendance report"));
        }
    }

    /// <summary>
    /// Get attendance calendar for an employee
    /// </summary>
    [HttpGet("calendar/{employeeId}/{year}/{month}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<AttendanceCalendarResponse>>> GetAttendanceCalendar(int employeeId, int year, int month)
    {
        try
        {
            var calendar = await _attendanceService.GetAttendanceCalendarAsync(employeeId, year, month);
            return Ok(ApiResponse<AttendanceCalendarResponse>.CreateSuccess(calendar, "Attendance calendar retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance calendar for employee {EmployeeId}", employeeId);
            return BadRequest(ApiResponse<AttendanceCalendarResponse>.CreateFailure("Failed to retrieve attendance calendar"));
        }
    }

    /// <summary>
    /// Get current employee's attendance calendar
    /// </summary>
    [HttpGet("calendar/{year}/{month}")]
    public async Task<ActionResult<ApiResponse<AttendanceCalendarResponse>>> GetMyAttendanceCalendar(int year, int month)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var calendar = await _attendanceService.GetAttendanceCalendarAsync(employeeId, year, month);
            return Ok(ApiResponse<AttendanceCalendarResponse>.CreateSuccess(calendar, "Attendance calendar retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance calendar for current employee");
            return BadRequest(ApiResponse<AttendanceCalendarResponse>.CreateFailure("Failed to retrieve attendance calendar"));
        }
    }

    /// <summary>
    /// Get attendance alerts (HR/Manager access)
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceAlertResponse>>>> GetAttendanceAlerts(
        [FromQuery] int? branchId = null,
        [FromQuery] bool unreadOnly = false)
    {
        try
        {
            var alerts = await _attendanceService.GetAttendanceAlertsAsync(branchId, unreadOnly);
            return Ok(ApiResponse<IEnumerable<AttendanceAlertResponse>>.CreateSuccess(alerts, "Attendance alerts retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance alerts");
            return BadRequest(ApiResponse<IEnumerable<AttendanceAlertResponse>>.CreateFailure("Failed to retrieve attendance alerts"));
        }
    }

    /// <summary>
    /// Create attendance alert (HR/Manager access)
    /// </summary>
    [HttpPost("alerts")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<AttendanceAlertResponse>>> CreateAttendanceAlert([FromBody] AttendanceAlertRequest request)
    {
        try
        {
            var alert = await _attendanceService.CreateAttendanceAlertAsync(request);
            return Ok(ApiResponse<AttendanceAlertResponse>.CreateSuccess(alert, "Attendance alert created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating attendance alert");
            return BadRequest(ApiResponse<AttendanceAlertResponse>.CreateFailure("Failed to create attendance alert"));
        }
    }

    /// <summary>
    /// Mark attendance alert as read (HR/Manager access)
    /// </summary>
    [HttpPut("alerts/{alertId}/read")]
    [Authorize(Roles = "HR,Manager,Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAlertAsRead(int alertId)
    {
        try
        {
            var result = await _attendanceService.MarkAlertAsReadAsync(alertId);
            if (result)
            {
                return Ok(ApiResponse<bool>.CreateSuccess(true, "Alert marked as read successfully"));
            }
            else
            {
                return NotFound(ApiResponse<bool>.CreateFailure("Alert not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking alert as read");
            return BadRequest(ApiResponse<bool>.CreateFailure("Failed to mark alert as read"));
        }
    }

    /// <summary>
    /// Get attendance records that need correction (HR access)
    /// </summary>
    [HttpGet("corrections/pending")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceRecord>>>> GetPendingCorrections(
        [FromQuery] int branchId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var records = await _attendanceService.GetAttendanceRecordsForCorrectionAsync(branchId, startDate, endDate);
            return Ok(ApiResponse<IEnumerable<AttendanceRecord>>.CreateSuccess(records, "Pending corrections retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending corrections");
            return BadRequest(ApiResponse<IEnumerable<AttendanceRecord>>.CreateFailure("Failed to retrieve pending corrections"));
        }
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(employeeIdClaim, out int employeeId))
        {
            return employeeId;
        }
        throw new UnauthorizedAccessException("Employee ID not found in token");
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}