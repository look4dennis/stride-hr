using Microsoft.AspNetCore.Authorization;
using StrideHR.Core.Models.Authorization;
using StrideHR.Infrastructure.Authorization;
using Microsoft.AspNetCore.Http;

namespace StrideHR.Tests.TestConfiguration;

public class TestBranchAccessAuthorizationHandler : BranchAccessAuthorizationHandler
{
    public TestBranchAccessAuthorizationHandler(IHttpContextAccessor httpContextAccessor) 
        : base(httpContextAccessor)
    {
    }

    public new Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BranchAccessRequirement requirement)
    {
        return base.HandleRequirementAsync(context, requirement);
    }
}
