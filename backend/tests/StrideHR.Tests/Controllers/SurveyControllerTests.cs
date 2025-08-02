using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Controllers;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Survey;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Controllers;

public class SurveyControllerTests
{
    private readonly Mock<ISurveyService> _mockSurveyService;
    private readonly Mock<ISurveyResponseService> _mockResponseService;
    private readonly Mock<ISurveyAnalyticsService> _mockAnalyticsService;
    private readonly Mock<ILogger<SurveyController>> _mockLogger;
    private readonly SurveyController _controller;

    public SurveyControllerTests()
    {
        _mockSurveyService = new Mock<ISurveyService>();
        _mockResponseService = new Mock<ISurveyResponseService>();
        _mockAnalyticsService = new Mock<ISurveyAnalyticsService>();
        _mockLogger = new Mock<ILogger<SurveyController>>();

        _controller = new SurveyController(
            _mockSurveyService.Object,
            _mockResponseService.Object,
            _mockAnalyticsService.Object,
            _mockLogger.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim("EmployeeId", "1"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetSurveys_ReturnsOkWithSurveys()
    {
        // Arrange
        var surveys = new List<SurveyDto>
        {
            new SurveyDto { Id = 1, Title = "Survey 1" },
            new SurveyDto { Id = 2, Title = "Survey 2" }
        };

        _mockSurveyService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _controller.GetSurveys();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSurveys = Assert.IsAssignableFrom<IEnumerable<SurveyDto>>(okResult.Value);
        Assert.Equal(2, returnedSurveys.Count());
    }
}