using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IPayrollFormulaRepository _formulaRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<AttendanceRecord> _attendanceRepository;
    private readonly IPayrollFormulaEngine _formulaEngine;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IPayrollRepository payrollRepository,
        IPayrollFormulaRepository formulaRepository,
        IRepository<Employee> employeeRepository,
        IRepository<AttendanceRecord> attendanceRepository,
        IPayrollFormulaEngine formulaEngine,
        ICurrencyService currencyService,
        ILogger<PayrollService> logger)
    {
        _payrollRepository = payrollRepository;
        _formulaRepository = formulaRepository;
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _formulaEngine = formulaEngine;
        _currencyService = currencyService;
        _logger = logger;
    }

    public async Task<PayrollCalculationResult> CalculatePayrollAsync(PayrollCalculationRequest request)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with ID {request.EmployeeId} not found");
            }

            var result = new PayrollCalculationResult
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                BasicSalary = employee.BasicSalary,
                Currency = employee.Branch.Currency
            };

            // Get working days information
            var (workingDays, actualWorkingDays, absentDays, leaveDays) = 
                await GetWorkingDaysInfoAsync(request.EmployeeId, request.PayrollPeriodStart, request.PayrollPeriodEnd);

            // Get overtime hours
            var overtimeHours = await GetOvertimeHoursAsync(request.EmployeeId, request.PayrollPeriodStart, request.PayrollPeriodEnd);

            // Create evaluation context
            var context = new FormulaEvaluationContext
            {
                Employee = employee,
                Branch = employee.Branch,
                Organization = employee.Branch.Organization,
                PayrollPeriodStart = request.PayrollPeriodStart,
                PayrollPeriodEnd = request.PayrollPeriodEnd,
                BasicSalary = employee.BasicSalary,
                OvertimeHours = overtimeHours,
                WorkingDays = workingDays,
                ActualWorkingDays = actualWorkingDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                CustomValues = request.CustomValues
            };

            // Calculate basic components
            result.BasicSalary = employee.BasicSalary;

            // Calculate overtime
            var overtimeRate = employee.Branch.Organization.OvertimeRate;
            result.OvertimeAmount = await _formulaEngine.CalculateOvertimeAmountAsync(overtimeHours, employee.BasicSalary, overtimeRate);

            if (request.IncludeCustomFormulas)
            {
                // Get applicable formulas
                var formulas = await _formulaRepository.GetFormulasForEmployeeAsync(request.EmployeeId);
                var activeFormulas = formulas.Where(f => f.IsActive).ToList();

                // Evaluate all formulas
                var formulaResults = await _formulaEngine.EvaluateAllFormulasAsync(context, activeFormulas);

                // Categorize results
                foreach (var formulaResult in formulaResults)
                {
                    var formula = activeFormulas.First(f => f.Name == formulaResult.Key);
                    switch (formula.Type)
                    {
                        case PayrollFormulaType.Allowance:
                            result.AllowanceBreakdown[formulaResult.Key] = formulaResult.Value;
                            result.TotalAllowances += formulaResult.Value;
                            break;
                        case PayrollFormulaType.Deduction:
                        case PayrollFormulaType.Tax:
                            result.DeductionBreakdown[formulaResult.Key] = formulaResult.Value;
                            result.TotalDeductions += formulaResult.Value;
                            break;
                        default:
                            result.CustomCalculations[formulaResult.Key] = formulaResult.Value;
                            break;
                    }
                }
            }

            // Calculate gross and net salary
            result.GrossSalary = result.BasicSalary + result.TotalAllowances + result.OvertimeAmount;
            result.NetSalary = result.GrossSalary - result.TotalDeductions;

            // Handle currency conversion if needed
            if (employee.Branch.Currency != "USD")
            {
                result.ExchangeRate = await _currencyService.GetExchangeRateAsync(employee.Branch.Currency, "USD");
            }

            _logger.LogInformation("Payroll calculated for employee {EmployeeId}: Net Salary = {NetSalary} {Currency}", 
                employee.Id, result.NetSalary, result.Currency);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating payroll for employee {EmployeeId}", request.EmployeeId);
            throw;
        }
    }

    public async Task<PayrollRecord> CreatePayrollRecordAsync(PayrollCalculationRequest request)
    {
        // Check if payroll already exists for this period
        var existingPayroll = await _payrollRepository.GetByEmployeeAndPeriodAsync(
            request.EmployeeId, request.PayrollYear, request.PayrollMonth);

        if (existingPayroll != null)
        {
            throw new InvalidOperationException($"Payroll already exists for employee {request.EmployeeId} for {request.PayrollMonth}/{request.PayrollYear}");
        }

        // Calculate payroll
        var calculation = await CalculatePayrollAsync(request);

        // Create payroll record
        var payrollRecord = new PayrollRecord
        {
            EmployeeId = request.EmployeeId,
            PayrollPeriodStart = request.PayrollPeriodStart,
            PayrollPeriodEnd = request.PayrollPeriodEnd,
            PayrollMonth = request.PayrollMonth,
            PayrollYear = request.PayrollYear,
            BasicSalary = calculation.BasicSalary,
            GrossSalary = calculation.GrossSalary,
            NetSalary = calculation.NetSalary,
            TotalAllowances = calculation.TotalAllowances,
            TotalDeductions = calculation.TotalDeductions,
            OvertimeAmount = calculation.OvertimeAmount,
            Currency = calculation.Currency,
            ExchangeRate = calculation.ExchangeRate,
            Status = PayrollStatus.Calculated,
            CustomCalculations = System.Text.Json.JsonSerializer.Serialize(calculation.CustomCalculations)
        };

        // Set individual allowance and deduction amounts
        SetAllowanceAmounts(payrollRecord, calculation.AllowanceBreakdown);
        SetDeductionAmounts(payrollRecord, calculation.DeductionBreakdown);

        await _payrollRepository.AddAsync(payrollRecord);
        await _payrollRepository.SaveChangesAsync();

        _logger.LogInformation("Payroll record created for employee {EmployeeId} for period {Month}/{Year}", 
            request.EmployeeId, request.PayrollMonth, request.PayrollYear);

        return payrollRecord;
    }

    public async Task<PayrollRecord?> GetPayrollRecordAsync(int payrollRecordId)
    {
        return await _payrollRepository.GetByIdAsync(payrollRecordId);
    }

    public async Task<List<PayrollRecord>> GetEmployeePayrollRecordsAsync(int employeeId, int year, int? month = null)
    {
        return await _payrollRepository.GetByEmployeeAsync(employeeId, year, month);
    }

    public async Task<List<PayrollRecord>> GetBranchPayrollRecordsAsync(int branchId, int year, int month)
    {
        return await _payrollRepository.GetByBranchAndPeriodAsync(branchId, year, month);
    }

    public async Task<bool> ApprovePayrollRecordAsync(int payrollRecordId, int approvedBy)
    {
        var payrollRecord = await _payrollRepository.GetByIdAsync(payrollRecordId);
        if (payrollRecord == null)
            return false;

        payrollRecord.Status = PayrollStatus.Approved;
        payrollRecord.ApprovedBy = approvedBy;
        payrollRecord.ApprovedAt = DateTime.UtcNow;

        await _payrollRepository.UpdateAsync(payrollRecord);
        await _payrollRepository.SaveChangesAsync();

        _logger.LogInformation("Payroll record {PayrollRecordId} approved by {ApprovedBy}", payrollRecordId, approvedBy);

        return true;
    }

    public async Task<List<PayrollCalculationResult>> ProcessBranchPayrollAsync(int branchId, int year, int month)
    {
        var employees = await _employeeRepository.GetAllAsync();
        var branchEmployees = employees.Where(e => e.BranchId == branchId && e.Status == EmployeeStatus.Active).ToList();

        var results = new List<PayrollCalculationResult>();
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        foreach (var employee in branchEmployees)
        {
            try
            {
                var request = new PayrollCalculationRequest
                {
                    EmployeeId = employee.Id,
                    PayrollPeriodStart = startDate,
                    PayrollPeriodEnd = endDate,
                    PayrollMonth = month,
                    PayrollYear = year
                };

                var result = await CalculatePayrollAsync(request);
                results.Add(result);

                // Create payroll record if it doesn't exist
                var existingRecord = await _payrollRepository.GetByEmployeeAndPeriodAsync(employee.Id, year, month);
                if (existingRecord == null)
                {
                    await CreatePayrollRecordAsync(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payroll for employee {EmployeeId}", employee.Id);
                
                var errorResult = new PayrollCalculationResult
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName
                };
                errorResult.Errors.Add($"Error processing payroll: {ex.Message}");
                results.Add(errorResult);
            }
        }

        return results;
    }

    public async Task<decimal> GetOvertimeHoursAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var attendanceRecords = await _attendanceRepository.GetAllAsync();
        var employeeAttendance = attendanceRecords
            .Where(a => a.EmployeeId == employeeId && 
                       a.Date >= startDate && 
                       a.Date <= endDate)
            .ToList();

        return employeeAttendance.Sum(a => (decimal)(a.OvertimeHours?.TotalHours ?? 0));
    }

    public async Task<(int workingDays, int actualWorkingDays, int absentDays, int leaveDays)> GetWorkingDaysInfoAsync(
        int employeeId, DateTime startDate, DateTime endDate)
    {
        var attendanceRecords = await _attendanceRepository.GetAllAsync();
        var employeeAttendance = attendanceRecords
            .Where(a => a.EmployeeId == employeeId && 
                       a.Date >= startDate && 
                       a.Date <= endDate)
            .ToList();

        var totalDays = (endDate - startDate).Days + 1;
        var weekends = 0;
        
        // Calculate weekends (assuming Saturday and Sunday are weekends)
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                weekends++;
        }

        var workingDays = totalDays - weekends;
        var actualWorkingDays = employeeAttendance.Count(a => a.Status == AttendanceStatus.Present);
        var absentDays = employeeAttendance.Count(a => a.Status == AttendanceStatus.Absent);
        var leaveDays = employeeAttendance.Count(a => a.Status == AttendanceStatus.OnLeave);

        return (workingDays, actualWorkingDays, absentDays, leaveDays);
    }

    private static void SetAllowanceAmounts(PayrollRecord record, Dictionary<string, decimal> allowances)
    {
        foreach (var allowance in allowances)
        {
            switch (allowance.Key.ToLower())
            {
                case "houserentallowance":
                case "hra":
                    record.HouseRentAllowance = allowance.Value;
                    break;
                case "transportallowance":
                case "ta":
                    record.TransportAllowance = allowance.Value;
                    break;
                case "medicalallowance":
                case "ma":
                    record.MedicalAllowance = allowance.Value;
                    break;
                case "foodallowance":
                case "fa":
                    record.FoodAllowance = allowance.Value;
                    break;
                default:
                    record.OtherAllowances += allowance.Value;
                    break;
            }
        }
    }

    private static void SetDeductionAmounts(PayrollRecord record, Dictionary<string, decimal> deductions)
    {
        foreach (var deduction in deductions)
        {
            switch (deduction.Key.ToLower())
            {
                case "tax":
                case "incometax":
                    record.TaxDeduction = deduction.Value;
                    break;
                case "pf":
                case "providentfund":
                    record.ProvidentFund = deduction.Value;
                    break;
                case "esi":
                case "employeestateinsurance":
                    record.EmployeeStateInsurance = deduction.Value;
                    break;
                case "professionaltax":
                case "pt":
                    record.ProfessionalTax = deduction.Value;
                    break;
                case "loan":
                case "loandeduction":
                    record.LoanDeduction = deduction.Value;
                    break;
                case "advance":
                case "advancededuction":
                    record.AdvanceDeduction = deduction.Value;
                    break;
                default:
                    record.OtherDeductions += deduction.Value;
                    break;
            }
        }
    }
}