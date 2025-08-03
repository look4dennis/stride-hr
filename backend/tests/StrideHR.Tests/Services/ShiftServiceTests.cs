using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Shift;
using StrideHR.Infrastructure.Mapping;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ShiftServiceTests
{
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly Mock<IShiftAssignmentRepository> _mockShiftAssignmentRepository;
    private readonly Mock<ILogger<ShiftService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly ShiftService _shiftService;

    public ShiftServiceTests()
    {
        _mockShiftRepository = new Mock<IShiftRepository>();
        _mockShiftAssignmentRepository = new Mock<IShiftAssignmentRepository>();
        _mockLogger = new Mock<ILogger<ShiftService>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ShiftMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _shiftService = new ShiftService(
            _mockShiftRepository.Object,
            _mockShiftAssignmentRepository.Object,
            _mapper,
            _mockLogger.Object);
    }

    #region Shift Management Tests

    [Fact]
    public async Task CreateShiftAsync_ValidData_ReturnsShiftDto()
    {
        // Arrange
        var createShiftDto = new CreateShiftDto
        {
            OrganizationId = 1,
            BranchId = 1,
            Name = "Day Shift",
            Description = "Regular day shift",
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            Type = ShiftType.Day,
            WorkingDays = new List<int> { 1, 2, 3, 4, 5 }
        };

        _mockShiftRepository.Setup(r => r.IsShiftNameUniqueAsync(1, "Day Shift", null))
            .ReturnsAsync(true);
        _mockShiftRepository.Setup(r => r.AddAsync(It.IsAny<Shift>()))
            .ReturnsAsync((Shift s) => { s.Id = 1; return s; });
        _mockShiftRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _shiftService.CreateShiftAsync(createShiftDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Day Shift", result.Name);
        Assert.Equal(ShiftType.Day, result.Type);
        Assert.Equal(TimeSpan.FromHours(9), result.StartTime);
        Assert.Equal(TimeSpan.FromHours(17), result.EndTime);
        _mockShiftRepository.Verify(r => r.AddAsync(It.IsAny<Shift>()), Times.Once);
        _mockShiftRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateShiftAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var createShiftDto = new CreateShiftDto
        {
            OrganizationId = 1,
            Name = "Existing Shift"
        };

        _mockShiftRepository.Setup(r => r.IsShiftNameUniqueAsync(1, "Existing Shift", null))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _shiftService.CreateShiftAsync(createShiftDto));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateShiftAsync_ValidData_ReturnsUpdatedShiftDto()
    {
        // Arrange
        var shiftId = 1;
        var existingShift = new Shift
        {
            Id = shiftId,
            OrganizationId = 1,
            Name = "Old Name",
            StartTime = TimeSpan.FromHours(8),
            EndTime = TimeSpan.FromHours(16)
        };

        var updateShiftDto = new UpdateShiftDto
        {
            Name = "Updated Shift",
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(existingShift);
        _mockShiftRepository.Setup(r => r.IsShiftNameUniqueAsync(1, "Updated Shift", shiftId))
            .ReturnsAsync(true);
        _mockShiftRepository.Setup(r => r.UpdateAsync(It.IsAny<Shift>()))
            .Returns(Task.CompletedTask);
        _mockShiftRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _shiftService.UpdateShiftAsync(shiftId, updateShiftDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Shift", result.Name);
        _mockShiftRepository.Verify(r => r.UpdateAsync(It.IsAny<Shift>()), Times.Once);
        _mockShiftRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateShiftAsync_NonExistentShift_ThrowsArgumentException()
    {
        // Arrange
        var shiftId = 999;
        var updateShiftDto = new UpdateShiftDto { Name = "Updated Shift" };

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync((Shift?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _shiftService.UpdateShiftAsync(shiftId, updateShiftDto));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task DeleteShiftAsync_ShiftWithoutActiveAssignments_ReturnsTrue()
    {
        // Arrange
        var shiftId = 1;
        var shift = new Shift
        {
            Id = shiftId,
            Name = "Test Shift",
            ShiftAssignments = new List<ShiftAssignment>
            {
                new() { IsActive = false }
            }
        };

        _mockShiftRepository.Setup(r => r.GetShiftWithAssignmentsAsync(shiftId))
            .ReturnsAsync(shift);
        _mockShiftRepository.Setup(r => r.DeleteAsync(shift))
            .Returns(Task.CompletedTask);
        _mockShiftRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _shiftService.DeleteShiftAsync(shiftId);

        // Assert
        Assert.True(result);
        _mockShiftRepository.Verify(r => r.DeleteAsync(shift), Times.Once);
        _mockShiftRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteShiftAsync_ShiftWithActiveAssignments_ThrowsInvalidOperationException()
    {
        // Arrange
        var shiftId = 1;
        var shift = new Shift
        {
            Id = shiftId,
            Name = "Test Shift",
            ShiftAssignments = new List<ShiftAssignment>
            {
                new() { IsActive = true }
            }
        };

        _mockShiftRepository.Setup(r => r.GetShiftWithAssignmentsAsync(shiftId))
            .ReturnsAsync(shift);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _shiftService.DeleteShiftAsync(shiftId));
        Assert.Contains("active assignments", exception.Message);
    }

    [Fact]
    public async Task GetShiftByIdAsync_ExistingShift_ReturnsShiftDto()
    {
        // Arrange
        var shiftId = 1;
        var shift = new Shift
        {
            Id = shiftId,
            Name = "Test Shift",
            Type = ShiftType.Day,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            WorkingDays = "[1,2,3,4,5]",
            ShiftAssignments = new List<ShiftAssignment>()
        };

        _mockShiftRepository.Setup(r => r.GetShiftWithAssignmentsAsync(shiftId))
            .ReturnsAsync(shift);

        // Act
        var result = await _shiftService.GetShiftByIdAsync(shiftId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Shift", result.Name);
        Assert.Equal(ShiftType.Day, result.Type);
    }

    [Fact]
    public async Task GetShiftByIdAsync_NonExistentShift_ReturnsNull()
    {
        // Arrange
        var shiftId = 999;
        _mockShiftRepository.Setup(r => r.GetShiftWithAssignmentsAsync(shiftId))
            .ReturnsAsync((Shift?)null);

        // Act
        var result = await _shiftService.GetShiftByIdAsync(shiftId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Shift Assignment Tests

    [Fact]
    public async Task AssignEmployeeToShiftAsync_ValidAssignment_ReturnsShiftAssignmentDto()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        var createdAssignment = new ShiftAssignment
        {
            Id = 1,
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            IsActive = true,
            Employee = new Employee { Id = 1, FirstName = "John", LastName = "Doe", EmployeeId = "EMP001" },
            Shift = new Shift { Id = 1, Name = "Day Shift", StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) }
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(1, 1, DateTime.Today, DateTime.Today.AddDays(30)))
            .ReturnsAsync(new List<ShiftAssignment>());
        _mockShiftAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<ShiftAssignment>()))
            .ReturnsAsync((ShiftAssignment sa) => { sa.Id = 1; return sa; });
        _mockShiftAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, object>>[]>()))
            .ReturnsAsync(createdAssignment);

        // Act
        var result = await _shiftService.AssignEmployeeToShiftAsync(assignmentDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal(1, result.ShiftId);
        Assert.Equal("John Doe", result.EmployeeName);
        _mockShiftAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<ShiftAssignment>()), Times.Once);
        _mockShiftAssignmentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignEmployeeToShiftAsync_ConflictingAssignment_ThrowsInvalidOperationException()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        var conflictingAssignment = new ShiftAssignment
        {
            Id = 2,
            EmployeeId = 1,
            ShiftId = 2,
            IsActive = true,
            Employee = new Employee { FirstName = "John", LastName = "Doe" },
            Shift = new Shift { Name = "Night Shift" }
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(1, 1, DateTime.Today, DateTime.Today.AddDays(30)))
            .ReturnsAsync(new List<ShiftAssignment> { conflictingAssignment });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _shiftService.AssignEmployeeToShiftAsync(assignmentDto));
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task BulkAssignEmployeesToShiftAsync_ValidAssignments_ReturnsAssignmentList()
    {
        // Arrange
        var bulkAssignmentDto = new BulkShiftAssignmentDto
        {
            EmployeeIds = new List<int> { 1, 2, 3 },
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(It.IsAny<int>(), 1, DateTime.Today, DateTime.Today.AddDays(30)))
            .ReturnsAsync(new List<ShiftAssignment>());
        _mockShiftAssignmentRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ShiftAssignment>>()))
            .ReturnsAsync((IEnumerable<ShiftAssignment> assignments) => assignments);
        _mockShiftAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockShiftAssignmentRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<ShiftAssignment, object>>[]>()))
            .ReturnsAsync(new List<ShiftAssignment>
            {
                new() { Id = 1, EmployeeId = 1, ShiftId = 1, Employee = new Employee { FirstName = "John", LastName = "Doe" }, Shift = new Shift { Name = "Day Shift" } },
                new() { Id = 2, EmployeeId = 2, ShiftId = 1, Employee = new Employee { FirstName = "Jane", LastName = "Smith" }, Shift = new Shift { Name = "Day Shift" } },
                new() { Id = 3, EmployeeId = 3, ShiftId = 1, Employee = new Employee { FirstName = "Bob", LastName = "Johnson" }, Shift = new Shift { Name = "Day Shift" } }
            });

        // Act
        var result = await _shiftService.BulkAssignEmployeesToShiftAsync(bulkAssignmentDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        _mockShiftAssignmentRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ShiftAssignment>>()), Times.Once);
        _mockShiftAssignmentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveEmployeeFromShiftAsync_ExistingAssignment_ReturnsTrue()
    {
        // Arrange
        var assignmentId = 1;
        var assignment = new ShiftAssignment
        {
            Id = assignmentId,
            EmployeeId = 1,
            ShiftId = 1,
            IsActive = true
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);
        _mockShiftAssignmentRepository.Setup(r => r.UpdateAsync(It.IsAny<ShiftAssignment>()))
            .Returns(Task.CompletedTask);
        _mockShiftAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _shiftService.RemoveEmployeeFromShiftAsync(assignmentId);

        // Assert
        Assert.True(result);
        Assert.False(assignment.IsActive);
        Assert.True(assignment.EndDate < DateTime.Today);
        _mockShiftAssignmentRepository.Verify(r => r.UpdateAsync(assignment), Times.Once);
        _mockShiftAssignmentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region Conflict Detection Tests

    [Fact]
    public async Task DetectShiftConflictsAsync_NoConflicts_ReturnsEmptyList()
    {
        // Arrange
        var employeeId = 1;
        var shiftId = 1;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate))
            .ReturnsAsync(new List<ShiftAssignment>());

        // Act
        var result = await _shiftService.DetectShiftConflictsAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DetectShiftConflictsAsync_HasConflicts_ReturnsConflictList()
    {
        // Arrange
        var employeeId = 1;
        var shiftId = 1;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        var conflictingAssignment = new ShiftAssignment
        {
            Id = 2,
            EmployeeId = employeeId,
            ShiftId = 2,
            StartDate = DateTime.Today.AddDays(10),
            IsActive = true,
            Employee = new Employee { Id = employeeId, FirstName = "John", LastName = "Doe" },
            Shift = new Shift { Id = 2, Name = "Night Shift" }
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate))
            .ReturnsAsync(new List<ShiftAssignment> { conflictingAssignment });

        // Act
        var result = await _shiftService.DetectShiftConflictsAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var conflict = result.First();
        Assert.Equal(employeeId, conflict.EmployeeId);
        Assert.Equal("John Doe", conflict.EmployeeName);
        Assert.Equal("Overlap", conflict.ConflictType);
    }

    [Fact]
    public async Task ValidateShiftAssignmentAsync_NoConflicts_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var shiftId = 1;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate))
            .ReturnsAsync(new List<ShiftAssignment>());

        // Act
        var result = await _shiftService.ValidateShiftAssignmentAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateShiftAssignmentAsync_HasConflicts_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;
        var shiftId = 1;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        var conflictingAssignment = new ShiftAssignment
        {
            Id = 2,
            EmployeeId = employeeId,
            ShiftId = 2,
            IsActive = true,
            Employee = new Employee { FirstName = "John", LastName = "Doe" },
            Shift = new Shift { Name = "Night Shift" }
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate))
            .ReturnsAsync(new List<ShiftAssignment> { conflictingAssignment });

        // Act
        var result = await _shiftService.ValidateShiftAssignmentAsync(employeeId, shiftId, startDate, endDate);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Shift Coverage Tests

    [Fact]
    public async Task GetShiftCoverageAsync_ValidBranchAndDate_ReturnsShiftCoverageList()
    {
        // Arrange
        var branchId = 1;
        var date = DateTime.Today;

        var shifts = new List<Shift>
        {
            new() { Id = 1, Name = "Day Shift", StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17), IsActive = true },
            new() { Id = 2, Name = "Night Shift", StartTime = TimeSpan.FromHours(21), EndTime = TimeSpan.FromHours(5), IsActive = true }
        };

        var assignments = new List<ShiftAssignment>
        {
            new() { Id = 1, ShiftId = 1, EmployeeId = 1, Employee = new Employee { FirstName = "John", LastName = "Doe" }, Shift = shifts[0] },
            new() { Id = 2, ShiftId = 1, EmployeeId = 2, Employee = new Employee { FirstName = "Jane", LastName = "Smith" }, Shift = shifts[0] }
        };

        _mockShiftRepository.Setup(r => r.GetByBranchIdAsync(branchId))
            .ReturnsAsync(shifts);
        _mockShiftAssignmentRepository.Setup(r => r.GetAssignmentsByBranchAsync(branchId, date))
            .ReturnsAsync(assignments);

        // Act
        var result = await _shiftService.GetShiftCoverageAsync(branchId, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        
        var dayShiftCoverage = result.First(c => c.ShiftId == 1);
        Assert.Equal("Day Shift", dayShiftCoverage.ShiftName);
        Assert.Equal(2, dayShiftCoverage.AssignedEmployees);
        Assert.Equal(2, dayShiftCoverage.Assignments.Count);

        var nightShiftCoverage = result.First(c => c.ShiftId == 2);
        Assert.Equal("Night Shift", nightShiftCoverage.ShiftName);
        Assert.Equal(0, nightShiftCoverage.AssignedEmployees);
        Assert.Empty(nightShiftCoverage.Assignments);
    }

    #endregion

    #region Analytics Tests

    [Fact]
    public async Task GetShiftAnalyticsAsync_ValidData_ReturnsAnalyticsDictionary()
    {
        // Arrange
        var branchId = 1;
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;

        var assignments = new List<ShiftAssignment>
        {
            new() { Id = 1, EmployeeId = 1, ShiftId = 1, IsActive = true, StartDate = startDate, Shift = new Shift { Type = ShiftType.Day, Name = "Day Shift" } },
            new() { Id = 2, EmployeeId = 2, ShiftId = 1, IsActive = true, StartDate = startDate, Shift = new Shift { Type = ShiftType.Day, Name = "Day Shift" } },
            new() { Id = 3, EmployeeId = 3, ShiftId = 2, IsActive = false, StartDate = startDate, Shift = new Shift { Type = ShiftType.Night, Name = "Night Shift" } }
        };

        _mockShiftAssignmentRepository.Setup(r => r.GetAssignmentsByBranchAsync(branchId, null))
            .ReturnsAsync(assignments);

        // Act
        var result = await _shiftService.GetShiftAnalyticsAsync(branchId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result["TotalAssignments"]);
        Assert.Equal(2, result["ActiveAssignments"]);
        Assert.Equal(3, result["UniqueEmployees"]);
        
        var shiftTypeDistribution = (Dictionary<string, int>)result["ShiftTypeDistribution"];
        Assert.Equal(2, shiftTypeDistribution["Day"]);
        Assert.Equal(1, shiftTypeDistribution["Night"]);
    }

    #endregion
}