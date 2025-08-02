using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Asset;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AssetAssignmentServiceTests
{
    private readonly Mock<IAssetAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IAssetRepository> _mockAssetRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AssetAssignmentService _assignmentService;

    public AssetAssignmentServiceTests()
    {
        _mockAssignmentRepository = new Mock<IAssetAssignmentRepository>();
        _mockAssetRepository = new Mock<IAssetRepository>();
        _mockMapper = new Mock<IMapper>();

        _assignmentService = new AssetAssignmentService(
            _mockAssignmentRepository.Object,
            _mockAssetRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task AssignAssetToEmployeeAsync_ValidAssignment_ReturnsAssignmentDto()
    {
        // Arrange
        var assignmentDto = new CreateAssetAssignmentDto
        {
            AssetId = 1,
            EmployeeId = 1,
            AssignedCondition = AssetCondition.Good,
            AssignedBy = 2
        };

        var asset = new Asset
        {
            Id = 1,
            Status = AssetStatus.Available,
            IsDeleted = false
        };

        var assignment = new AssetAssignment
        {
            Id = 1,
            AssetId = assignmentDto.AssetId,
            EmployeeId = assignmentDto.EmployeeId,
            AssignedCondition = assignmentDto.AssignedCondition,
            AssignedBy = assignmentDto.AssignedBy,
            IsActive = true
        };

        var assignmentDtoResult = new AssetAssignmentDto
        {
            Id = 1,
            AssetId = assignment.AssetId,
            EmployeeId = assignment.EmployeeId,
            IsActive = assignment.IsActive
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assignmentDto.AssetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.HasActiveAssignmentAsync(assignmentDto.AssetId))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<AssetAssignment>(assignmentDto))
            .Returns(assignment);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<AssetAssignment>()))
            .ReturnsAsync(assignment);
        _mockAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockAssignmentRepository.Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<System.Linq.Expressions.Expression<Func<AssetAssignment, object>>[]>()))
            .ReturnsAsync(assignment);
        _mockMapper.Setup(m => m.Map<AssetAssignmentDto>(assignment))
            .Returns(assignmentDtoResult);

        // Act
        var result = await _assignmentService.AssignAssetToEmployeeAsync(assignmentDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignmentDtoResult.Id, result.Id);
        Assert.Equal(assignmentDtoResult.AssetId, result.AssetId);
        Assert.Equal(assignmentDtoResult.EmployeeId, result.EmployeeId);
        _mockAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<AssetAssignment>()), Times.Once);
        _mockAssetRepository.Verify(r => r.UpdateAsync(It.IsAny<Asset>()), Times.Once);
    }

    [Fact]
    public async Task AssignAssetToEmployeeAsync_NoEmployeeId_ThrowsArgumentException()
    {
        // Arrange
        var assignmentDto = new CreateAssetAssignmentDto
        {
            AssetId = 1,
            EmployeeId = null, // Missing employee ID
            AssignedCondition = AssetCondition.Good,
            AssignedBy = 2
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.AssignAssetToEmployeeAsync(assignmentDto));
        
        Assert.Contains("Employee ID is required for employee assignment", exception.Message);
    }

    [Fact]
    public async Task AssignAssetToProjectAsync_NoProjectId_ThrowsArgumentException()
    {
        // Arrange
        var assignmentDto = new CreateAssetAssignmentDto
        {
            AssetId = 1,
            ProjectId = null, // Missing project ID
            AssignedCondition = AssetCondition.Good,
            AssignedBy = 2
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.AssignAssetToProjectAsync(assignmentDto));
        
        Assert.Contains("Project ID is required for project assignment", exception.Message);
    }

    [Fact]
    public async Task ReturnAssetAsync_ValidReturn_ReturnsUpdatedAssignment()
    {
        // Arrange
        var assignmentId = 1;
        var returnDto = new ReturnAssetDto
        {
            ReturnedCondition = AssetCondition.Fair,
            ReturnNotes = "Minor wear and tear",
            ReturnedBy = 2
        };

        var assignment = new AssetAssignment
        {
            Id = assignmentId,
            AssetId = 1,
            IsActive = true,
            IsDeleted = false,
            Asset = new Asset
            {
                Id = 1,
                Status = AssetStatus.Assigned
            }
        };

        var assignmentDto = new AssetAssignmentDto
        {
            Id = assignmentId,
            AssetId = assignment.AssetId,
            IsActive = false,
            ReturnedCondition = returnDto.ReturnedCondition
        };

        _mockAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetAssignment, object>>[]>()))
            .ReturnsAsync(assignment);
        _mockAssignmentRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetAssignment>()))
            .Returns(Task.CompletedTask);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockAssignmentRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<AssetAssignmentDto>(assignment))
            .Returns(assignmentDto);

        // Act
        var result = await _assignmentService.ReturnAssetAsync(assignmentId, returnDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(assignment.IsActive);
        Assert.Equal(returnDto.ReturnedCondition, assignment.ReturnedCondition);
        Assert.Equal(returnDto.ReturnNotes, assignment.ReturnNotes);
        Assert.Equal(AssetStatus.Available, assignment.Asset.Status);
        _mockAssignmentRepository.Verify(r => r.UpdateAsync(assignment), Times.Once);
        _mockAssetRepository.Verify(r => r.UpdateAsync(assignment.Asset), Times.Once);
    }

    [Fact]
    public async Task ReturnAssetAsync_AssignmentNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var assignmentId = 1;
        var returnDto = new ReturnAssetDto
        {
            ReturnedCondition = AssetCondition.Fair,
            ReturnedBy = 2
        };

        var assignment = new AssetAssignment
        {
            Id = assignmentId,
            IsActive = false, // Already returned
            IsDeleted = false
        };

        _mockAssignmentRepository.Setup(r => r.GetByIdAsync(assignmentId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetAssignment, object>>[]>()))
            .ReturnsAsync(assignment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.ReturnAssetAsync(assignmentId, returnDto));
        
        Assert.Contains("Assignment is already returned", exception.Message);
    }

    [Fact]
    public async Task CanAssignAssetAsync_AvailableAsset_ReturnsTrue()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            Status = AssetStatus.Available,
            IsDeleted = false
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.HasActiveAssignmentAsync(assetId))
            .ReturnsAsync(false);

        // Act
        var result = await _assignmentService.CanAssignAssetAsync(assetId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanAssignAssetAsync_AssetWithActiveAssignment_ReturnsFalse()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            Status = AssetStatus.Available,
            IsDeleted = false
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.HasActiveAssignmentAsync(assetId))
            .ReturnsAsync(true);

        // Act
        var result = await _assignmentService.CanAssignAssetAsync(assetId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanAssignAssetAsync_DeletedAsset_ReturnsFalse()
    {
        // Arrange
        var assetId = 1;
        var asset = new Asset
        {
            Id = assetId,
            Status = AssetStatus.Available,
            IsDeleted = true
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        // Act
        var result = await _assignmentService.CanAssignAssetAsync(assetId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanAssignAssetAsync_NonExistentAsset_ReturnsFalse()
    {
        // Arrange
        var assetId = 999;
        _mockAssetRepository.Setup(r => r.GetByIdAsync(assetId))
            .ReturnsAsync((Asset?)null);

        // Act
        var result = await _assignmentService.CanAssignAssetAsync(assetId);

        // Assert
        Assert.False(result);
    }
}