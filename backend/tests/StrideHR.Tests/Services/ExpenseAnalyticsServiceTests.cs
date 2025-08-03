using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ExpenseAnalyticsServiceTests
{
    private readonly Mock<IExpenseClaimRepository> _mockExpenseClaimRepository;
    private readonly Mock<IExpenseCategoryRepository> _mockExpenseCategoryRepository;
    private readonly Mock<IExpenseDocumentRepository> _mockExpenseDocumentRepository;
    private readonly Mock<ITravelExpenseRepository> _mockTravelExpenseRepository;
    private readonly Mock<IExpenseBudgetRepository> _mockExpenseBudgetRepository;
    private readonly Mock<IExpenseComplianceViolationRepository> _mockComplianceViolationRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ExpenseService>> _mockLogger;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly ExpenseService _expenseService;

    public ExpenseAnalyticsServiceTests()
    {
        _mockExpenseClaimRepository = new Mock<IExpenseClaimRepository>();
        _mockExpenseCategoryRepository = new Mock<IExpenseCategoryRepository>();
        _mockExpenseDocumentRepository = new Mock<IExpenseDocumentRepository>();
        _mockTravelExpenseRepository = new Mock<ITravelExpenseRepository>();
        _mockExpenseBudgetRepository = new Mock<IExpenseBudgetRepository>();
        _mockComplianceViolationRepository = new Mock<IExpenseComplianceViolationRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ExpenseService>>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockNotificationService = new Mock<INotificationService>();

        _expenseService = new ExpenseService(
            _mockExpenseClaimRepository.Object,
            _mockExpenseCategoryRepository.Object,
            _mockExpenseDocumentRepository.Object,
            _mockTravelExpenseRepository.Object,
            _mockExpenseBudgetRepository.Object,
            _mockComplianceViolationRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockFileStorageService.Object,
            _mockNotificationService.Object);
    }

    [Fact]
    public async Task GetExpenseAnalyticsAsync_ValidData_ReturnsAnalytics()
    {
        // Arrange
        var organizationId = 1;
        var period = ExpenseAnalyticsPeriod.Monthly;
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        var expenseClaims = new List<ExpenseClaim>
        {
            new() 
            { 
                Id = 1, 
                TotalAmount = 100m, 
                Status = ExpenseClaimStatus.Approved,
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            },
            new() 
            { 
                Id = 2, 
                TotalAmount = 200m, 
                Status = ExpenseClaimStatus.Submitted,
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            },
            new() 
            { 
                Id = 3, 
                TotalAmount = 150m, 
                Status = ExpenseClaimStatus.Rejected,
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            }
        };

        var travelExpenses = new List<TravelExpense>
        {
            new() 
            { 
                Id = 1, 
                TravelMode = TravelMode.Car,
                CalculatedMileageAmount = 50m,
                TravelItems = new List<TravelExpenseItem>
                {
                    new() { Amount = 75m }
                }
            }
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expenseClaims);
        _mockTravelExpenseRepository.Setup(r => r.GetByEmployeeIdAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(travelExpenses);
        _mockExpenseCategoryRepository.Setup(r => r.GetActiveByOrganizationAsync(organizationId))
            .ReturnsAsync(new List<ExpenseCategory>());

        // Act
        var result = await _expenseService.GetExpenseAnalyticsAsync(organizationId, period, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(period, result.Period);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(450m, result.TotalExpenses); // 100 + 200 + 150
        Assert.Equal(3, result.TotalClaims);
        Assert.Equal(1, result.ApprovedClaims);
        Assert.Equal(1, result.PendingClaims);
        Assert.Equal(1, result.RejectedClaims);
        Assert.Equal(150m, result.AverageClaimAmount); // 450 / 3
    }

    [Fact]
    public async Task GetCategoryAnalyticsAsync_ValidData_ReturnsCategoryBreakdown()
    {
        // Arrange
        var organizationId = 1;
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        var categories = new List<ExpenseCategory>
        {
            new() { Id = 1, Name = "Travel", Code = "TRV", MonthlyLimit = 1000m },
            new() { Id = 2, Name = "Meals", Code = "MEL", MonthlyLimit = 500m }
        };

        var expenseClaims = new List<ExpenseClaim>
        {
            new() 
            { 
                Id = 1, 
                TotalAmount = 300m,
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } },
                ExpenseItems = new List<ExpenseItem>
                {
                    new() { ExpenseCategoryId = 1, Amount = 200m },
                    new() { ExpenseCategoryId = 2, Amount = 100m }
                }
            },
            new() 
            { 
                Id = 2, 
                TotalAmount = 150m,
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } },
                ExpenseItems = new List<ExpenseItem>
                {
                    new() { ExpenseCategoryId = 1, Amount = 150m }
                }
            }
        };

        _mockExpenseCategoryRepository.Setup(r => r.GetActiveByOrganizationAsync(organizationId))
            .ReturnsAsync(categories);
        _mockExpenseClaimRepository.Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(expenseClaims);

        // Act
        var result = await _expenseService.GetCategoryAnalyticsAsync(organizationId, startDate, endDate);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);

        var travelCategory = resultList.First(c => c.CategoryName == "Travel");
        Assert.Equal(350m, travelCategory.TotalAmount); // 200 + 150
        Assert.Equal(2, travelCategory.ClaimCount);
        Assert.Equal(175m, travelCategory.AverageAmount); // 350 / 2
        Assert.Equal(1000m, travelCategory.BudgetLimit);
        Assert.Equal(35m, travelCategory.BudgetUtilization); // (350 / 1000) * 100
        Assert.False(travelCategory.IsOverBudget);

        var mealsCategory = resultList.First(c => c.CategoryName == "Meals");
        Assert.Equal(100m, mealsCategory.TotalAmount);
        Assert.Equal(1, mealsCategory.ClaimCount);
        Assert.Equal(100m, mealsCategory.AverageAmount);
        Assert.Equal(500m, mealsCategory.BudgetLimit);
        Assert.Equal(20m, mealsCategory.BudgetUtilization); // (100 / 500) * 100
        Assert.False(mealsCategory.IsOverBudget);
    }

    [Fact]
    public async Task GetEmployeeAnalyticsAsync_ValidData_ReturnsEmployeeBreakdown()
    {
        // Arrange
        var organizationId = 1;
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        var employee1 = new Employee 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            Department = "Sales",
            Branch = new Branch { OrganizationId = organizationId }
        };
        var employee2 = new Employee 
        { 
            Id = 2, 
            FirstName = "Jane", 
            LastName = "Smith", 
            Department = "Marketing",
            Branch = new Branch { OrganizationId = organizationId }
        };

        var expenseClaims = new List<ExpenseClaim>
        {
            new() { Id = 1, TotalAmount = 300m, Employee = employee1 },
            new() { Id = 2, TotalAmount = 200m, Employee = employee1 },
            new() { Id = 3, TotalAmount = 150m, Employee = employee2 }
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(expenseClaims);

        // Act
        var result = await _expenseService.GetEmployeeAnalyticsAsync(organizationId, startDate, endDate);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);

        var johnDoe = resultList.First(e => e.EmployeeName == "John Doe");
        Assert.Equal(500m, johnDoe.TotalExpenses); // 300 + 200
        Assert.Equal(2, johnDoe.ClaimCount);
        Assert.Equal(250m, johnDoe.AverageClaimAmount); // 500 / 2
        Assert.Equal("Sales", johnDoe.Department);

        var janeSmith = resultList.First(e => e.EmployeeName == "Jane Smith");
        Assert.Equal(150m, janeSmith.TotalExpenses);
        Assert.Equal(1, janeSmith.ClaimCount);
        Assert.Equal(150m, janeSmith.AverageClaimAmount);
        Assert.Equal("Marketing", janeSmith.Department);
    }

    [Fact]
    public async Task GetMonthlyTrendsAsync_ValidData_ReturnsMonthlyTrends()
    {
        // Arrange
        var organizationId = 1;
        var months = 3;

        var expenseClaims = new List<ExpenseClaim>
        {
            new() 
            { 
                Id = 1, 
                TotalAmount = 100m, 
                ExpenseDate = new DateTime(2024, 1, 15),
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            },
            new() 
            { 
                Id = 2, 
                TotalAmount = 200m, 
                ExpenseDate = new DateTime(2024, 1, 20),
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            },
            new() 
            { 
                Id = 3, 
                TotalAmount = 150m, 
                ExpenseDate = new DateTime(2024, 2, 10),
                Employee = new Employee { Branch = new Branch { OrganizationId = organizationId } }
            }
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expenseClaims);

        // Act
        var result = await _expenseService.GetMonthlyTrendsAsync(organizationId, months);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count); // Only 2 months have data

        var january = resultList.First(t => t.Month == 1);
        Assert.Equal(2024, january.Year);
        Assert.Equal(300m, january.TotalAmount); // 100 + 200
        Assert.Equal(2, january.ClaimCount);
        Assert.Equal(150m, january.AverageClaimAmount); // 300 / 2

        var february = resultList.First(t => t.Month == 2);
        Assert.Equal(2024, february.Year);
        Assert.Equal(150m, february.TotalAmount);
        Assert.Equal(1, february.ClaimCount);
        Assert.Equal(150m, february.AverageClaimAmount);
    }

    [Fact]
    public async Task GetBudgetTrackingAsync_ExistingBudget_ReturnsBudgetTracking()
    {
        // Arrange
        var organizationId = 1;
        var employeeId = 1;
        var budget = new ExpenseBudget
        {
            Id = 1,
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            BudgetLimit = 1000m,
            Period = ExpenseAnalyticsPeriod.Monthly,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today
        };

        var today = DateTime.Today;
        _mockExpenseBudgetRepository.Setup(r => r.GetActiveBudgetAsync(organizationId, null, employeeId, null, today))
            .ReturnsAsync(budget);
        _mockExpenseBudgetRepository.Setup(r => r.GetBudgetUtilizationAsync(budget.Id, It.IsAny<DateTime?>()))
            .ReturnsAsync(750m);

        // Act
        var result = await _expenseService.GetBudgetTrackingAsync(organizationId, null, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(1000m, result.BudgetLimit);
        Assert.Equal(750m, result.ActualExpenses);
        Assert.Equal(250m, result.RemainingBudget); // 1000 - 750
        Assert.Equal(75m, result.BudgetUtilization); // (750 / 1000) * 100
        Assert.False(result.IsOverBudget);
        Assert.Equal(825m, result.ProjectedExpenses); // 750 * 1.1
    }

    [Fact]
    public async Task GetBudgetTrackingAsync_NoBudget_ReturnsDefaultBudgetTracking()
    {
        // Arrange
        var organizationId = 1;
        var employeeId = 1;

        _mockExpenseBudgetRepository.Setup(r => r.GetActiveBudgetAsync(organizationId, null, employeeId, null, DateTime.Today))
            .ReturnsAsync((ExpenseBudget?)null);

        // Act
        var result = await _expenseService.GetBudgetTrackingAsync(organizationId, null, employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(0m, result.BudgetLimit);
        Assert.Equal(0m, result.ActualExpenses);
        Assert.Equal(0m, result.RemainingBudget);
        Assert.Equal(0m, result.BudgetUtilization);
        Assert.False(result.IsOverBudget);
        Assert.Equal(0m, result.ProjectedExpenses);
    }

    [Fact]
    public async Task CheckBudgetComplianceAsync_WithinBudget_ReturnsTrue()
    {
        // Arrange
        var expenseClaimId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = expenseClaimId,
            EmployeeId = 1,
            TotalAmount = 100m,
            ExpenseDate = DateTime.Today,
            Employee = new Employee 
            { 
                Id = 1,
                Branch = new Branch { OrganizationId = 1 }
            }
        };
        var budget = new ExpenseBudget { Id = 1, BudgetLimit = 1000m };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(expenseClaimId))
            .ReturnsAsync(expenseClaim);
        _mockExpenseBudgetRepository.Setup(r => r.GetActiveBudgetAsync(1, null, 1, null, DateTime.Today))
            .ReturnsAsync(budget);
        _mockExpenseBudgetRepository.Setup(r => r.IsBudgetExceededAsync(budget.Id, 100m))
            .ReturnsAsync(false);

        // Act
        var result = await _expenseService.CheckBudgetComplianceAsync(expenseClaimId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckBudgetComplianceAsync_ExceedsBudget_ReturnsFalse()
    {
        // Arrange
        var expenseClaimId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = expenseClaimId,
            EmployeeId = 1,
            TotalAmount = 1500m,
            ExpenseDate = DateTime.Today,
            Employee = new Employee 
            { 
                Id = 1,
                Branch = new Branch { OrganizationId = 1 }
            }
        };
        var budget = new ExpenseBudget { Id = 1, BudgetLimit = 1000m };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(expenseClaimId))
            .ReturnsAsync(expenseClaim);
        _mockExpenseBudgetRepository.Setup(r => r.GetActiveBudgetAsync(1, null, 1, null, DateTime.Today))
            .ReturnsAsync(budget);
        _mockExpenseBudgetRepository.Setup(r => r.IsBudgetExceededAsync(budget.Id, 1500m))
            .ReturnsAsync(true);

        // Act
        var result = await _expenseService.CheckBudgetComplianceAsync(expenseClaimId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckBudgetComplianceAsync_NoBudget_ReturnsTrue()
    {
        // Arrange
        var expenseClaimId = 1;
        var expenseClaim = new ExpenseClaim
        {
            Id = expenseClaimId,
            EmployeeId = 1,
            TotalAmount = 100m,
            ExpenseDate = DateTime.Today,
            Employee = new Employee 
            { 
                Id = 1,
                Branch = new Branch { OrganizationId = 1 }
            }
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdWithDetailsAsync(expenseClaimId))
            .ReturnsAsync(expenseClaim);
        _mockExpenseBudgetRepository.Setup(r => r.GetActiveBudgetAsync(1, null, 1, null, DateTime.Today))
            .ReturnsAsync((ExpenseBudget?)null);

        // Act
        var result = await _expenseService.CheckBudgetComplianceAsync(expenseClaimId);

        // Assert
        Assert.True(result); // No budget means no violation
    }
}