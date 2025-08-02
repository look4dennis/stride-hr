namespace StrideHR.Core.Models.Payroll;

public class PayrollCalculationRequest
{
    public int EmployeeId { get; set; }
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public bool IncludeCustomFormulas { get; set; } = true;
    public Dictionary<string, decimal> CustomValues { get; set; } = new();
}