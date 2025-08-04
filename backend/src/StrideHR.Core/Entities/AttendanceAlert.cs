using StrideHR.Core.Models.Attendance;

namespace StrideHR.Core.Entities;

public class AttendanceAlert : BaseEntity
{
    public AttendanceAlertType AlertType { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public new DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; } = false;
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Metadata { get; set; } = "{}"; // JSON string for additional data
    public new int? CreatedBy { get; set; }
    public DateTime? ReadAt { get; set; }
    public int? ReadBy { get; set; }
    
    // Navigation Properties
    public virtual Employee? Employee { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual Employee? CreatedByEmployee { get; set; }
    public virtual Employee? ReadByEmployee { get; set; }
}