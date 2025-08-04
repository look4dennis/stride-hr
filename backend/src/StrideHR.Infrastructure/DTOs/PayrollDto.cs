using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.DTOs;

public class CalculatePayrollDto
{
    public int EmployeeId { get; set; }
    public PayrollPeriod Period { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollPeriodDto? PayrollPeriod { get; set; }
}

public class PayrollPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class ProcessBranchPayrollDto
{
    public int BranchId { get; set; }
    public PayrollPeriod Period { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public PayrollPeriodDto? PayrollPeriod { get; set; }
}

public class PayrollRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal OvertimeAmount { get; set; }
    public PayrollStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovePayrollDto
{
    public PayrollApprovalLevel ApprovalLevel { get; set; }
    public string? Notes { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class ReleasePayrollDto
{
    public string? Notes { get; set; }
    public string? ReleaseNotes { get; set; }
    public bool NotifyEmployees { get; set; } = true;
}

public class PayslipDto
{
    public int Id { get; set; }
    public int PayrollRecordId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string PayslipPath { get; set; } = string.Empty;
    public decimal NetSalary { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class PayrollReportCriteria
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? BranchId { get; set; }
    public PayrollStatus? Status { get; set; }
    public bool IncludeDeductions { get; set; } = true;
    public bool IncludeAllowances { get; set; } = true;
}

public class PayrollReportDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
}

public class CreatePayrollFormulaDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FormulaExpression { get; set; } = string.Empty;
    public PayrollFormulaCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PayrollFormulaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FormulaExpression { get; set; } = string.Empty;
    public PayrollFormulaCategory Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}