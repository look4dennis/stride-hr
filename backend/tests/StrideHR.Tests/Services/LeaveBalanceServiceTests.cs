using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class LeaveBalanceServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<LeaveManagementService>> _mockLogger;
    private readonly Mock<ILeaveBalanceRepository> _mockLeaveBalanceRepository;
    private readonly LeaveManagementService _service;

    public LeaveBalanceServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<LeaveManagementService>>();
        _mockLeaveBalanceRepository = new Mock<ILeaveBalanceRepository>();

        _mockUnitOfWork.Setup(u => u.LeaveBalances).Returns(_mockLeaveBalanceRepository.Object);

        _service = new LeaveManagementService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetEmployeeLeaveBalancesAsync_ValidEmployeeId_ReturnsBalances()
    {
        // Arrange
        var employeeId = 1;
        var leaveBalances = new List<LeaveBalance>
        {
            new LeaveBalance
            {
                Id = 1,
                EmployeeId = employeeId,
                LeavePolicyId = 1,
                Year = 2024,
                AllocatedDays = 20,
                UsedDays = 5,
                CarriedForwardDays = 2,
                EncashedDays = 0,
                LeavePolicy = new LeavePolicy
                {
                    Id = 1,
                    LeaveType = LeaveType.Annual,
                    Name = "Annual Leave"
                }
            }
        };

        _mockLeaveBalanceRepository
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(leaveBalances);

        // Act
        var result = await _service.GetEmployeeLeaveBalancesAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var balance = result.First();
        Assert.Equal(employeeId, balance.EmployeeId);
        Assert.Equal(20, balance.AllocatedDays);
        Assert.Equal(5, balance.UsedDays);
        Assert.Equal(17, balance.RemainingDays); // 20 + 2 - 5 - 0
    }

    [Fact]
    public async Task ValidateLeaveBalanceAsync_SufficientBalance_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var leavePolicyId = 1;
        var requestedDays = 5m;
        var year = 2024;
        var remainingBalance = 10m;

        _mockLeaveBalanceRepository
            .Setup(r => r.GetRemainingBalanceAsync(employeeId, leavePolicyId, year))
            .ReturnsAsync(remainingBalance);

        // Act
        var result = await _service.ValidateLeaveBalanceAsync(employeeId, leavePolicyId, requestedDays, year);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateLeaveBalanceAsync_InsufficientBalance_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;
        var leavePolicyId = 1;
        var requestedDays = 15m;
        var year = 2024;
        var remainingBalance = 10m;

        _mockLeaveBalanceRepository
            .Setup(r => r.GetRemainingBalanceAsync(employeeId, leavePolicyId, year))
            .ReturnsAsync(remainingBalance);

        // Act
        var result = await _service.ValidateLeaveBalanceAsync(employeeId, leavePolicyId, requestedDays, year);

        // Assert
        Assert.False(result);
    }
}