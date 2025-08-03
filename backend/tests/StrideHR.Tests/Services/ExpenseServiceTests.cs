using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;
using StrideHR.Core.Models.Notification;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseClaimRepository> _mockExpenseClaimRepository;
    private readonly Mock<IExpenseCategoryRepository> _mockExpenseCategoryRepository;
    private readonly Mock<IExpenseDocumentRepository> _mockExpenseDocumentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ExpenseService>> _mockLogger;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly ExpenseService _expenseService;

    public ExpenseServiceTests()
    {
        _mockExpenseClaimRepository = new Mock<IExpenseClaimRepository>();
        _mockExpenseCategoryRepository = new Mock<IExpenseCategoryRepository>();
        _mockExpenseDocumentRepository = new Mock<IExpenseDocumentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ExpenseService>>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockNotificationService = new Mock<INotificationService>();

        _expenseService = new ExpenseService(
            _mockExpenseClaimRepository.Object,
            _mockExpenseCategoryRepository.Object,
            _mockExpenseDocumentRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockFileStorageService.Object,
            _mockNotificationService.Object);
    }

    [Fact]
    public async Task CreateExpenseClaimAsync_ValidData_ReturnsExpenseClaimDto()
    {
        // Arrange
        var employeeId = 1;
        var dto = new CreateExpenseClaimDto
        {
            Title = "Business Travel",
            Description = "Travel to client site",
            ExpenseDate = DateTime.Today,
            Currency = "USD",
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

        var claimNumber = "EXP-2025-01-0001";
        var expenseClaim = new ExpenseClaim
        {
            Id = 1,
            EmployeeId = employeeId,
            ClaimNumber = claimNumber,
            Title = dto.Title,
            Description = dto.Description,
            TotalAmount = 500.00m,
            Status = ExpenseClaimStatus.Draft
        };

        var expectedDto = new ExpenseClaimDto
        {
            Id = 1,
            ClaimNumber = claimNumber,
            Title = dto.Title,
            Status = ExpenseClaimStatus.Draft
        };

        _mockExpenseClaimRepository.Setup(r => r.GenerateClaimNumberAsync())
            .ReturnsAsync(claimNumber);

        _mockExpenseClaimRepository.Setup(r => r.AddAsync(It.IsAny<ExpenseClaim>()))
            .ReturnsAsync(expenseClaim);

        _mockExpenseClaimRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(expenseClaim);

        _mockMapper.Setup(m => m.Map<ExpenseClaimDto>(It.IsAny<ExpenseClaim>()))
            .Returns(expectedDto);

        // Act
        var result = await _expenseService.CreateExpenseClaimAsync(employeeId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.ClaimNumber, result.ClaimNumber);
        Assert.Equal(expectedDto.Title, result.Title);
        _mockExpenseClaimRepository.Verify(r => r.AddAsync(It.IsAny<ExpenseClaim>()), Times.Once);
        _mockExpenseClaimRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateExpenseClaimAsync_EmptyExpenseItems_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 1;
        var dto = new CreateExpenseClaimDto
        {
            Title = "Business Travel",
            Description = "Travel to client site",
            ExpenseDate = DateTime.Today,
            Currency = "USD",
            ExpenseItems = new List<CreateExpenseItemDto>() // Empty list
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _expenseService.CreateExpenseClaimAsync(employeeId, dto));

        Assert.Contains("At least one expense item is required", exception.Message);
    }

    [Fact]
    public async Task GetExpenseClaimByIdAsync_ExistingClaim_ReturnsExpenseClaimDto()
    {
        // Arrange
        var claimId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            ClaimNumber = "EXP-2025-01-0001",
            Title = "Business Travel",
            Status = ExpenseClaimStatus.Draft
        };

        var expectedDto = new ExpenseClaimDto
        {
            Id = claimId,
            ClaimNumber = "EXP-2025-01-0001",
            Title = "Business Travel",
            Status = ExpenseClaimStatus.Draft
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        _mockMapper.Setup(m => m.Map<ExpenseClaimDto>(expenseClaim))
            .Returns(expectedDto);

        // Act
        var result = await _expenseService.GetExpenseClaimByIdAsync(claimId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.ClaimNumber, result.ClaimNumber);
    }

    [Fact]
    public async Task GetExpenseClaimByIdAsync_NonExistingClaim_ReturnsNull()
    {
        // Arrange
        var claimId = 999;
        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync((ExpenseClaim?)null);

        // Act
        var result = await _expenseService.GetExpenseClaimByIdAsync(claimId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SubmitExpenseClaimAsync_ValidDraftClaim_ReturnsTrue()
    {
        // Arrange
        var claimId = 1;
        var employeeId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            EmployeeId = employeeId,
            Status = ExpenseClaimStatus.Draft,
            ExpenseItems = new List<ExpenseItem>
            {
                new ExpenseItem { Amount = 100.00m }
            }
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        _mockExpenseClaimRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpenseClaim>()))
            .Returns(Task.CompletedTask);

        _mockExpenseClaimRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockNotificationService.Setup(n => n.CreateFromTemplateAsync("ExpenseClaimSubmitted", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new NotificationDto());

        // Act
        var result = await _expenseService.SubmitExpenseClaimAsync(claimId, employeeId);

        // Assert
        Assert.True(result);
        Assert.Equal(ExpenseClaimStatus.Submitted, expenseClaim.Status);
        _mockExpenseClaimRepository.Verify(r => r.UpdateAsync(expenseClaim), Times.Once);
        _mockNotificationService.Verify(n => n.CreateFromTemplateAsync("ExpenseClaimSubmitted", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task SubmitExpenseClaimAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claimId = 1;
        var employeeId = 1;
        var unauthorizedEmployeeId = 2;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            EmployeeId = employeeId,
            Status = ExpenseClaimStatus.Draft
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _expenseService.SubmitExpenseClaimAsync(claimId, unauthorizedEmployeeId));

        Assert.Equal("You can only submit your own expense claims", exception.Message);
    }

    [Fact]
    public async Task SubmitExpenseClaimAsync_NonDraftStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var claimId = 1;
        var employeeId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            EmployeeId = employeeId,
            Status = ExpenseClaimStatus.Submitted // Already submitted
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _expenseService.SubmitExpenseClaimAsync(claimId, employeeId));

        Assert.Equal("Only draft expense claims can be submitted", exception.Message);
    }

    [Fact]
    public async Task ApproveExpenseClaimAsync_ValidClaim_ReturnsTrue()
    {
        // Arrange
        var claimId = 1;
        var approverId = 2;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            Status = ExpenseClaimStatus.Submitted,
            TotalAmount = 500.00m,
            ApprovalHistory = new List<ExpenseApprovalHistory>()
        };

        var approvalDto = new ExpenseApprovalDto
        {
            Action = ApprovalAction.Approved,
            Comments = "Approved for business travel"
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        _mockExpenseClaimRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpenseClaim>()))
            .Returns(Task.CompletedTask);

        _mockExpenseClaimRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockNotificationService.Setup(n => n.CreateFromTemplateAsync("ExpenseClaimApproved", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new NotificationDto());

        // Act
        var result = await _expenseService.ApproveExpenseClaimAsync(claimId, approverId, approvalDto);

        // Assert
        Assert.True(result);
        Assert.Equal(ExpenseClaimStatus.Approved, expenseClaim.Status);
        Assert.Equal(approverId, expenseClaim.ApprovedBy);
        Assert.NotNull(expenseClaim.ApprovedDate);
        Assert.Single(expenseClaim.ApprovalHistory);
        _mockNotificationService.Verify(n => n.CreateFromTemplateAsync("ExpenseClaimApproved", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task RejectExpenseClaimAsync_ValidClaim_ReturnsTrue()
    {
        // Arrange
        var claimId = 1;
        var approverId = 2;
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            Status = ExpenseClaimStatus.Submitted,
            ApprovalHistory = new List<ExpenseApprovalHistory>()
        };

        var approvalDto = new ExpenseApprovalDto
        {
            Action = ApprovalAction.Rejected,
            Comments = "Insufficient documentation",
            RejectionReason = "Missing receipts"
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(claimId))
            .ReturnsAsync(expenseClaim);

        _mockExpenseClaimRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpenseClaim>()))
            .Returns(Task.CompletedTask);

        _mockExpenseClaimRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockNotificationService.Setup(n => n.CreateFromTemplateAsync("ExpenseClaimRejected", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new NotificationDto());

        // Act
        var result = await _expenseService.RejectExpenseClaimAsync(claimId, approverId, approvalDto);

        // Assert
        Assert.True(result);
        Assert.Equal(ExpenseClaimStatus.Rejected, expenseClaim.Status);
        Assert.Equal("Missing receipts", expenseClaim.RejectionReason);
        Assert.Single(expenseClaim.ApprovalHistory);
        _mockNotificationService.Verify(n => n.CreateFromTemplateAsync("ExpenseClaimRejected", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task ValidateExpenseClaimAsync_EmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateExpenseClaimDto
        {
            Title = "", // Empty title
            ExpenseDate = DateTime.Today,
            ExpenseItems = new List<CreateExpenseItemDto>
            {
                new CreateExpenseItemDto
                {
                    ExpenseCategoryId = 1,
                    Description = "Test item",
                    Amount = 100.00m,
                    ExpenseDate = DateTime.Today
                }
            }
        };

        // Act
        var result = await _expenseService.ValidateExpenseClaimAsync(dto, 1);

        // Assert
        Assert.Contains("Title is required", result);
    }

    [Fact]
    public async Task ValidateExpenseClaimAsync_FutureExpenseDate_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateExpenseClaimDto
        {
            Title = "Test Claim",
            ExpenseDate = DateTime.Today.AddDays(1), // Future date
            ExpenseItems = new List<CreateExpenseItemDto>
            {
                new CreateExpenseItemDto
                {
                    ExpenseCategoryId = 1,
                    Description = "Test item",
                    Amount = 100.00m,
                    ExpenseDate = DateTime.Today
                }
            }
        };

        // Act
        var result = await _expenseService.ValidateExpenseClaimAsync(dto, 1);

        // Assert
        Assert.Contains("Expense date cannot be in the future", result);
    }

    [Fact]
    public async Task CreateExpenseCategoryAsync_ValidData_ReturnsExpenseCategoryDto()
    {
        // Arrange
        var organizationId = 1;
        var dto = new CreateExpenseCategoryDto
        {
            Name = "Travel",
            Code = "TRV",
            Description = "Travel expenses"
        };

        var category = new ExpenseCategory
        {
            Id = 1,
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            OrganizationId = organizationId
        };

        var expectedDto = new ExpenseCategoryDto
        {
            Id = 1,
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description
        };

        _mockExpenseCategoryRepository.Setup(r => r.IsCodeUniqueAsync(dto.Code, organizationId, null))
            .ReturnsAsync(true);

        _mockExpenseCategoryRepository.Setup(r => r.AddAsync(It.IsAny<ExpenseCategory>()))
            .ReturnsAsync(category);

        _mockExpenseCategoryRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockMapper.Setup(m => m.Map<ExpenseCategoryDto>(It.IsAny<ExpenseCategory>()))
            .Returns(expectedDto);

        // Act
        var result = await _expenseService.CreateExpenseCategoryAsync(dto, organizationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Code, result.Code);
        _mockExpenseCategoryRepository.Verify(r => r.AddAsync(It.IsAny<ExpenseCategory>()), Times.Once);
    }

    [Fact]
    public async Task CreateExpenseCategoryAsync_DuplicateCode_ThrowsArgumentException()
    {
        // Arrange
        var organizationId = 1;
        var dto = new CreateExpenseCategoryDto
        {
            Name = "Travel",
            Code = "TRV",
            Description = "Travel expenses"
        };

        _mockExpenseCategoryRepository.Setup(r => r.IsCodeUniqueAsync(dto.Code, organizationId, null))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _expenseService.CreateExpenseCategoryAsync(dto, organizationId));

        Assert.Contains("Category code 'TRV' already exists", exception.Message);
    }

    [Fact]
    public async Task MarkAsReimbursedAsync_ApprovedClaim_ReturnsTrue()
    {
        // Arrange
        var claimId = 1;
        var processedBy = 2;
        var reimbursementReference = "REF-001";
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            Status = ExpenseClaimStatus.Approved
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdAsync(claimId))
            .ReturnsAsync(expenseClaim);

        _mockExpenseClaimRepository.Setup(r => r.UpdateAsync(It.IsAny<ExpenseClaim>()))
            .Returns(Task.CompletedTask);

        _mockExpenseClaimRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        _mockNotificationService.Setup(n => n.CreateFromTemplateAsync("ExpenseClaimReimbursed", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new NotificationDto());

        // Act
        var result = await _expenseService.MarkAsReimbursedAsync(claimId, reimbursementReference, processedBy);

        // Assert
        Assert.True(result);
        Assert.Equal(ExpenseClaimStatus.Reimbursed, expenseClaim.Status);
        Assert.Equal(reimbursementReference, expenseClaim.ReimbursementReference);
        Assert.NotNull(expenseClaim.ReimbursedDate);
        _mockNotificationService.Verify(n => n.CreateFromTemplateAsync("ExpenseClaimReimbursed", It.IsAny<int>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReimbursedAsync_NonApprovedClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        var claimId = 1;
        var processedBy = 2;
        var reimbursementReference = "REF-001";
        var expenseClaim = new ExpenseClaim
        {
            Id = claimId,
            Status = ExpenseClaimStatus.Submitted // Not approved
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdAsync(claimId))
            .ReturnsAsync(expenseClaim);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _expenseService.MarkAsReimbursedAsync(claimId, reimbursementReference, processedBy));

        Assert.Equal("Only approved expense claims can be marked as reimbursed", exception.Message);
    }
}