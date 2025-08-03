using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;
using Xunit;

namespace StrideHR.Tests.Repositories;

public class TravelExpenseRepositoryTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly TravelExpenseRepository _repository;

    public TravelExpenseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _repository = new TravelExpenseRepository(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var organization = new Organization { Id = 1, Name = "Test Org" };
        var branch = new Branch { Id = 1, Name = "Test Branch", OrganizationId = 1, Organization = organization };
        var employee = new Employee 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            BranchId = 1, 
            Branch = branch 
        };
        var project = new Project { Id = 1, Name = "Test Project" };

        var expenseClaim = new ExpenseClaim
        {
            Id = 1,
            EmployeeId = 1,
            Employee = employee,
            ClaimNumber = "EXP-001",
            Title = "Business Trip",
            TotalAmount = 500m,
            ExpenseDate = DateTime.Today,
            Status = ExpenseClaimStatus.Approved
        };

        var travelExpense = new TravelExpense
        {
            Id = 1,
            ExpenseClaimId = 1,
            ExpenseClaim = expenseClaim,
            TravelPurpose = "Client Meeting",
            FromLocation = "New York",
            ToLocation = "Boston",
            DepartureDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            TravelMode = TravelMode.Car,
            MileageDistance = 200,
            MileageRate = 0.65m,
            CalculatedMileageAmount = 260m,
            IsRoundTrip = true,
            ProjectId = 1,
            Project = project
        };

        var travelItem = new TravelExpenseItem
        {
            Id = 1,
            TravelExpenseId = 1,
            TravelExpense = travelExpense,
            ExpenseType = TravelExpenseType.Fuel,
            Description = "Gas for trip",
            Amount = 50m,
            Currency = "USD",
            ExpenseDate = DateTime.Today
        };

        travelExpense.TravelItems.Add(travelItem);

        _context.Organizations.Add(organization);
        _context.Branches.Add(branch);
        _context.Employees.Add(employee);
        _context.Projects.Add(project);
        _context.ExpenseClaims.Add(expenseClaim);
        _context.TravelExpenses.Add(travelExpense);
        _context.TravelExpenseItems.Add(travelItem);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByExpenseClaimIdAsync_ExistingClaimId_ReturnsTravelExpense()
    {
        // Act
        var result = await _repository.GetByExpenseClaimIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ExpenseClaimId);
        Assert.Equal("Client Meeting", result.TravelPurpose);
        Assert.Equal("New York", result.FromLocation);
        Assert.Equal("Boston", result.ToLocation);
        Assert.Single(result.TravelItems);
    }

    [Fact]
    public async Task GetByExpenseClaimIdAsync_NonExistingClaimId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByExpenseClaimIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_ExistingEmployeeId_ReturnsTravelExpenses()
    {
        // Act
        var result = await _repository.GetByEmployeeIdAsync(1);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(1, resultList[0].ExpenseClaim.EmployeeId);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_WithDateRange_ReturnsFilteredTravelExpenses()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _repository.GetByEmployeeIdAsync(1, startDate, endDate);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.True(resultList[0].DepartureDate >= startDate);
        Assert.True(resultList[0].ReturnDate <= endDate);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_OutsideDateRange_ReturnsEmpty()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(10);
        var endDate = DateTime.Today.AddDays(20);

        // Act
        var result = await _repository.GetByEmployeeIdAsync(1, startDate, endDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ExistingProjectId_ReturnsTravelExpenses()
    {
        // Act
        var result = await _repository.GetByProjectIdAsync(1);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(1, resultList[0].ProjectId);
    }

    [Fact]
    public async Task GetByProjectIdAsync_NonExistingProjectId_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByProjectIdAsync(999);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTotalMileageByEmployeeAsync_ExistingEmployee_ReturnsTotalMileage()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _repository.GetTotalMileageByEmployeeAsync(1, startDate, endDate);

        // Assert
        Assert.Equal(200m, result);
    }

    [Fact]
    public async Task GetTotalMileageByEmployeeAsync_NoMileage_ReturnsZero()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(10);
        var endDate = DateTime.Today.AddDays(20);

        // Act
        var result = await _repository.GetTotalMileageByEmployeeAsync(1, startDate, endDate);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task GetTotalTravelExpensesByEmployeeAsync_ExistingEmployee_ReturnsTotalAmount()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _repository.GetTotalTravelExpensesByEmployeeAsync(1, startDate, endDate);

        // Assert
        Assert.Equal(50m, result); // Amount from travel item
    }

    [Fact]
    public async Task GetByTravelModeAsync_ExistingTravelMode_ReturnsTravelExpenses()
    {
        // Act
        var result = await _repository.GetByTravelModeAsync(TravelMode.Car);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(TravelMode.Car, resultList[0].TravelMode);
    }

    [Fact]
    public async Task GetByTravelModeAsync_NonExistingTravelMode_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByTravelModeAsync(TravelMode.Flight);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPopularRoutesAsync_ExistingRoutes_ReturnsRoutesDictionary()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var result = await _repository.GetPopularRoutesAsync(startDate, endDate, 10);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey("New York → Boston"));
        Assert.Equal(1, result["New York → Boston"]);
    }

    [Fact]
    public async Task GetPopularRoutesAsync_NoRoutes_ReturnsEmptyDictionary()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(10);
        var endDate = DateTime.Today.AddDays(20);

        // Act
        var result = await _repository.GetPopularRoutesAsync(startDate, endDate, 10);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CalculateMileageAmountAsync_RoundTrip_ReturnsCorrectAmount()
    {
        // Arrange
        var distance = 100m;
        var rate = 0.65m;
        var isRoundTrip = true;

        // Act
        var result = await _repository.CalculateMileageAmountAsync(distance, rate, isRoundTrip);

        // Assert
        Assert.Equal(130m, result); // 100 * 2 * 0.65
    }

    [Fact]
    public async Task CalculateMileageAmountAsync_OneWay_ReturnsCorrectAmount()
    {
        // Arrange
        var distance = 100m;
        var rate = 0.65m;
        var isRoundTrip = false;

        // Act
        var result = await _repository.CalculateMileageAmountAsync(distance, rate, isRoundTrip);

        // Assert
        Assert.Equal(65m, result); // 100 * 0.65
    }

    [Fact]
    public async Task CalculateMileageAmountAsync_ZeroDistance_ReturnsZero()
    {
        // Arrange
        var distance = 0m;
        var rate = 0.65m;
        var isRoundTrip = true;

        // Act
        var result = await _repository.CalculateMileageAmountAsync(distance, rate, isRoundTrip);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateMileageAmountAsync_ZeroRate_ReturnsZero()
    {
        // Arrange
        var distance = 100m;
        var rate = 0m;
        var isRoundTrip = true;

        // Act
        var result = await _repository.CalculateMileageAmountAsync(distance, rate, isRoundTrip);

        // Assert
        Assert.Equal(0m, result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}