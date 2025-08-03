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

public class ShiftSwapServiceTests
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

    public ShiftSwapServiceTests()
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
    public async Task CreateShiftSwapRequestAsync_ValidRequest_ReturnsShiftSwapRequestDto()
    {
        // Arrange
        var requesterId = 1;
        var createDto = new CreateShiftSwapRequestDto
        {
            RequesterShiftAssignmentId = 1,
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency",
            IsEmergency = true
        };

        var requesterAssignment = new ShiftAssignment
        {
            Id = 1,
            EmployeeId = requesterId,
            ShiftId = 1,
            StartDate = DateTime.Today.AddDays(1),
            IsActive = true
        };

        var swapRequest = new ShiftSwapRequest
        {
            Id = 1,
            RequesterId = requesterId,
            RequesterShiftAssignmentId = 1,
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency",
            IsEmergency = true,
            Status = ShiftSwapStatus.Pending
        };

        var expectedDto = new ShiftSwapRequestDto
        {
            Id = 1,
            RequesterId = requesterId,
            RequesterShiftAssignmentId = 1,
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency",
            IsEmergency = true,
            Status = ShiftSwapStatus.Pending
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(requesterAssignment);

        _mockMapper.Setup(m => m.Map<ShiftSwapRequest>(createDto))
            .Returns(swapRequest);

        _mockShiftSwapRequestRepository.Setup(r => r.AddAsync(It.IsAny<ShiftSwapRequest>()))
            .ReturnsAsync(swapRequest);

        _mockShiftSwapRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockShiftSwapRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(swapRequest);

        _mockMapper.Setup(m => m.Map<ShiftSwapRequestDto>(swapRequest))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.CreateShiftSwapRequestAsync(requesterId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.RequesterId, result.RequesterId);
        Assert.Equal(expectedDto.Reason, result.Reason);
        Assert.Equal(expectedDto.IsEmergency, result.IsEmergency);
        Assert.Equal(ShiftSwapStatus.Pending, result.Status);

        _mockShiftSwapRequestRepository.Verify(r => r.AddAsync(It.IsAny<ShiftSwapRequest>()), Times.Once);
        _mockShiftSwapRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateShiftSwapRequestAsync_InvalidShiftAssignment_ThrowsArgumentException()
    {
        // Arrange
        var requesterId = 1;
        var createDto = new CreateShiftSwapRequestDto
        {
            RequesterShiftAssignmentId = 999, // Non-existent assignment
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency"
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ShiftAssignment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _shiftService.CreateShiftSwapRequestAsync(requesterId, createDto));

        Assert.Equal("Invalid shift assignment for requester.", exception.Message);
    }

    [Fact]
    public async Task RespondToShiftSwapRequestAsync_AcceptedResponse_UpdatesRequestStatus()
    {
        // Arrange
        var responderId = 2;
        var responseDto = new CreateShiftSwapResponseDto
        {
            ShiftSwapRequestId = 1,
            ResponderShiftAssignmentId = 2,
            IsAccepted = true,
            Notes = "I can cover this shift"
        };

        var swapRequest = new ShiftSwapRequest
        {
            Id = 1,
            RequesterId = 1,
            Status = ShiftSwapStatus.Pending
        };

        var responderAssignment = new ShiftAssignment
        {
            Id = 2,
            EmployeeId = responderId,
            ShiftId = 2,
            IsActive = true
        };

        var swapResponse = new ShiftSwapResponse
        {
            Id = 1,
            ShiftSwapRequestId = 1,
            ResponderId = responderId,
            ResponderShiftAssignmentId = 2,
            IsAccepted = true,
            Notes = "I can cover this shift"
        };

        var expectedDto = new ShiftSwapRequestDto
        {
            Id = 1,
            Status = ShiftSwapStatus.ManagerApprovalRequired
        };

        _mockShiftSwapRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(swapRequest);

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(responderAssignment);

        _mockMapper.Setup(m => m.Map<ShiftSwapResponse>(responseDto))
            .Returns(swapResponse);

        _mockShiftSwapResponseRepository.Setup(r => r.AddAsync(It.IsAny<ShiftSwapResponse>()))
            .ReturnsAsync(swapResponse);

        _mockShiftSwapRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ShiftSwapRequestDto>(It.IsAny<ShiftSwapRequest>()))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.RespondToShiftSwapRequestAsync(responderId, responseDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ShiftSwapStatus.ManagerApprovalRequired, result.Status);

        _mockShiftSwapResponseRepository.Verify(r => r.AddAsync(It.IsAny<ShiftSwapResponse>()), Times.Once);
        _mockShiftSwapRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftSwapRequest>()), Times.Once);
        _mockShiftSwapRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveShiftSwapRequestAsync_ApprovedRequest_CompletesSwap()
    {
        // Arrange
        var approverId = 3;
        var requestId = 1;
        var approvalDto = new ApproveShiftSwapDto
        {
            IsApproved = true,
            Notes = "Approved by manager"
        };

        var swapRequest = new ShiftSwapRequest
        {
            Id = 1,
            RequesterId = 1,
            RequesterShiftAssignmentId = 1,
            TargetEmployeeId = 2,
            TargetShiftAssignmentId = 2,
            Status = ShiftSwapStatus.ManagerApprovalRequired
        };

        var requesterAssignment = new ShiftAssignment
        {
            Id = 1,
            EmployeeId = 1,
            ShiftId = 1
        };

        var targetAssignment = new ShiftAssignment
        {
            Id = 2,
            EmployeeId = 2,
            ShiftId = 2
        };

        var expectedDto = new ShiftSwapRequestDto
        {
            Id = 1,
            Status = ShiftSwapStatus.Approved
        };

        _mockShiftSwapRequestRepository.Setup(r => r.GetWithDetailsAsync(1))
            .ReturnsAsync(swapRequest);

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(requesterAssignment);

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(targetAssignment);

        _mockShiftSwapRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ShiftSwapRequestDto>(It.IsAny<ShiftSwapRequest>()))
            .Returns(expectedDto);

        // Act
        var result = await _shiftService.ApproveShiftSwapRequestAsync(requestId, approverId, approvalDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ShiftSwapStatus.Approved, result.Status);

        _mockShiftAssignmentRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftAssignment>()), Times.Exactly(2));
        _mockShiftSwapRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<ShiftSwapRequest>()), Times.Once);
        _mockShiftSwapRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelShiftSwapRequestAsync_ValidRequest_CancelsRequest()
    {
        // Arrange
        var userId = 1;
        var requestId = 1;

        var swapRequest = new ShiftSwapRequest
        {
            Id = 1,
            RequesterId = userId,
            Status = ShiftSwapStatus.Pending
        };

        _mockShiftSwapRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(swapRequest);

        _mockShiftSwapRequestRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _shiftService.CancelShiftSwapRequestAsync(requestId, userId);

        // Assert
        Assert.True(result);
        Assert.Equal(ShiftSwapStatus.Cancelled, swapRequest.Status);

        _mockShiftSwapRequestRepository.Verify(r => r.UpdateAsync(swapRequest), Times.Once);
        _mockShiftSwapRequestRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelShiftSwapRequestAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = 2; // Different from requester
        var requestId = 1;

        var swapRequest = new ShiftSwapRequest
        {
            Id = 1,
            RequesterId = 1, // Different user
            Status = ShiftSwapStatus.Pending
        };

        _mockShiftSwapRequestRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(swapRequest);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _shiftService.CancelShiftSwapRequestAsync(requestId, userId));

        Assert.Equal("Only the requester can cancel the swap request.", exception.Message);
    }

    [Fact]
    public async Task GetShiftSwapRequestsAsync_ValidEmployeeId_ReturnsRequests()
    {
        // Arrange
        var employeeId = 1;
        var swapRequests = new List<ShiftSwapRequest>
        {
            new ShiftSwapRequest { Id = 1, RequesterId = employeeId },
            new ShiftSwapRequest { Id = 2, RequesterId = employeeId }
        };

        var expectedDtos = new List<ShiftSwapRequestDto>
        {
            new ShiftSwapRequestDto { Id = 1, RequesterId = employeeId },
            new ShiftSwapRequestDto { Id = 2, RequesterId = employeeId }
        };

        _mockShiftSwapRequestRepository.Setup(r => r.GetByRequesterIdAsync(employeeId))
            .ReturnsAsync(swapRequests);

        _mockShiftSwapRequestRepository.Setup(r => r.GetByTargetEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<ShiftSwapRequest>());

        _mockMapper.Setup(m => m.Map<IEnumerable<ShiftSwapRequestDto>>(It.IsAny<IEnumerable<ShiftSwapRequest>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _shiftService.GetShiftSwapRequestsAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, dto => Assert.Equal(employeeId, dto.RequesterId));
    }
}