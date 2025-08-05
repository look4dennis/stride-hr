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
/// Unit tests for branch-based access control authorization
/// Tests multi-tenancy and branch isolation security features
/// </summary>
public class BranchAccessAuthorizationTests
{
    private readonly TestBranchAccessAuthorizationHandler _handler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public BranchAccessAuthorizationTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _handler = new TestBranchAccessAuthorizationHandler(_mockHttpContextAccessor.Object);
    }

    #region Branch Access Requirement Tests

    [Fact]
    public async Task HandleRequirementAsync_WithMatchingBranchId_ShouldSucceed()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "1" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDifferentBranchId_ShouldFail()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "2" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_SuperAdmin_ShouldBypassBranchRestriction()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "2" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutBranchIdClaim_ShouldFail()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
            // Missing BranchId claim
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "1" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithoutRouteValue_ShouldSucceed()
    {
        // Arrange - When no branchId in route, access should be allowed (general endpoints)
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary()); // No branchId in route
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Query Parameter Branch Access Tests

    [Fact]
    public async Task HandleRequirementAsync_WithBranchIdInQuery_ShouldValidateAccess()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "branchId", "1" }
        });
        
        mockRequest.Setup(x => x.Query).Returns(queryCollection);
        mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary());
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDifferentBranchIdInQuery_ShouldFail()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "branchId", "2" }
        });
        
        mockRequest.Setup(x => x.Query).Returns(queryCollection);
        mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary());
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Request Body Branch Access Tests

    [Fact]
    public async Task HandleRequirementAsync_WithBranchIdInRequestBody_ShouldValidateAccess()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary());
        mockHttpContext.Setup(x => x.Items)
            .Returns(new Dictionary<object, object> { { "RequestBranchId", "1" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    #endregion

    #region Multi-Branch Access Tests

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleBranchAccess_ShouldValidateAll()
    {
        // Arrange - User with access to multiple branches
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("BranchId", "2"), // Multiple branch access
            new Claim("BranchId", "3"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "RegionalManager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "2" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleBranchAccess_ShouldFailForUnauthorizedBranch()
    {
        // Arrange - User with access to multiple branches but not the requested one
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("BranchId", "2"),
            new Claim("BranchId", "3"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "RegionalManager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "4" } }); // Not in user's branches
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Role-Based Branch Access Tests

    [Theory]
    [InlineData("SuperAdmin", "1", "2", true)] // SuperAdmin can access any branch
    [InlineData("HRManager", "1", "1", true)] // HRManager can access own branch
    [InlineData("HRManager", "1", "2", false)] // HRManager cannot access other branch
    [InlineData("Manager", "1", "1", true)] // Manager can access own branch
    [InlineData("Manager", "1", "2", false)] // Manager cannot access other branch
    [InlineData("Employee", "1", "1", true)] // Employee can access own branch
    [InlineData("Employee", "1", "2", false)] // Employee cannot access other branch
    public async Task HandleRequirementAsync_RoleBasedBranchAccess_ShouldEnforceCorrectly(
        string role, string userBranchId, string requestedBranchId, bool shouldSucceed)
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", userBranchId),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, role)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", requestedBranchId } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().Be(shouldSucceed, 
            $"Role {role} with branch {userBranchId} should {(shouldSucceed ? "have" : "not have")} access to branch {requestedBranchId}");
    }

    #endregion

    #region Organization-Level Access Tests

    [Fact]
    public async Task HandleRequirementAsync_WithOrganizationAccess_ShouldAllowCrossBranchAccess()
    {
        // Arrange - User with organization-level access
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("OrganizationId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "OrganizationAdmin"),
            new Claim("permission", "Organization.CrossBranchAccess")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "2" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDifferentOrganization_ShouldFail()
    {
        // Arrange - User from different organization
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("OrganizationId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "2" } });
        mockHttpContext.Setup(x => x.Items)
            .Returns(new Dictionary<object, object> { { "RequestOrganizationId", "2" } }); // Different org
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidBranchIdFormat_ShouldHandleGracefully()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "invalid-branch-id"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "not-a-number" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act & Assert - Should not throw exception
        await _handler.HandleRequirementAsync(context, requirement);
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNullHttpContext_ShouldFail()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", "1"),
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithEmptyBranchIdClaim_ShouldFail()
    {
        // Arrange
        var requirement = new BranchAccessRequirement();
        var claims = new[]
        {
            new Claim("BranchId", ""), // Empty branch ID
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "Manager")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "1" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        await _handler.HandleRequirementAsync(context, requirement);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task HandleRequirementAsync_WithManyBranchClaims_ShouldPerformEfficiently()
    {
        // Arrange - User with access to many branches
        var branchClaims = Enumerable.Range(1, 100)
            .Select(i => new Claim("BranchId", i.ToString()))
            .ToList();
        
        branchClaims.AddRange(new[]
        {
            new Claim("EmployeeId", "123"),
            new Claim(ClaimTypes.Role, "RegionalManager")
        });

        var requirement = new BranchAccessRequirement();
        var user = new ClaimsPrincipal(new ClaimsIdentity(branchClaims, "test"));
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.Request.RouteValues)
            .Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "branchId", "50" } });
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, mockHttpContext.Object);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _handler.HandleRequirementAsync(context, requirement);
        stopwatch.Stop();

        // Assert
        context.HasSucceeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50, "Branch access check should be fast even with many branch claims");
    }

    #endregion
}