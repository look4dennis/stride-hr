using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayrollServiceTests
{
    private readonly Mock<IPayrollRepository> _mockPayrollRepository;
    private readonly Mock<IPayrollFormulaRepository> _mockFormulaRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<AttendanceRecord>> _mockAttendanceRepository;
    private readonly Mock<IPayrollFormulaEngine> _mockFormulaEngine;
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly Mock<ILogger<PayrollService>> _mockLogger;
    private readonly PayrollService _payrollService;

    public PayrollServiceTests()
    {
        _mockPayrollRepository = new Mock<IPayrollRepository>();
        _mockFormulaRepository = new Mock<IPayrollFormulaRepository>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockAttendanceRepository = new Mock<IRepository<AttendanceRecord>>();
        _mockFormulaEngine = new Mock<IPayrollFormulaEngine>();
        _mockCurrencyService = new Mock<ICurrencyService>();
        _mockLogger = new Mock<ILogger<PayrollService>>();

        _payrollService = new PayrollService(
            _mockPayrollRepository.Object,
            _mockFormulaRepository.Object,
            _mockEmployeeRepository.Object,
            _mockAttendanceRepository.Object,
            _mockFormulaEngine.Object,
            _mockCurrencyService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CalculatePayrollAsync_ValidEmployee_ReturnsCorrectCalculation()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var request = new PayrollCalculationRequest
        {
            EmployeeId = 1,
            PayrollPeriodStart = new DateTime(2024, 1, 1),
            PayrollPeriodEnd = new DateTime(2024, 1, 31),
            PayrollMonth = 1,
            PayrollYear = 2024,
            IncludeCustomFormulas = true
        };

        var attendanceRecords = new List<AttendanceRecord>
        {
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 1), Status = AttendanceStatus.Present, OvertimeHours = TimeSpan.FromHours(2) },
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 2), Status = AttendanceStatus.Present, OvertimeHours = TimeSpan.FromHours(1) }
        };

        var formulas = new List<PayrollFormula>
        {
            new() { Name = "HRA", Formula = "BasicSalary * 0.4", Type = PayrollFormulaType.Allowance, IsActive = true },
            new() { Name = "PF", Formula = "BasicSalary * 0.12", Type = PayrollFormulaType.Deduction, IsActive = true }
        };

        var formulaResults = new Dictionary<string, decimal>
        {
            { "HRA", 4000m },
            { "PF", 1200m }
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employee);

        _mockAttendanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendanceRecords);

        _mockFormulaRepository.Setup(r => r.GetFormulasForEmployeeAsync(1))
            .ReturnsAsync(formulas);

        _mockFormulaEngine.Setup(e => e.CalculateOvertimeAmountAsync(3m, 10000m, 1.5m))
            .ReturnsAsync(187.5m);

        _mockFormulaEngine.Setup(e => e.EvaluateAllFormulasAsync(It.IsAny<FormulaEvaluationContext>(), formulas))
            .ReturnsAsync(formulaResults);

        _mockCurrencyService.Setup(s => s.GetExchangeRateAsync("USD", "USD"))
            .ReturnsAsync(1.0m);

        // Act
        var result = await _payrollService.CalculatePayrollAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal("John Doe", result.EmployeeName);
        Assert.Equal(10000m, result.BasicSalary);
        Assert.Equal(4000m, result.TotalAllowances);
        Assert.Equal(1200m, result.TotalDeductions);
        Assert.Equal(187.5m, result.OvertimeAmount);
        Assert.Equal(14187.5m, result.GrossSalary); // 10000 + 4000 + 187.5
        Assert.Equal(12987.5m, result.NetSalary); // 14187.5 - 1200
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task CalculatePayrollAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new PayrollCalculationRequest
        {
            EmployeeId = 999,
            PayrollPeriodStart = new DateTime(2024, 1, 1),
            PayrollPeriodEnd = new DateTime(2024, 1, 31),
            PayrollMonth = 1,
            PayrollYear = 2024
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _payrollService.CalculatePayrollAsync(request));
    }

    [Fact]
    public async Task CreatePayrollRecordAsync_ValidRequest_CreatesRecord()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var request = new PayrollCalculationRequest
        {
            EmployeeId = 1,
            PayrollPeriodStart = new DateTime(2024, 1, 1),
            PayrollPeriodEnd = new DateTime(2024, 1, 31),
            PayrollMonth = 1,
            PayrollYear = 2024
        };

        _mockPayrollRepository.Setup(r => r.GetByEmployeeAndPeriodAsync(1, 2024, 1))
            .ReturnsAsync((PayrollRecord?)null);

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employee);

        _mockAttendanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AttendanceRecord>());

        _mockFormulaRepository.Setup(r => r.GetFormulasForEmployeeAsync(1))
            .ReturnsAsync(new List<PayrollFormula>());

        _mockFormulaEngine.Setup(e => e.CalculateOvertimeAmountAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ReturnsAsync(0m);

        _mockFormulaEngine.Setup(e => e.EvaluateAllFormulasAsync(It.IsAny<FormulaEvaluationContext>(), It.IsAny<List<PayrollFormula>>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        _mockCurrencyService.Setup(s => s.GetExchangeRateAsync("USD", "USD"))
            .ReturnsAsync(1.0m);

        _mockPayrollRepository.Setup(r => r.AddAsync(It.IsAny<PayrollRecord>()))
            .ReturnsAsync((PayrollRecord r) => r);

        _mockPayrollRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _payrollService.CreatePayrollRecordAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal(1, result.PayrollMonth);
        Assert.Equal(2024, result.PayrollYear);
        Assert.Equal(PayrollStatus.Calculated, result.Status);

        _mockPayrollRepository.Verify(r => r.AddAsync(It.IsAny<PayrollRecord>()), Times.Once);
        _mockPayrollRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePayrollRecordAsync_PayrollAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingPayroll = new PayrollRecord { Id = 1, EmployeeId = 1, PayrollMonth = 1, PayrollYear = 2024 };
        var request = new PayrollCalculationRequest
        {
            EmployeeId = 1,
            PayrollPeriodStart = new DateTime(2024, 1, 1),
            PayrollPeriodEnd = new DateTime(2024, 1, 31),
            PayrollMonth = 1,
            PayrollYear = 2024
        };

        _mockPayrollRepository.Setup(r => r.GetByEmployeeAndPeriodAsync(1, 2024, 1))
            .ReturnsAsync(existingPayroll);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _payrollService.CreatePayrollRecordAsync(request));
    }

    [Fact]
    public async Task ApprovePayrollRecordAsync_ValidRecord_ApprovesSuccessfully()
    {
        // Arrange
        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            Status = PayrollStatus.Calculated
        };

        _mockPayrollRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockPayrollRepository.Setup(r => r.UpdateAsync(It.IsAny<PayrollRecord>()))
            .Returns(Task.CompletedTask);

        _mockPayrollRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _payrollService.ApprovePayrollRecordAsync(1, 100);

        // Assert
        Assert.True(result);
        Assert.Equal(PayrollStatus.Approved, payrollRecord.Status);
        Assert.Equal(100, payrollRecord.ApprovedBy);
        Assert.NotNull(payrollRecord.ApprovedAt);

        _mockPayrollRepository.Verify(r => r.UpdateAsync(payrollRecord), Times.Once);
        _mockPayrollRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApprovePayrollRecordAsync_RecordNotFound_ReturnsFalse()
    {
        // Arrange
        _mockPayrollRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((PayrollRecord?)null);

        // Act
        var result = await _payrollService.ApprovePayrollRecordAsync(999, 100);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetOvertimeHoursAsync_WithAttendanceRecords_ReturnsCorrectTotal()
    {
        // Arrange
        var attendanceRecords = new List<AttendanceRecord>
        {
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 1), OvertimeHours = TimeSpan.FromHours(2) },
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 2), OvertimeHours = TimeSpan.FromHours(1.5) },
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 3), OvertimeHours = null },
            new() { EmployeeId = 2, Date = new DateTime(2024, 1, 1), OvertimeHours = TimeSpan.FromHours(3) } // Different employee
        };

        _mockAttendanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendanceRecords);

        // Act
        var result = await _payrollService.GetOvertimeHoursAsync(1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        // Assert
        Assert.Equal(3.5m, result); // 2 + 1.5 + 0 = 3.5 hours
    }

    [Fact]
    public async Task GetWorkingDaysInfoAsync_WithAttendanceRecords_ReturnsCorrectInfo()
    {
        // Arrange
        var attendanceRecords = new List<AttendanceRecord>
        {
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 1), Status = AttendanceStatus.Present }, // Monday
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 2), Status = AttendanceStatus.Present }, // Tuesday
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 3), Status = AttendanceStatus.Absent }, // Wednesday
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 4), Status = AttendanceStatus.OnLeave }, // Thursday
            new() { EmployeeId = 1, Date = new DateTime(2024, 1, 5), Status = AttendanceStatus.Present } // Friday
        };

        _mockAttendanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendanceRecords);

        // Act
        var result = await _payrollService.GetWorkingDaysInfoAsync(1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 5));

        // Assert
        Assert.Equal(5, result.workingDays); // 5 weekdays
        Assert.Equal(3, result.actualWorkingDays); // 3 present days
        Assert.Equal(1, result.absentDays); // 1 absent day
        Assert.Equal(1, result.leaveDays); // 1 leave day
    }

    [Fact]
    public async Task ProcessBranchPayrollAsync_MultipleEmployees_ProcessesAll()
    {
        // Arrange
        var employees = new List<Employee>
        {
            CreateTestEmployee(1, "John", "Doe"),
            CreateTestEmployee(2, "Jane", "Smith")
        };

        _mockEmployeeRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(employees);

        // Setup GetByIdAsync for individual employee lookups in CalculatePayrollAsync
        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employees[0]);
        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(employees[1]);

        // Setup for both CalculatePayrollAsync calls and CreatePayrollRecordAsync calls
        _mockPayrollRepository.Setup(r => r.GetByEmployeeAndPeriodAsync(It.IsAny<int>(), 2024, 1))
            .ReturnsAsync((PayrollRecord?)null);

        _mockAttendanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AttendanceRecord>());

        _mockFormulaRepository.Setup(r => r.GetFormulasForEmployeeAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PayrollFormula>());

        _mockFormulaEngine.Setup(e => e.CalculateOvertimeAmountAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .ReturnsAsync(0m);

        _mockFormulaEngine.Setup(e => e.EvaluateAllFormulasAsync(It.IsAny<FormulaEvaluationContext>(), It.IsAny<List<PayrollFormula>>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        _mockCurrencyService.Setup(s => s.GetExchangeRateAsync("USD", "USD"))
            .ReturnsAsync(1.0m);

        _mockPayrollRepository.Setup(r => r.AddAsync(It.IsAny<PayrollRecord>()))
            .ReturnsAsync((PayrollRecord r) => r);

        _mockPayrollRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _payrollService.ProcessBranchPayrollAsync(1, 2024, 1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.EmployeeId == 1);
        Assert.Contains(result, r => r.EmployeeId == 2);

        // The method should process all employees successfully
        Assert.All(result, r => Assert.Empty(r.Errors));
    }

    private static Employee CreateTestEmployee(int id = 1, string firstName = "John", string lastName = "Doe")
    {
        var organization = new Organization { Id = 1, Name = "Test Org", OvertimeRate = 1.5m };
        var branch = new Branch 
        { 
            Id = 1, 
            Name = "Test Branch", 
            Currency = "USD", 
            OrganizationId = 1, 
            Organization = organization 
        };

        return new Employee
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            BasicSalary = 10000m,
            BranchId = 1,
            Branch = branch,
            Status = EmployeeStatus.Active,
            Department = "IT",
            Designation = "Developer"
        };
    }
}