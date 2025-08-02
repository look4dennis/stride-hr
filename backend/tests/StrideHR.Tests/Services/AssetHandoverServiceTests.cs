using AutoMapper;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Asset;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AssetHandoverServiceTests
{
    private readonly Mock<IAssetHandoverRepository> _mockHandoverRepository;
    private readonly Mock<IAssetAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IAssetRepository> _mockAssetRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AssetHandoverService _handoverService;

    public AssetHandoverServiceTests()
    {
        _mockHandoverRepository = new Mock<IAssetHandoverRepository>();
        _mockAssignmentRepository = new Mock<IAssetAssignmentRepository>();
        _mockAssetRepository = new Mock<IAssetRepository>();
        _mockMapper = new Mock<IMapper>();

        _handoverService = new AssetHandoverService(
            _mockHandoverRepository.Object,
            _mockAssignmentRepository.Object,
            _mockAssetRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task InitiateHandoverAsync_ValidHandover_ReturnsHandoverDto()
    {
        // Arrange
        var handoverDto = new CreateAssetHandoverDto
        {
            AssetId = 1,
            EmployeeId = 1,
            DueDate = DateTime.UtcNow.AddDays(7),
            InitiatedBy = 2
        };

        var asset = new Asset
        {
            Id = 1,
            Name = "Test Laptop",
            IsDeleted = false
        };

        var activeAssignment = new AssetAssignment
        {
            Id = 1,
            AssetId = 1,
            EmployeeId = 1,
            IsActive = true
        };

        var handover = new AssetHandover
        {
            Id = 1,
            AssetId = handoverDto.AssetId,
            EmployeeId = handoverDto.EmployeeId,
            Status = HandoverStatus.Pending,
            InitiatedBy = handoverDto.InitiatedBy
        };

        var handoverDtoResult = new AssetHandoverDto
        {
            Id = 1,
            AssetId = handover.AssetId,
            EmployeeId = handover.EmployeeId,
            Status = handover.Status
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(handoverDto.AssetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.GetActiveAssignmentByAssetIdAsync(handoverDto.AssetId))
            .ReturnsAsync(activeAssignment);
        _mockHandoverRepository.Setup(r => r.HasPendingHandoverAsync(handoverDto.AssetId))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<AssetHandover>(handoverDto))
            .Returns(handover);
        _mockHandoverRepository.Setup(r => r.AddAsync(It.IsAny<AssetHandover>()))
            .ReturnsAsync(handover);
        _mockHandoverRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handover.Id, It.IsAny<System.Linq.Expressions.Expression<Func<AssetHandover, object>>[]>()))
            .ReturnsAsync(handover);
        _mockMapper.Setup(m => m.Map<AssetHandoverDto>(handover))
            .Returns(handoverDtoResult);

        // Act
        var result = await _handoverService.InitiateHandoverAsync(handoverDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(handoverDtoResult.Id, result.Id);
        Assert.Equal(handoverDtoResult.AssetId, result.AssetId);
        Assert.Equal(handoverDtoResult.Status, result.Status);
        _mockHandoverRepository.Verify(r => r.AddAsync(It.IsAny<AssetHandover>()), Times.Once);
        _mockHandoverRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task InitiateHandoverAsync_AssetNotAssignedToEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var handoverDto = new CreateAssetHandoverDto
        {
            AssetId = 1,
            EmployeeId = 1,
            InitiatedBy = 2
        };

        var asset = new Asset
        {
            Id = 1,
            IsDeleted = false
        };

        var activeAssignment = new AssetAssignment
        {
            Id = 1,
            AssetId = 1,
            EmployeeId = 2, // Different employee
            IsActive = true
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(handoverDto.AssetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.GetActiveAssignmentByAssetIdAsync(handoverDto.AssetId))
            .ReturnsAsync(activeAssignment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handoverService.InitiateHandoverAsync(handoverDto));
        
        Assert.Contains("Asset is not currently assigned to the specified employee", exception.Message);
        _mockHandoverRepository.Verify(r => r.AddAsync(It.IsAny<AssetHandover>()), Times.Never);
    }

    [Fact]
    public async Task InitiateHandoverAsync_PendingHandoverExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var handoverDto = new CreateAssetHandoverDto
        {
            AssetId = 1,
            EmployeeId = 1,
            InitiatedBy = 2
        };

        var asset = new Asset
        {
            Id = 1,
            IsDeleted = false
        };

        var activeAssignment = new AssetAssignment
        {
            Id = 1,
            AssetId = 1,
            EmployeeId = 1,
            IsActive = true
        };

        _mockAssetRepository.Setup(r => r.GetByIdAsync(handoverDto.AssetId))
            .ReturnsAsync(asset);
        _mockAssignmentRepository.Setup(r => r.GetActiveAssignmentByAssetIdAsync(handoverDto.AssetId))
            .ReturnsAsync(activeAssignment);
        _mockHandoverRepository.Setup(r => r.HasPendingHandoverAsync(handoverDto.AssetId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handoverService.InitiateHandoverAsync(handoverDto));
        
        Assert.Contains("Asset already has a pending handover", exception.Message);
        _mockHandoverRepository.Verify(r => r.AddAsync(It.IsAny<AssetHandover>()), Times.Never);
    }

    [Fact]
    public async Task CompleteHandoverAsync_PendingHandover_CompletesHandoverAndReturnsAsset()
    {
        // Arrange
        var handoverId = 1;
        var completionDto = new CompleteAssetHandoverDto
        {
            ReturnedCondition = AssetCondition.Good,
            HandoverNotes = "Asset returned in good condition",
            CompletedBy = 2
        };

        var handover = new AssetHandover
        {
            Id = handoverId,
            AssetId = 1,
            EmployeeId = 1,
            Status = HandoverStatus.Pending,
            IsDeleted = false
        };

        var activeAssignment = new AssetAssignment
        {
            Id = 1,
            AssetId = 1,
            EmployeeId = 1,
            IsActive = true
        };

        var asset = new Asset
        {
            Id = 1,
            Status = AssetStatus.Assigned
        };

        var handoverDto = new AssetHandoverDto
        {
            Id = handoverId,
            Status = HandoverStatus.Completed,
            ReturnedCondition = completionDto.ReturnedCondition
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);
        _mockAssignmentRepository.Setup(r => r.GetActiveAssignmentByAssetIdAsync(handover.AssetId))
            .ReturnsAsync(activeAssignment);
        _mockAssetRepository.Setup(r => r.GetByIdAsync(handover.AssetId))
            .ReturnsAsync(asset);
        _mockHandoverRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetHandover>()))
            .Returns(Task.CompletedTask);
        _mockAssignmentRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetAssignment>()))
            .Returns(Task.CompletedTask);
        _mockAssetRepository.Setup(r => r.UpdateAsync(It.IsAny<Asset>()))
            .Returns(Task.CompletedTask);
        _mockHandoverRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetHandover, object>>[]>()))
            .ReturnsAsync(handover);
        _mockMapper.Setup(m => m.Map<AssetHandoverDto>(handover))
            .Returns(handoverDto);

        // Act
        var result = await _handoverService.CompleteHandoverAsync(handoverId, completionDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HandoverStatus.Completed, handover.Status);
        Assert.Equal(completionDto.ReturnedCondition, handover.ReturnedCondition);
        Assert.Equal(completionDto.HandoverNotes, handover.HandoverNotes);
        Assert.NotNull(handover.CompletedDate);
        Assert.False(activeAssignment.IsActive);
        Assert.Equal(AssetStatus.Available, asset.Status);
        _mockHandoverRepository.Verify(r => r.UpdateAsync(handover), Times.Once);
        _mockAssignmentRepository.Verify(r => r.UpdateAsync(activeAssignment), Times.Once);
        _mockAssetRepository.Verify(r => r.UpdateAsync(asset), Times.Once);
    }

    [Fact]
    public async Task CompleteHandoverAsync_NonPendingHandover_ThrowsInvalidOperationException()
    {
        // Arrange
        var handoverId = 1;
        var completionDto = new CompleteAssetHandoverDto
        {
            ReturnedCondition = AssetCondition.Good,
            CompletedBy = 2
        };

        var handover = new AssetHandover
        {
            Id = handoverId,
            Status = HandoverStatus.Completed, // Already completed
            IsDeleted = false
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handoverService.CompleteHandoverAsync(handoverId, completionDto));
        
        Assert.Contains("Only pending handovers can be completed", exception.Message);
    }

    [Fact]
    public async Task ApproveHandoverAsync_CompletedHandover_ApprovesHandover()
    {
        // Arrange
        var handoverId = 1;
        var approvalDto = new ApproveAssetHandoverDto
        {
            ApprovedBy = 3
        };

        var handover = new AssetHandover
        {
            Id = handoverId,
            Status = HandoverStatus.Completed,
            IsApproved = false,
            IsDeleted = false
        };

        var handoverDto = new AssetHandoverDto
        {
            Id = handoverId,
            Status = HandoverStatus.Completed,
            IsApproved = true
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);
        _mockHandoverRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetHandover>()))
            .Returns(Task.CompletedTask);
        _mockHandoverRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);
        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId, It.IsAny<System.Linq.Expressions.Expression<Func<AssetHandover, object>>[]>()))
            .ReturnsAsync(handover);
        _mockMapper.Setup(m => m.Map<AssetHandoverDto>(handover))
            .Returns(handoverDto);

        // Act
        var result = await _handoverService.ApproveHandoverAsync(handoverId, approvalDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(handover.IsApproved);
        Assert.Equal(approvalDto.ApprovedBy, handover.ApprovedBy);
        Assert.NotNull(handover.ApprovedDate);
        _mockHandoverRepository.Verify(r => r.UpdateAsync(handover), Times.Once);
        _mockHandoverRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveHandoverAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var handoverId = 1;
        var approvalDto = new ApproveAssetHandoverDto
        {
            ApprovedBy = 3
        };

        var handover = new AssetHandover
        {
            Id = handoverId,
            Status = HandoverStatus.Completed,
            IsApproved = true, // Already approved
            IsDeleted = false
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handoverService.ApproveHandoverAsync(handoverId, approvalDto));
        
        Assert.Contains("Handover is already approved", exception.Message);
    }

    [Fact]
    public async Task CancelHandoverAsync_PendingHandover_CancelsHandover()
    {
        // Arrange
        var handoverId = 1;
        var cancelledBy = 2;

        var handover = new AssetHandover
        {
            Id = handoverId,
            Status = HandoverStatus.Pending,
            IsDeleted = false
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);
        _mockHandoverRepository.Setup(r => r.UpdateAsync(It.IsAny<AssetHandover>()))
            .Returns(Task.CompletedTask);
        _mockHandoverRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _handoverService.CancelHandoverAsync(handoverId, cancelledBy);

        // Assert
        Assert.True(result);
        Assert.Equal(HandoverStatus.Cancelled, handover.Status);
        _mockHandoverRepository.Verify(r => r.UpdateAsync(handover), Times.Once);
        _mockHandoverRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelHandoverAsync_NonPendingHandover_ThrowsInvalidOperationException()
    {
        // Arrange
        var handoverId = 1;
        var cancelledBy = 2;

        var handover = new AssetHandover
        {
            Id = handoverId,
            Status = HandoverStatus.Completed, // Cannot cancel completed handover
            IsDeleted = false
        };

        _mockHandoverRepository.Setup(r => r.GetByIdAsync(handoverId))
            .ReturnsAsync(handover);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handoverService.CancelHandoverAsync(handoverId, cancelledBy));
        
        Assert.Contains("Only pending handovers can be cancelled", exception.Message);
        _mockHandoverRepository.Verify(r => r.UpdateAsync(It.IsAny<AssetHandover>()), Times.Never);
    }
}