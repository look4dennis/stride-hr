using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayrollRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    
    // Basic Salary Components
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    
    // Allowances
    public decimal HouseRentAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal FoodAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal TotalAllowances { get; set; }
    
    // Overtime Calculations
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal OvertimeAmount { get; set; }
    
    // Deductions
    public decimal TaxDeduction { get; set; }
    public decimal ProvidentFund { get; set; }
    public decimal EmployeeStateInsurance { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    
    // Attendance-based calculations
    public int WorkingDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal LeaveDeduction { get; set; }
    
    // Currency and Exchange Rate
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public string BaseCurrency { get; set; } = "USD";
    
    // Status and Approval
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Custom Formula Results
    public string CustomCalculations { get; set; } = "{}"; // JSON string for custom formula results
    
    // Audit Trail
    public string? Notes { get; set; }
    public string? PayslipPath { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? ApprovedByEmployee { get; set; }
    public virtual Employee? ProcessedByEmployee { get; set; }
    public virtual ICollection<PayrollAdjustment> PayrollAdjustments { get; set; } = new List<PayrollAdjustment>();
}