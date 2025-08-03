using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using StrideHR.Core.Enums;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ShiftSwapControllerTests
{
    private readonly Mock<IShiftService> _mockShiftService;
    private readonly Mock<ILogger<ShiftSwapController>> _mockLogger;
    private readonly ShiftSwapController _controller;

    public ShiftSwapControllerTests()
    {
        _mockShiftService = new Mock<IShiftService>();
        _mockLogger = new Mock<ILogger<ShiftSwapController>>();
        _controller = new ShiftSwapController(_mockShiftService.Object, _mockLogger.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task CreateShiftSwapRequest_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var createDto = new CreateShiftSwapRequestDto
        {
            RequesterShiftAssignmentId = 1,
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency"
        };

        var expectedResult = new ShiftSwapRequestDto
        {
            Id = 1,
            RequesterId = 1,
            RequesterShiftAssignmentId = 1,
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency",
            Status = ShiftSwapStatus.Pending
        };

        _mockShiftService.Setup(s => s.CreateShiftSwapRequestAsync(1, createDto))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateShiftSwapRequest(createDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ShiftSwapRequestDto>(okResult.Value);
        Assert.Equal(expectedResult.Id, returnValue.Id);
        Assert.Equal(expectedResult.RequesterId, returnValue.RequesterId);
    }

    [Fact]
    public async Task CreateShiftSwapRequest_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateShiftSwapRequestDto
        {
            RequesterShiftAssignmentId = 999, // Invalid assignment
            RequestedDate = DateTime.Today.AddDays(1),
            Reason = "Personal emergency"
        };

        _mockShiftService.Setup(s => s.CreateShiftSwapRequestAsync(1, createDto))
            .ThrowsAsync(new ArgumentException("Invalid shift assignment for requester."));

        // Act
        var result = await _controller.CreateShiftSwapRequest(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid shift assignment for requester.", badRequestResult.Value);
    }

    [Fact]
    public async Task RespondToShiftSwapRequest_ValidResponse_ReturnsOkResult()
    {
        // Arrange
        var requestId = 1;
        var responseDto = new CreateShiftSwapResponseDto
        {
            ResponderShiftAssignmentId = 2,
            IsAccepted = true,
            Notes = "I can cover this shift"
        };

        var expectedResult = new ShiftSwapRequestDto
        {
            Id = 1,
            Status = ShiftSwapStatus.ManagerApprovalRequired
        };

        _mockShiftService.Setup(s => s.RespondToShiftSwapRequestAsync(1, It.IsAny<CreateShiftSwapResponseDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RespondToShiftSwapRequest(requestId, responseDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ShiftSwapRequestDto>(okResult.Value);
        Assert.Equal(ShiftSwapStatus.ManagerApprovalRequired, returnValue.Status);
    }

    [Fact]
    public async Task ApproveShiftSwapRequest_ValidApproval_ReturnsOkResult()
    {
        // Arrange
        var requestId = 1;
        var approvalDto = new ApproveShiftSwapDto
        {
            IsApproved = true,
            Notes = "Approved by manager"
        };

        var expectedResult = new ShiftSwapRequestDto
        {
            Id = 1,
            Status = ShiftSwapStatus.Approved
        };

        _mockShiftService.Setup(s => s.ApproveShiftSwapRequestAsync(requestId, 1, approvalDto))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ApproveShiftSwapRequest(requestId, approvalDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ShiftSwapRequestDto>(okResult.Value);
        Assert.Equal(ShiftSwapStatus.Approved, returnValue.Status);
    }

    [Fact]
    public async Task CancelShiftSwapRequest_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var requestId = 1;

        _mockShiftService.Setup(s => s.CancelShiftSwapRequestAsync(requestId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelShiftSwapRequest(requestId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CancelShiftSwapRequest_RequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = 999;

        _mockShiftService.Setup(s => s.CancelShiftSwapRequestAsync(requestId, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CancelShiftSwapRequest(requestId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Shift swap request not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetShiftSwapRequests_ValidUser_ReturnsOkResult()
    {
        // Arrange
        var expectedRequests = new List<ShiftSwapRequestDto>
        {
            new ShiftSwapRequestDto { Id = 1, RequesterId = 1 },
            new ShiftSwapRequestDto { Id = 2, RequesterId = 1 }
        };

        _mockShiftService.Setup(s => s.GetShiftSwapRequestsAsync(1))
            .ReturnsAsync(expectedRequests);

        // Act
        var result = await _controller.GetShiftSwapRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ShiftSwapRequestDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public async Task SearchShiftSwapRequests_ValidCriteria_ReturnsOkResult()
    {
        // Arrange
        var criteria = new ShiftSwapSearchCriteria
        {
            RequesterId = 1,
            Status = ShiftSwapStatus.Pending,
            Page = 1,
            PageSize = 10
        };

        var expectedRequests = new List<ShiftSwapRequestDto>
        {
            new ShiftSwapRequestDto { Id = 1, RequesterId = 1, Status = ShiftSwapStatus.Pending }
        };

        _mockShiftService.Setup(s => s.SearchShiftSwapRequestsAsync(criteria))
            .ReturnsAsync((expectedRequests, 1));

        // Act
        var result = await _controller.SearchShiftSwapRequests(criteria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = okResult.Value;
        Assert.NotNull(returnValue);
        
        // Verify the anonymous object structure
        var resultType = returnValue.GetType();
        var requestsProperty = resultType.GetProperty("requests");
        var totalCountProperty = resultType.GetProperty("totalCount");
        
        Assert.NotNull(requestsProperty);
        Assert.NotNull(totalCountProperty);
        
        var requests = requestsProperty.GetValue(returnValue) as IEnumerable<ShiftSwapRequestDto>;
        var totalCount = (int)totalCountProperty.GetValue(returnValue);
        
        Assert.Single(requests);
        Assert.Equal(1, totalCount);
    }
}