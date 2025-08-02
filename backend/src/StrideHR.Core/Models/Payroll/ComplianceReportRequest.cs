namespace StrideHR.Core.Models.Payroll;

public class ComplianceReportRequest
{
    public int BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ComplianceReportType ReportType { get; set; }
    public string Country { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; } = new();
}

public enum ComplianceReportType
{
    TaxDeduction,
    ProvidentFund,
    EmployeeStateInsurance,
    ProfessionalTax,
    SocialSecurity,
    LaborLaw,
    StatutoryDeductions,
    AnnualTaxStatement,
    QuarterlyCompliance
}

public class ComplianceReportResult
{
    public string ReportTitle { get; set; } = string.Empty;
    public ComplianceReportType ReportType { get; set; }
    public string Country { get; set; } = string.Empty;
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public ComplianceReportSummary Summary { get; set; } = new();
    public List<ComplianceReportItem> Items { get; set; } = new();
    public List<ComplianceViolation> Violations { get; set; } = new();
    public Dictionary<string, object> StatutoryInformation { get; set; } = new();
}

public class ComplianceReportSummary
{
    public int TotalEmployees { get; set; }
    public decimal TotalTaxDeducted { get; set; }
    public decimal TotalProvidentFund { get; set; }
    public decimal TotalESI { get; set; }
    public decimal TotalProfessionalTax { get; set; }
    public decimal TotalStatutoryDeductions { get; set; }
    public Dictionary<string, decimal> MonthlyBreakdown { get; set; } = new();
}

public class ComplianceReportItem
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string PANNumber { get; set; } = string.Empty;
    public string PFNumber { get; set; } = string.Empty;
    public string ESINumber { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal TaxDeducted { get; set; }
    public decimal ProvidentFund { get; set; }
    public decimal ESIContribution { get; set; }
    public decimal ProfessionalTax { get; set; }
    public Dictionary<string, decimal> StatutoryDeductions { get; set; } = new();
}

public class ComplianceViolation
{
    public string ViolationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public decimal? Amount { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}