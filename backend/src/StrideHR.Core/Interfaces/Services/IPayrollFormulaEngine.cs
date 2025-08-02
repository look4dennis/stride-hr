using StrideHR.Core.Entities;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayrollFormulaEngine
{
    /// <summary>
    /// Evaluates a mathematical formula with given variables
    /// </summary>
    Task<decimal> EvaluateFormulaAsync(string formula, Dictionary<string, decimal> variables);
    
    /// <summary>
    /// Validates if a formula is syntactically correct
    /// </summary>
    Task<bool> ValidateFormulaAsync(string formula, List<string> variables);
    
    /// <summary>
    /// Gets all available variables for formula evaluation
    /// </summary>
    Task<List<FormulaVariable>> GetAvailableVariablesAsync(FormulaEvaluationContext context);
    
    /// <summary>
    /// Evaluates all formulas for a specific employee and payroll period
    /// </summary>
    Task<Dictionary<string, decimal>> EvaluateAllFormulasAsync(FormulaEvaluationContext context, List<PayrollFormula> formulas);
    
    /// <summary>
    /// Calculates overtime amount based on attendance data
    /// </summary>
    Task<decimal> CalculateOvertimeAmountAsync(decimal overtimeHours, decimal basicSalary, decimal overtimeRate);
    
    /// <summary>
    /// Parses and extracts variables from a formula
    /// </summary>
    Task<List<string>> ExtractVariablesFromFormulaAsync(string formula);
}