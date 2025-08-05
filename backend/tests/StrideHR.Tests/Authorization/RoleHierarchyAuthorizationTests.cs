using Microsoft.AspNetCore.Authorization;
using Moq;
using StrideHR.Core.Models.Authorization;
using StrideHR.Infrastructure.Authorization;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Authorization;

/// <summary>
/// Unit tests for role hierarchy-based authorization
/// Tests hierarchical role access control and role-based permissions
/// </summary>
public class RoleHierarchyAuthorizationTests
{
    private readonly RoleHierarchyAuthorizationHandler _handler;

    public RoleHierarchyAuthorizationTests()
    {
        _handler = new RoleHierarchyAuthorizationHandler();
    }

    #region Role Hierarchy Level Tests

    private readonly Dictionary<string, int> _roleHierarchy = new()
    {
        ["SuperAdmin"] = 100,
        ["OrganizationAdmin"] = 90,
        ["HRManager"] = 80,
        ["RegionalManager"] = 70,
        ["Manager"] = 60,
        ["TeamLead"] = 50,
        ["SeniorEmployee"] = 40,
        ["Employee"] = 30,
        ["Intern"] = 20,
        ["Guest"] = 10
    };

    [Theory]
    [InlineData("SuperAdmin", 50, true)] // SuperAdmin (100) >= 50
    [InlineData("HRManager", 50, true)] // HRManager (80) >= 50
    [InlineData("Manager", 50, true)] // Manager (60) >= 50
    [InlineData("TeamLead", 50, true)] // TeamLead (50) >= 50
    [InlineData("Employee", 50, false)] // Employee (30) < 50
    [InlineData("Intern", 50, false)] // Intern (20) < 50
    [InlineData("Guest", 50, false)] // Guest (10) < 50
    public async Task HandleRequirementAsync_WithRoleHierarchy_ShouldEnforceMinimumLevel(
        string userRole, int minimumLevel, bool shouldSucceed)
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(minimumLevel);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, userRole),
            new Claim("RoleLevel", _roleHierarchy[userRole].ToString()),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().Be(shouldSucceed,
            $"Role {userRole} (level {_roleHierarchy[userRole]}) should {(shouldSucceed ? "meet" : "not meet")} minimum level {minimumLevel}");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutRoleLevelClaim_ShouldUseRoleNameMapping()
    {
        // Arrange - User without explicit role level claim
        var requirement = new RoleHierarchyRequirement(60);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("EmployeeId", "123")
            // Missing RoleLevel claim
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed if handler has built-in role mapping for Manager
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnknownRole_ShouldFail()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(30);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "UnknownRole"),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Multiple Role Tests

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRoles_ShouldUseHighestLevel()
    {
        // Arrange - User with multiple roles
        var requirement = new RoleHierarchyRequirement(70);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Employee"), // Level 30
            new Claim(ClaimTypes.Role, "Manager"), // Level 60
            new Claim(ClaimTypes.Role, "RegionalManager"), // Level 70
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed because RegionalManager (70) meets the requirement
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRoles_ShouldFailIfNoneMeetRequirement()
    {
        // Arrange - User with multiple roles but none meet requirement
        var requirement = new RoleHierarchyRequirement(80);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Employee"), // Level 30
            new Claim(ClaimTypes.Role, "TeamLead"), // Level 50
            new Claim(ClaimTypes.Role, "Manager"), // Level 60
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should fail because highest role (Manager - 60) < 80
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Temporary Role Elevation Tests

    [Fact]
    public async Task HandleRequirementAsync_WithTemporaryRoleElevation_ShouldUseElevatedRole()
    {
        // Arrange - User with temporary role elevation
        var requirement = new RoleHierarchyRequirement(80);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"), // Base role - Level 60
            new Claim("TemporaryRole", "HRManager"), // Elevated role - Level 80
            new Claim("TemporaryRoleExpiry", DateTime.UtcNow.AddHours(1).ToString()), // Valid elevation
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed because temporary HRManager role (80) meets requirement
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithExpiredTemporaryRole_ShouldUseBaseRole()
    {
        // Arrange - User with expired temporary role elevation
        var requirement = new RoleHierarchyRequirement(80);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"), // Base role - Level 60
            new Claim("TemporaryRole", "HRManager"), // Elevated role - Level 80
            new Claim("TemporaryRoleExpiry", DateTime.UtcNow.AddHours(-1).ToString()), // Expired elevation
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should fail because base Manager role (60) < 80 and temporary role is expired
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Context-Specific Role Tests

    [Fact]
    public async Task HandleRequirementAsync_WithContextSpecificRole_ShouldApplyCorrectly()
    {
        // Arrange - User with different roles in different contexts
        var requirement = new RoleHierarchyRequirement(70);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"), // Base role - Level 60
            new Claim("ProjectRole", "ProjectManager"), // Context-specific role
            new Claim("ProjectRoleLevel", "75"), // Higher level in project context
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed if handler considers context-specific roles
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Delegation and Acting On Behalf Tests

    [Fact]
    public async Task HandleRequirementAsync_WithDelegatedAuthority_ShouldUseOriginalUserLevel()
    {
        // Arrange - User acting on behalf of another user
        var requirement = new RoleHierarchyRequirement(80);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"), // Acting user role - Level 60
            new Claim("ActingOnBehalfOf", "456"), // Original user ID
            new Claim("OriginalUserRole", "HRManager"), // Original user role - Level 80
            new Claim("DelegationExpiry", DateTime.UtcNow.AddHours(1).ToString()),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed because original user (HRManager - 80) meets requirement
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithExpiredDelegation_ShouldUseActingUserLevel()
    {
        // Arrange - User with expired delegation
        var requirement = new RoleHierarchyRequirement(80);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"), // Acting user role - Level 60
            new Claim("ActingOnBehalfOf", "456"), // Original user ID
            new Claim("OriginalUserRole", "HRManager"), // Original user role - Level 80
            new Claim("DelegationExpiry", DateTime.UtcNow.AddHours(-1).ToString()), // Expired
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should fail because acting user (Manager - 60) < 80 and delegation is expired
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task HandleRequirementAsync_WithoutRoleClaim_ShouldFail()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(30);
        var claims = new[]
        {
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Name, "Test User")
            // Missing role claim
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidRoleLevel_ShouldHandleGracefully()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(50);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("RoleLevel", "invalid-number"), // Invalid role level
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act & Assert - Should not throw exception
        await _handler.HandleRequirementAsync(context, requirement);
        // Result depends on implementation - could fall back to role name mapping
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNegativeRequirement_ShouldHandleCorrectly()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(-10); // Negative requirement
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Guest"),
            new Claim("RoleLevel", "10"),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed because any positive role level > negative requirement
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithZeroRequirement_ShouldAllowAllRoles()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(0);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Guest"),
            new Claim("RoleLevel", "10"),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Role Inheritance Tests

    [Fact]
    public async Task HandleRequirementAsync_WithInheritedRoles_ShouldConsiderAllLevels()
    {
        // Arrange - User with inherited roles from organizational structure
        var requirement = new RoleHierarchyRequirement(65);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "TeamLead"), // Direct role - Level 50
            new Claim("InheritedRole", "Manager"), // Inherited from department - Level 60
            new Claim("InheritedRole", "RegionalManager"), // Inherited from region - Level 70
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Should succeed because inherited RegionalManager role (70) > 65
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task HandleRequirementAsync_WithManyRoles_ShouldPerformEfficiently()
    {
        // Arrange - User with many roles
        var roleClaims = Enumerable.Range(1, 100)
            .Select(i => new Claim(ClaimTypes.Role, $"Role{i}"))
            .ToList();
        
        roleClaims.Add(new Claim(ClaimTypes.Role, "SuperAdmin")); // High-level role
        roleClaims.Add(new Claim("RoleLevel", "100"));
        roleClaims.Add(new Claim("EmployeeId", "123"));

        var requirement = new RoleHierarchyRequirement(90);
        var user = new ClaimsPrincipal(new ClaimsIdentity(roleClaims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _handler.HandleRequirementAsync(context, requirement);
        stopwatch.Stop();

        // Assert
        context.HasSucceeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50, "Role hierarchy check should be fast even with many roles");
    }

    [Fact]
    public async Task HandleRequirementAsync_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(60);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("RoleLevel", "60"),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act - Run multiple authorization checks concurrently
        var tasks = Enumerable.Range(1, 100).Select(async _ =>
        {
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            await _handler.HandleRequirementAsync(context, requirement);
            return context.HasSucceeded;
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(true, "All concurrent role hierarchy checks should succeed");
    }

    #endregion

    #region Complex Hierarchy Scenarios

    [Theory]
    [InlineData(10, "Guest", true)] // Minimum access level
    [InlineData(30, "Employee", true)] // Standard employee access
    [InlineData(50, "TeamLead", true)] // Team leadership access
    [InlineData(60, "Manager", true)] // Management access
    [InlineData(80, "HRManager", true)] // HR management access
    [InlineData(100, "SuperAdmin", true)] // Full system access
    [InlineData(101, "SuperAdmin", false)] // Beyond maximum level
    public async Task HandleRequirementAsync_AccessLevelBoundaries_ShouldEnforceCorrectly(
        int requiredLevel, string userRole, bool shouldSucceed)
    {
        // Arrange
        var requirement = new RoleHierarchyRequirement(requiredLevel);
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, userRole),
            new Claim("RoleLevel", _roleHierarchy[userRole].ToString()),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().Be(shouldSucceed,
            $"Role {userRole} (level {_roleHierarchy[userRole]}) should {(shouldSucceed ? "meet" : "not meet")} requirement level {requiredLevel}");
    }

    #endregion
}