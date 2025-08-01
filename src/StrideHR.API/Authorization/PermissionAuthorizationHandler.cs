using Microsoft.AspNetCore.Authorization;

namespace StrideHR.API.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Authorization handler for permission-based access control
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        try
        {
            // Check if user has the required permission
            var hasPermission = context.User.HasClaim("permission", requirement.Permission);
            
            if (hasPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug("User {UserId} has permission {Permission}", 
                    context.User.FindFirst("UserId")?.Value, requirement.Permission);
            }
            else
            {
                _logger.LogWarning("User {UserId} lacks permission {Permission}", 
                    context.User.FindFirst("UserId")?.Value, requirement.Permission);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", 
                requirement.Permission, context.User.FindFirst("UserId")?.Value);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement for multiple permissions (any one required)
/// </summary>
public class AnyPermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public AnyPermissionRequirement(params string[] permissions)
    {
        Permissions = permissions;
    }
}

/// <summary>
/// Authorization handler for any permission requirement
/// </summary>
public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly ILogger<AnyPermissionAuthorizationHandler> _logger;

    public AnyPermissionAuthorizationHandler(ILogger<AnyPermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        try
        {
            // Check if user has any of the required permissions
            var hasAnyPermission = requirement.Permissions.Any(permission => 
                context.User.HasClaim("permission", permission));
            
            if (hasAnyPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug("User {UserId} has at least one of the required permissions: {Permissions}", 
                    context.User.FindFirst("UserId")?.Value, string.Join(", ", requirement.Permissions));
            }
            else
            {
                _logger.LogWarning("User {UserId} lacks all required permissions: {Permissions}", 
                    context.User.FindFirst("UserId")?.Value, string.Join(", ", requirement.Permissions));
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions {Permissions} for user {UserId}", 
                string.Join(", ", requirement.Permissions), context.User.FindFirst("UserId")?.Value);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement for all permissions (all required)
/// </summary>
public class AllPermissionsRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }

    public AllPermissionsRequirement(params string[] permissions)
    {
        Permissions = permissions;
    }
}

/// <summary>
/// Authorization handler for all permissions requirement
/// </summary>
public class AllPermissionsAuthorizationHandler : AuthorizationHandler<AllPermissionsRequirement>
{
    private readonly ILogger<AllPermissionsAuthorizationHandler> _logger;

    public AllPermissionsAuthorizationHandler(ILogger<AllPermissionsAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllPermissionsRequirement requirement)
    {
        try
        {
            // Check if user has all required permissions
            var hasAllPermissions = requirement.Permissions.All(permission => 
                context.User.HasClaim("permission", permission));
            
            if (hasAllPermissions)
            {
                context.Succeed(requirement);
                _logger.LogDebug("User {UserId} has all required permissions: {Permissions}", 
                    context.User.FindFirst("UserId")?.Value, string.Join(", ", requirement.Permissions));
            }
            else
            {
                var missingPermissions = requirement.Permissions.Where(permission => 
                    !context.User.HasClaim("permission", permission)).ToArray();
                
                _logger.LogWarning("User {UserId} lacks required permissions: {MissingPermissions}", 
                    context.User.FindFirst("UserId")?.Value, string.Join(", ", missingPermissions));
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking all permissions {Permissions} for user {UserId}", 
                string.Join(", ", requirement.Permissions), context.User.FindFirst("UserId")?.Value);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}