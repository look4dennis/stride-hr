using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ShiftCoverageServiceTests
{
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly Mock<IShiftAssignmentRepository> _mockShiftAssignmentRepository;
    private readonly Mock<IShiftSwapRequestRepository> _mockShiftSwapRequestRepository;
    private readonly Mock<IShiftSwapResponseRepository> _mockShiftSwapResponseRepository;
    private readonly Mock<IShiftCoverageRequestRepository> _mockShiftCoverageRequestRepository;
    private readonly Mock<IShiftCoverageResponseRepository> _mockShiftCoverageResponseRepository;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ShiftService>> _mockLogger;
    private readonly ShiftService _shiftService;

    public ShiftCoverageServiceTests()
    {
        _mockShiftRepository = new Mock<IShiftRepository>();
        _mockShiftAssignmentRepository = new Mock<IShiftAssignmentRepository>();
        _mockShiftSwapRequestRepository = new Mock<IShiftSwapRequestRepository>();
        _mockShiftSwapResponseRepository = new Mock<IShiftSwapResponseRepository>();
        _mockShiftCoverageRequestRepository = new Mock<IShiftCoverageRequestRepository>();
        _mockShiftCoverageResponseRepository = new Mock<IShiftCoverageResponseRepository>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ShiftService>>();

        _shiftService = new ShiftService(
            _mockShiftRepository.Object,
            _mockShiftAssignmentRepository.Object,
            _mockShiftSwapRequestRepository.Object,
            _mockShiftSwapResponseRepository.Object,
            _mockShiftCoverageRequestRepository.Object,
            _mockShiftCoverageResponseRepository.Object,
            _mockEmployeeRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateShiftCoverageRequestAsync_ValidRequest_ReturnsShiftCoverageRequestDto()
    {
        // Arrange
        var requesterId = 1;
        var createDto = new CreateShiftCoverageRequestDto
        {
            ShiftAssignmentId = 1,
            ShiftDate = DateTime.Today.AddDays(1),
            Reason = "Medical appointment",
            IsEmergency = false
        };

        var shiftAssignment = new ShiftAssignment
        {
            Id = 1,
            EmployeeId = requesterId,
            ShiftId = 1,
            StartDate = DateTime.Today.AddDays(1),
            IsActive = true
        };

        var coverageRequest = new ShiftCoverageRequest
        {
            Id = 1,
            RequesterId = requesterId,
            ShiftAssignmentId = 1,
            ShiftDate = DateTime.Today.AddDays(1),
            Reason = "Medical appointment",
            IsEmergency = false,
            Status = ShiftCoverageRequestStatus.Open
        };

        var expectedDto = new ShiftCoverageRequestDto
        {
            Id = 1,
            RequesterId = requesterId,
            ShiftAssignmentId = 1,
            ShiftDate = DateTime.Today.AddDays(1),
            Reason = "Medical appointment",
            IsEmergency = false,
            Status = ShiftCoverageRequestStatus.Open
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(shiftAssignment);

        _mockMapper.Setup(m => m.Map<ShiftCoverageRequest>(createDto))
            .Returns(coverageRequest);

        _mockShiftCoverageRequestRepository.Setup(r => r.AddAsync(It.IsAny<ShiftCoverageRequest>()))
            .ReturnsAsync(coverageRequest);

        _mockShiftCoverageRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockShiftCoverageRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(coverageRequest);

        _mockMapper.Setup(m => m.Map<ShiftCoverageRequestDto>(coverageRequest))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.CreateShiftCoverageRequestAsync(requesterId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.RequesterId, result.RequesterId);
        Assert.Equal(expectedDto.Reason, result.Reason);
        Assert.Equal(expectedDto.IsEmergency, result.IsEmergency);
        Assert.Equal(ShiftCoverageRequestStatus.Open, result.Status);

        _mockShiftCoverageRequestRepository.Verify(r => r.AddAsync(It.IsAny<ShiftCoverageRequest>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateShiftCoverageRequestAsync_InvalidShiftAssignment_ThrowsArgumentException()
    {
        // Arrange
        var requesterId = 1;
        var createDto = new CreateShiftCoverageRequestDto
        {
            ShiftAssignmentId = 999, // Non-existent assignment
            ShiftDate = DateTime.Today.AddDays(1),
            Reason = "Medical appointment"
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ShiftAssignment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _shiftService.CreateShiftCoverageRequestAsync(requesterId, createDto));

        Assert.Equal("Invalid shift assignment for requester.", exception.Message);
    }

    [Fact]
    public async Task RespondToShiftCoverageRequestAsync_AcceptedResponse_UpdatesRequestStatus()
    {
        // Arrange
        var responderId = 2;
        var responseDto = new CreateShiftCoverageResponseDto
        {
            ShiftCoverageRequestId = 1,
            IsAccepted = true,
            Notes = "I can cover this shift"
        };

        var coverageRequest = new ShiftCoverageRequest
        {
            Id = 1,
            RequesterId = 1,
            Status = ShiftCoverageRequestStatus.Open
        };

        var coverageResponse = new ShiftCoverageResponse
        {
            Id = 1,
            ShiftCoverageRequestId = 1,
            ResponderId = responderId,
            IsAccepted = true,
            Notes = "I can cover this shift"
        };

        var expectedDto = new ShiftCoverageRequestDto
        {
            Id = 1,
            Status = ShiftCoverageRequestStatus.Accepted
        };

        _mockShiftCoverageRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(coverageRequest);

        _mockMapper.Setup(m => m.Map<ShiftCoverageResponse>(responseDto))
            .Returns(coverageResponse);

        _mockShiftCoverageResponseRepository.Setup(r => r.AddAsync(It.IsAny<ShiftCoverageResponse>()))
            .ReturnsAsync(coverageResponse);

        _mockShiftCoverageRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ShiftCoverageRequestDto>(It.IsAny<ShiftCoverageRequest>()))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.RespondToShiftCoverageRequestAsync(responderId, responseDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ShiftCoverageRequestStatus.Accepted, result.Status);

        _mockShiftCoverageResponseRepository.Verify(r => r.AddAsync(It.IsAny<ShiftCoverageResponse>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftCoverageRequest>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveShiftCoverageRequestAsync_ApprovedRequest_CreatesNewAssignment()
    {
        // Arrange
        var approverId = 3;
        var requestId = 1;
        var approvalDto = new ApproveShiftCoverageDto
        {
            IsApproved = true,
            Notes = "Approved by manager"
        };

        var coverageRequest = new ShiftCoverageRequest
        {
            Id = 1,
            RequesterId = 1,
            ShiftAssignmentId = 1,
            AcceptedBy = 2,
            Status = ShiftCoverageRequestStatus.Accepted
        };

        var originalAssignment = new ShiftAssignment
        {
            Id = 1,
            EmployeeId = 1,
            ShiftId = 1,
            Employee = new Employee { FirstName = "John", LastName = "Doe" }
        };

        var expectedDto = new ShiftCoverageRequestDto
        {
            Id = 1,
            ApprovedBy = approverId
        };

        _mockShiftCoverageRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(coverageRequest);

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(originalAssignment);

        _mockShiftCoverageRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ShiftCoverageRequestDto>(It.IsAny<ShiftCoverageRequest>()))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.ApproveShiftCoverageRequestAsync(requestId, approverId, approvalDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(approverId, result.ApprovedBy);

        _mockShiftAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<ShiftAssignment>()), Times.Once);
        _mockShiftAssignmentRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftAssignment>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftCoverageRequest>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task BroadcastEmergencyShiftCoverageAsync_ValidRequest_CreatesMultipleRequests()
    {
        // Arrange
        var broadcasterId = 1;
        var broadcastDto = new EmergencyShiftCoverageBroadcastDto
        {
            BranchId = 1,
            ShiftId = 1,
            ShiftDate = DateTime.Today.AddDays(1),
            Reason = "Employee called in sick",
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        var eligibleEmployees = new List<Employee>
        {
            new Employee { Id = 2, Status = EmployeeStatus.Active },
            new Employee { Id = 3, Status = EmployeeStatus.Active }
        };

        var createdRequests = new List<ShiftCoverageRequest>
        {
            new ShiftCoverageRequest { Id = 1, RequesterId = broadcasterId },
            new ShiftCoverageRequest { Id = 2, RequesterId = broadcasterId }
        };

        var expectedDtos = new List<ShiftCoverageRequestDto>
        {
            new ShiftCoverageRequestDto { Id = 1, RequesterId = broadcasterId },
            new ShiftCoverageRequestDto { Id = 2, RequesterId = broadcasterId }
        };

        _mockEmployeeRepository.Setup(r => r.GetByBranchIdAsync(1))
            .ReturnsAsync(eligibleEmployees);

        _mockShiftAssignmentRepository.Setup(r => r.HasConflictingAssignmentAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        _mockShiftAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<ShiftAssignment>()))
            .ReturnsAsync(new ShiftAssignment { Id = 1 });

        _mockShiftAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockShiftCoverageRequestRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ShiftCoverageRequest>>()))
            .ReturnsAsync(createdRequests);

        _mockShiftCoverageRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockShiftCoverageRequestRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftCoverageRequest, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftCoverageRequest, object>>[]>()))
            .ReturnsAsync(createdRequests);

        _mockMapper.Setup(m => m.Map<List<ShiftCoverageRequestDto>>(It.IsAny<IEnumerable<ShiftCoverageRequest>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _shiftService.BroadcastEmergencyShiftCoverageAsync(broadcasterId, broadcastDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(broadcasterId, dto.RequesterId));

        _mockShiftCoverageRequestRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ShiftCoverageRequest>>()), Times.Once);
        _mockShiftCoverageRequestRepository.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetEmergencyShiftCoverageRequestsAsync_ValidBranchId_ReturnsEmergencyRequests()
    {
        // Arrange
        var branchId = 1;
        var emergencyRequests = new List<ShiftCoverageRequest>
        {
            new ShiftCoverageRequest { Id = 1, IsEmergency = true },
            new ShiftCoverageRequest { Id = 2, IsEmergency = true }
        };

        var expectedDtos = new List<ShiftCoverageRequestDto>
        {
            new ShiftCoverageRequestDto { Id = 1, IsEmergency = true },
            new ShiftCoverageRequestDto { Id = 2, IsEmergency = true }
        };

        _mockShiftCoverageRequestRepository.Setup(r => r.GetEmergencyRequestsAsync(branchId))
            .ReturnsAsync(emergencyRequests);

        _mockMapper.Setup(m => m.Map<IEnumerable<ShiftCoverageRequestDto>>(emergencyRequests))
            .Returns(expectedDtos);

        // Act
        var result = await _shiftService.GetEmergencyShiftCoverageRequestsAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, dto => Assert.True(dto.IsEmergency));
    }
}