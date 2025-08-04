using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Attendance;

public class AttendanceReportRequest
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public int? EmployeeId { get; set; }
    
    public int? BranchId { get; set; }
    
    public int? DepartmentId { get; set; }
    
    public string? ReportType { get; set; } = "summary"; // summary, detailed, analytics
    
    public string? Format { get; set; } = "json"; // json, excel, pdf
    
    public bool IncludeBreakDetails { get; set; } = false;
    
    public bool IncludeOvertimeDetails { get; set; } = false;
    
    public bool IncludeLateArrivals { get; set; } = true;
    
    public bool IncludeEarlyDepartures { get; set; } = true;
}