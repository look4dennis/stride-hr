using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using StrideHR.Core.Models.Authorization;
using System.Security.Claims;

namespace StrideHR.Infrastructure.Authorization;

public class BranchAccessAuthorizationHandler : AuthorizationHandler<BranchAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchAccessAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BranchAccessRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Get user's branch ID from claims
        var userBranchIdClaim = context.User.FindFirst("BranchId")?.Value;
        if (!int.TryParse(userBranchIdClaim, out var userBranchId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if user has SuperAdmin role (can access all branches)
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (roles.Contains("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!requirement.RequireSameBranch)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Get target branch ID from route parameters or query string
        var targetBranchId = GetTargetBranchId(httpContext);
        
        if (targetBranchId == null)
        {
            // If no target branch specified, allow access to user's own branch
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user is trying to access their own branch
        if (userBranchId == targetBranchId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private static int? GetTargetBranchId(HttpContext httpContext)
    {
        // Check route parameters
        if (httpContext.GetRouteData()?.Values.TryGetValue("branchId", out var branchIdValue) == true)
        {
            if (int.TryParse(branchIdValue?.ToString(), out var branchId))
            {
                return branchId;
            }
        }

        // Check query parameters
        if (httpContext.Request.Query.TryGetValue("branchId", out var queryBranchId))
        {
            if (int.TryParse(queryBranchId.FirstOrDefault(), out var branchId))
            {
                return branchId;
            }
        }

        // Check request body for branch ID (for POST/PUT requests)
        if (httpContext.Items.TryGetValue("TargetBranchId", out var targetBranchIdValue))
        {
            if (int.TryParse(targetBranchIdValue?.ToString(), out var branchId))
            {
                return branchId;
            }
        }

        return null;
    }
}