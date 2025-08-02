namespace StrideHR.Core.Models.Payroll;

public class PayrollCalculationResult
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal OvertimeAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public Dictionary<string, decimal> AllowanceBreakdown { get; set; } = new();
    public Dictionary<string, decimal> DeductionBreakdown { get; set; } = new();
    public Dictionary<string, decimal> CustomCalculations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}