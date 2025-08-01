using Microsoft.AspNetCore.Authorization;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authorization;
using System.Security.Claims;

namespace StrideHR.Infrastructure.Authorization;

public class RoleHierarchyAuthorizationHandler : AuthorizationHandler<RoleHierarchyRequirement>
{
    private readonly IRoleService _roleService;

    public RoleHierarchyAuthorizationHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleHierarchyRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (!roles.Any())
        {
            context.Fail();
            return;
        }

        // Get the highest hierarchy level among user's roles
        var userMaxHierarchyLevel = await _roleService.GetMaxHierarchyLevelAsync(roles);
        
        if (userMaxHierarchyLevel >= requirement.MinimumHierarchyLevel)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}