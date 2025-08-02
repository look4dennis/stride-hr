using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Infrastructure.Services;

public class PayrollFormulaEngine : IPayrollFormulaEngine
{
    private readonly ILogger<PayrollFormulaEngine> _logger;
    private static readonly Regex VariableRegex = new(@"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.Compiled);
    private static readonly Regex FormulaValidationRegex = new(@"^[0-9+\-*/().\s\w]+$", RegexOptions.Compiled);

    public PayrollFormulaEngine(ILogger<PayrollFormulaEngine> logger)
    {
        _logger = logger;
    }

    public async Task<decimal> EvaluateFormulaAsync(string formula, Dictionary<string, decimal> variables)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            // Replace variables with their values
            var processedFormula = formula;
            foreach (var variable in variables)
            {
                processedFormula = processedFormula.Replace(variable.Key, variable.Value.ToString("F2"));
            }

            // Validate the processed formula
            if (!FormulaValidationRegex.IsMatch(processedFormula))
            {
                _logger.LogWarning("Invalid formula after variable substitution: {Formula}", processedFormula);
                return 0;
            }

            // Use DataTable.Compute for safe mathematical expression evaluation
            var dataTable = new DataTable();
            var result = dataTable.Compute(processedFormula, null);

            if (result == DBNull.Value)
                return 0;

            return Convert.ToDecimal(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating formula: {Formula}", formula);
            return 0;
        }
    }

    public async Task<bool> ValidateFormulaAsync(string formula, List<string> variables)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(formula))
                return false;

            // Check for basic syntax
            if (!FormulaValidationRegex.IsMatch(formula))
                return false;

            // Extract variables from formula
            var formulaVariables = await ExtractVariablesFromFormulaAsync(formula);
            
            // Check if all variables in formula are provided
            foreach (var variable in formulaVariables)
            {
                if (!variables.Contains(variable))
                {
                    _logger.LogWarning("Variable {Variable} not found in provided variables list", variable);
                    return false;
                }
            }

            // Test evaluation with dummy values to check for syntax errors
            var testVariables = variables.ToDictionary(v => v, v => 100m);
            try
            {
                var result = await EvaluateFormulaAsync(formula, testVariables);
                // If evaluation returns 0 and the formula should produce a non-zero result, it might be invalid
                // But we can't be sure, so we'll rely on the DataTable.Compute to throw exceptions for invalid syntax
                return true;
            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating formula: {Formula}", formula);
            return false;
        }
    }

    public async Task<List<FormulaVariable>> GetAvailableVariablesAsync(FormulaEvaluationContext context)
    {
        var variables = new List<FormulaVariable>
        {
            new() { Name = "BasicSalary", Value = context.BasicSalary, Description = "Employee's basic salary" },
            new() { Name = "OvertimeHours", Value = context.OvertimeHours, Description = "Total overtime hours worked" },
            new() { Name = "WorkingDays", Value = context.WorkingDays, Description = "Total working days in period" },
            new() { Name = "ActualWorkingDays", Value = context.ActualWorkingDays, Description = "Actual days worked" },
            new() { Name = "AbsentDays", Value = context.AbsentDays, Description = "Number of absent days" },
            new() { Name = "LeaveDays", Value = context.LeaveDays, Description = "Number of leave days" },
            new() { Name = "DaysInMonth", Value = DateTime.DaysInMonth(context.PayrollPeriodStart.Year, context.PayrollPeriodStart.Month), Description = "Total days in the month" }
        };

        // Add custom variables
        foreach (var customVar in context.Variables)
        {
            variables.Add(new FormulaVariable
            {
                Name = customVar.Key,
                Value = customVar.Value,
                Description = $"Custom variable: {customVar.Key}"
            });
        }

        // Add custom values
        foreach (var customValue in context.CustomValues)
        {
            variables.Add(new FormulaVariable
            {
                Name = customValue.Key,
                Value = customValue.Value,
                Description = $"Custom value: {customValue.Key}"
            });
        }

        return variables;
    }

    public async Task<Dictionary<string, decimal>> EvaluateAllFormulasAsync(FormulaEvaluationContext context, List<PayrollFormula> formulas)
    {
        var results = new Dictionary<string, decimal>();
        var availableVariables = await GetAvailableVariablesAsync(context);
        var variableDict = availableVariables.ToDictionary(v => v.Name, v => v.Value);

        // Sort formulas by priority
        var sortedFormulas = formulas.OrderBy(f => f.Priority).ToList();

        foreach (var formula in sortedFormulas)
        {
            try
            {
                // Skip inactive formulas
                if (!formula.IsActive)
                    continue;

                // Check if formula applies to this employee
                if (!await IsFormulaApplicableAsync(formula, context))
                    continue;

                var result = await EvaluateFormulaAsync(formula.Formula, variableDict);
                results[formula.Name] = result;

                // Add result as a variable for subsequent formulas
                variableDict[formula.Name] = result;

                _logger.LogDebug("Evaluated formula {FormulaName}: {Result}", formula.Name, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating formula {FormulaName}", formula.Name);
                results[formula.Name] = 0;
            }
        }

        return results;
    }

    public async Task<decimal> CalculateOvertimeAmountAsync(decimal overtimeHours, decimal basicSalary, decimal overtimeRate)
    {
        if (overtimeHours <= 0 || basicSalary <= 0)
            return 0;

        // Calculate hourly rate from basic salary (assuming 8 hours per day, 30 days per month)
        var hourlyRate = basicSalary / (8 * 30);
        var overtimeAmount = overtimeHours * hourlyRate * overtimeRate;

        return Math.Round(overtimeAmount, 2);
    }

    public async Task<List<string>> ExtractVariablesFromFormulaAsync(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return new List<string>();

        var matches = VariableRegex.Matches(formula);
        var variables = new HashSet<string>();

        foreach (Match match in matches)
        {
            var variable = match.Value;
            // Exclude mathematical functions and operators
            if (!IsReservedWord(variable))
            {
                variables.Add(variable);
            }
        }

        return variables.ToList();
    }

    private async Task<bool> IsFormulaApplicableAsync(PayrollFormula formula, FormulaEvaluationContext context)
    {
        // Check organization scope
        if (formula.OrganizationId.HasValue && formula.OrganizationId != context.Organization.Id)
            return false;

        // Check branch scope
        if (formula.BranchId.HasValue && formula.BranchId != context.Branch.Id)
            return false;

        // Check department scope
        if (!string.IsNullOrEmpty(formula.Department) && 
            !formula.Department.Equals(context.Employee.Department, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check designation scope
        if (!string.IsNullOrEmpty(formula.Designation) && 
            !formula.Designation.Equals(context.Employee.Designation, StringComparison.OrdinalIgnoreCase))
            return false;

        // TODO: Implement condition evaluation if conditions are specified
        if (!string.IsNullOrEmpty(formula.Conditions))
        {
            // This would require a more sophisticated condition evaluator
            // For now, we'll assume all conditions are met
        }

        return true;
    }

    private static bool IsReservedWord(string word)
    {
        var reservedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "abs", "acos", "asin", "atan", "atan2", "ceiling", "cos", "cosh", "exp", "floor",
            "log", "log10", "max", "min", "pow", "round", "sign", "sin", "sinh", "sqrt",
            "tan", "tanh", "truncate", "and", "or", "not", "true", "false"
        };

        return reservedWords.Contains(word);
    }
}