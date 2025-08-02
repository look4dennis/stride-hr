namespace StrideHR.Core.Models.Payroll;

public class PayrollReportResult
{
    public string ReportTitle { get; set; } = string.Empty;
    public PayrollReportType ReportType { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public PayrollReportSummary Summary { get; set; } = new();
    public List<PayrollReportItem> Items { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class PayrollReportSummary
{
    public int TotalEmployees { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalTaxDeduction { get; set; }
    public decimal TotalProvidentFund { get; set; }
    public decimal AverageGrossSalary { get; set; }
    public decimal AverageNetSalary { get; set; }
    public Dictionary<string, decimal> CurrencyBreakdown { get; set; } = new();
    public Dictionary<string, decimal> DepartmentBreakdown { get; set; } = new();
}

public class PayrollReportItem
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal OvertimeAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal ConvertedGrossSalary { get; set; }
    public decimal ConvertedNetSalary { get; set; }
    public Dictionary<string, decimal> AllowanceBreakdown { get; set; } = new();
    public Dictionary<string, decimal> DeductionBreakdown { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}