using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IPermissionRepository> _mockPermissionRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<RoleService>> _mockLogger;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPermissionRepository = new Mock<IPermissionRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<RoleService>>();

        _roleService = new RoleService(
            _mockRoleRepository.Object,
            _mockPermissionRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateRoleAsync_ValidInput_ReturnsRole()
    {
        // Arrange
        var name = "TestRole";
        var description = "Test Role Description";
        var hierarchyLevel = 3;

        _mockRoleRepository.Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync((Role?)null);
        _mockRoleRepository.Setup(x => x.AddAsync(It.IsAny<Role>()))
            .ReturnsAsync((Role r) => r);
        _mockRoleRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.CreateRoleAsync(name, description, hierarchyLevel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(description, result.Description);
        Assert.Equal(hierarchyLevel, result.HierarchyLevel);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateRoleAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var name = "ExistingRole";
        var description = "Test Role Description";
        var hierarchyLevel = 3;

        var existingRole = new Role { Id = 1, Name = name };
        _mockRoleRepository.Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync(existingRole);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleService.CreateRoleAsync(name, description, hierarchyLevel));
        
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateRoleAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        var roleId = 1;
        var name = "UpdatedRole";
        var description = "Updated Description";
        var hierarchyLevel = 4;

        var existingRole = new Role
        {
            Id = roleId,
            Name = "OldRole",
            Description = "Old Description",
            HierarchyLevel = 2
        };

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(existingRole);
        _mockRoleRepository.Setup(x => x.GetByNameAsync(name))
            .ReturnsAsync((Role?)null);
        _mockRoleRepository.Setup(x => x.UpdateAsync(existingRole))
            .Returns(Task.CompletedTask);
        _mockRoleRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, name, description, hierarchyLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(name, existingRole.Name);
        Assert.Equal(description, existingRole.Description);
        Assert.Equal(hierarchyLevel, existingRole.HierarchyLevel);
    }

    [Fact]
    public async Task UpdateRoleAsync_NonExistentRole_ReturnsFalse()
    {
        // Arrange
        var roleId = 999;
        var name = "UpdatedRole";
        var description = "Updated Description";
        var hierarchyLevel = 4;

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _roleService.UpdateRoleAsync(roleId, name, description, hierarchyLevel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        var roleId = 1;
        var permissionIds = new List<int> { 1, 2, 3 };

        var role = new Role { Id = roleId, Name = "TestRole" };
        var permissions = new List<Permission>
        {
            new() { Id = 1, Name = "Permission1" },
            new() { Id = 2, Name = "Permission2" },
            new() { Id = 3, Name = "Permission3" }
        };

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);
        _mockPermissionRepository.Setup(x => x.GetByIdsAsync(permissionIds))
            .ReturnsAsync(permissions);
        _mockRoleRepository.Setup(x => x.AssignPermissionsAsync(roleId, permissionIds))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.AssignPermissionsToRoleAsync(roleId, permissionIds);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_InvalidPermissionIds_ThrowsException()
    {
        // Arrange
        var roleId = 1;
        var permissionIds = new List<int> { 1, 2, 999 }; // 999 doesn't exist

        var role = new Role { Id = roleId, Name = "TestRole" };
        var permissions = new List<Permission>
        {
            new() { Id = 1, Name = "Permission1" },
            new() { Id = 2, Name = "Permission2" }
            // Missing permission with ID 999
        };

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);
        _mockPermissionRepository.Setup(x => x.GetByIdsAsync(permissionIds))
            .ReturnsAsync(permissions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _roleService.AssignPermissionsToRoleAsync(roleId, permissionIds));
        
        Assert.Contains("invalid", exception.Message);
    }

    [Fact]
    public async Task GetMaxHierarchyLevelAsync_ValidRoles_ReturnsMaxLevel()
    {
        // Arrange
        var roleNames = new List<string> { "Employee", "Manager", "Admin" };
        var roles = new List<Role>
        {
            new() { Name = "Employee", HierarchyLevel = 1 },
            new() { Name = "Manager", HierarchyLevel = 3 },
            new() { Name = "Admin", HierarchyLevel = 5 }
        };

        _mockRoleRepository.Setup(x => x.GetRolesByNamesAsync(roleNames))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.GetMaxHierarchyLevelAsync(roleNames);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetMaxHierarchyLevelAsync_EmptyRoles_ReturnsZero()
    {
        // Arrange
        var roleNames = new List<string>();

        // Act
        var result = await _roleService.GetMaxHierarchyLevelAsync(roleNames);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task AssignRoleToEmployeeAsync_ValidInput_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var roleId = 2;
        var expiryDate = DateTime.UtcNow.AddYears(1);

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = true };

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);
        _mockRoleRepository.Setup(x => x.AssignRoleToEmployeeAsync(employeeId, roleId, expiryDate))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.AssignRoleToEmployeeAsync(employeeId, roleId, expiryDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AssignRoleToEmployeeAsync_InactiveRole_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;
        var roleId = 2;

        var role = new Role { Id = roleId, Name = "TestRole", IsActive = false };

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId))
            .ReturnsAsync(role);

        // Act
        var result = await _roleService.AssignRoleToEmployeeAsync(employeeId, roleId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasPermissionAsync_UserHasPermission_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "Employee.View";
        var userPermissions = new List<string> { "Employee.View", "Employee.Create" };

        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync(userId))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _roleService.HasPermissionAsync(userId, permission);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_UserDoesNotHavePermission_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var permission = "Employee.Delete";
        var userPermissions = new List<string> { "Employee.View", "Employee.Create" };

        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync(userId))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _roleService.HasPermissionAsync(userId, permission);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanAccessBranchAsync_SuperAdmin_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var branchId = 2;
        var user = new User
        {
            Id = userId,
            Employee = new Employee { BranchId = 1 } // Different branch
        };
        var roles = new List<string> { "SuperAdmin" };

        _mockUserRepository.Setup(x => x.GetWithEmployeeAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.CanAccessBranchAsync(userId, branchId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanAccessBranchAsync_SameBranch_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var branchId = 2;
        var user = new User
        {
            Id = userId,
            Employee = new Employee { BranchId = branchId } // Same branch
        };
        var roles = new List<string> { "Employee" };

        _mockUserRepository.Setup(x => x.GetWithEmployeeAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.CanAccessBranchAsync(userId, branchId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanAccessBranchAsync_DifferentBranch_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var branchId = 2;
        var user = new User
        {
            Id = userId,
            Employee = new Employee { BranchId = 1 } // Different branch
        };
        var roles = new List<string> { "Employee" };

        _mockUserRepository.Setup(x => x.GetWithEmployeeAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.CanAccessBranchAsync(userId, branchId);

        // Assert
        Assert.False(result);
    }
}