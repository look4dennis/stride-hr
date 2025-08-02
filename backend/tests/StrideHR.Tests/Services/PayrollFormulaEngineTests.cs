using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayrollFormulaEngineTests
{
    private readonly Mock<ILogger<PayrollFormulaEngine>> _mockLogger;
    private readonly PayrollFormulaEngine _formulaEngine;

    public PayrollFormulaEngineTests()
    {
        _mockLogger = new Mock<ILogger<PayrollFormulaEngine>>();
        _formulaEngine = new PayrollFormulaEngine(_mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateFormulaAsync_SimpleAddition_ReturnsCorrectResult()
    {
        // Arrange
        var formula = "BasicSalary + 1000";
        var variables = new Dictionary<string, decimal>
        {
            { "BasicSalary", 5000m }
        };

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(6000m, result);
    }

    [Fact]
    public async Task EvaluateFormulaAsync_ComplexFormula_ReturnsCorrectResult()
    {
        // Arrange
        var formula = "(BasicSalary * 0.4) + (BasicSalary * OvertimeHours / 160)";
        var variables = new Dictionary<string, decimal>
        {
            { "BasicSalary", 10000m },
            { "OvertimeHours", 20m }
        };

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(5250m, result); // (10000 * 0.4) + (10000 * 20 / 160) = 4000 + 1250 = 5250
    }

    [Fact]
    public async Task EvaluateFormulaAsync_DivisionByZero_ReturnsZero()
    {
        // Arrange
        var formula = "BasicSalary / WorkingDays";
        var variables = new Dictionary<string, decimal>
        {
            { "BasicSalary", 5000m },
            { "WorkingDays", 0m }
        };

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task EvaluateFormulaAsync_EmptyFormula_ReturnsZero()
    {
        // Arrange
        var formula = "";
        var variables = new Dictionary<string, decimal>();

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task EvaluateFormulaAsync_NullFormula_ReturnsZero()
    {
        // Arrange
        string formula = null!;
        var variables = new Dictionary<string, decimal>();

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task ValidateFormulaAsync_ValidFormula_ReturnsTrue()
    {
        // Arrange
        var formula = "BasicSalary * 0.4";
        var variables = new List<string> { "BasicSalary" };

        // Act
        var result = await _formulaEngine.ValidateFormulaAsync(formula, variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateFormulaAsync_InvalidFormula_ReturnsFalse()
    {
        // Arrange - Use a formula with invalid characters that will fail regex validation
        var formula = "BasicSalary * 0.4 & InvalidChar"; // Contains invalid character
        var variables = new List<string> { "BasicSalary" };

        // Act
        var result = await _formulaEngine.ValidateFormulaAsync(formula, variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFormulaAsync_MissingVariable_ReturnsFalse()
    {
        // Arrange
        var formula = "BasicSalary + MissingVariable";
        var variables = new List<string> { "BasicSalary" };

        // Act
        var result = await _formulaEngine.ValidateFormulaAsync(formula, variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFormulaAsync_EmptyFormula_ReturnsFalse()
    {
        // Arrange
        var formula = "";
        var variables = new List<string>();

        // Act
        var result = await _formulaEngine.ValidateFormulaAsync(formula, variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAvailableVariablesAsync_ReturnsExpectedVariables()
    {
        // Arrange
        var context = CreateTestEvaluationContext();

        // Act
        var result = await _formulaEngine.GetAvailableVariablesAsync(context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, v => v.Name == "BasicSalary");
        Assert.Contains(result, v => v.Name == "OvertimeHours");
        Assert.Contains(result, v => v.Name == "WorkingDays");
        Assert.Contains(result, v => v.Name == "ActualWorkingDays");
        Assert.Contains(result, v => v.Name == "AbsentDays");
        Assert.Contains(result, v => v.Name == "LeaveDays");
        Assert.Contains(result, v => v.Name == "DaysInMonth");
    }

    [Fact]
    public async Task GetAvailableVariablesAsync_IncludesCustomVariables()
    {
        // Arrange
        var context = CreateTestEvaluationContext();
        context.Variables["CustomVar1"] = 100m;
        context.CustomValues["CustomValue1"] = 200m;

        // Act
        var result = await _formulaEngine.GetAvailableVariablesAsync(context);

        // Assert
        Assert.Contains(result, v => v.Name == "CustomVar1" && v.Value == 100m);
        Assert.Contains(result, v => v.Name == "CustomValue1" && v.Value == 200m);
    }

    [Fact]
    public async Task EvaluateAllFormulasAsync_MultipleFormulas_ReturnsCorrectResults()
    {
        // Arrange
        var context = CreateTestEvaluationContext();
        var formulas = new List<PayrollFormula>
        {
            new()
            {
                Name = "HRA",
                Formula = "BasicSalary * 0.4",
                Type = PayrollFormulaType.Allowance,
                Priority = 1,
                IsActive = true
            },
            new()
            {
                Name = "PF",
                Formula = "BasicSalary * 0.12",
                Type = PayrollFormulaType.Deduction,
                Priority = 2,
                IsActive = true
            },
            new()
            {
                Name = "Bonus",
                Formula = "HRA * 0.1",
                Type = PayrollFormulaType.Allowance,
                Priority = 3,
                IsActive = true
            }
        };

        // Act
        var result = await _formulaEngine.EvaluateAllFormulasAsync(context, formulas);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(4000m, result["HRA"]); // 10000 * 0.4
        Assert.Equal(1200m, result["PF"]); // 10000 * 0.12
        Assert.Equal(400m, result["Bonus"]); // 4000 * 0.1 (uses HRA result)
    }

    [Fact]
    public async Task EvaluateAllFormulasAsync_InactiveFormula_SkipsFormula()
    {
        // Arrange
        var context = CreateTestEvaluationContext();
        var formulas = new List<PayrollFormula>
        {
            new()
            {
                Name = "HRA",
                Formula = "BasicSalary * 0.4",
                Type = PayrollFormulaType.Allowance,
                Priority = 1,
                IsActive = false // Inactive
            }
        };

        // Act
        var result = await _formulaEngine.EvaluateAllFormulasAsync(context, formulas);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CalculateOvertimeAmountAsync_ValidInputs_ReturnsCorrectAmount()
    {
        // Arrange
        var overtimeHours = 10m;
        var basicSalary = 12000m;
        var overtimeRate = 1.5m;

        // Act
        var result = await _formulaEngine.CalculateOvertimeAmountAsync(overtimeHours, basicSalary, overtimeRate);

        // Assert
        // Hourly rate = 12000 / (8 * 30) = 50
        // Overtime amount = 10 * 50 * 1.5 = 750
        Assert.Equal(750m, result);
    }

    [Fact]
    public async Task CalculateOvertimeAmountAsync_ZeroOvertimeHours_ReturnsZero()
    {
        // Arrange
        var overtimeHours = 0m;
        var basicSalary = 12000m;
        var overtimeRate = 1.5m;

        // Act
        var result = await _formulaEngine.CalculateOvertimeAmountAsync(overtimeHours, basicSalary, overtimeRate);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateOvertimeAmountAsync_ZeroBasicSalary_ReturnsZero()
    {
        // Arrange
        var overtimeHours = 10m;
        var basicSalary = 0m;
        var overtimeRate = 1.5m;

        // Act
        var result = await _formulaEngine.CalculateOvertimeAmountAsync(overtimeHours, basicSalary, overtimeRate);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task ExtractVariablesFromFormulaAsync_ComplexFormula_ReturnsAllVariables()
    {
        // Arrange
        var formula = "(BasicSalary + HRA) * TaxRate - PreviousDeduction";

        // Act
        var result = await _formulaEngine.ExtractVariablesFromFormulaAsync(formula);

        // Assert
        Assert.Contains("BasicSalary", result);
        Assert.Contains("HRA", result);
        Assert.Contains("TaxRate", result);
        Assert.Contains("PreviousDeduction", result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task ExtractVariablesFromFormulaAsync_EmptyFormula_ReturnsEmptyList()
    {
        // Arrange
        var formula = "";

        // Act
        var result = await _formulaEngine.ExtractVariablesFromFormulaAsync(formula);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractVariablesFromFormulaAsync_OnlyNumbers_ReturnsEmptyList()
    {
        // Arrange
        var formula = "100 + 200 * 0.5";

        // Act
        var result = await _formulaEngine.ExtractVariablesFromFormulaAsync(formula);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("BasicSalary * 0.4", "BasicSalary", "5000", 2000)]
    [InlineData("BasicSalary / WorkingDays", "BasicSalary,WorkingDays", "10000,20", 500)]
    [InlineData("(BasicSalary + Allowance) * 0.1", "BasicSalary,Allowance", "8000,2000", 1000)]
    public async Task EvaluateFormulaAsync_VariousFormulas_ReturnsExpectedResults(
        string formula, string variableNames, string variableValues, decimal expected)
    {
        // Arrange
        var names = variableNames.Split(',');
        var values = variableValues.Split(',').Select(decimal.Parse).ToArray();
        var variables = names.Zip(values, (name, value) => new { name, value })
            .ToDictionary(x => x.name, x => x.value);

        // Act
        var result = await _formulaEngine.EvaluateFormulaAsync(formula, variables);

        // Assert
        Assert.Equal(expected, result);
    }

    private static FormulaEvaluationContext CreateTestEvaluationContext()
    {
        var organization = new Organization { Id = 1, Name = "Test Org", OvertimeRate = 1.5m };
        var branch = new Branch { Id = 1, Name = "Test Branch", Currency = "USD", OrganizationId = 1, Organization = organization };
        var employee = new Employee 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            BasicSalary = 10000m, 
            BranchId = 1, 
            Branch = branch,
            Department = "IT",
            Designation = "Developer"
        };

        return new FormulaEvaluationContext
        {
            Employee = employee,
            Branch = branch,
            Organization = organization,
            PayrollPeriodStart = new DateTime(2024, 1, 1),
            PayrollPeriodEnd = new DateTime(2024, 1, 31),
            BasicSalary = 10000m,
            OvertimeHours = 20m,
            WorkingDays = 22,
            ActualWorkingDays = 20,
            AbsentDays = 2,
            LeaveDays = 0
        };
    }
}