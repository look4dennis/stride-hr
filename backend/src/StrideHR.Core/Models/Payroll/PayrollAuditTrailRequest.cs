namespace StrideHR.Core.Models.Payroll;

public class PayrollAuditTrailRequest
{
    public int? PayrollRecordId { get; set; }
    public int? EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PayrollAuditAction? Action { get; set; }
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public enum PayrollAuditAction
{
    Created,
    Modified,
    Approved,
    Rejected,
    Processed,
    Cancelled,
    ErrorCorrected,
    Recalculated,
    PayslipGenerated,
    PayslipRegenerated
}

public class PayrollAuditTrailResult
{
    public List<PayrollAuditTrailItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PayrollAuditTrailItem
{
    public int Id { get; set; }
    public int PayrollRecordId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public PayrollAuditAction Action { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }
    public string? IPAddress { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}