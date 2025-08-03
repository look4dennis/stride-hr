using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ExpenseControllerTests
{
    private readonly Mock<IExpenseService> _mockExpenseService;
    private readonly Mock<ILogger<ExpenseController>> _mockLogger;
    private readonly ExpenseController _controller;

    public ExpenseControllerTests()
    {
        _mockExpenseService = new Mock<IExpenseService>();
        _mockLogger = new Mock<ILogger<ExpenseController>>();
        _controller = new ExpenseController(_mockExpenseService.Object, _mockLogger.Object);

        // Setup user claims for authentication
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "1"),
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
    public async Task CreateExpenseClaim_ValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = new CreateExpenseClaimDto
        {
            Title = "Business Travel",
            Description = "Travel to client site",
            ExpenseDate = DateTime.Today,
            ExpenseItems = new List<CreateExpenseItemDto>
            {
                new CreateExpenseItemDto
                {
                    ExpenseCategoryId = 1,
                    Description = "Flight ticket",
                    Amount = 500.00m,
                    ExpenseDate = DateTime.Today
                }
            }
        };

        var expectedResult = new ExpenseClaimDto
        {
            Id = 1,
            ClaimNumber = "EXP-2025-01-0001",
            Title = dto.Title,
            Status = ExpenseClaimStatus.Draft
        };

        _mockExpenseService.Setup(s => s.CreateExpenseClaimAsync(1, dto))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateExpenseClaim(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.CreateExpenseClaimAsync(1, dto), Times.Once);
    }

    [Fact]
    public async Task CreateExpenseClaim_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateExpenseClaimDto
        {
            Title = "Business Travel",
            ExpenseItems = new List<CreateExpenseItemDto>() // Empty items
        };

        _mockExpenseService.Setup(s => s.CreateExpenseClaimAsync(1, dto))
            .ThrowsAsync(new ArgumentException("At least one expense item is required"));

        // Act
        var result = await _controller.CreateExpenseClaim(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetExpenseClaim_ExistingClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        var expectedResult = new ExpenseClaimDto
        {
            Id = claimId,
            ClaimNumber = "EXP-2025-01-0001",
            Title = "Business Travel",
            Status = ExpenseClaimStatus.Draft
        };

        _mockExpenseService.Setup(s => s.GetExpenseClaimByIdAsync(claimId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetExpenseClaim(claimId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetExpenseClaimByIdAsync(claimId), Times.Once);
    }

    [Fact]
    public async Task GetExpenseClaim_NonExistingClaim_ReturnsNotFound()
    {
        // Arrange
        var claimId = 999;
        _mockExpenseService.Setup(s => s.GetExpenseClaimByIdAsync(claimId))
            .ReturnsAsync((ExpenseClaimDto?)null);

        // Act
        var result = await _controller.GetExpenseClaim(claimId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SubmitExpenseClaim_ValidClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        _mockExpenseService.Setup(s => s.SubmitExpenseClaimAsync(claimId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitExpenseClaim(claimId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.SubmitExpenseClaimAsync(claimId, 1), Times.Once);
    }

    [Fact]
    public async Task SubmitExpenseClaim_NonExistingClaim_ReturnsNotFound()
    {
        // Arrange
        var claimId = 999;
        _mockExpenseService.Setup(s => s.SubmitExpenseClaimAsync(claimId, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SubmitExpenseClaim(claimId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SubmitExpenseClaim_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var claimId = 1;
        _mockExpenseService.Setup(s => s.SubmitExpenseClaimAsync(claimId, 1))
            .ThrowsAsync(new UnauthorizedAccessException("You can only submit your own expense claims"));

        // Act
        var result = await _controller.SubmitExpenseClaim(claimId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ApproveExpenseClaim_ValidClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        var approvalDto = new ExpenseApprovalDto
        {
            Action = ApprovalAction.Approved,
            Comments = "Approved for business travel"
        };

        _mockExpenseService.Setup(s => s.ApproveExpenseClaimAsync(claimId, 1, approvalDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApproveExpenseClaim(claimId, approvalDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.ApproveExpenseClaimAsync(claimId, 1, approvalDto), Times.Once);
    }

    [Fact]
    public async Task RejectExpenseClaim_ValidClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        var approvalDto = new ExpenseApprovalDto
        {
            Action = ApprovalAction.Rejected,
            Comments = "Insufficient documentation",
            RejectionReason = "Missing receipts"
        };

        _mockExpenseService.Setup(s => s.RejectExpenseClaimAsync(claimId, 1, approvalDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectExpenseClaim(claimId, approvalDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.RejectExpenseClaimAsync(claimId, 1, approvalDto), Times.Once);
    }

    [Fact]
    public async Task GetMyExpenseClaims_ReturnsSuccessResponse()
    {
        // Arrange
        var expectedClaims = new List<ExpenseClaimDto>
        {
            new ExpenseClaimDto
            {
                Id = 1,
                ClaimNumber = "EXP-2025-01-0001",
                Title = "Business Travel",
                Status = ExpenseClaimStatus.Draft
            },
            new ExpenseClaimDto
            {
                Id = 2,
                ClaimNumber = "EXP-2025-01-0002",
                Title = "Office Supplies",
                Status = ExpenseClaimStatus.Submitted
            }
        };

        _mockExpenseService.Setup(s => s.GetExpenseClaimsByEmployeeAsync(1))
            .ReturnsAsync(expectedClaims);

        // Act
        var result = await _controller.GetMyExpenseClaims();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetExpenseClaimsByEmployeeAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetPendingApprovals_ReturnsSuccessResponse()
    {
        // Arrange
        var expectedClaims = new List<ExpenseClaimDto>
        {
            new ExpenseClaimDto
            {
                Id = 1,
                ClaimNumber = "EXP-2025-01-0001",
                Title = "Business Travel",
                Status = ExpenseClaimStatus.Submitted
            }
        };

        _mockExpenseService.Setup(s => s.GetPendingApprovalsAsync(1))
            .ReturnsAsync(expectedClaims);

        // Act
        var result = await _controller.GetPendingApprovals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetPendingApprovalsAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteExpenseClaim_ValidClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        _mockExpenseService.Setup(s => s.DeleteExpenseClaimAsync(claimId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteExpenseClaim(claimId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.DeleteExpenseClaimAsync(claimId, 1), Times.Once);
    }

    [Fact]
    public async Task DeleteExpenseClaim_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var claimId = 1;
        _mockExpenseService.Setup(s => s.DeleteExpenseClaimAsync(claimId, 1))
            .ThrowsAsync(new UnauthorizedAccessException("You can only delete your own expense claims"));

        // Act
        var result = await _controller.DeleteExpenseClaim(claimId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetExpenseCategories_ReturnsSuccessResponse()
    {
        // Arrange
        var organizationId = 1;
        var expectedCategories = new List<ExpenseCategoryDto>
        {
            new ExpenseCategoryDto
            {
                Id = 1,
                Name = "Travel",
                Code = "TRV",
                IsActive = true
            },
            new ExpenseCategoryDto
            {
                Id = 2,
                Name = "Meals",
                Code = "MEAL",
                IsActive = true
            }
        };

        _mockExpenseService.Setup(s => s.GetExpenseCategoriesAsync(organizationId))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _controller.GetExpenseCategories(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetExpenseCategoriesAsync(organizationId), Times.Once);
    }

    [Fact]
    public async Task CreateExpenseCategory_ValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var organizationId = 1;
        var dto = new CreateExpenseCategoryDto
        {
            Name = "Travel",
            Code = "TRV",
            Description = "Travel expenses"
        };

        var expectedResult = new ExpenseCategoryDto
        {
            Id = 1,
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description
        };

        _mockExpenseService.Setup(s => s.CreateExpenseCategoryAsync(dto, organizationId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.CreateExpenseCategory(dto, organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.CreateExpenseCategoryAsync(dto, organizationId), Times.Once);
    }

    [Fact]
    public async Task MarkAsReimbursed_ValidClaim_ReturnsSuccessResponse()
    {
        // Arrange
        var claimId = 1;
        var dto = new ReimbursementDto
        {
            ReimbursementReference = "REF-001"
        };

        _mockExpenseService.Setup(s => s.MarkAsReimbursedAsync(claimId, dto.ReimbursementReference, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsReimbursed(claimId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.MarkAsReimbursedAsync(claimId, dto.ReimbursementReference, 1), Times.Once);
    }

    [Fact]
    public async Task GetExpenseReport_ValidParameters_ReturnsSuccessResponse()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var employeeId = 1;
        var status = ExpenseClaimStatus.Approved;

        var expectedReport = new List<ExpenseClaimDto>
        {
            new ExpenseClaimDto
            {
                Id = 1,
                ClaimNumber = "EXP-2025-01-0001",
                Title = "Business Travel",
                Status = ExpenseClaimStatus.Approved
            }
        };

        _mockExpenseService.Setup(s => s.GetExpenseReportAsync(startDate, endDate, employeeId, status))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _controller.GetExpenseReport(startDate, endDate, employeeId, status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetExpenseReportAsync(startDate, endDate, employeeId, status), Times.Once);
    }

    [Fact]
    public async Task GetTotalExpensesByEmployee_ValidParameters_ReturnsSuccessResponse()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var expectedTotal = 1500.00m;

        _mockExpenseService.Setup(s => s.GetTotalExpensesByEmployeeAsync(employeeId, startDate, endDate))
            .ReturnsAsync(expectedTotal);

        // Act
        var result = await _controller.GetTotalExpensesByEmployee(employeeId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockExpenseService.Verify(s => s.GetTotalExpensesByEmployeeAsync(employeeId, startDate, endDate), Times.Once);
    }
}