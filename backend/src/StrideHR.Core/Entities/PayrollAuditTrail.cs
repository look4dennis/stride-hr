using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Entities;

public class PayrollAuditTrail : BaseEntity
{
    public int PayrollRecordId { get; set; }
    public int EmployeeId { get; set; }
    public PayrollAuditAction Action { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Reason { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string AdditionalData { get; set; } = "{}"; // JSON
    
    // Navigation Properties
    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}