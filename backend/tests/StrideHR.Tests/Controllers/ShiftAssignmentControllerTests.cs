using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ShiftAssignmentControllerTests
{
    private readonly Mock<IShiftService> _mockShiftService;
    private readonly Mock<ILogger<ShiftAssignmentController>> _mockLogger;
    private readonly ShiftAssignmentController _controller;

    public ShiftAssignmentControllerTests()
    {
        _mockShiftService = new Mock<IShiftService>();
        _mockLogger = new Mock<ILogger<ShiftAssignmentController>>();
        _controller = new ShiftAssignmentController(_mockShiftService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AssignEmployeeToShift_ValidData_ReturnsSuccessResult()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        var expectedAssignment = new ShiftAssignmentDto
        {
            Id = 1,
            EmployeeId = 1,
            ShiftId = 1,
            EmployeeName = "John Doe",
            ShiftName = "Day Shift"
        };

        _mockShiftService.Setup(s => s.AssignEmployeeToShiftAsync(assignmentDto))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.AssignEmployeeToShift(assignmentDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.AssignEmployeeToShiftAsync(assignmentDto), Times.Once);
    }

    [Fact]
    public async Task AssignEmployeeToShift_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today
        };

        _mockShiftService.Setup(s => s.AssignEmployeeToShiftAsync(assignmentDto))
            .ThrowsAsync(new InvalidOperationException("Shift assignment validation failed"));

        // Act
        var result = await _controller.AssignEmployeeToShift(assignmentDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task BulkAssignEmployeesToShift_ValidData_ReturnsSuccessResult()
    {
        // Arrange
        var bulkAssignmentDto = new BulkShiftAssignmentDto
        {
            EmployeeIds = new List<int> { 1, 2, 3 },
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        var expectedAssignments = new List<ShiftAssignmentDto>
        {
            new() { Id = 1, EmployeeId = 1, ShiftId = 1, EmployeeName = "John Doe" },
            new() { Id = 2, EmployeeId = 2, ShiftId = 1, EmployeeName = "Jane Smith" },
            new() { Id = 3, EmployeeId = 3, ShiftId = 1, EmployeeName = "Bob Johnson" }
        };

        _mockShiftService.Setup(s => s.BulkAssignEmployeesToShiftAsync(bulkAssignmentDto))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.BulkAssignEmployeesToShift(bulkAssignmentDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.BulkAssignEmployeesToShiftAsync(bulkAssignmentDto), Times.Once);
    }

    [Fact]
    public async Task UpdateShiftAssignment_ValidData_ReturnsSuccessResult()
    {
        // Arrange
        var assignmentId = 1;
        var updateDto = new UpdateShiftAssignmentDto
        {
            ShiftId = 2,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(60),
            IsActive = true
        };

        var expectedAssignment = new ShiftAssignmentDto
        {
            Id = assignmentId,
            ShiftId = 2,
            EmployeeName = "John Doe",
            ShiftName = "Night Shift"
        };

        _mockShiftService.Setup(s => s.UpdateShiftAssignmentAsync(assignmentId, updateDto))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.UpdateShiftAssignment(assignmentId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.UpdateShiftAssignmentAsync(assignmentId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateShiftAssignment_ArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var assignmentId = 999;
        var updateDto = new UpdateShiftAssignmentDto { ShiftId = 2 };

        _mockShiftService.Setup(s => s.UpdateShiftAssignmentAsync(assignmentId, updateDto))
            .ThrowsAsync(new ArgumentException("Shift assignment not found"));

        // Act
        var result = await _controller.UpdateShiftAssignment(assignmentId, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task RemoveEmployeeFromShift_ExistingAssignment_ReturnsSuccessResult()
    {
        // Arrange
        var assignmentId = 1;
        _mockShiftService.Setup(s => s.RemoveEmployeeFromShiftAsync(assignmentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveEmployeeFromShift(assignmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.RemoveEmployeeFromShiftAsync(assignmentId), Times.Once);
    }

    [Fact]
    public async Task RemoveEmployeeFromShift_NonExistentAssignment_ReturnsBadRequest()
    {
        // Arrange
        var assignmentId = 999;
        _mockShiftService.Setup(s => s.RemoveEmployeeFromShiftAsync(assignmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveEmployeeFromShift(assignmentId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetEmployeeShiftAssignments_ValidEmployeeId_ReturnsAssignments()
    {
        // Arrange
        var employeeId = 1;
        var expectedAssignments = new List<ShiftAssignmentDto>
        {
            new() { Id = 1, EmployeeId = employeeId, ShiftName = "Day Shift" },
            new() { Id = 2, EmployeeId = employeeId, ShiftName = "Night Shift" }
        };

        _mockShiftService.Setup(s => s.GetEmployeeShiftAssignmentsAsync(employeeId))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GetEmployeeShiftAssignments(employeeId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetEmployeeShiftAssignmentsAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task GetShiftAssignments_ValidShiftId_ReturnsAssignments()
    {
        // Arrange
        var shiftId = 1;
        var expectedAssignments = new List<ShiftAssignmentDto>
        {
            new() { Id = 1, ShiftId = shiftId, EmployeeName = "John Doe" },
            new() { Id = 2, ShiftId = shiftId, EmployeeName = "Jane Smith" }
        };

        _mockShiftService.Setup(s => s.GetShiftAssignmentsAsync(shiftId))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GetShiftAssignments(shiftId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftAssignmentsAsync(shiftId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentEmployeeShift_ValidData_ReturnsCurrentShift()
    {
        // Arrange
        var employeeId = 1;
        var date = DateTime.Today;
        var expectedAssignment = new ShiftAssignmentDto
        {
            Id = 1,
            EmployeeId = employeeId,
            ShiftName = "Day Shift",
            EmployeeName = "John Doe"
        };

        _mockShiftService.Setup(s => s.GetCurrentEmployeeShiftAsync(employeeId, date))
            .ReturnsAsync(expectedAssignment);

        // Act
        var result = await _controller.GetCurrentEmployeeShift(employeeId, date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetCurrentEmployeeShiftAsync(employeeId, date), Times.Once);
    }

    [Fact]
    public async Task GetUpcomingShiftAssignments_ValidData_ReturnsUpcomingAssignments()
    {
        // Arrange
        var employeeId = 1;
        var days = 7;
        var expectedAssignments = new List<ShiftAssignmentDto>
        {
            new() { Id = 1, EmployeeId = employeeId, ShiftName = "Day Shift", StartDate = DateTime.Today.AddDays(1) },
            new() { Id = 2, EmployeeId = employeeId, ShiftName = "Night Shift", StartDate = DateTime.Today.AddDays(3) }
        };

        _mockShiftService.Setup(s => s.GetUpcomingShiftAssignmentsAsync(employeeId, days))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GetUpcomingShiftAssignments(employeeId, days);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetUpcomingShiftAssignmentsAsync(employeeId, days), Times.Once);
    }

    [Fact]
    public async Task GetShiftCoverage_ValidData_ReturnsShiftCoverage()
    {
        // Arrange
        var branchId = 1;
        var date = DateTime.Today;
        var expectedCoverage = new List<ShiftCoverageDto>
        {
            new()
            {
                ShiftId = 1,
                ShiftName = "Day Shift",
                AssignedEmployees = 3,
                RequiredEmployees = 5,
                HasConflict = false
            }
        };

        _mockShiftService.Setup(s => s.GetShiftCoverageAsync(branchId, date))
            .ReturnsAsync(expectedCoverage);

        // Act
        var result = await _controller.GetShiftCoverage(branchId, date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftCoverageAsync(branchId, date), Times.Once);
    }

    [Fact]
    public async Task DetectShiftConflicts_ValidData_ReturnsConflicts()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        var expectedConflicts = new List<ShiftConflictDto>
        {
            new()
            {
                EmployeeId = 1,
                EmployeeName = "John Doe",
                ConflictType = "Overlap",
                Description = "Employee is already assigned to another shift during this period"
            }
        };

        _mockShiftService.Setup(s => s.DetectShiftConflictsAsync(
                assignmentDto.EmployeeId,
                assignmentDto.ShiftId,
                assignmentDto.StartDate,
                assignmentDto.EndDate))
            .ReturnsAsync(expectedConflicts);

        // Act
        var result = await _controller.DetectShiftConflicts(assignmentDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.DetectShiftConflictsAsync(
            assignmentDto.EmployeeId,
            assignmentDto.ShiftId,
            assignmentDto.StartDate,
            assignmentDto.EndDate), Times.Once);
    }

    [Fact]
    public async Task GetAllShiftConflicts_ValidData_ReturnsAllConflicts()
    {
        // Arrange
        var branchId = 1;
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);
        var expectedConflicts = new List<ShiftConflictDto>
        {
            new()
            {
                EmployeeId = 1,
                EmployeeName = "John Doe",
                ConflictType = "Overlap",
                ConflictDate = DateTime.Today.AddDays(5)
            }
        };

        _mockShiftService.Setup(s => s.GetAllShiftConflictsAsync(branchId, startDate, endDate))
            .ReturnsAsync(expectedConflicts);

        // Act
        var result = await _controller.GetAllShiftConflicts(branchId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetAllShiftConflictsAsync(branchId, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task ValidateShiftAssignment_ValidData_ReturnsValidationResult()
    {
        // Arrange
        var assignmentDto = new CreateShiftAssignmentDto
        {
            EmployeeId = 1,
            ShiftId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30)
        };

        _mockShiftService.Setup(s => s.ValidateShiftAssignmentAsync(
                assignmentDto.EmployeeId,
                assignmentDto.ShiftId,
                assignmentDto.StartDate,
                assignmentDto.EndDate))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateShiftAssignment(assignmentDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.ValidateShiftAssignmentAsync(
            assignmentDto.EmployeeId,
            assignmentDto.ShiftId,
            assignmentDto.StartDate,
            assignmentDto.EndDate), Times.Once);
    }

    [Fact]
    public async Task GenerateRotatingShiftSchedule_ValidData_ReturnsGeneratedSchedule()
    {
        // Arrange
        var scheduleDto = new GenerateRotatingScheduleDto
        {
            BranchId = 1,
            EmployeeIds = new List<int> { 1, 2, 3 },
            ShiftIds = new List<int> { 1, 2 },
            StartDate = DateTime.Today,
            Weeks = 4
        };

        var expectedAssignments = new List<ShiftAssignmentDto>
        {
            new() { Id = 1, EmployeeId = 1, ShiftId = 1, StartDate = DateTime.Today },
            new() { Id = 2, EmployeeId = 2, ShiftId = 2, StartDate = DateTime.Today },
            new() { Id = 3, EmployeeId = 3, ShiftId = 1, StartDate = DateTime.Today.AddDays(1) }
        };

        _mockShiftService.Setup(s => s.GenerateRotatingShiftScheduleAsync(
                scheduleDto.BranchId,
                scheduleDto.EmployeeIds,
                scheduleDto.ShiftIds,
                scheduleDto.StartDate,
                scheduleDto.Weeks))
            .ReturnsAsync(expectedAssignments);

        // Act
        var result = await _controller.GenerateRotatingShiftSchedule(scheduleDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GenerateRotatingShiftScheduleAsync(
            scheduleDto.BranchId,
            scheduleDto.EmployeeIds,
            scheduleDto.ShiftIds,
            scheduleDto.StartDate,
            scheduleDto.Weeks), Times.Once);
    }
}