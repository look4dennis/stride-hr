using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class ShiftControllerTests
{
    private readonly Mock<IShiftService> _mockShiftService;
    private readonly Mock<ILogger<ShiftController>> _mockLogger;
    private readonly ShiftController _controller;

    public ShiftControllerTests()
    {
        _mockShiftService = new Mock<IShiftService>();
        _mockLogger = new Mock<ILogger<ShiftController>>();
        _controller = new ShiftController(_mockShiftService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateShift_ValidData_ReturnsSuccessResult()
    {
        // Arrange
        var createShiftDto = new CreateShiftDto
        {
            OrganizationId = 1,
            Name = "Day Shift",
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            Type = ShiftType.Day
        };

        var expectedShift = new ShiftDto
        {
            Id = 1,
            Name = "Day Shift",
            Type = ShiftType.Day
        };

        _mockShiftService.Setup(s => s.CreateShiftAsync(createShiftDto))
            .ReturnsAsync(expectedShift);

        // Act
        var result = await _controller.CreateShift(createShiftDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.CreateShiftAsync(createShiftDto), Times.Once);
    }

    [Fact]
    public async Task CreateShift_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var createShiftDto = new CreateShiftDto
        {
            OrganizationId = 1,
            Name = "Duplicate Shift"
        };

        _mockShiftService.Setup(s => s.CreateShiftAsync(createShiftDto))
            .ThrowsAsync(new InvalidOperationException("Shift name already exists"));

        // Act
        var result = await _controller.CreateShift(createShiftDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateShift_ValidData_ReturnsSuccessResult()
    {
        // Arrange
        var shiftId = 1;
        var updateShiftDto = new UpdateShiftDto
        {
            Name = "Updated Shift",
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(18)
        };

        var expectedShift = new ShiftDto
        {
            Id = shiftId,
            Name = "Updated Shift"
        };

        _mockShiftService.Setup(s => s.UpdateShiftAsync(shiftId, updateShiftDto))
            .ReturnsAsync(expectedShift);

        // Act
        var result = await _controller.UpdateShift(shiftId, updateShiftDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.UpdateShiftAsync(shiftId, updateShiftDto), Times.Once);
    }

    [Fact]
    public async Task UpdateShift_ArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var shiftId = 999;
        var updateShiftDto = new UpdateShiftDto { Name = "Updated Shift" };

        _mockShiftService.Setup(s => s.UpdateShiftAsync(shiftId, updateShiftDto))
            .ThrowsAsync(new ArgumentException("Shift not found"));

        // Act
        var result = await _controller.UpdateShift(shiftId, updateShiftDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteShift_ExistingShift_ReturnsSuccessResult()
    {
        // Arrange
        var shiftId = 1;
        _mockShiftService.Setup(s => s.DeleteShiftAsync(shiftId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteShift(shiftId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.DeleteShiftAsync(shiftId), Times.Once);
    }

    [Fact]
    public async Task DeleteShift_NonExistentShift_ReturnsBadRequest()
    {
        // Arrange
        var shiftId = 999;
        _mockShiftService.Setup(s => s.DeleteShiftAsync(shiftId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteShift(shiftId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetShift_ExistingShift_ReturnsShift()
    {
        // Arrange
        var shiftId = 1;
        var expectedShift = new ShiftDto
        {
            Id = shiftId,
            Name = "Day Shift",
            Type = ShiftType.Day
        };

        _mockShiftService.Setup(s => s.GetShiftByIdAsync(shiftId))
            .ReturnsAsync(expectedShift);

        // Act
        var result = await _controller.GetShift(shiftId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftByIdAsync(shiftId), Times.Once);
    }

    [Fact]
    public async Task GetShift_NonExistentShift_ReturnsBadRequest()
    {
        // Arrange
        var shiftId = 999;
        _mockShiftService.Setup(s => s.GetShiftByIdAsync(shiftId))
            .ReturnsAsync((ShiftDto?)null);

        // Act
        var result = await _controller.GetShift(shiftId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetShiftsByOrganization_ValidOrganizationId_ReturnsShifts()
    {
        // Arrange
        var organizationId = 1;
        var expectedShifts = new List<ShiftDto>
        {
            new() { Id = 1, Name = "Day Shift", OrganizationId = organizationId },
            new() { Id = 2, Name = "Night Shift", OrganizationId = organizationId }
        };

        _mockShiftService.Setup(s => s.GetShiftsByOrganizationAsync(organizationId))
            .ReturnsAsync(expectedShifts);

        // Act
        var result = await _controller.GetShiftsByOrganization(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftsByOrganizationAsync(organizationId), Times.Once);
    }

    [Fact]
    public async Task GetShiftsByBranch_ValidBranchId_ReturnsShifts()
    {
        // Arrange
        var branchId = 1;
        var expectedShifts = new List<ShiftDto>
        {
            new() { Id = 1, Name = "Day Shift", BranchId = branchId },
            new() { Id = 2, Name = "Night Shift", BranchId = branchId }
        };

        _mockShiftService.Setup(s => s.GetShiftsByBranchAsync(branchId))
            .ReturnsAsync(expectedShifts);

        // Act
        var result = await _controller.GetShiftsByBranch(branchId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftsByBranchAsync(branchId), Times.Once);
    }

    [Fact]
    public async Task GetActiveShifts_ValidOrganizationId_ReturnsActiveShifts()
    {
        // Arrange
        var organizationId = 1;
        var expectedShifts = new List<ShiftDto>
        {
            new() { Id = 1, Name = "Day Shift", IsActive = true },
            new() { Id = 2, Name = "Night Shift", IsActive = true }
        };

        _mockShiftService.Setup(s => s.GetActiveShiftsAsync(organizationId))
            .ReturnsAsync(expectedShifts);

        // Act
        var result = await _controller.GetActiveShifts(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetActiveShiftsAsync(organizationId), Times.Once);
    }

    [Fact]
    public async Task SearchShifts_ValidCriteria_ReturnsSearchResults()
    {
        // Arrange
        var criteria = new ShiftSearchCriteria
        {
            OrganizationId = 1,
            SearchTerm = "Day",
            Page = 1,
            PageSize = 10
        };

        var expectedShifts = new List<ShiftDto>
        {
            new() { Id = 1, Name = "Day Shift" }
        };

        _mockShiftService.Setup(s => s.SearchShiftsAsync(criteria))
            .ReturnsAsync((expectedShifts, 1));

        // Act
        var result = await _controller.SearchShifts(criteria);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.SearchShiftsAsync(criteria), Times.Once);
    }

    [Fact]
    public async Task GetShiftTemplates_ValidOrganizationId_ReturnsTemplates()
    {
        // Arrange
        var organizationId = 1;
        var expectedTemplates = new List<ShiftTemplateDto>
        {
            new() { Id = 1, Name = "Standard Day Template", Type = ShiftType.Day },
            new() { Id = 2, Name = "Standard Night Template", Type = ShiftType.Night }
        };

        _mockShiftService.Setup(s => s.GetShiftTemplatesAsync(organizationId))
            .ReturnsAsync(expectedTemplates);

        // Act
        var result = await _controller.GetShiftTemplates(organizationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftTemplatesAsync(organizationId), Times.Once);
    }

    [Fact]
    public async Task CreateShiftFromTemplate_ValidData_ReturnsCreatedShift()
    {
        // Arrange
        var templateId = 1;
        var createShiftDto = new CreateShiftDto
        {
            OrganizationId = 1,
            Name = "New Shift from Template"
        };

        var expectedShift = new ShiftDto
        {
            Id = 1,
            Name = "New Shift from Template"
        };

        _mockShiftService.Setup(s => s.CreateShiftFromTemplateAsync(templateId, createShiftDto))
            .ReturnsAsync(expectedShift);

        // Act
        var result = await _controller.CreateShiftFromTemplate(templateId, createShiftDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.CreateShiftFromTemplateAsync(templateId, createShiftDto), Times.Once);
    }

    [Fact]
    public async Task GetShiftsByPattern_ValidData_ReturnsShiftsByPattern()
    {
        // Arrange
        var organizationId = 1;
        var shiftType = ShiftType.Day;
        var expectedShifts = new List<ShiftDto>
        {
            new() { Id = 1, Name = "Day Shift", Type = ShiftType.Day }
        };

        _mockShiftService.Setup(s => s.GetShiftsByPatternAsync(organizationId, shiftType))
            .ReturnsAsync(expectedShifts);

        // Act
        var result = await _controller.GetShiftsByPattern(organizationId, shiftType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftsByPatternAsync(organizationId, shiftType), Times.Once);
    }

    [Fact]
    public async Task GetShiftAnalytics_ValidData_ReturnsAnalytics()
    {
        // Arrange
        var branchId = 1;
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var expectedAnalytics = new Dictionary<string, object>
        {
            ["TotalAssignments"] = 10,
            ["ActiveAssignments"] = 8,
            ["UniqueEmployees"] = 5
        };

        _mockShiftService.Setup(s => s.GetShiftAnalyticsAsync(branchId, startDate, endDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetShiftAnalytics(branchId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        _mockShiftService.Verify(s => s.GetShiftAnalyticsAsync(branchId, startDate, endDate), Times.Once);
    }
}