using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;
using Xunit;

namespace StrideHR.Tests.Repositories;

public class ShiftAssignmentRepositoryTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly ShiftAssignmentRepository _repository;

    public ShiftAssignmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _repository = new ShiftAssignmentRepository(_context);

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

        var employees = new List<Employee>
        {
            new()
            {
                Id = 1,
                EmployeeId = "EMP001",
                BranchId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                EmployeeId = "EMP002",
                BranchId = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                Status = EmployeeStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        var shifts = new List<Shift>
        {
            new()
            {
                Id = 1,
                OrganizationId = 1,
                BranchId = 1,
                Name = "Day Shift",
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                Type = ShiftType.Day,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                OrganizationId = 1,
                BranchId = 1,
                Name = "Night Shift",
                StartTime = TimeSpan.FromHours(21),
                EndTime = TimeSpan.FromHours(5),
                Type = ShiftType.Night,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var assignments = new List<ShiftAssignment>
        {
            new()
            {
                Id = 1,
                EmployeeId = 1,
                ShiftId = 1,
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today.AddDays(30),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                EmployeeId = 2,
                ShiftId = 1,
                StartDate = DateTime.Today.AddDays(-15),
                EndDate = DateTime.Today.AddDays(15),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                EmployeeId = 1,
                ShiftId = 2,
                StartDate = DateTime.Today.AddDays(-60),
                EndDate = DateTime.Today.AddDays(-31),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Organizations.Add(organization);
        _context.Branches.Add(branch);
        _context.Employees.AddRange(employees);
        _context.Shifts.AddRange(shifts);
        _context.ShiftAssignments.AddRange(assignments);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_ValidEmployeeId_ReturnsAssignments()
    {
        // Act
        var result = await _repository.GetByEmployeeIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.Equal(1, assignment.EmployeeId));
    }

    [Fact]
    public async Task GetByShiftIdAsync_ValidShiftId_ReturnsAssignments()
    {
        // Act
        var result = await _repository.GetByShiftIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.Equal(1, assignment.ShiftId));
    }

    [Fact]
    public async Task GetActiveAssignmentsAsync_ValidEmployeeId_ReturnsOnlyActiveAssignments()
    {
        // Act
        var result = await _repository.GetActiveAssignmentsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, assignment => Assert.True(assignment.IsActive));
        Assert.All(result, assignment => 
        {
            Assert.True(assignment.StartDate <= DateTime.Today);
            Assert.True(assignment.EndDate == null || assignment.EndDate >= DateTime.Today);
        });
    }

    [Fact]
    public async Task GetCurrentAssignmentAsync_ValidEmployeeAndDate_ReturnsCurrentAssignment()
    {
        // Act
        var result = await _repository.GetCurrentAssignmentAsync(1, DateTime.Today);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.True(result.IsActive);
        Assert.True(result.StartDate <= DateTime.Today);
        Assert.True(result.EndDate == null || result.EndDate >= DateTime.Today);
    }

    [Fact]
    public async Task GetCurrentAssignmentAsync_NoCurrentAssignment_ReturnsNull()
    {
        // Act - Looking for assignment on a date outside the range
        var result = await _repository.GetCurrentAssignmentAsync(1, DateTime.Today.AddDays(100));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAssignmentsByDateRangeAsync_ValidRange_ReturnsAssignmentsInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-20);
        var endDate = DateTime.Today.AddDays(20);

        // Act
        var result = await _repository.GetAssignmentsByDateRangeAsync(1, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only the active assignment overlaps with this range
        Assert.All(result, assignment => 
        {
            Assert.True(assignment.StartDate <= endDate);
            Assert.True(assignment.EndDate == null || assignment.EndDate >= startDate);
        });
    }

    [Fact]
    public async Task SearchAssignmentsAsync_WithEmployeeId_ReturnsMatchingAssignments()
    {
        // Arrange
        var criteria = new ShiftAssignmentSearchCriteria
        {
            EmployeeId = 1,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchAssignmentsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.Equal(1, assignment.EmployeeId));
    }

    [Fact]
    public async Task SearchAssignmentsAsync_WithShiftId_ReturnsMatchingAssignments()
    {
        // Arrange
        var criteria = new ShiftAssignmentSearchCriteria
        {
            ShiftId = 1,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchAssignmentsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.Equal(1, assignment.ShiftId));
    }

    [Fact]
    public async Task SearchAssignmentsAsync_WithActiveFilter_ReturnsOnlyActiveAssignments()
    {
        // Arrange
        var criteria = new ShiftAssignmentSearchCriteria
        {
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchAssignmentsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.True(assignment.IsActive));
    }

    [Fact]
    public async Task SearchAssignmentsAsync_WithSearchTerm_ReturnsMatchingAssignments()
    {
        // Arrange
        var criteria = new ShiftAssignmentSearchCriteria
        {
            SearchTerm = "John",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.SearchAssignmentsAsync(criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => Assert.Equal("John", assignment.Employee.FirstName));
    }

    [Fact]
    public async Task GetTotalCountAsync_WithCriteria_ReturnsCorrectCount()
    {
        // Arrange
        var criteria = new ShiftAssignmentSearchCriteria
        {
            IsActive = true
        };

        // Act
        var result = await _repository.GetTotalCountAsync(criteria);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetConflictingAssignmentsAsync_OverlappingDates_ReturnsConflictingAssignments()
    {
        // Arrange - Looking for conflicts for employee 1, shift 2, in a date range that overlaps with existing assignment
        var employeeId = 1;
        var shiftId = 2;
        var startDate = DateTime.Today.AddDays(-10);
        var endDate = DateTime.Today.AddDays(10);

        // Act
        var result = await _repository.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Should find the active Day Shift assignment that conflicts
        Assert.Equal(1, result.First().ShiftId); // The conflicting assignment is for shift 1
    }

    [Fact]
    public async Task GetConflictingAssignmentsAsync_NonOverlappingDates_ReturnsEmpty()
    {
        // Arrange - Looking for conflicts in a date range that doesn't overlap
        var employeeId = 1;
        var shiftId = 2;
        var startDate = DateTime.Today.AddDays(100);
        var endDate = DateTime.Today.AddDays(130);

        // Act
        var result = await _repository.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAssignmentsByBranchAsync_ValidBranch_ReturnsAssignments()
    {
        // Act
        var result = await _repository.GetAssignmentsByBranchAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // Only active assignments
        Assert.All(result, assignment => Assert.True(assignment.IsActive));
    }

    [Fact]
    public async Task GetAssignmentsByBranchAsync_WithDate_ReturnsAssignmentsForDate()
    {
        // Act
        var result = await _repository.GetAssignmentsByBranchAsync(1, DateTime.Today);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, assignment => 
        {
            Assert.True(assignment.StartDate <= DateTime.Today);
            Assert.True(assignment.EndDate == null || assignment.EndDate >= DateTime.Today);
        });
    }

    [Fact]
    public async Task HasActiveAssignmentAsync_ExistingActiveAssignment_ReturnsTrue()
    {
        // Act
        var result = await _repository.HasActiveAssignmentAsync(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasActiveAssignmentAsync_NoActiveAssignment_ReturnsFalse()
    {
        // Act
        var result = await _repository.HasActiveAssignmentAsync(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUpcomingAssignmentsAsync_ValidEmployee_ReturnsUpcomingAssignments()
    {
        // Act
        var result = await _repository.GetUpcomingAssignmentsAsync(1, 7);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only the active assignment that extends into the future
        Assert.All(result, assignment => 
        {
            Assert.True(assignment.StartDate <= DateTime.Today.AddDays(7));
            Assert.True(assignment.EndDate == null || assignment.EndDate >= DateTime.Today);
        });
    }

    [Fact]
    public async Task GetAssignedEmployeesCountAsync_ValidShift_ReturnsCorrectCount()
    {
        // Act
        var result = await _repository.GetAssignedEmployeesCountAsync(1);

        // Assert
        Assert.Equal(2, result); // Two active assignments for shift 1
    }

    [Fact]
    public async Task GetAssignedEmployeesCountAsync_WithDate_ReturnsCorrectCount()
    {
        // Act
        var result = await _repository.GetAssignedEmployeesCountAsync(1, DateTime.Today);

        // Assert
        Assert.Equal(2, result); // Two assignments active on today's date
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}