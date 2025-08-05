using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using StrideHR.Core.Models.Authorization;
using StrideHR.Infrastructure.Authorization;
using StrideHR.Tests.TestConfiguration;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Authorization;

/// <summary>
/// Unit tests for permission-based authorization handlers
/// Tests the core authorization logic for permission validation
/// </summary>
public class PermissionAuthorizationTests
{
    private readonly TestPermissionAuthorizationHandler _handler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public PermissionAuthorizationTests()
    {
        _handler = new TestPermissionAuthorizationHandler();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    #region Permission Requirement Tests

    [Fact]
    public async Task HandleRequirementAsync_WithExactPermission_ShouldSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "Employee.View"),
            new Claim("permission", "Employee.Create")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutPermission_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.Delete");
        var claims = new[]
        {
            new Claim("permission", "Employee.View"),
            new Claim("permission", "Employee.Create")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithWildcardPermission_ShouldSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "Employee.*.View"), // Wildcard for resource
            new Claim("permission", "Payroll.Create")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithFullWildcardPermission_ShouldSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "Employee.*.*"), // Full wildcard for module
            new Claim("permission", "Payroll.Create")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidWildcard_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "Payroll.*.View"), // Wrong module wildcard
            new Claim("permission", "Branch.*.*") // Wrong module full wildcard
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData("Employee.View", "Employee.View", true)]
    [InlineData("Employee.View", "Employee.Create", false)]
    [InlineData("Employee.View", "Employee.*.View", true)]
    [InlineData("Employee.View", "Employee.*.*", true)]
    [InlineData("Employee.View", "Payroll.*.View", false)]
    [InlineData("Employee.View", "Payroll.*.*", false)]
    [InlineData("Payroll.Calculate", "Payroll.Calculate", true)]
    [InlineData("Payroll.Calculate", "Payroll.*.Calculate", true)]
    [InlineData("Payroll.Calculate", "Payroll.*.*", true)]
    [InlineData("Branch.Create", "Branch.Create", true)]
    [InlineData("Branch.Create", "Branch.View", false)]
    public async Task HandleRequirementAsync_PermissionMatching_ShouldReturnExpectedResult(
        string requiredPermission, string userPermission, bool shouldSucceed)
    {
        // Arrange
        var requirement = new PermissionRequirement(requiredPermission);
        var claims = new[] { new Claim("permission", userPermission) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().Be(shouldSucceed);
    }

    #endregion

    #region Multiple Permission Tests

    [Fact]
    public async Task HandleRequirementAsync_WithMultiplePermissions_ShouldSucceedIfAnyMatches()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.Update");
        var claims = new[]
        {
            new Claim("permission", "Employee.View"),
            new Claim("permission", "Employee.Update"), // This should match
            new Claim("permission", "Payroll.View")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoPermissionClaims_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "Employee"),
            new Claim("EmployeeId", "123")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithEmptyPermissionClaims_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", ""), // Empty permission
            new Claim("permission", "   ") // Whitespace permission
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task HandleRequirementAsync_WithMalformedPermission_ShouldHandleGracefully()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "Employee"), // Missing action
            new Claim("permission", ".View"), // Missing module
            new Claim("permission", "Employee."), // Missing action
            new Claim("permission", "Employee.View.Extra.Parts") // Too many parts
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act & Assert - Should not throw exception
        await _handler.HandleRequirementAsync(context, requirement);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNullUser_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnauthenticatedUser_ShouldFail()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var user = new ClaimsPrincipal(); // No identity
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Complex Permission Scenarios

    [Fact]
    public async Task HandleRequirementAsync_WithHierarchicalPermissions_ShouldRespectHierarchy()
    {
        // Test that higher-level permissions grant access to lower-level operations
        var testCases = new[]
        {
            ("Employee.View", "Employee.*.*", true), // Full access grants view
            ("Employee.Create", "Employee.*.*", true), // Full access grants create
            ("Employee.Delete", "Employee.*.*", true), // Full access grants delete
            ("Employee.View", "Employee.*.View", true), // Resource wildcard grants view
            ("Employee.View", "Employee.*.Create", false), // Different action wildcard doesn't grant view
            ("Payroll.Calculate", "Employee.*.*", false) // Different module doesn't grant access
        };

        foreach (var (requiredPermission, userPermission, shouldSucceed) in testCases)
        {
            // Arrange
            var requirement = new PermissionRequirement(requiredPermission);
            var claims = new[] { new Claim("permission", userPermission) };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            // Act
            await _handler.HandleRequirementAsync(context, requirement);

            // Assert
            context.HasSucceeded.Should().Be(shouldSucceed, 
                $"Permission {userPermission} should {(shouldSucceed ? "grant" : "deny")} access to {requiredPermission}");
        }
    }

    [Fact]
    public async Task HandleRequirementAsync_WithCaseSensitivePermissions_ShouldMatchExactly()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[]
        {
            new Claim("permission", "employee.view"), // Different case
            new Claim("permission", "EMPLOYEE.VIEW"), // Different case
            new Claim("permission", "Employee.view") // Different case
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        // Assuming case-sensitive matching (adjust based on actual implementation)
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee-Management.View");
        var claims = new[]
        {
            new Claim("permission", "Employee-Management.View"),
            new Claim("permission", "Employee_Management.View"), // Different separator
            new Claim("permission", "Employee Management.View") // Space separator
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue(); // First permission should match exactly
    }

    #endregion

    #region Performance and Concurrency Tests

    [Fact]
    public async Task HandleRequirementAsync_WithManyPermissions_ShouldPerformEfficiently()
    {
        // Arrange - Create user with many permissions
        var permissions = Enumerable.Range(1, 1000)
            .Select(i => new Claim("permission", $"Module{i}.Action{i}"))
            .ToArray();
        
        var requirement = new PermissionRequirement("Module500.Action500"); // Permission in the middle
        var user = new ClaimsPrincipal(new ClaimsIdentity(permissions, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _handler.HandleRequirementAsync(context, requirement);
        stopwatch.Stop();

        // Assert
        context.HasSucceeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "Permission check should be fast even with many permissions");
    }

    [Fact]
    public async Task HandleRequirementAsync_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var requirement = new PermissionRequirement("Employee.View");
        var claims = new[] { new Claim("permission", "Employee.View") };
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
        results.Should().AllBeEquivalentTo(true, "All concurrent authorization checks should succeed");
    }

    #endregion
}