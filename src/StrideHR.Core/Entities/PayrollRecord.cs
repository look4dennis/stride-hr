using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Payroll record entity - placeholder for payroll management task
/// </summary>
public class PayrollRecord : AuditableEntity
{
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// Payroll period (month/year)
    /// </summary>
    [MaxLength(20)]
    public string Period { get; set; } = string.Empty;
    
    /// <summary>
    /// Basic salary amount
    /// </summary>
    public decimal BasicSalary { get; set; }
    
    /// <summary>
    /// Total allowances
    /// </summary>
    public decimal Allowances { get; set; }
    
    /// <summary>
    /// Overtime amount
    /// </summary>
    public decimal OvertimeAmount { get; set; }
    
    /// <summary>
    /// Gross salary (before deductions)
    /// </summary>
    public decimal GrossSalary { get; set; }
    
    /// <summary>
    /// Total deductions
    /// </summary>
    public decimal Deductions { get; set; }
    
    /// <summary>
    /// Net salary (after deductions)
    /// </summary>
    public decimal NetSalary { get; set; }
    
    /// <summary>
    /// Currency code
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Payroll status
    /// </summary>
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    
    /// <summary>
    /// Approved by (HR/Finance)
    /// </summary>
    public int? ApprovedBy { get; set; }
    
    /// <summary>
    /// Approval date
    /// </summary>
    public DateTime? ApprovalDate { get; set; }
    
    /// <summary>
    /// Payment date
    /// </summary>
    public DateTime? PaymentDate { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee? Approver { get; set; }
}

/// <summary>
/// Payroll status enumeration
/// </summary>
public enum PayrollStatus
{
    Draft = 1,
    Calculated = 2,
    HRApproved = 3,
    FinanceApproved = 4,
    Paid = 5,
    Cancelled = 6
}