using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// User branch access model for branch-based data isolation
/// </summary>
public class UserBranchAccess : BaseEntity
{
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    public int BranchId { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    [Required]
    [MaxLength(450)]
    public string GrantedBy { get; set; } = string.Empty;
    
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(450)]
    public string? RevokedBy { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Branch Branch { get; set; } = null!;
}