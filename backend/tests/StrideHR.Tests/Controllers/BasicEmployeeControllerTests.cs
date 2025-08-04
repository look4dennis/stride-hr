using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Employee;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Controllers;

public class BasicEmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<ILogger<EmployeeController>> _mockLogger;
    private readonly EmployeeController _controller;

    public BasicEmployeeControllerTests()
    {
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _controller = new EmployeeController(_mockEmployeeService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllEmployees_ReturnsOkResult()
    {
        // Arrange
        var employees = new List<EmployeeDto>
        {
            new EmployeeDto { Id = 1, FirstName = "John", LastName = "Doe" },
            new EmployeeDto { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _mockEmployeeService
            .Setup(s => s.GetEmployeeDtosAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.GetAllEmployees();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<EmployeeDto>>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmployee_ExistingEmployee_ReturnsOkResult()
    {
        // Arrange
        var employeeId = 1;
        var employeeDto = new EmployeeDto
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            EmployeeId = "EMP001"
        };

        _mockEmployeeService
            .Setup(s => s.GetEmployeeDtoAsync(employeeId))
            .ReturnsAsync(employeeDto);

        // Act
        var result = await _controller.GetEmployee(employeeId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<EmployeeDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(employeeId);
        apiResponse.Data.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetEmployee_NonExistingEmployee_ReturnsNotFound()
    {
        // Arrange
        var employeeId = 999;
        _mockEmployeeService
            .Setup(s => s.GetEmployeeDtoAsync(employeeId))
            .ReturnsAsync((EmployeeDto?)null);

        // Act
        var result = await _controller.GetEmployee(employeeId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAllEmployees_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockEmployeeService
            .Setup(s => s.GetEmployeeDtosAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllEmployees();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var apiResponse = statusCodeResult.Value.Should().BeOfType<ApiResponse<IEnumerable<EmployeeDto>>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("error occurred");
    }
}