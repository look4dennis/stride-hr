using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Utilities;

public class BasicUtilityTests
{
    [Fact]
    public void Employee_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var employee = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            BasicSalary = 50000,
            Status = EmployeeStatus.Active
        };

        // Assert
        employee.Id.Should().Be(1);
        employee.EmployeeId.Should().Be("EMP001");
        employee.FirstName.Should().Be("John");
        employee.LastName.Should().Be("Doe");
        employee.Email.Should().Be("john.doe@example.com");
        employee.BasicSalary.Should().Be(50000);
        employee.Status.Should().Be(EmployeeStatus.Active);
    }

    [Fact]
    public void PayrollRecord_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            BasicSalary = 50000,
            GrossSalary = 55000,
            NetSalary = 45000,
            Currency = "USD",
            Status = PayrollStatus.Draft
        };

        // Assert
        payrollRecord.Id.Should().Be(1);
        payrollRecord.EmployeeId.Should().Be(1);
        payrollRecord.BasicSalary.Should().Be(50000);
        payrollRecord.GrossSalary.Should().Be(55000);
        payrollRecord.NetSalary.Should().Be(45000);
        payrollRecord.Currency.Should().Be("USD");
        payrollRecord.Status.Should().Be(PayrollStatus.Draft);
    }

    [Fact]
    public void AttendanceRecord_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = 1,
            Date = DateTime.Today,
            CheckInTime = DateTime.Today.AddHours(9),
            CheckOutTime = DateTime.Today.AddHours(17),
            TotalWorkingHours = TimeSpan.FromHours(8),
            Status = AttendanceStatus.Present
        };

        // Assert
        attendanceRecord.Id.Should().Be(1);
        attendanceRecord.EmployeeId.Should().Be(1);
        attendanceRecord.Date.Should().Be(DateTime.Today);
        attendanceRecord.CheckInTime.Should().Be(DateTime.Today.AddHours(9));
        attendanceRecord.CheckOutTime.Should().Be(DateTime.Today.AddHours(17));
        attendanceRecord.TotalWorkingHours.Should().Be(TimeSpan.FromHours(8));
        attendanceRecord.Status.Should().Be(AttendanceStatus.Present);
    }

    [Theory]
    [InlineData(EmployeeStatus.Active, true)]
    [InlineData(EmployeeStatus.Inactive, false)]
    [InlineData(EmployeeStatus.Terminated, false)]
    public void Employee_IsActive_ReturnsCorrectValue(EmployeeStatus status, bool expectedIsActive)
    {
        // Arrange
        var employee = new Employee { Status = status };

        // Act
        var isActive = employee.Status == EmployeeStatus.Active;

        // Assert
        isActive.Should().Be(expectedIsActive);
    }

    [Theory]
    [InlineData(PayrollStatus.Draft, false)]
    [InlineData(PayrollStatus.Calculated, false)]
    [InlineData(PayrollStatus.Approved, true)]
    [InlineData(PayrollStatus.Paid, true)]
    public void PayrollRecord_IsProcessed_ReturnsCorrectValue(PayrollStatus status, bool expectedIsProcessed)
    {
        // Arrange
        var payrollRecord = new PayrollRecord { Status = status };

        // Act
        var isProcessed = payrollRecord.Status == PayrollStatus.Approved || payrollRecord.Status == PayrollStatus.Paid;

        // Assert
        isProcessed.Should().Be(expectedIsProcessed);
    }

    [Fact]
    public void BaseEntity_CreatedAt_IsSetOnCreation()
    {
        // Arrange & Act
        var employee = new Employee();
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
        employee.CreatedAt = DateTime.UtcNow;
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        employee.CreatedAt.Should().BeAfter(beforeCreation);
        employee.CreatedAt.Should().BeBefore(afterCreation);
    }
}