namespace StrideHR.Core.Models.Payroll;

public class PayrollReportRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Currency { get; set; }
    public bool IncludeCurrencyConversion { get; set; } = true;
    public PayrollReportType ReportType { get; set; } = PayrollReportType.Summary;
    public List<string>? IncludeColumns { get; set; }
    public Dictionary<string, object>? Filters { get; set; } = new();
}

public enum PayrollReportType
{
    Summary,
    Detailed,
    Compliance,
    Analytics,
    BudgetVariance,
    AuditTrail
}