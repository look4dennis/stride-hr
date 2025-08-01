using Microsoft.AspNetCore.Authorization;

namespace StrideHR.Core.Models.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class BranchAccessRequirement : IAuthorizationRequirement
{
    public bool RequireSameBranch { get; }

    public BranchAccessRequirement(bool requireSameBranch = true)
    {
        RequireSameBranch = requireSameBranch;
    }
}

public class RoleHierarchyRequirement : IAuthorizationRequirement
{
    public int MinimumHierarchyLevel { get; }

    public RoleHierarchyRequirement(int minimumHierarchyLevel)
    {
        MinimumHierarchyLevel = minimumHierarchyLevel;
    }
}