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
    public int MinimumLevel => MinimumHierarchyLevel; // Alias for compatibility

    public RoleHierarchyRequirement(int minimumHierarchyLevel)
    {
        MinimumHierarchyLevel = minimumHierarchyLevel;
    }
}

/// <summary>
/// Authorization requirement for resource ownership validation
/// Ensures users can only access resources they own or have been granted access to
/// </summary>
public class ResourceOwnershipRequirement : IAuthorizationRequirement
{
    public string ResourceType { get; }
    public string ResourceIdParameter { get; }
    public bool AllowManagerAccess { get; }
    public bool AllowHRAccess { get; }

    public ResourceOwnershipRequirement(string resourceType, string resourceIdParameter = "id", 
        bool allowManagerAccess = false, bool allowHRAccess = false)
    {
        ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        ResourceIdParameter = resourceIdParameter;
        AllowManagerAccess = allowManagerAccess;
        AllowHRAccess = allowHRAccess;
    }
}

/// <summary>
/// Authorization requirement for time-based access control
/// Ensures operations are performed within allowed time windows
/// </summary>
public class TimeBasedAccessRequirement : IAuthorizationRequirement
{
    public TimeSpan? AllowedStartTime { get; }
    public TimeSpan? AllowedEndTime { get; }
    public DayOfWeek[]? AllowedDays { get; }
    public bool RespectUserTimeZone { get; }

    public TimeBasedAccessRequirement(TimeSpan? allowedStartTime = null, TimeSpan? allowedEndTime = null,
        DayOfWeek[]? allowedDays = null, bool respectUserTimeZone = true)
    {
        AllowedStartTime = allowedStartTime;
        AllowedEndTime = allowedEndTime;
        AllowedDays = allowedDays;
        RespectUserTimeZone = respectUserTimeZone;
    }
}