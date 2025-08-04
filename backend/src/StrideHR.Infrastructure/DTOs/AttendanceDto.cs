namespace StrideHR.Infrastructure.DTOs;

public class CheckInDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
}

public class CheckOutDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Notes { get; set; }
}

public class StartBreakDto
{
    public string? Notes { get; set; }
    public string? BreakType { get; set; }
}

public class EndBreakDto
{
    public string? Notes { get; set; }
}

public class AttendanceRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public double? CheckOutLatitude { get; set; }
    public double? CheckOutLongitude { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BreakRecordDto
{
    public int Id { get; set; }
    public int AttendanceRecordId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal Duration { get; set; }
    public string? Notes { get; set; }
    public string? Type { get; set; }
}

public class AttendanceReportCriteria
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public List<int>? EmployeeIds { get; set; }
}

public class AttendanceReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal TotalHours { get; set; }
    public decimal OvertimeHours { get; set; }
}

public class AttendanceCorrectionDto
{
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}