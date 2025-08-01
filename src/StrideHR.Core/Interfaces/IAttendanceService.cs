using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for attendance tracking operations
/// </summary>
public interface IAttendanceService
{
    // Check-in/Check-out Operations
    Task<AttendanceRecord> CheckInAsync(int employeeId, CheckInRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> CheckOutAsync(int employeeId, CheckOutRequest request, CancellationToken cancellationToken = default);
    
    // Break Management
    Task<BreakRecord> StartBreakAsync(int employeeId, StartBreakRequest request, CancellationToken cancellationToken = default);
    Task<BreakRecord> EndBreakAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BreakRecord>> GetActiveBreaksAsync(int employeeId, CancellationToken cancellationToken = default);
    
    // Real-time Status Tracking
    Task<AttendanceStatus> GetCurrentStatusAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetTodayAttendanceAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetTodayBranchAttendanceAsync(int branchId, CancellationToken cancellationToken = default);
    Task<AttendanceStatusSummary> GetBranchAttendanceSummaryAsync(int branchId, DateTime? date = null, CancellationToken cancellationToken = default);
    
    // Attendance Queries
    Task<AttendanceRecord?> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<(IEnumerable<AttendanceRecord> Records, int TotalCount)> SearchAttendanceAsync(AttendanceSearchCriteria criteria, CancellationToken cancellationToken = default);
    
    // Attendance Corrections
    Task<AttendanceCorrection> RequestCorrectionAsync(int attendanceId, CorrectionRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceCorrection> ApproveCorrectionAsync(int correctionId, int approvedBy, string? comments = null, CancellationToken cancellationToken = default);
    Task<AttendanceCorrection> RejectCorrectionAsync(int correctionId, int rejectedBy, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceCorrection>> GetPendingCorrectionsAsync(int? branchId = null, CancellationToken cancellationToken = default);
    
    // Manual Attendance Entry (HR)
    Task<AttendanceRecord> CreateManualEntryAsync(ManualAttendanceRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> UpdateManualEntryAsync(int attendanceId, ManualAttendanceRequest request, CancellationToken cancellationToken = default);
    
    // Analytics and Reporting
    Task<AttendanceReport> GenerateAttendanceReportAsync(AttendanceReportCriteria criteria, CancellationToken cancellationToken = default);
    Task<ProductivityReport> GenerateProductivityReportAsync(ProductivityReportCriteria criteria, CancellationToken cancellationToken = default);
    Task<IEnumerable<LateArrivalRecord>> GetLateArrivalsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<OvertimeRecord>> GetOvertimeRecordsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // Location Validation
    Task<bool> ValidateLocationAsync(string location, int branchId, CancellationToken cancellationToken = default);
    Task<LocationValidationResult> GetLocationValidationAsync(string location, int branchId, CancellationToken cancellationToken = default);
    
    // Shift Management Integration
    Task<Shift?> GetEmployeeCurrentShiftAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default);
    Task<bool> IsWithinShiftHoursAsync(int employeeId, DateTime time, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for check-in
/// </summary>
public class CheckInRequest
{
    public string Location { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? WeatherInfo { get; set; }
}

/// <summary>
/// Request model for check-out
/// </summary>
public class CheckOutRequest
{
    public string Location { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for starting a break
/// </summary>
public class StartBreakRequest
{
    public BreakType Type { get; set; }
    public string? Location { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for attendance correction
/// </summary>
public class CorrectionRequest
{
    public int RequestedBy { get; set; }
    public CorrectionType Type { get; set; }
    public string? OriginalValue { get; set; }
    public string CorrectedValue { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for manual attendance entry
/// </summary>
public class ManualAttendanceRequest
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Location { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int EnteredBy { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Search criteria for attendance records
/// </summary>
public class AttendanceSearchCriteria
{
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public string? Department { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public AttendanceStatus? Status { get; set; }
    public bool? IsLate { get; set; }
    public bool? HasOvertime { get; set; }
    public bool? IsManualEntry { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Attendance status summary for branch
/// </summary>
public class AttendanceStatusSummary
{
    public DateTime Date { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int OnBreakCount { get; set; }
    public int OnLeaveCount { get; set; }
    public decimal AttendancePercentage { get; set; }
    public List<EmployeeAttendanceStatus> EmployeeStatuses { get; set; } = new();
}

/// <summary>
/// Employee attendance status
/// </summary>
public class EmployeeAttendanceStatus
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public BreakType? CurrentBreakType { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
}

/// <summary>
/// Attendance report criteria
/// </summary>
public class AttendanceReportCriteria
{
    public int? BranchId { get; set; }
    public int? EmployeeId { get; set; }
    public string? Department { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public AttendanceReportType ReportType { get; set; }
    public bool IncludeBreakDetails { get; set; } = false;
    public bool IncludeOvertimeDetails { get; set; } = false;
}

/// <summary>
/// Attendance report
/// </summary>
public class AttendanceReport
{
    public AttendanceReportCriteria Criteria { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public int TotalRecords { get; set; }
    public List<AttendanceReportItem> Items { get; set; } = new();
    public AttendanceReportSummary Summary { get; set; } = new();
}

/// <summary>
/// Attendance report item
/// </summary>
public class AttendanceReportItem
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? OvertimeHours { get; set; }
    public bool IsLate { get; set; }
    public TimeSpan? LateBy { get; set; }
    public bool IsManualEntry { get; set; }
    public List<BreakRecord>? BreakDetails { get; set; }
}

/// <summary>
/// Attendance report summary
/// </summary>
public class AttendanceReportSummary
{
    public int TotalWorkingDays { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLateDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public TimeSpan TotalWorkingHours { get; set; }
    public TimeSpan TotalOvertimeHours { get; set; }
    public TimeSpan AverageWorkingHours { get; set; }
}

/// <summary>
/// Productivity report criteria
/// </summary>
public class ProductivityReportCriteria
{
    public int? BranchId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Productivity report
/// </summary>
public class ProductivityReport
{
    public ProductivityReportCriteria Criteria { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public List<ProductivityReportItem> Items { get; set; } = new();
    public ProductivityReportSummary Summary { get; set; } = new();
}

/// <summary>
/// Productivity report item
/// </summary>
public class ProductivityReportItem
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan ProductiveHours { get; set; }
    public TimeSpan IdleTime { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public bool MetProductivityThreshold { get; set; }
}

/// <summary>
/// Productivity report summary
/// </summary>
public class ProductivityReportSummary
{
    public TimeSpan TotalWorkingHours { get; set; }
    public TimeSpan TotalProductiveHours { get; set; }
    public TimeSpan TotalIdleTime { get; set; }
    public decimal AverageProductivityPercentage { get; set; }
    public int DaysMetThreshold { get; set; }
    public int TotalDays { get; set; }
}

/// <summary>
/// Late arrival record
/// </summary>
public class LateArrivalRecord
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime ExpectedTime { get; set; }
    public DateTime ActualTime { get; set; }
    public TimeSpan LateBy { get; set; }
}

/// <summary>
/// Overtime record
/// </summary>
public class OvertimeRecord
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan RegularHours { get; set; }
    public TimeSpan OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
}

/// <summary>
/// Location validation result
/// </summary>
public class LocationValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? DistanceFromOffice { get; set; }
    public string? NearestOfficeLocation { get; set; }
}

/// <summary>
/// Attendance report type enumeration
/// </summary>
public enum AttendanceReportType
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Custom = 4
}