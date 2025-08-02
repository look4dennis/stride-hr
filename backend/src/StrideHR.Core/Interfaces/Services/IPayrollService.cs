using StrideHR.Core.Entities;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayrollService
{
    /// <summary>
    /// Calculates payroll for a specific employee
    /// </summary>
    Task<PayrollCalculationResult> CalculatePayrollAsync(PayrollCalculationRequest request);
    
    /// <summary>
    /// Creates and saves a payroll record
    /// </summary>
    Task<PayrollRecord> CreatePayrollRecordAsync(PayrollCalculationRequest request);
    
    /// <summary>
    /// Gets payroll record by ID
    /// </summary>
    Task<PayrollRecord?> GetPayrollRecordAsync(int payrollRecordId);
    
    /// <summary>
    /// Gets payroll records for an employee
    /// </summary>
    Task<List<PayrollRecord>> GetEmployeePayrollRecordsAsync(int employeeId, int year, int? month = null);
    
    /// <summary>
    /// Gets payroll records for a branch
    /// </summary>
    Task<List<PayrollRecord>> GetBranchPayrollRecordsAsync(int branchId, int year, int month);
    
    /// <summary>
    /// Approves a payroll record
    /// </summary>
    Task<bool> ApprovePayrollRecordAsync(int payrollRecordId, int approvedBy);
    
    /// <summary>
    /// Processes payroll for multiple employees
    /// </summary>
    Task<List<PayrollCalculationResult>> ProcessBranchPayrollAsync(int branchId, int year, int month);
    
    /// <summary>
    /// Gets overtime hours from attendance data
    /// </summary>
    Task<decimal> GetOvertimeHoursAsync(int employeeId, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets working days information for payroll calculation
    /// </summary>
    Task<(int workingDays, int actualWorkingDays, int absentDays, int leaveDays)> GetWorkingDaysInfoAsync(
        int employeeId, DateTime startDate, DateTime endDate);
}