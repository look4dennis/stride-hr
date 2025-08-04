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
using FluentAssertions;

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
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CalculatePayrollAsync_ValidEmployee_ReturnsPayrollCalculationResult()
    {
        // Arrange
        var request = new PayrollCalculationRequest
        {
            EmployeeId = 1,
            PayrollPeriodStart = DateTime.Today.AddDays(-30),
            PayrollPeriodEnd = DateTime.Today,
            PayrollMonth = DateTime.Today.Month,
            PayrollYear = DateTime.Today.Year
        };
        
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            BasicSalary = 50000,
            BranchId = 1
        };

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(request.EmployeeId))
            .ReturnsAsync(employee);

        _mockAttendanceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AttendanceRecord>());

        // Setup employee with branch currency
        employee.Branch = new Branch { Id = employee.BranchId, Currency = "USD" };
        
        _mockCurrencyService
            .Setup(c => c.GetExchangeRateAsync("USD", "USD"))
            .ReturnsAsync(1.0m);

        // Act
        var result = await _payrollService.CalculatePayrollAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PayrollCalculationResult>();
        result.EmployeeId.Should().Be(request.EmployeeId);
        result.BasicSalary.Should().Be(employee.BasicSalary);
    }

    [Fact]
    public async Task ProcessBranchPayrollAsync_ValidBranch_ProcessesAllEmployees()
    {
        // Arrange
        var branchId = 1;
        var year = 2025;
        var month = 1;

        var employees = new List<Employee>
        {
            new Employee { Id = 1, FirstName = "John", LastName = "Doe", BasicSalary = 50000, BranchId = branchId, Status = EmployeeStatus.Active },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith", BasicSalary = 60000, BranchId = branchId, Status = EmployeeStatus.Active }
        };

        _mockEmployeeRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(employees);

        _mockAttendanceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<AttendanceRecord>());

        // Setup employees with branch currency
        foreach (var emp in employees)
        {
            emp.Branch = new Branch { Id = branchId, Currency = "USD" };
        }
        
        _mockCurrencyService
            .Setup(c => c.GetExchangeRateAsync("USD", "USD"))
            .ReturnsAsync(1.0m);

        _mockPayrollRepository
            .Setup(r => r.GetByEmployeeAndPeriodAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((PayrollRecord?)null);

        // Act
        var result = await _payrollService.ProcessBranchPayrollAsync(branchId, year, month);

        // Assert
        result.Should().HaveCount(2);
        result.All(pr => pr.EmployeeId > 0).Should().BeTrue();
        result.All(pr => pr.PayrollYear == year).Should().BeTrue();
        result.All(pr => pr.PayrollMonth == month).Should().BeTrue();
    }

    [Fact]
    public async Task ApprovePayrollRecordAsync_ValidPayroll_UpdatesStatus()
    {
        // Arrange
        var payrollRecordId = 1;
        var approverId = 2;

        var payrollRecord = new PayrollRecord
        {
            Id = payrollRecordId,
            EmployeeId = 1,
            Status = PayrollStatus.Draft,
            GrossSalary = 55000m,
            NetSalary = 50000m
        };

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollRecordId))
            .ReturnsAsync(payrollRecord);

        _mockPayrollRepository
            .Setup(r => r.UpdateAsync(It.IsAny<PayrollRecord>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _payrollService.ApprovePayrollRecordAsync(payrollRecordId, approverId);

        // Assert
        result.Should().BeTrue();
        payrollRecord.ApprovedBy.Should().Be(approverId);
        payrollRecord.ApprovedAt.Should().NotBeNull();
        _mockPayrollRepository.Verify(r => r.UpdateAsync(It.IsAny<PayrollRecord>()), Times.Once);
    }

    [Fact]
    public async Task ApprovePayrollRecordAsync_InvalidPayroll_ReturnsFalse()
    {
        // Arrange
        var payrollRecordId = 999;
        var approverId = 2;

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollRecordId))
            .ReturnsAsync((PayrollRecord?)null);

        // Act
        var result = await _payrollService.ApprovePayrollRecordAsync(payrollRecordId, approverId);

        // Assert
        result.Should().BeFalse();
        _mockPayrollRepository.Verify(r => r.UpdateAsync(It.IsAny<PayrollRecord>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeePayrollRecordsAsync_ValidEmployee_ReturnsRecords()
    {
        // Arrange
        var employeeId = 1;
        var year = 2025;

        var payrollRecords = new List<PayrollRecord>
        {
            new PayrollRecord
            {
                Id = 1,
                EmployeeId = employeeId,
                PayrollYear = year,
                PayrollMonth = 1,
                GrossSalary = 55000m,
                NetSalary = 50000m,
                Status = PayrollStatus.Paid
            },
            new PayrollRecord
            {
                Id = 2,
                EmployeeId = employeeId,
                PayrollYear = year,
                PayrollMonth = 2,
                GrossSalary = 56000m,
                NetSalary = 51000m,
                Status = PayrollStatus.Paid
            }
        };

        _mockPayrollRepository
            .Setup(r => r.GetByEmployeeAsync(employeeId, year, null))
            .ReturnsAsync(payrollRecords);

        // Act
        var result = await _payrollService.GetEmployeePayrollRecordsAsync(employeeId, year);

        // Assert
        result.Should().HaveCount(2);
        result.All(pr => pr.EmployeeId == employeeId).Should().BeTrue();
        result.All(pr => pr.PayrollYear == year).Should().BeTrue();
    }

    [Fact]
    public async Task GetOvertimeHoursAsync_ValidEmployee_ReturnsOvertimeHours()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        var attendanceRecords = new List<AttendanceRecord>
        {
            new AttendanceRecord
            {
                EmployeeId = employeeId,
                Date = DateTime.Today.AddDays(-1),
                OvertimeHours = TimeSpan.FromHours(2)
            },
            new AttendanceRecord
            {
                EmployeeId = employeeId,
                Date = DateTime.Today.AddDays(-2),
                OvertimeHours = TimeSpan.FromHours(1.5)
            }
        };

        _mockAttendanceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendanceRecords);

        // Act
        var result = await _payrollService.GetOvertimeHoursAsync(employeeId, startDate, endDate);

        // Assert
        result.Should().Be(3.5m); // 2 + 1.5 hours
    }

    [Fact]
    public async Task GetWorkingDaysInfoAsync_ValidEmployee_ReturnsWorkingDaysInfo()
    {
        // Arrange
        var employeeId = 1;
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        var attendanceRecords = new List<AttendanceRecord>
        {
            new AttendanceRecord { EmployeeId = employeeId, Date = new DateTime(2025, 1, 1), Status = AttendanceStatus.Present },
            new AttendanceRecord { EmployeeId = employeeId, Date = new DateTime(2025, 1, 2), Status = AttendanceStatus.Present },
            new AttendanceRecord { EmployeeId = employeeId, Date = new DateTime(2025, 1, 3), Status = AttendanceStatus.Absent },
            new AttendanceRecord { EmployeeId = employeeId, Date = new DateTime(2025, 1, 4), Status = AttendanceStatus.OnLeave }
        };

        _mockAttendanceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(attendanceRecords);

        // Act
        var result = await _payrollService.GetWorkingDaysInfoAsync(employeeId, startDate, endDate);

        // Assert
        result.workingDays.Should().BeGreaterThan(0);
        result.actualWorkingDays.Should().Be(2); // 2 present days
        result.absentDays.Should().Be(1); // 1 absent day
        result.leaveDays.Should().Be(1); // 1 leave day
    }
}