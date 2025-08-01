using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for branch-based data isolation operations
/// </summary>
public class BranchIsolationService : IBranchIsolationService
{
    private readonly StrideHRDbContext _context;
    private readonly ILogger<BranchIsolationService> _logger;
    private readonly IAuditService _auditService;
    private readonly IRoleService _roleService;

    public BranchIsolationService(
        StrideHRDbContext context,
        ILogger<BranchIsolationService> logger,
        IAuditService auditService,
        IRoleService roleService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
        _roleService = roleService;
    }

    public async Task<bool> ValidateBranchAccessAsync(int branchId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user is super admin
            if (await IsSuperAdminAsync(userId, cancellationToken))
            {
                return true;
            }

            // Check if branch isolation is enabled for this branch's organization
            var branch = await _context.Branches
                .Include(b => b.Organization)
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted, cancellationToken);

            if (branch == null)
            {
                return false;
            }

            // If branch isolation is not enabled, allow access
            if (!branch.Organization.BranchIsolationEnabled)
            {
                return true;
            }

            // Check user's branch access
            var hasAccess = await _context.Set<UserBranchAccess>()
                .AnyAsync(uba => uba.UserId == userId && 
                               uba.BranchId == branchId && 
                               uba.IsActive && 
                               !uba.IsDeleted, cancellationToken);

            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating branch access for user {UserId} and branch {BranchId}", userId, branchId);
            return false;
        }
    }

    public async Task<IEnumerable<int>> GetUserAccessibleBranchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user is super admin
            if (await IsSuperAdminAsync(userId, cancellationToken))
            {
                // Super admin has access to all active branches
                var allBranches = await _context.Branches
                    .Where(b => b.IsActive && !b.IsDeleted)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);
                return allBranches;
            }

            // Get user's accessible branches
            var accessibleBranches = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && uba.IsActive && !uba.IsDeleted)
                .Include(uba => uba.Branch)
                .Where(uba => uba.Branch.IsActive && !uba.Branch.IsDeleted)
                .Select(uba => uba.BranchId)
                .ToListAsync(cancellationToken);

            return accessibleBranches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible branches for user {UserId}", userId);
            return Enumerable.Empty<int>();
        }
    }

    public async Task<IEnumerable<Branch>> GetUserAccessibleBranchesWithDetailsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user is super admin
            if (await IsSuperAdminAsync(userId, cancellationToken))
            {
                // Super admin has access to all active branches
                var allBranches = await _context.Branches
                    .Include(b => b.Organization)
                    .Where(b => b.IsActive && !b.IsDeleted)
                    .OrderBy(b => b.Name)
                    .ToListAsync(cancellationToken);
                return allBranches;
            }

            // Get user's accessible branches with details
            var accessibleBranches = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && uba.IsActive && !uba.IsDeleted)
                .Include(uba => uba.Branch)
                .ThenInclude(b => b.Organization)
                .Where(uba => uba.Branch.IsActive && !uba.Branch.IsDeleted)
                .Select(uba => uba.Branch)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);

            return accessibleBranches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible branches with details for user {UserId}", userId);
            return Enumerable.Empty<Branch>();
        }
    }

    public async Task<bool> IsBranchIsolationEnabledAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            return organization?.BranchIsolationEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking branch isolation status for organization {OrganizationId}", organizationId);
            return false;
        }
    }

    public async Task<IQueryable<T>> ApplyBranchIsolationAsync<T>(IQueryable<T> query, string userId, CancellationToken cancellationToken = default) 
        where T : class, IBranchEntity
    {
        try
        {
            // Check if user is super admin
            if (await IsSuperAdminAsync(userId, cancellationToken))
            {
                return query; // No filtering for super admin
            }

            // Get user's accessible branches
            var accessibleBranchIds = await GetUserAccessibleBranchesAsync(userId, cancellationToken);
            
            if (!accessibleBranchIds.Any())
            {
                // User has no branch access, return empty query
                return query.Where(x => false);
            }

            // Filter query by accessible branches
            return query.Where(x => accessibleBranchIds.Contains(x.BranchId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying branch isolation for user {UserId}", userId);
            return query.Where(x => false); // Return empty query on error for security
        }
    }

    public async Task<bool> GrantBranchAccessAsync(string userId, IEnumerable<int> branchIds, string grantedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var branchIdsList = branchIds.ToList();
            
            // Validate that all branches exist and are active
            var validBranches = await _context.Branches
                .Where(b => branchIdsList.Contains(b.Id) && b.IsActive && !b.IsDeleted)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            if (validBranches.Count != branchIdsList.Count)
            {
                var invalidBranches = branchIdsList.Except(validBranches);
                _logger.LogWarning("Invalid or inactive branches found: {InvalidBranches}", string.Join(", ", invalidBranches));
            }

            // Get existing access records
            var existingAccess = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && validBranches.Contains(uba.BranchId) && !uba.IsDeleted)
                .ToListAsync(cancellationToken);

            var newAccessRecords = new List<UserBranchAccess>();

            foreach (var branchId in validBranches)
            {
                var existing = existingAccess.FirstOrDefault(ea => ea.BranchId == branchId);
                
                if (existing == null)
                {
                    // Create new access record
                    newAccessRecords.Add(new UserBranchAccess
                    {
                        UserId = userId,
                        BranchId = branchId,
                        GrantedBy = grantedBy,
                        GrantedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = grantedBy
                    });
                }
                else if (!existing.IsActive)
                {
                    // Reactivate existing record
                    existing.IsActive = true;
                    existing.GrantedBy = grantedBy;
                    existing.GrantedAt = DateTime.UtcNow;
                    existing.RevokedBy = null;
                    existing.RevokedAt = null;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = grantedBy;
                }
            }

            if (newAccessRecords.Any())
            {
                await _context.Set<UserBranchAccess>().AddRangeAsync(newAccessRecords, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogAsync("UserBranchAccess", 0, "GRANT", 
                $"Branch access granted to user {userId} for branches: {string.Join(", ", validBranches)}", cancellationToken);

            _logger.LogInformation("Branch access granted to user {UserId} for branches: {BranchIds}", userId, string.Join(", ", validBranches));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting branch access to user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RevokeBranchAccessAsync(string userId, IEnumerable<int> branchIds, string revokedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var branchIdsList = branchIds.ToList();
            
            // Get existing access records
            var existingAccess = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && branchIdsList.Contains(uba.BranchId) && uba.IsActive && !uba.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!existingAccess.Any())
            {
                _logger.LogWarning("No active branch access found for user {UserId} and branches: {BranchIds}", userId, string.Join(", ", branchIdsList));
                return false;
            }

            // Revoke access
            foreach (var access in existingAccess)
            {
                access.IsActive = false;
                access.RevokedBy = revokedBy;
                access.RevokedAt = DateTime.UtcNow;
                access.UpdatedAt = DateTime.UtcNow;
                access.UpdatedBy = revokedBy;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log audit trail
            var revokedBranchIds = existingAccess.Select(ea => ea.BranchId);
            await _auditService.LogAsync("UserBranchAccess", 0, "REVOKE", 
                $"Branch access revoked from user {userId} for branches: {string.Join(", ", revokedBranchIds)}", cancellationToken);

            _logger.LogInformation("Branch access revoked from user {UserId} for branches: {BranchIds}", userId, string.Join(", ", revokedBranchIds));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking branch access from user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetBranchUsersAsync(int branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.BranchId == branchId && uba.IsActive && !uba.IsDeleted)
                .Select(uba => uba.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // For now, just return users with explicit access
            // Super admin logic would be added here when role service is fully implemented
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users for branch {BranchId}", branchId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsSuperAdminAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _roleService.IsUserInRoleAsync(userId, "SuperAdmin", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking super admin status for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int?> GetUserPrimaryBranchAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var primaryBranch = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && uba.IsPrimary && uba.IsActive && !uba.IsDeleted)
                .Select(uba => uba.BranchId)
                .FirstOrDefaultAsync(cancellationToken);

            return primaryBranch == 0 ? null : primaryBranch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary branch for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> SetUserPrimaryBranchAsync(string userId, int branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate that user has access to this branch
            if (!await ValidateBranchAccessAsync(branchId, userId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} does not have access to branch {BranchId}", userId, branchId);
                return false;
            }

            // Remove primary flag from all user's branches
            var userAccess = await _context.Set<UserBranchAccess>()
                .Where(uba => uba.UserId == userId && uba.IsActive && !uba.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var access in userAccess)
            {
                access.IsPrimary = access.BranchId == branchId;
                access.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogAsync("UserBranchAccess", 0, "SET_PRIMARY", 
                $"Primary branch set to {branchId} for user {userId}", cancellationToken);

            _logger.LogInformation("Primary branch set to {BranchId} for user {UserId}", branchId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary branch for user {UserId}", userId);
            return false;
        }
    }
}