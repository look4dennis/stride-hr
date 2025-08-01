using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Interface for branch-based data isolation operations
/// </summary>
public interface IBranchIsolationService
{
    /// <summary>
    /// Validate if a user has access to a specific branch
    /// </summary>
    Task<bool> ValidateBranchAccessAsync(int branchId, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all branches accessible to a user
    /// </summary>
    Task<IEnumerable<int>> GetUserAccessibleBranchesAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all branches accessible to a user with full branch information
    /// </summary>
    Task<IEnumerable<Branch>> GetUserAccessibleBranchesWithDetailsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if branch isolation is enabled for an organization
    /// </summary>
    Task<bool> IsBranchIsolationEnabledAsync(int organizationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply branch isolation filter to a query based on user permissions
    /// </summary>
    Task<IQueryable<T>> ApplyBranchIsolationAsync<T>(IQueryable<T> query, string userId, CancellationToken cancellationToken = default) 
        where T : class, IBranchEntity;
    
    /// <summary>
    /// Grant user access to specific branches
    /// </summary>
    Task<bool> GrantBranchAccessAsync(string userId, IEnumerable<int> branchIds, string grantedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoke user access from specific branches
    /// </summary>
    Task<bool> RevokeBranchAccessAsync(string userId, IEnumerable<int> branchIds, string revokedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get users with access to a specific branch
    /// </summary>
    Task<IEnumerable<string>> GetBranchUsersAsync(int branchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a user is a super admin (has access to all branches)
    /// </summary>
    Task<bool> IsSuperAdminAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the primary branch for a user
    /// </summary>
    Task<int?> GetUserPrimaryBranchAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set the primary branch for a user
    /// </summary>
    Task<bool> SetUserPrimaryBranchAsync(string userId, int branchId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for entities that belong to a branch (for data isolation)
/// </summary>
public interface IBranchEntity
{
    int BranchId { get; set; }
}



/// <summary>
/// Branch access request model
/// </summary>
public class BranchAccessRequest
{
    public string UserId { get; set; } = string.Empty;
    public List<int> BranchIds { get; set; } = new();
    public int? PrimaryBranchId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// Branch access response model
/// </summary>
public class BranchAccessResponse
{
    public string UserId { get; set; } = string.Empty;
    public List<BranchAccessInfo> BranchAccess { get; set; } = new();
    public int? PrimaryBranchId { get; set; }
    public bool IsSuperAdmin { get; set; } = false;
}

/// <summary>
/// Branch access information model
/// </summary>
public class BranchAccessInfo
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}