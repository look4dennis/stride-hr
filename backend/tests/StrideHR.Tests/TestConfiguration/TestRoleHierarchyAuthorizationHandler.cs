using Microsoft.AspNetCore.Authorization;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authorization;
using StrideHR.Infrastructure.Authorization;

namespace StrideHR.Tests.TestConfiguration;

public class TestRoleHierarchyAuthorizationHandler : RoleHierarchyAuthorizationHandler 
{
    public TestRoleHierarchyAuthorizationHandler(IRoleService roleService)
        : base(roleService)
    {
    }

    public new Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleHierarchyRequirement requirement)
    {
        return base.HandleRequirementAsync(context, requirement);
    }
}
