using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using StrideHR.API.Extensions;

namespace StrideHR.API.Authorization;

/// <summary>
/// Dynamic authorization policy provider for permission-based policies
/// </summary>
public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Handle permission-based policies
        if (policyName.StartsWith("Permission."))
        {
            var permission = policyName.Substring("Permission.".Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle any permission policies
        if (policyName.StartsWith("AnyPermission."))
        {
            var permissionsString = policyName.Substring("AnyPermission.".Length);
            var permissions = permissionsString.Split(',');
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AnyPermissionRequirement(permissions))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle all permissions policies
        if (policyName.StartsWith("AllPermissions."))
        {
            var permissionsString = policyName.Substring("AllPermissions.".Length);
            var permissions = permissionsString.Split(',');
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AllPermissionsRequirement(permissions))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle branch-based policies
        if (policyName.StartsWith("Branch."))
        {
            var branchIdString = policyName.Substring("Branch.".Length);
            if (int.TryParse(branchIdString, out var branchId))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new BranchAccessRequirement(branchId))
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        // Handle same branch policy
        if (policyName == "SameBranch")
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new SameBranchRequirement())
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}