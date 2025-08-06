using StrideHR.Core.Enums;
using StrideHR.Core.Models.Attendance;

namespace StrideHR.Core.Entities;

public class AttendanceAlert : BaseEntity
{
    public int EmployeeId { get; set; }
    public AttendanceAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public DateTime AlertDate { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedDate { get; set; }
    public int? ResolvedById { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Additional properties referenced in code
    public int? BranchId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ResolvedBy { get; set; }
    public virtual Branch? Branch { get; set; }
}