using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class ReportBuilderServiceTests
{
    private readonly Mock<IReportRepository> _mockReportRepository;
    private readonly Mock<IReportExecutionRepository> _mockExecutionRepository;
    private readonly StrideHRDbContext _context;
    private readonly Mock<ILogger<ReportBuilderService>> _mockLogger;
    private readonly ReportBuilderService _service;

    public ReportBuilderServiceTests()
    {
        _mockReportRepository = new Mock<IReportRepository>();
        _mockExecutionRepository = new Mock<IReportExecutionRepository>();
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new StrideHRDbContext(options);
        
        _mockLogger = new Mock<ILogger<ReportBuilderService>>();
        
        _service = new ReportBuilderService(
            _mockReportRepository.Object,
            _mockExecutionRepository.Object,
            _context,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateReportAsync_ValidData_ReturnsReport()
    {
        // Arrange
        var configuration = new ReportBuilderConfiguration
        {
            DataSource = "employees",
            Columns = new List<ReportColumn>
            {
                new() { Name = "Id", DisplayName = "Employee ID", DataType = "int", IsVisible = true, Order = 0 },
                new() { Name = "Name", DisplayName = "Full Name", DataType = "string", IsVisible = true, Order = 1 }
            },
            Filters = new List<ReportFilter>(),
            Groupings = new List<ReportGrouping>(),
            Sortings = new List<ReportSorting>()
        };

        var expectedReport = new Report
        {
            Id = 1,
            Name = "Test Report",
            Description = "Test Description",
            Type = ReportType.Table,
            CreatedBy = 1,
            Status = ReportStatus.Active
        };

        _mockReportRepository.Setup(r => r.AddAsync(It.IsAny<Report>()))
            .ReturnsAsync(expectedReport);
        _mockReportRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateReportAsync(
            "Test Report",
            "Test Description",
            ReportType.Table,
            configuration,
            1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Report", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(ReportType.Table, result.Type);
        Assert.Equal(1, result.CreatedBy);
        Assert.Equal(ReportStatus.Active, result.Status);

        _mockReportRepository.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Once);
        _mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReportAsync_ValidData_ReturnsUpdatedReport()
    {
        // Arrange
        var existingReport = new Report
        {
            Id = 1,
            Name = "Original Report",
            Description = "Original Description",
            CreatedBy = 1,
            Status = ReportStatus.Active
        };

        var configuration = new ReportBuilderConfiguration
        {
            DataSource = "employees",
            Columns = new List<ReportColumn>(),
            Filters = new List<ReportFilter>(),
            Groupings = new List<ReportGrouping>(),
            Sortings = new List<ReportSorting>()
        };

        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingReport);
        _mockReportRepository.Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);
        _mockReportRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateReportAsync(
            1,
            "Updated Report",
            "Updated Description",
            configuration,
            1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Report", result.Name);
        Assert.Equal("Updated Description", result.Description);

        _mockReportRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockReportRepository.Verify(r => r.UpdateAsync(It.IsAny<Report>()), Times.Once);
        _mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReportAsync_ReportNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Report?)null);

        var configuration = new ReportBuilderConfiguration();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateReportAsync(1, "Updated Report", "Updated Description", configuration, 1));
    }

    [Fact]
    public async Task UpdateReportAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var existingReport = new Report
        {
            Id = 1,
            Name = "Original Report",
            CreatedBy = 1
        };

        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingReport);

        var configuration = new ReportBuilderConfiguration();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateReportAsync(1, "Updated Report", "Updated Description", configuration, 2));
    }

    [Fact]
    public async Task DeleteReportAsync_ValidReport_ReturnsTrue()
    {
        // Arrange
        var existingReport = new Report
        {
            Id = 1,
            Name = "Test Report",
            CreatedBy = 1
        };

        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingReport);
        _mockReportRepository.Setup(r => r.DeleteAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);
        _mockReportRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteReportAsync(1, 1);

        // Assert
        Assert.True(result);
        _mockReportRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockReportRepository.Verify(r => r.DeleteAsync(It.IsAny<Report>()), Times.Once);
        _mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteReportAsync_ReportNotFound_ReturnsFalse()
    {
        // Arrange
        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Report?)null);

        // Act
        var result = await _service.DeleteReportAsync(1, 1);

        // Assert
        Assert.False(result);
        _mockReportRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockReportRepository.Verify(r => r.DeleteAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task GetUserReportsAsync_ValidUser_ReturnsReports()
    {
        // Arrange
        var expectedReports = new List<Report>
        {
            new() { Id = 1, Name = "Report 1", CreatedBy = 1 },
            new() { Id = 2, Name = "Report 2", CreatedBy = 1 }
        };

        _mockReportRepository.Setup(r => r.GetReportsByUserAsync(1, null))
            .ReturnsAsync(expectedReports);

        // Act
        var result = await _service.GetUserReportsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockReportRepository.Verify(r => r.GetReportsByUserAsync(1, null), Times.Once);
    }

    [Fact]
    public async Task GetAvailableDataSourcesAsync_ReturnsDataSources()
    {
        // Act
        var result = await _service.GetAvailableDataSourcesAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        var dataSources = result.ToList();
        Assert.Contains(dataSources, ds => ds.Name == "employees");
        Assert.Contains(dataSources, ds => ds.Name == "attendance");
        Assert.Contains(dataSources, ds => ds.Name == "payroll");
    }

    [Fact]
    public async Task GetDataSourceSchemaAsync_ValidDataSource_ReturnsSchema()
    {
        // Act
        var result = await _service.GetDataSourceSchemaAsync("employees", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("employees", result.Name);
        Assert.Equal("Employees", result.DisplayName);
        Assert.NotEmpty(result.Columns);
    }

    [Fact]
    public async Task GetDataSourceSchemaAsync_InvalidDataSource_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetDataSourceSchemaAsync("invalid_source", 1));
    }

    [Fact]
    public async Task ShareReportAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var report = new Report
        {
            Id = 1,
            Name = "Test Report",
            CreatedBy = 1
        };

        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(report);

        // Act
        var result = await _service.ShareReportAsync(1, 2, ReportPermission.View, 1);

        // Assert
        Assert.True(result);
        _mockReportRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task ShareReportAsync_ReportNotFound_ReturnsFalse()
    {
        // Arrange
        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Report?)null);

        // Act
        var result = await _service.ShareReportAsync(1, 2, ReportPermission.View, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShareReportAsync_UnauthorizedUser_ReturnsFalse()
    {
        // Arrange
        var report = new Report
        {
            Id = 1,
            Name = "Test Report",
            CreatedBy = 1
        };

        _mockReportRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(report);

        // Act
        var result = await _service.ShareReportAsync(1, 2, ReportPermission.View, 2);

        // Assert
        Assert.False(result);
    }
}