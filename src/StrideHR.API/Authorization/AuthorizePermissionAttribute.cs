using Microsoft.AspNetCore.Authorization;

namespace StrideHR.API.Authorization;

/// <summary>
/// Authorization attribute for permission-based access control
/// </summary>
public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public AuthorizePermissionAttribute(string permission)
    {
        Policy = $"Permission.{permission}";
    }
}

/// <summary>
/// Authorization attribute for any permission requirement
/// </summary>
public class AuthorizeAnyPermissionAttribute : AuthorizeAttribute
{
    public AuthorizeAnyPermissionAttribute(params string[] permissions)
    {
        Policy = $"AnyPermission.{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Authorization attribute for all permissions requirement
/// </summary>
public class AuthorizeAllPermissionsAttribute : AuthorizeAttribute
{
    public AuthorizeAllPermissionsAttribute(params string[] permissions)
    {
        Policy = $"AllPermissions.{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Authorization attribute for branch access
/// </summary>
public class AuthorizeBranchAttribute : AuthorizeAttribute
{
    public AuthorizeBranchAttribute(int? branchId = null)
    {
        Policy = branchId.HasValue ? $"Branch.{branchId}" : "SameBranch";
    }
}

/// <summary>
/// Authorization attribute for role-based access with branch isolation
/// </summary>
public class AuthorizeRoleWithBranchAttribute : AuthorizeAttribute
{
    public AuthorizeRoleWithBranchAttribute(string role)
    {
        Roles = role;
        Policy = "SameBranchOnly";
    }
}

/// <summary>
/// Authorization attribute for HR operations
/// </summary>
public class AuthorizeHRAttribute : AuthorizeAttribute
{
    public AuthorizeHRAttribute()
    {
        Policy = "RequireHRRole";
    }
}

/// <summary>
/// Authorization attribute for admin operations
/// </summary>
public class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute()
    {
        Policy = "RequireAdminRole";
    }
}

/// <summary>
/// Authorization attribute for manager operations
/// </summary>
public class AuthorizeManagerAttribute : AuthorizeAttribute
{
    public AuthorizeManagerAttribute()
    {
        Policy = "RequireManagerRole";
    }
}