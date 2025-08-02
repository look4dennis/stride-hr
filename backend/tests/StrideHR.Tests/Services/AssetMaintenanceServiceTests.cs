using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Asset;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AssetMaintenanceServiceTests
{
    private readonly Mock<IAssetMaintenanceRepository> _mockMaintenanceRepository;
    private readonly Mock<IAssetRepository> _mockAssetRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AssetMaintenanceService _maintenanceService;

    public AssetMaintenanceServiceTests()
    {
        _mockMaintenanceRepository = new Mock<IAssetMaintenanceRepository>();
        _mockAssetRepository = new Mock<IAssetRepository>();
        _mockMapper = new Mock<IMapper>();

        _maintenanceService = new AssetMaintenanceService(
            _mockMaintenanceRepository.Object,
            _mockAssetRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task CreateMaintenanceAsync_ValidMaintenance_ReturnsMaintenanceDto()
    {
        // Arrange
        var createDto = new CreateAssetMaintenanceDto
        {
            AssetId = 1,
            Type = MaintenanceType.Preventive,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            Description = "Regular maintenance",
            RequestedBy = 1
        };

        var asset = new Asset
        {
            Id = 1,
            Name = "Test Laptop",
            IsDeleted = false
        };

        var maintenance = new AssetMaintenance
        {
            Id = 1,
            AssetId = createDto.AssetId,
            Type = createDto.Type,
            Status = MaintenanceStatus.Scheduled,
            ScheduledDate = createDto.ScheduledDate,
            Description = createDto.Description,
            RequestedBy = createDto.RequestedBy
        };

        var maintenanceDto = new AssetMaintenanceDto
        {
            Id = 1,
            AssetId = maintenance.AssetId,
            Type = maintenance.Type,
            Status = maintenance.Status,
            Description = maintenance.Description
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(createDto.AssetId))
            .ReturnsAsync(asset);
        _mockMapper.Setup(m => m.Map<AssetMaintenance>(createDto))
            .Returns(maintenance);
        _mockMaintenanceRepository.Setup(r => r.AddAsync(It.IsAny<AssetMaintenance>()))
            .ReturnsAsync(maintenance);
        _mockMaintenanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenance.Id, It.IsAny<System.Linq.Expressions.Expression<Func<AssetMaintenance, object>>[]>()))
            .ReturnsAsync(maintenance);
        _mockMapper.Setup(m => m.Map<AssetMaintenanceDto>(maintenance))
            .Returns(maintenanceDto);

        // Act
        var result = await _maintenanceService.CreateMaintenanceAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(maintenanceDto.Id, result.Id);
        Assert.Equal(maintenanceDto.AssetId, result.AssetId);
        Assert.Equal(maintenanceDto.Type, result.Type);
        _mockMaintenanceRepository.Verify(r => r.AddAsync(It.IsAny<AssetMaintenance>()), Times.Once);
        _mockMaintenanceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateMaintenanceAsync_NonExistentAsset_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateAssetMaintenanceDto
        {
            AssetId = 999,
            Type = MaintenanceType.Preventive,
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            Description = "Regular maintenance",
            RequestedBy = 1
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(createDto.AssetId))
            .ReturnsAsync((Asset?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _maintenanceService.CreateMaintenanceAsync(createDto));
        
        Assert.Contains($"Asset with ID {createDto.AssetId} not found", exception.Message);
        _mockMaintenanceRepository.Verify(r => r.AddAsync(It.IsAny<AssetMaintenance>()), Times.Never);
    }

    [Fact]
    public async Task StartMaintenanceAsync_ScheduledMaintenance_UpdatesStatusAndAsset()
    {
        // Arrange
        var maintenanceId = 1;
        var technicianId = 2;

        var maintenance = new AssetMaintenance
        {
            Id = maintenanceId,
            AssetId = 1,
            Status = MaintenanceStatus.Scheduled,
            IsDeleted = false
        };

        var asset = new Asset
        {
            Id = 1,
            Status = AssetStatus.Available
        };

        var maintenanceDto = new AssetMaintenanceDto
        {
            Id = maintenanceId,
            Status = MaintenanceStatus.InProgress,
            TechnicianName = "John Doe"
        };

        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(maintenance);
        _mockAssetRepository.Setup(r => r.GetByIdAsync(maintenance.AssetId))
            .ReturnsAsync(asset);
        _mockMaintenanceRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetMaintenance>()))
            .Returns(Task.CompletedTask);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockMaintenanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetMaintenance, object>>[]>()))
            .ReturnsAsync(maintenance);
        _mockMapper.Setup(m => m.Map<AssetMaintenanceDto>(maintenance))
            .Returns(maintenanceDto);

        // Act
        var result = await _maintenanceService.StartMaintenanceAsync(maintenanceId, technicianId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MaintenanceStatus.InProgress, maintenance.Status);
        Assert.Equal(technicianId, maintenance.TechnicianId);
        Assert.NotNull(maintenance.StartDate);
        Assert.Equal(AssetStatus.InMaintenance, asset.Status);
        _mockMaintenanceRepository.Verify(r => r.UpdateAsync(maintenance), Times.Once);
        _mockAssetRepository.Verify(r => r.UpdateAsync(asset), Times.Once);
    }

    [Fact]
    public async Task StartMaintenanceAsync_NonScheduledMaintenance_ThrowsInvalidOperationException()
    {
        // Arrange
        var maintenanceId = 1;
        var technicianId = 2;

        var maintenance = new AssetMaintenance
        {
            Id = maintenanceId,
            Status = MaintenanceStatus.InProgress, // Already in progress
            IsDeleted = false
        };

        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(maintenance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _maintenanceService.StartMaintenanceAsync(maintenanceId, technicianId));
        
        Assert.Contains("Only scheduled maintenance can be started", exception.Message);
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_InProgressMaintenance_UpdatesStatusAndAsset()
    {
        // Arrange
        var maintenanceId = 1;
        var completionDto = new UpdateAssetMaintenanceDto
        {
            WorkPerformed = "Replaced battery",
            PartsReplaced = "Battery",
            Notes = "Maintenance completed successfully",
            NextMaintenanceDate = DateTime.UtcNow.AddMonths(6)
        };

        var maintenance = new AssetMaintenance
        {
            Id = maintenanceId,
            AssetId = 1,
            Status = MaintenanceStatus.InProgress,
            IsDeleted = false
        };

        var asset = new Asset
        {
            Id = 1,
            Status = AssetStatus.InMaintenance
        };

        var maintenanceDto = new AssetMaintenanceDto
        {
            Id = maintenanceId,
            Status = MaintenanceStatus.Completed,
            WorkPerformed = completionDto.WorkPerformed
        };

        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(maintenance);
        _mockAssetRepository.Setup(r => r.GetByIdAsync(maintenance.AssetId))
            .ReturnsAsync(asset);
        _mockMaintenanceRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetMaintenance>()))
            .Returns(Task.CompletedTask);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockMaintenanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetMaintenance, object>>[]>()))
            .ReturnsAsync(maintenance);
        _mockMapper.Setup(m => m.Map<AssetMaintenanceDto>(maintenance))
            .Returns(maintenanceDto);

        // Act
        var result = await _maintenanceService.CompleteMaintenanceAsync(maintenanceId, completionDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(MaintenanceStatus.Completed, maintenance.Status);
        Assert.Equal(completionDto.WorkPerformed, maintenance.WorkPerformed);
        Assert.Equal(completionDto.PartsReplaced, maintenance.PartsReplaced);
        Assert.NotNull(maintenance.CompletedDate);
        Assert.Equal(AssetStatus.Available, asset.Status);
        Assert.NotNull(asset.LastMaintenanceDate);
        Assert.Equal(completionDto.NextMaintenanceDate, asset.NextMaintenanceDate);
        _mockMaintenanceRepository.Verify(r => r.UpdateAsync(maintenance), Times.Once);
        _mockAssetRepository.Verify(r => r.UpdateAsync(asset), Times.Once);
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_ScheduledMaintenance_SoftDeletesRecord()
    {
        // Arrange
        var maintenanceId = 1;
        var maintenance = new AssetMaintenance
        {
            Id = maintenanceId,
            Status = MaintenanceStatus.Scheduled,
            IsDeleted = false
        };

        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(maintenance);
        _mockMaintenanceRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetMaintenance>()))
            .Returns(Task.CompletedTask);
        _mockMaintenanceRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _maintenanceService.DeleteMaintenanceAsync(maintenanceId);

        // Assert
        Assert.True(result);
        Assert.True(maintenance.IsDeleted);
        Assert.Equal(MaintenanceStatus.Cancelled, maintenance.Status);
        Assert.NotNull(maintenance.DeletedAt);
        _mockMaintenanceRepository.Verify(r => r.UpdateAsync(maintenance), Times.Once);
        _mockMaintenanceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_NonScheduledMaintenance_ThrowsInvalidOperationException()
    {
        // Arrange
        var maintenanceId = 1;
        var maintenance = new AssetMaintenance
        {
            Id = maintenanceId,
            Status = MaintenanceStatus.Completed, // Cannot delete completed maintenance
            IsDeleted = false
        };

        _mockMaintenanceRepository.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(maintenance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _maintenanceService.DeleteMaintenanceAsync(maintenanceId));
        
        Assert.Contains("Only scheduled maintenance can be deleted", exception.Message);
        _mockMaintenanceRepository.Verify(r => r.UpdateAsync(It.IsAny<AssetMaintenance>()), Times.Never);
    }
}