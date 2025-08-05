using Microsoft.AspNetCore.Authorization;
using StrideHR.Core.Models.Authorization;
using StrideHR.Infrastructure.Authorization;

namespace StrideHR.Tests.TestConfiguration;

public class TestPermissionAuthorizationHandler : PermissionAuthorizationHandler
{
    public new Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        return base.HandleRequirementAsync(context, requirement);
    }
}
