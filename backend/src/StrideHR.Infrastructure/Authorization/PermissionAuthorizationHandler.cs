using Microsoft.AspNetCore.Authorization;
using StrideHR.Core.Models.Authorization;
using System.Security.Claims;

namespace StrideHR.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if user has the required permission
        var permissions = context.User.FindAll("permission").Select(c => c.Value).ToList();
        
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            // Check if user has a wildcard permission for the module
            var parts = requirement.Permission.Split('.');
            if (parts.Length >= 2)
            {
                var wildcardPermission = $"{parts[0]}.*.{parts[2]}";
                if (permissions.Contains(wildcardPermission))
                {
                    context.Succeed(requirement);
                }
                
                // Check for full wildcard permission
                var fullWildcardPermission = $"{parts[0]}.*.*";
                if (permissions.Contains(fullWildcardPermission))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}