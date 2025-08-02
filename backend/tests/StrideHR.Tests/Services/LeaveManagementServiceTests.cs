using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Leave;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class LeaveManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<LeaveManagementService>> _mockLogger;
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRequestRepository;
    private readonly Mock<ILeaveBalanceRepository> _mockLeaveBalanceRepository;
    private readonly Mock<ILeavePolicyRepository> _mockLeavePolicyRepository;
    private readonly Mock<ILeaveApprovalHistoryRepository> _mockLeaveApprovalHistoryRepository;
    private readonly Mock<ILeaveCalendarRepository> _mockLeaveCalendarRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<Branch>> _mockBranchRepository;
    private readonly LeaveManagementService _leaveManagementService;

    public LeaveManagementServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<LeaveManagementService>>();
        _mockLeaveRequestRepository = new Mock<ILeaveRequestRepository>();
        _mockLeaveBalanceRepository = new Mock<ILeaveBalanceRepository>();
        _mockLeavePolicyRepository = new Mock<ILeavePolicyRepository>();
        _mockLeaveApprovalHistoryRepository = new Mock<ILeaveApprovalHistoryRepository>();
        _mockLeaveCalendarRepository = new Mock<ILeaveCalendarRepository>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockBranchRepository = new Mock<IRepository<Branch>>();

        _mockUnitOfWork.Setup(u => u.LeaveRequests).Returns(_mockLeaveRequestRepository.Object);
        _mockUnitOfWork.Setup(u => u.LeaveBalances).Returns(_mockLeaveBalanceRepository.Object);
        _mockUnitOfWork.Setup(u => u.LeavePolicies).Returns(_mockLeavePolicyRepository.Object);
        _mockUnitOfWork.Setup(u => u.LeaveApprovalHistory).Returns(_mockLeaveApprovalHistoryRepository.Object);
        _mockUnitOfWork.Setup(u => u.LeaveCalendar).Returns(_mockLeaveCalendarRepository.Object);
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);
        _mockUnitOfWork.Setup(u => u.Branches).Returns(_mockBranchRepository.Object);

        _leaveManagementService = new LeaveManagementService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    #region CreateLeaveRequestAsync Tests

    [Fact]
    public async Task CreateLeaveRequestAsync_ValidRequest_ReturnsLeaveRequestDto()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            BranchId = 1,
            ReportingManagerId = 2
        };

        var leavePolicy = new LeavePolicy
        {
            Id = 1,
            LeaveType = LeaveType.Annual,
            Name = "Annual Leave",
            MinAdvanceNoticeDays = 1,
            MaxConsecutiveDays = 30,
            AnnualAllocation = 20
        };

        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Reason = "Personal vacation",
            IsEmergency = false
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(createRequest.LeavePolicyId))
            .ReturnsAsync(leavePolicy);
        _mockLeaveRequestRepository.Setup(r => r.HasOverlappingRequestsAsync(employeeId, createRequest.StartDate, createRequest.EndDate, null))
            .ReturnsAsync(false);
        _mockLeaveBalanceRepository.Setup(r => r.GetRemainingBalanceAsync(employeeId, createRequest.LeavePolicyId, DateTime.Now.Year))
            .ReturnsAsync(15);
        _mockLeaveRequestRepository.Setup(r => r.AddAsync(It.IsAny<LeaveRequest>()))
            .ReturnsAsync((LeaveRequest lr) => lr);
        _mockLeaveApprovalHistoryRepository.Setup(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()))
            .ReturnsAsync((LeaveApprovalHistory ah) => ah);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(createRequest.Reason, result.Reason);
        Assert.Equal(LeaveStatus.Pending, result.Status);
        _mockLeaveRequestRepository.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
        _mockLeaveApprovalHistoryRepository.Verify(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Reason = "Personal vacation"
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest));
        Assert.Equal("Employee not found (Parameter 'employeeId')", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_LeavePolicyNotFound_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId, BranchId = 1 };
        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Reason = "Personal vacation"
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(createRequest.LeavePolicyId))
            .ReturnsAsync((LeavePolicy?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest));
        Assert.Equal("Leave policy not found (Parameter 'LeavePolicyId')", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId, BranchId = 1 };
        var leavePolicy = new LeavePolicy { Id = 1, MinAdvanceNoticeDays = 1 };
        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddDays(5),
            Reason = "Personal vacation"
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(createRequest.LeavePolicyId))
            .ReturnsAsync(leavePolicy);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest));
        Assert.Equal("Start date cannot be after end date", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_InsufficientAdvanceNotice_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId, BranchId = 1 };
        var leavePolicy = new LeavePolicy 
        { 
            Id = 1, 
            MinAdvanceNoticeDays = 7,
            MaxConsecutiveDays = 30
        };
        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(5),
            Reason = "Personal vacation",
            IsEmergency = false
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(createRequest.LeavePolicyId))
            .ReturnsAsync(leavePolicy);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest));
        Assert.Equal("Leave request requires at least 7 days advance notice", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_OverlappingRequests_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId, BranchId = 1 };
        var leavePolicy = new LeavePolicy 
        { 
            Id = 1, 
            MinAdvanceNoticeDays = 1,
            MaxConsecutiveDays = 30
        };
        var createRequest = new CreateLeaveRequestDto
        {
            LeavePolicyId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(7),
            Reason = "Personal vacation"
        };

        _mockEmployeeRepository.Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(createRequest.LeavePolicyId))
            .ReturnsAsync(leavePolicy);
        _mockLeaveRequestRepository.Setup(r => r.HasOverlappingRequestsAsync(employeeId, createRequest.StartDate, createRequest.EndDate, null))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.CreateLeaveRequestAsync(employeeId, createRequest));
        Assert.Equal("You already have a leave request for the selected dates", exception.Message);
    }

    #endregion

    #region ApproveLeaveRequestAsync Tests

    [Fact]
    public async Task ApproveLeaveRequestAsync_ValidRequest_ReturnsApprovedLeaveRequest()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            EmployeeId = 1,
            LeavePolicyId = 1,
            Status = LeaveStatus.Pending,
            RequestedDays = 3,
            Employee = new Employee 
            { 
                Id = 1, 
                FirstName = "John", 
                LastName = "Doe", 
                ReportingManagerId = approverId,
                BranchId = 1
            },
            LeavePolicy = new LeavePolicy 
            { 
                Id = 1, 
                LeaveType = LeaveType.Annual, 
                Name = "Annual Leave" 
            },
            ApprovalHistory = new List<LeaveApprovalHistory>()
        };

        var approval = new LeaveApprovalDto
        {
            LeaveRequestId = requestId,
            Action = ApprovalAction.Approved,
            Comments = "Approved by manager"
        };

        _mockLeaveRequestRepository.Setup(r => r.GetWithDetailsAsync(requestId))
            .ReturnsAsync(leaveRequest);
        _mockLeaveApprovalHistoryRepository.Setup(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()))
            .ReturnsAsync((LeaveApprovalHistory ah) => ah);
        _mockLeaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockLeaveApprovalHistoryRepository.Setup(r => r.GetByLeaveRequestIdAsync(requestId))
            .ReturnsAsync(new List<LeaveApprovalHistory>());

        // Act
        var result = await _leaveManagementService.ApproveLeaveRequestAsync(requestId, approval, approverId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Approved, result.Status);
        Assert.Equal(approverId, result.ApprovedBy);
        _mockLeaveApprovalHistoryRepository.Verify(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()), Times.Once);
        _mockLeaveRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_RequestNotFound_ThrowsArgumentException()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var approval = new LeaveApprovalDto
        {
            LeaveRequestId = requestId,
            Action = ApprovalAction.Approved
        };

        _mockLeaveRequestRepository.Setup(r => r.GetWithDetailsAsync(requestId))
            .ReturnsAsync((LeaveRequest?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _leaveManagementService.ApproveLeaveRequestAsync(requestId, approval, approverId));
        Assert.Equal("Leave request not found (Parameter 'requestId')", exception.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_RequestNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            Status = LeaveStatus.Approved,
            Employee = new Employee { ReportingManagerId = approverId }
        };
        var approval = new LeaveApprovalDto
        {
            LeaveRequestId = requestId,
            Action = ApprovalAction.Approved
        };

        _mockLeaveRequestRepository.Setup(r => r.GetWithDetailsAsync(requestId))
            .ReturnsAsync(leaveRequest);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _leaveManagementService.ApproveLeaveRequestAsync(requestId, approval, approverId));
        Assert.Equal("Leave request is not pending approval", exception.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_UnauthorizedApprover_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var requestId = 1;
        var approverId = 3;
        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            Status = LeaveStatus.Pending,
            Employee = new Employee { ReportingManagerId = 2 },
            ApprovalHistory = new List<LeaveApprovalHistory>()
        };
        var approval = new LeaveApprovalDto
        {
            LeaveRequestId = requestId,
            Action = ApprovalAction.Approved
        };

        _mockLeaveRequestRepository.Setup(r => r.GetWithDetailsAsync(requestId))
            .ReturnsAsync(leaveRequest);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _leaveManagementService.ApproveLeaveRequestAsync(requestId, approval, approverId));
        Assert.Equal("You don't have permission to approve this leave request", exception.Message);
    }

    #endregion

    #region RejectLeaveRequestAsync Tests

    [Fact]
    public async Task RejectLeaveRequestAsync_ValidRequest_ReturnsRejectedLeaveRequest()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            EmployeeId = 1,
            Status = LeaveStatus.Pending,
            Employee = new Employee 
            { 
                Id = 1, 
                FirstName = "John", 
                LastName = "Doe", 
                ReportingManagerId = approverId,
                BranchId = 1
            },
            LeavePolicy = new LeavePolicy 
            { 
                Id = 1, 
                LeaveType = LeaveType.Annual, 
                Name = "Annual Leave" 
            },
            ApprovalHistory = new List<LeaveApprovalHistory>()
        };

        var rejection = new LeaveApprovalDto
        {
            LeaveRequestId = requestId,
            Action = ApprovalAction.Rejected,
            Comments = "Insufficient staffing"
        };

        _mockLeaveRequestRepository.Setup(r => r.GetWithDetailsAsync(requestId))
            .ReturnsAsync(leaveRequest);
        _mockLeaveApprovalHistoryRepository.Setup(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()))
            .ReturnsAsync((LeaveApprovalHistory ah) => ah);
        _mockLeaveRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockLeaveApprovalHistoryRepository.Setup(r => r.GetByLeaveRequestIdAsync(requestId))
            .ReturnsAsync(new List<LeaveApprovalHistory>());

        // Act
        var result = await _leaveManagementService.RejectLeaveRequestAsync(requestId, rejection, approverId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Rejected, result.Status);
        Assert.Equal(rejection.Comments, result.RejectionReason);
        _mockLeaveApprovalHistoryRepository.Verify(r => r.AddAsync(It.IsAny<LeaveApprovalHistory>()), Times.Once);
        _mockLeaveRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region ValidateLeaveBalanceAsync Tests

    [Fact]
    public async Task ValidateLeaveBalanceAsync_SufficientBalance_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var leavePolicyId = 1;
        var requestedDays = 5m;
        var year = DateTime.Now.Year;

        _mockLeaveBalanceRepository.Setup(r => r.GetRemainingBalanceAsync(employeeId, leavePolicyId, year))
            .ReturnsAsync(10m);

        // Act
        var result = await _leaveManagementService.ValidateLeaveBalanceAsync(employeeId, leavePolicyId, requestedDays, year);

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
        var year = DateTime.Now.Year;

        _mockLeaveBalanceRepository.Setup(r => r.GetRemainingBalanceAsync(employeeId, leavePolicyId, year))
            .ReturnsAsync(10m);

        // Act
        var result = await _leaveManagementService.ValidateLeaveBalanceAsync(employeeId, leavePolicyId, requestedDays, year);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateLeaveBalanceAsync_NoExistingBalance_CreatesNewBalance()
    {
        // Arrange
        var employeeId = 1;
        var leavePolicyId = 1;
        var requestedDays = 5m;
        var year = DateTime.Now.Year;
        var leavePolicy = new LeavePolicy { Id = leavePolicyId, AnnualAllocation = 20 };

        _mockLeaveBalanceRepository.Setup(r => r.GetRemainingBalanceAsync(employeeId, leavePolicyId, year))
            .ReturnsAsync(0m);
        _mockLeavePolicyRepository.Setup(r => r.GetByIdAsync(leavePolicyId))
            .ReturnsAsync(leavePolicy);

        // Act
        var result = await _leaveManagementService.ValidateLeaveBalanceAsync(employeeId, leavePolicyId, requestedDays, year);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region CalculateLeaveDaysAsync Tests

    [Fact]
    public async Task CalculateLeaveDaysAsync_WeekdaysOnly_ReturnsCorrectCount()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1); // Monday
        var endDate = new DateTime(2024, 1, 5);   // Friday
        var branchId = 1;

        // Act
        var result = await _leaveManagementService.CalculateLeaveDaysAsync(startDate, endDate, branchId);

        // Assert
        Assert.Equal(5m, result); // 5 weekdays
    }

    [Fact]
    public async Task CalculateLeaveDaysAsync_IncludesWeekend_ExcludesWeekendDays()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1); // Monday
        var endDate = new DateTime(2024, 1, 7);   // Sunday
        var branchId = 1;

        // Act
        var result = await _leaveManagementService.CalculateLeaveDaysAsync(startDate, endDate, branchId);

        // Assert
        Assert.Equal(5m, result); // 5 weekdays (excludes Saturday and Sunday)
    }

    #endregion

    #region IsWorkingDayAsync Tests

    [Fact]
    public async Task IsWorkingDayAsync_Weekday_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1); // Monday
        var branchId = 1;

        // Act
        var result = await _leaveManagementService.IsWorkingDayAsync(date, branchId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsWorkingDayAsync_Saturday_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 1, 6); // Saturday
        var branchId = 1;

        // Act
        var result = await _leaveManagementService.IsWorkingDayAsync(date, branchId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsWorkingDayAsync_Sunday_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(2024, 1, 7); // Sunday
        var branchId = 1;

        // Act
        var result = await _leaveManagementService.IsWorkingDayAsync(date, branchId);

        // Assert
        Assert.False(result);
    }

    #endregion
}