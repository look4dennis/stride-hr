using Microsoft.AspNetCore.Authorization;

namespace StrideHR.API.Attributes;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission:{permission}";
    }

    public RequirePermissionAttribute(string resource, string action)
    {
        Policy = $"Permission:{resource}.{action}";
    }
}

public class RequireBranchAccessAttribute : AuthorizeAttribute
{
    public RequireBranchAccessAttribute()
    {
        Policy = "BranchAccess";
    }
}

public class RequireRoleHierarchyAttribute : AuthorizeAttribute
{
    public RequireRoleHierarchyAttribute(int minimumLevel)
    {
        Policy = $"RoleHierarchy:{minimumLevel}";
    }
}

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}