using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;
using Xunit;

namespace StrideHR.Tests.Repositories;

public class ShiftRepositoryTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly ShiftRepository _repository;

    public ShiftRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _repository = new ShiftRepository(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var organization = new Organization
        {
            Id = 1,
            Name = "Test Organization",
            CreatedAt = DateTime.UtcNow
        };

        var branch = new Branch
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Test Branch",
            Country = "US",
            Currency = "USD",
            TimeZone = "UTC",
            CreatedAt = DateTime.UtcNow
        };

        var shifts = new List<Shift>
        {
            new()
            {
                Id = 1,
                OrganizationId = 1,
                BranchId = 1,
                Name = "Day Shift",
                Description = "Regular day shift",
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                Type = ShiftType.Day,
                WorkingHours = TimeSpan.FromHours(8),
                WorkingDays = "[1,2,3,4,5]",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                OrganizationId = 1,
                BranchId = 1,
                Name = "Night Shift",
                Description = "Night shift",
                StartTime = TimeSpan.FromHours(21),
                EndTime = TimeSpan.FromHours(5),
                Type = ShiftType.Night,
                WorkingHours = TimeSpan.FromHours(8),
                WorkingDays = "[1,2,3,4,5]",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                OrganizationId = 1,
                BranchId = 1,
                Name = "Inactive Shift",
                Description = "Inactive shift",
                StartTime = TimeSpan.FromHours(13),
                EndTime = TimeSpan.FromHours(21),
                Type = ShiftType.Day,
                WorkingHours = TimeSpan.FromHours(8),
                WorkingDays = "[1,2,3,4,5]",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Organizations.Add(organization);
        _context.Branches.Add(branch);
        _context.Shifts.AddRange(shifts);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_ValidOrganizationId_ReturnsShifts()
    {
        // Act
        var result = await _repository.GetByOrganizationIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, shift => Assert.Equal(1, shift.OrganizationId));
    }

    [Fact]
    public async Task GetByBranchIdAsync_ValidBranchId_ReturnsShifts()
    {
        // Act
        var result = await _repository.GetByBranchIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, shift => Assert.Equal(1, shift.BranchId));
    }

    [Fact]
    public async Task GetActiveShiftsAsync_ValidOrganizationId_ReturnsOnlyActiveShifts()
    {
        // Act
        var result = await _repository.GetActiveShiftsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, shift => Assert.True(shift.IsActive));
    }

    [Fact]
    public async Task SearchShiftsAsync_WithSearchTerm_ReturnsMatchingShifts()
    {
        // Arrange
        var criteria = new ShiftSearchCriteria
        {
            OrganizationId = 1,
            SearchTerm = "Day",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchShiftsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // "Day Shift" and "Inactive Shift" (contains "Day" in description)
        Assert.All(result, shift => Assert.True(
            shift.Name.Contains("Day", StringComparison.OrdinalIgnoreCase) ||
            shift.Description.Contains("Day", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task SearchShiftsAsync_WithShiftType_ReturnsMatchingShifts()
    {
        // Arrange
        var criteria = new ShiftSearchCriteria
        {
            OrganizationId = 1,
            Type = ShiftType.Night,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchShiftsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(ShiftType.Night, result.First().Type);
    }

    [Fact]
    public async Task SearchShiftsAsync_WithActiveFilter_ReturnsOnlyActiveShifts()
    {
        // Arrange
        var criteria = new ShiftSearchCriteria
        {
            OrganizationId = 1,
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchShiftsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, shift => Assert.True(shift.IsActive));
    }

    [Fact]
    public async Task GetTotalCountAsync_WithCriteria_ReturnsCorrectCount()
    {
        // Arrange
        var criteria = new ShiftSearchCriteria
        {
            OrganizationId = 1,
            IsActive = true
        };

        // Act
        var result = await _repository.GetTotalCountAsync(criteria);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task IsShiftNameUniqueAsync_UniqueName_ReturnsTrue()
    {
        // Act
        var result = await _repository.IsShiftNameUniqueAsync(1, "Unique Shift Name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsShiftNameUniqueAsync_ExistingName_ReturnsFalse()
    {
        // Act
        var result = await _repository.IsShiftNameUniqueAsync(1, "Day Shift");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsShiftNameUniqueAsync_ExistingNameWithExclusion_ReturnsTrue()
    {
        // Act
        var result = await _repository.IsShiftNameUniqueAsync(1, "Day Shift", 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetShiftsByTypeAsync_ValidType_ReturnsMatchingShifts()
    {
        // Act
        var result = await _repository.GetShiftsByTypeAsync(1, ShiftType.Day);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only active Day shifts
        Assert.All(result, shift => Assert.Equal(ShiftType.Day, shift.Type));
        Assert.All(result, shift => Assert.True(shift.IsActive));
    }

    [Fact]
    public async Task GetOverlappingShiftsAsync_OverlappingTimes_ReturnsOverlappingShifts()
    {
        // Arrange - Looking for shifts that overlap with 8:00 AM to 10:00 AM
        var startTime = TimeSpan.FromHours(8);
        var endTime = TimeSpan.FromHours(10);

        // Act
        var result = await _repository.GetOverlappingShiftsAsync(1, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only Day Shift (9-17) overlaps with 8-10
        Assert.Equal("Day Shift", result.First().Name);
    }

    [Fact]
    public async Task GetOverlappingShiftsAsync_NonOverlappingTimes_ReturnsEmpty()
    {
        // Arrange - Looking for shifts that overlap with 6:00 AM to 7:00 AM
        var startTime = TimeSpan.FromHours(6);
        var endTime = TimeSpan.FromHours(7);

        // Act
        var result = await _repository.GetOverlappingShiftsAsync(1, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOverlappingShiftsAsync_WithExclusion_ExcludesSpecifiedShift()
    {
        // Arrange - Looking for shifts that overlap with 9:00 AM to 17:00 PM, excluding shift ID 1
        var startTime = TimeSpan.FromHours(9);
        var endTime = TimeSpan.FromHours(17);

        // Act
        var result = await _repository.GetOverlappingShiftsAsync(1, startTime, endTime, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Day Shift is excluded, no other shifts overlap exactly
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}