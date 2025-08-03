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

public class TravelExpenseServiceTests
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

    public TravelExpenseServiceTests()
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
    public async Task CreateTravelExpenseAsync_ValidData_ReturnsTravelExpenseDto()
    {
        // Arrange
        var expenseClaimId = 1;
        var dto = new CreateTravelExpenseDto
        {
            TravelPurpose = "Business Meeting",
            FromLocation = "New York",
            ToLocation = "Boston",
            DepartureDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            TravelMode = TravelMode.Car,
            MileageDistance = 200,
            MileageRate = 0.65m,
            IsRoundTrip = true,
            TravelItems = new List<CreateTravelExpenseItemDto>
            {
                new()
                {
                    ExpenseType = TravelExpenseType.Fuel,
                    Description = "Gas for trip",
                    Amount = 50.00m,
                    Currency = "USD",
                    ExpenseDate = DateTime.Today
                }
            }
        };

        var expenseClaim = new ExpenseClaim { Id = expenseClaimId, EmployeeId = 1 };
        var travelExpense = new TravelExpense { Id = 1, ExpenseClaimId = expenseClaimId };
        var travelExpenseDto = new TravelExpenseDto { Id = 1, ExpenseClaimId = expenseClaimId };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdAsync(expenseClaimId))
            .ReturnsAsync(expenseClaim);
        _mockTravelExpenseRepository.Setup(r => r.CalculateMileageAmountAsync(200, 0.65m, true))
            .ReturnsAsync(260m); // 200 * 2 * 0.65
        _mockTravelExpenseRepository.Setup(r => r.AddAsync(It.IsAny<TravelExpense>()))
            .ReturnsAsync(travelExpense);
        _mockTravelExpenseRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(travelExpense);
        _mockMapper.Setup(m => m.Map<TravelExpenseDto>(It.IsAny<TravelExpense>()))
            .Returns(travelExpenseDto);

        // Act
        var result = await _expenseService.CreateTravelExpenseAsync(expenseClaimId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expenseClaimId, result.ExpenseClaimId);
        _mockTravelExpenseRepository.Verify(r => r.AddAsync(It.IsAny<TravelExpense>()), Times.Once);
        _mockTravelExpenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTravelExpenseAsync_ExpenseClaimNotFound_ThrowsArgumentException()
    {
        // Arrange
        var expenseClaimId = 1;
        var dto = new CreateTravelExpenseDto
        {
            TravelPurpose = "Business Meeting",
            FromLocation = "New York",
            ToLocation = "Boston",
            DepartureDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            TravelMode = TravelMode.Car
        };

        _mockExpenseClaimRepository.Setup(r => r.GetByIdAsync(expenseClaimId))
            .ReturnsAsync((ExpenseClaim?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _expenseService.CreateTravelExpenseAsync(expenseClaimId, dto));
    }

    [Fact]
    public async Task CalculateMileageAsync_ValidData_ReturnsCorrectCalculation()
    {
        // Arrange
        var dto = new MileageCalculationDto
        {
            Distance = 100,
            Rate = 0.65m,
            IsRoundTrip = true,
            FromLocation = "New York",
            ToLocation = "Boston",
            TravelMode = TravelMode.PersonalVehicle
        };

        _mockTravelExpenseRepository.Setup(r => r.CalculateMileageAmountAsync(100, 0.65m, true))
            .ReturnsAsync(130m); // 100 * 2 * 0.65

        // Act
        var result = await _expenseService.CalculateMileageAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.TotalDistance); // 100 * 2 for round trip
        Assert.Equal(0.65m, result.Rate);
        Assert.Equal(130m, result.TotalAmount);
        Assert.True(result.IsRoundTrip);
        Assert.Contains("Distance: 100 miles", result.CalculationDetails);
    }

    [Fact]
    public async Task CalculateMileageAsync_OneWayTrip_ReturnsCorrectCalculation()
    {
        // Arrange
        var dto = new MileageCalculationDto
        {
            Distance = 100,
            Rate = 0.65m,
            IsRoundTrip = false,
            FromLocation = "New York",
            ToLocation = "Boston",
            TravelMode = TravelMode.PersonalVehicle
        };

        _mockTravelExpenseRepository.Setup(r => r.CalculateMileageAmountAsync(100, 0.65m, false))
            .ReturnsAsync(65m); // 100 * 0.65

        // Act
        var result = await _expenseService.CalculateMileageAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.TotalDistance);
        Assert.Equal(0.65m, result.Rate);
        Assert.Equal(65m, result.TotalAmount);
        Assert.False(result.IsRoundTrip);
    }

    [Fact]
    public async Task GetMileageRateAsync_PersonalVehicle_ReturnsStandardRate()
    {
        // Arrange
        var organizationId = 1;
        var travelMode = TravelMode.PersonalVehicle;

        // Act
        var result = await _expenseService.GetMileageRateAsync(organizationId, travelMode);

        // Assert
        Assert.Equal(0.65m, result); // Standard IRS rate
    }

    [Fact]
    public async Task GetMileageRateAsync_Car_ReturnsStandardRate()
    {
        // Arrange
        var organizationId = 1;
        var travelMode = TravelMode.Car;

        // Act
        var result = await _expenseService.GetMileageRateAsync(organizationId, travelMode);

        // Assert
        Assert.Equal(0.65m, result);
    }

    [Fact]
    public async Task GetMileageRateAsync_Motorcycle_ReturnsMotorcycleRate()
    {
        // Arrange
        var organizationId = 1;
        var travelMode = TravelMode.Motorcycle;

        // Act
        var result = await _expenseService.GetMileageRateAsync(organizationId, travelMode);

        // Assert
        Assert.Equal(0.58m, result);
    }

    [Fact]
    public async Task GetMileageRateAsync_OtherTravelMode_ReturnsZero()
    {
        // Arrange
        var organizationId = 1;
        var travelMode = TravelMode.Flight;

        // Act
        var result = await _expenseService.GetMileageRateAsync(organizationId, travelMode);

        // Assert
        Assert.Equal(0.00m, result);
    }

    [Fact]
    public async Task UpdateTravelExpenseAsync_ValidData_ReturnsTravelExpenseDto()
    {
        // Arrange
        var travelExpenseId = 1;
        var dto = new CreateTravelExpenseDto
        {
            TravelPurpose = "Updated Business Meeting",
            FromLocation = "New York",
            ToLocation = "Philadelphia",
            DepartureDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            TravelMode = TravelMode.Train,
            MileageDistance = 150,
            MileageRate = 0.65m,
            IsRoundTrip = false
        };

        var existingTravelExpense = new TravelExpense 
        { 
            Id = travelExpenseId, 
            ExpenseClaimId = 1,
            TravelPurpose = "Original Purpose"
        };
        var updatedTravelExpenseDto = new TravelExpenseDto 
        { 
            Id = travelExpenseId, 
            TravelPurpose = "Updated Business Meeting" 
        };

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync(existingTravelExpense);
        _mockTravelExpenseRepository.Setup(r => r.CalculateMileageAmountAsync(150, 0.65m, false))
            .ReturnsAsync(97.5m); // 150 * 0.65
        _mockTravelExpenseRepository.Setup(r => r.UpdateAsync(It.IsAny<TravelExpense>()))
            .Returns(Task.CompletedTask);
        _mockTravelExpenseRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<TravelExpenseDto>(It.IsAny<TravelExpense>()))
            .Returns(updatedTravelExpenseDto);

        // Act
        var result = await _expenseService.UpdateTravelExpenseAsync(travelExpenseId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Business Meeting", result.TravelPurpose);
        _mockTravelExpenseRepository.Verify(r => r.UpdateAsync(It.IsAny<TravelExpense>()), Times.Once);
        _mockTravelExpenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTravelExpenseAsync_TravelExpenseNotFound_ThrowsArgumentException()
    {
        // Arrange
        var travelExpenseId = 1;
        var dto = new CreateTravelExpenseDto
        {
            TravelPurpose = "Updated Business Meeting",
            FromLocation = "New York",
            ToLocation = "Philadelphia",
            DepartureDate = DateTime.Today,
            ReturnDate = DateTime.Today.AddDays(1),
            TravelMode = TravelMode.Train
        };

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync((TravelExpense?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _expenseService.UpdateTravelExpenseAsync(travelExpenseId, dto));
    }

    [Fact]
    public async Task GetTravelExpenseByIdAsync_ExistingId_ReturnsTravelExpenseDto()
    {
        // Arrange
        var travelExpenseId = 1;
        var travelExpense = new TravelExpense { Id = travelExpenseId, ExpenseClaimId = 1 };
        var travelExpenseDto = new TravelExpenseDto { Id = travelExpenseId, ExpenseClaimId = 1 };

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync(travelExpense);
        _mockMapper.Setup(m => m.Map<TravelExpenseDto>(travelExpense))
            .Returns(travelExpenseDto);

        // Act
        var result = await _expenseService.GetTravelExpenseByIdAsync(travelExpenseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(travelExpenseId, result.Id);
    }

    [Fact]
    public async Task GetTravelExpenseByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var travelExpenseId = 1;

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync((TravelExpense?)null);

        // Act
        var result = await _expenseService.GetTravelExpenseByIdAsync(travelExpenseId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTravelExpenseAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var travelExpenseId = 1;
        var travelExpense = new TravelExpense { Id = travelExpenseId, ExpenseClaimId = 1 };

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync(travelExpense);
        _mockTravelExpenseRepository.Setup(r => r.DeleteAsync(travelExpense))
            .Returns(Task.CompletedTask);
        _mockTravelExpenseRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _expenseService.DeleteTravelExpenseAsync(travelExpenseId);

        // Assert
        Assert.True(result);
        _mockTravelExpenseRepository.Verify(r => r.DeleteAsync(travelExpense), Times.Once);
        _mockTravelExpenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTravelExpenseAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var travelExpenseId = 1;

        _mockTravelExpenseRepository.Setup(r => r.GetByIdAsync(travelExpenseId))
            .ReturnsAsync((TravelExpense?)null);

        // Act
        var result = await _expenseService.DeleteTravelExpenseAsync(travelExpenseId);

        // Assert
        Assert.False(result);
        _mockTravelExpenseRepository.Verify(r => r.DeleteAsync(It.IsAny<TravelExpense>()), Times.Never);
        _mockTravelExpenseRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}