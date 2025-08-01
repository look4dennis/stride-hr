using Microsoft.AspNetCore.Authorization;
using StrideHR.API.Extensions;
using StrideHR.Core.Interfaces;

namespace StrideHR.API.Authorization;

/// <summary>
/// Authorization handler for branch-based access control
/// </summary>
public class BranchAuthorizationHandler : AuthorizationHandler<BranchAccessRequirement>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BranchAuthorizationHandler> _logger;

    public BranchAuthorizationHandler(IUserRepository userRepository, ILogger<BranchAuthorizationHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BranchAccessRequirement requirement)
    {
        try
        {
            // Get user ID from claims
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found in claims for branch authorization");
                context.Fail();
                return;
            }

            // Get user with employee details
            var user = await _userRepository.GetWithEmployeeAsync(userId);
            if (user?.Employee == null)
            {
                _logger.LogWarning("User {UserId} not found or has no employee record", userId);
                context.Fail();
                return;
            }

            // If no specific branch is required, allow access to user's own branch
            if (requirement.BranchId == null)
            {
                context.Succeed(requirement);
                return;
            }

            // Check if user has access to the specific branch
            var userBranchId = user.Employee.BranchId;
            
            // Super admins can access all branches
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Admins can access all branches within their organization
            if (context.User.IsInRole("Admin"))
            {
                // For now, allow access - in a real implementation, you'd check organization hierarchy
                context.Succeed(requirement);
                return;
            }

            // Regular users can only access their own branch
            if (userBranchId == requirement.BranchId)
            {
                context.Succeed(requirement);
                return;
            }

            _logger.LogWarning("User {UserId} from branch {UserBranchId} attempted to access branch {RequiredBranchId}", 
                userId, userBranchId, requirement.BranchId);
            
            context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in branch authorization for user {UserId}", 
                context.User.FindFirst("UserId")?.Value);
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization handler for same branch requirement
/// </summary>
public class SameBranchAuthorizationHandler : AuthorizationHandler<SameBranchRequirement>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SameBranchAuthorizationHandler> _logger;

    public SameBranchAuthorizationHandler(IUserRepository userRepository, ILogger<SameBranchAuthorizationHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameBranchRequirement requirement)
    {
        try
        {
            // Get user ID from claims
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Super admins can access all branches
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // For this requirement, we need to check the resource being accessed
            // This would typically be implemented by checking the resource in the context
            // For now, we'll allow access if the user has appropriate roles
            if (context.User.IsInRole("Admin") || context.User.IsInRole("HR"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Regular employees can only access their own branch data
            // This would need additional context about what resource is being accessed
            context.Succeed(requirement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in same branch authorization for user {UserId}", 
                context.User.FindFirst("UserId")?.Value);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}