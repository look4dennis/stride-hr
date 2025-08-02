using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Asset;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AssetServiceTests
{
    private readonly Mock<IAssetRepository> _mockAssetRepository;
    private readonly Mock<IAssetAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AssetService _assetService;

    public AssetServiceTests()
    {
        _mockAssetRepository = new Mock<IAssetRepository>();
        _mockAssignmentRepository = new Mock<IAssetAssignmentRepository>();
        _mockMapper = new Mock<IMapper>();

        _assetService = new AssetService(
            _mockAssetRepository.Object,
            _mockAssignmentRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task CreateAssetAsync_ValidAsset_ReturnsAssetDto()
    {
        // Arrange
        var createAssetDto = new CreateAssetDto
        {
            AssetTag = "TEST-001",
            Name = "Test Laptop",
            Type = AssetType.Laptop,
            BranchId = 1,
            PurchasePrice = 1000m,
            PurchaseDate = DateTime.UtcNow.AddDays(-30)
        };

        var asset = new Asset
        {
            Id = 1,
            AssetTag = createAssetDto.AssetTag,
            Name = createAssetDto.Name,
            Type = createAssetDto.Type,
            BranchId = createAssetDto.BranchId,
            PurchasePrice = createAssetDto.PurchasePrice,
            PurchaseDate = createAssetDto.PurchaseDate
        };

        var assetDto = new AssetDto
        {
            Id = 1,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Type = asset.Type,
            BranchId = asset.BranchId
        };

        _mockAssetRepository.Setup(r => r.IsAssetTagUniqueAsync(createAssetDto.AssetTag, null))
            .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<Asset>(createAssetDto))
            .Returns(asset);
        _mockAssetRepository.Setup(r => r.AddAsync(It.IsAny<Asset>()))
            .ReturnsAsync(asset);
        _mockAssetRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockAssetRepository.Setup(r => r.GetAssetWithDetailsAsync(asset.Id))
            .ReturnsAsync(asset);
        _mockMapper.Setup(m => m.Map<AssetDto>(asset))
            .Returns(assetDto);

        // Act
        var result = await _assetService.CreateAssetAsync(createAssetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assetDto.AssetTag, result.AssetTag);
        Assert.Equal(assetDto.Name, result.Name);
        _mockAssetRepository.Verify(r => r.IsAssetTagUniqueAsync(createAssetDto.AssetTag, null), Times.Once);
        _mockAssetRepository.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Once);
        _mockAssetRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAssetAsync_DuplicateAssetTag_ThrowsInvalidOperationException()
    {
        // Arrange
        var createAssetDto = new CreateAssetDto
        {
            AssetTag = "DUPLICATE-001",
            Name = "Test Laptop",
            Type = AssetType.Laptop,
            BranchId = 1
        };

        _mockAssetRepository.Setup(r => r.IsAssetTagUniqueAsync(createAssetDto.AssetTag, null))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assetService.CreateAssetAsync(createAssetDto));
        
        Assert.Contains("Asset tag 'DUPLICATE-001' already exists", exception.Message);
        _mockAssetRepository.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Never);
    }

    [Fact]
    public async Task GetAssetByIdAsync_ExistingAsset_ReturnsAssetDto()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            AssetTag = "TEST-001",
            Name = "Test Laptop",
            Type = AssetType.Laptop,
            Status = AssetStatus.Available
        };

        var assetDto = new AssetDto
        {
            Id = assetId,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            Type = asset.Type,
            Status = asset.Status
        };

        _mockAssetRepository.Setup(r => r.GetAssetWithDetailsAsync(assetId))
            .ReturnsAsync(asset);
        _mockMapper.Setup(m => m.Map<AssetDto>(asset))
            .Returns(assetDto);

        // Act
        var result = await _assetService.GetAssetByIdAsync(assetId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assetDto.Id, result.Id);
        Assert.Equal(assetDto.AssetTag, result.AssetTag);
        _mockAssetRepository.Verify(r => r.GetAssetWithDetailsAsync(assetId), Times.Once);
    }

    [Fact]
    public async Task GetAssetByIdAsync_NonExistingAsset_ReturnsNull()
    {
        // Arrange
        var assetId = 999;
        _mockAssetRepository.Setup(r => r.GetAssetWithDetailsAsync(assetId))
            .ReturnsAsync((Asset?)null);

        // Act
        var result = await _assetService.GetAssetByIdAsync(assetId);

        // Assert
        Assert.Null(result);
        _mockAssetRepository.Verify(r => r.GetAssetWithDetailsAsync(assetId), Times.Once);
    }

    [Fact]
    public async Task DeleteAssetAsync_AssetWithActiveAssignment_ThrowsInvalidOperationException()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            AssetTag = "TEST-001",
            Name = "Test Laptop",
            IsDeleted = false
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.HasActiveAssignmentAsync(assetId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assetService.DeleteAssetAsync(assetId));
        
        Assert.Contains("Cannot delete asset that has active assignments", exception.Message);
        _mockAssetRepository.Verify(r => r.UpdateAsync(It.IsAny<Asset>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAssetAsync_ValidAsset_SoftDeletesAsset()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            AssetTag = "TEST-001",
            Name = "Test Laptop",
            IsDeleted = false,
            Status = AssetStatus.Available
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.HasActiveAssignmentAsync(assetId))
            .ReturnsAsync(false);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockAssetRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _assetService.DeleteAssetAsync(assetId);

        // Assert
        Assert.True(result);
        Assert.True(asset.IsDeleted);
        Assert.Equal(AssetStatus.Retired, asset.Status);
        Assert.NotNull(asset.DeletedAt);
        _mockAssetRepository.Verify(r => r.UpdateAsync(asset), Times.Once);
        _mockAssetRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateCurrentValueAsync_AssetWithDepreciation_ReturnsDepreciatedValue()
    {
        // Arrange
        var assetId = 1;
        var purchasePrice = 1000m;
        var depreciationRate = 20m; // 20% per year
        var purchaseDate = DateTime.UtcNow.AddYears(-1); // 1 year ago

        var asset = new Asset
        {
            Id = assetId,
            PurchasePrice = purchasePrice,
            DepreciationRate = depreciationRate,
            PurchaseDate = purchaseDate
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        // Act
        var result = await _assetService.CalculateCurrentValueAsync(assetId);

        // Assert
        // After 1 year with 20% depreciation rate, value should be approximately 800
        Assert.True(Math.Abs(result - 800m) < 1m, $"Expected approximately 800, but got {result}");
        _mockAssetRepository.Verify(r => r.GetByIdAsync(assetId), Times.Once);
    }

    [Fact]
    public async Task CalculateCurrentValueAsync_AssetWithoutDepreciation_ReturnsPurchasePrice()
    {
        // Arrange
        var assetId = 1;
        var purchasePrice = 1000m;

        var asset = new Asset
        {
            Id = assetId,
            PurchasePrice = purchasePrice,
            DepreciationRate = null,
            PurchaseDate = DateTime.UtcNow.AddYears(-1)
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        // Act
        var result = await _assetService.CalculateCurrentValueAsync(assetId);

        // Assert
        Assert.Equal(purchasePrice, result);
        _mockAssetRepository.Verify(r => r.GetByIdAsync(assetId), Times.Once);
    }

    [Fact]
    public async Task IsAssetTagUniqueAsync_UniqueTag_ReturnsTrue()
    {
        // Arrange
        var assetTag = "UNIQUE-001";
        _mockAssetRepository.Setup(r => r.IsAssetTagUniqueAsync(assetTag, null))
            .ReturnsAsync(true);

        // Act
        var result = await _assetService.IsAssetTagUniqueAsync(assetTag);

        // Assert
        Assert.True(result);
        _mockAssetRepository.Verify(r => r.IsAssetTagUniqueAsync(assetTag, null), Times.Once);
    }

    [Fact]
    public async Task IsAssetTagUniqueAsync_DuplicateTag_ReturnsFalse()
    {
        // Arrange
        var assetTag = "DUPLICATE-001";
        _mockAssetRepository.Setup(r => r.IsAssetTagUniqueAsync(assetTag, null))
            .ReturnsAsync(false);

        // Act
        var result = await _assetService.IsAssetTagUniqueAsync(assetTag);

        // Assert
        Assert.False(result);
        _mockAssetRepository.Verify(r => r.IsAssetTagUniqueAsync(assetTag, null), Times.Once);
    }
}