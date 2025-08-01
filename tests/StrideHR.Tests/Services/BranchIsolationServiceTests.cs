using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class BranchIsolationServiceTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly Mock<ILogger<BranchIsolationService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly BranchIsolationService _branchIsolationService;

    public BranchIsolationServiceTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _mockLogger = new Mock<ILogger<BranchIsolationService>>();
        _mockAuditService = new Mock<IAuditService>();
        _mockRoleService = new Mock<IRoleService>();

        _branchIsolationService = new BranchIsolationService(
            _context,
            _mockLogger.Object,
            _mockAuditService.Object,
            _mockRoleService.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var organization = new Organization
        {
            Id = 1,
            Name = "Test Organization",
            BranchIsolationEnabled = true
        };

        var branches = new List<Branch>
        {
            new() { Id = 1, OrganizationId = 1, Name = "Branch 1", IsActive = true },
            new() { Id = 2, OrganizationId = 1, Name = "Branch 2", IsActive = true },
            new() { Id = 3, OrganizationId = 1, Name = "Branch 3", IsActive = false }
        };

        var userBranchAccess = new List<UserBranchAccess>
        {
            new() { Id = 1, UserId = "2", BranchId = 1, IsActive = true, IsPrimary = true, GrantedBy = "admin" },
            new() { Id = 2, UserId = "2", BranchId = 2, IsActive = true, IsPrimary = false, GrantedBy = "admin" },
            new() { Id = 3, UserId = "3", BranchId = 2, IsActive = true, IsPrimary = true, GrantedBy = "admin" },
            new() { Id = 4, UserId = "3", BranchId = 1, IsActive = false, IsPrimary = false, GrantedBy = "admin" }
        };

        _context.Organizations.Add(organization);
        _context.Branches.AddRange(branches);
        _context.Set<UserBranchAccess>().AddRange(userBranchAccess);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_SuperAdmin_ReturnsTrue()
    {
        // Arrange
        var userId = "1";
        var branchId = 1;

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _branchIsolationService.ValidateBranchAccessAsync(branchId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_UserWithAccess_ReturnsTrue()
    {
        // Arrange
        var userId = "2";
        var branchId = 1;

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.ValidateBranchAccessAsync(branchId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_UserWithoutAccess_ReturnsFalse()
    {
        // Arrange
        var userId = "3";
        var branchId = 1; // user2 doesn't have access to branch 1

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.ValidateBranchAccessAsync(branchId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBranchAccessAsync_InactiveBranch_ReturnsFalse()
    {
        // Arrange
        var userId = "2";
        var branchId = 999; // Non-existent branch

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.ValidateBranchAccessAsync(branchId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserAccessibleBranchesAsync_SuperAdmin_ReturnsAllActiveBranches()
    {
        // Arrange
        var userId = "1";

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _branchIsolationService.GetUserAccessibleBranchesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // Only active branches (1 and 2)
        Assert.Contains(1, result);
        Assert.Contains(2, result);
        Assert.DoesNotContain(3, result); // Branch 3 is inactive
    }

    [Fact]
    public async Task GetUserAccessibleBranchesAsync_RegularUser_ReturnsUserBranches()
    {
        // Arrange
        var userId = "2";

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.GetUserAccessibleBranchesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public async Task GetUserAccessibleBranchesAsync_UserWithNoAccess_ReturnsEmpty()
    {
        // Arrange
        var userId = "4"; // User with no access

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.GetUserAccessibleBranchesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task IsBranchIsolationEnabledAsync_EnabledOrganization_ReturnsTrue()
    {
        // Arrange
        var organizationId = 1;

        // Act
        var result = await _branchIsolationService.IsBranchIsolationEnabledAsync(organizationId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsBranchIsolationEnabledAsync_NonExistentOrganization_ReturnsFalse()
    {
        // Arrange
        var organizationId = 999;

        // Act
        var result = await _branchIsolationService.IsBranchIsolationEnabledAsync(organizationId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GrantBranchAccessAsync_ValidBranches_ReturnsTrue()
    {
        // Arrange
        var userId = "5";
        var branchIds = new[] { 1, 2 };
        var grantedBy = "admin";

        // Act
        var result = await _branchIsolationService.GrantBranchAccessAsync(userId, branchIds, grantedBy);

        // Assert
        Assert.True(result);

        // Verify access was granted
        var accessRecords = await _context.Set<UserBranchAccess>()
            .Where(uba => uba.UserId == userId && uba.IsActive)
            .ToListAsync();

        Assert.Equal(2, accessRecords.Count);
        Assert.All(accessRecords, record => Assert.Equal(grantedBy, record.GrantedBy));
    }

    [Fact]
    public async Task GrantBranchAccessAsync_ReactivateExistingAccess_ReturnsTrue()
    {
        // Arrange
        var userId = "3"; // Has inactive access to branch 1
        var branchIds = new[] { 1 };
        var grantedBy = "admin";

        // Act
        var result = await _branchIsolationService.GrantBranchAccessAsync(userId, branchIds, grantedBy);

        // Assert
        Assert.True(result);

        // Verify access was reactivated
        var accessRecord = await _context.Set<UserBranchAccess>()
            .FirstOrDefaultAsync(uba => uba.UserId == userId && uba.BranchId == 1);

        Assert.NotNull(accessRecord);
        Assert.True(accessRecord.IsActive);
        Assert.Equal(grantedBy, accessRecord.GrantedBy);
    }

    [Fact]
    public async Task RevokeBranchAccessAsync_ExistingAccess_ReturnsTrue()
    {
        // Arrange
        var userId = "2";
        var branchIds = new[] { 1 };
        var revokedBy = "admin";

        // Act
        var result = await _branchIsolationService.RevokeBranchAccessAsync(userId, branchIds, revokedBy);

        // Assert
        Assert.True(result);

        // Verify access was revoked
        var accessRecord = await _context.Set<UserBranchAccess>()
            .FirstOrDefaultAsync(uba => uba.UserId == userId && uba.BranchId == 1);

        Assert.NotNull(accessRecord);
        Assert.False(accessRecord.IsActive);
        Assert.Equal(revokedBy, accessRecord.RevokedBy);
        Assert.NotNull(accessRecord.RevokedAt);
    }

    [Fact]
    public async Task RevokeBranchAccessAsync_NoExistingAccess_ReturnsFalse()
    {
        // Arrange
        var userId = "2";
        var branchIds = new[] { 999 }; // Non-existent branch
        var revokedBy = "admin";

        // Act
        var result = await _branchIsolationService.RevokeBranchAccessAsync(userId, branchIds, revokedBy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetBranchUsersAsync_ExistingBranch_ReturnsUsers()
    {
        // Arrange
        var branchId = 1;
        var superAdmins = new List<User>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        _mockRoleService.Setup(x => x.GetUsersInRoleAsync("SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(superAdmins);

        // Act
        var result = await _branchIsolationService.GetBranchUsersAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("user1", result); // Has access to branch 1
        Assert.Contains("superadmin1", result); // Super admin
        Assert.Contains("superadmin2", result); // Super admin
        Assert.DoesNotContain("user2", result); // Doesn't have access to branch 1
    }

    [Fact]
    public async Task GetUserPrimaryBranchAsync_UserWithPrimaryBranch_ReturnsBranchId()
    {
        // Arrange
        var userId = "2";

        // Act
        var result = await _branchIsolationService.GetUserPrimaryBranchAsync(userId);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetUserPrimaryBranchAsync_UserWithoutPrimaryBranch_ReturnsNull()
    {
        // Arrange
        var userId = "4"; // No primary branch

        // Act
        var result = await _branchIsolationService.GetUserPrimaryBranchAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetUserPrimaryBranchAsync_ValidBranch_ReturnsTrue()
    {
        // Arrange
        var userId = "2";
        var newPrimaryBranchId = 2;

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.SetUserPrimaryBranchAsync(userId, newPrimaryBranchId);

        // Assert
        Assert.True(result);

        // Verify primary branch was changed
        var primaryBranch = await _branchIsolationService.GetUserPrimaryBranchAsync(userId);
        Assert.Equal(newPrimaryBranchId, primaryBranch);

        // Verify old primary was removed
        var oldPrimaryAccess = await _context.Set<UserBranchAccess>()
            .FirstOrDefaultAsync(uba => uba.UserId == userId && uba.BranchId == 1);
        Assert.NotNull(oldPrimaryAccess);
        Assert.False(oldPrimaryAccess.IsPrimary);
    }

    [Fact]
    public async Task SetUserPrimaryBranchAsync_UserWithoutAccess_ReturnsFalse()
    {
        // Arrange
        var userId = "3";
        var branchId = 1; // user2 doesn't have access to branch 1

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.SetUserPrimaryBranchAsync(userId, branchId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsSuperAdminAsync_SuperAdminUser_ReturnsTrue()
    {
        // Arrange
        var userId = "1";

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _branchIsolationService.IsSuperAdminAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsSuperAdminAsync_RegularUser_ReturnsFalse()
    {
        // Arrange
        var userId = "2";

        _mockRoleService.Setup(x => x.IsUserInRoleAsync(userId, "SuperAdmin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _branchIsolationService.IsSuperAdminAsync(userId);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}