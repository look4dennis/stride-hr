using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Junction table linking employees to roles
/// </summary>
public class EmployeeRole : BaseEntity
{
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
    
    /// <summary>
    /// Role assignment date
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Role assignment end date (null for active roles)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Is this role assignment currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Who assigned this role
    /// </summary>
    public int? AssignedBy { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

/// <summary>
/// Role entity for role-based access control
/// </summary>
public class Role : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Role hierarchy level (higher number = higher authority)
    /// </summary>
    public int HierarchyLevel { get; set; } = 1;
    
    /// <summary>
    /// Is this role currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Is this a system role (cannot be deleted)
    /// </summary>
    public bool IsSystemRole { get; set; } = false;
    
    /// <summary>
    /// Role color for UI display
    /// </summary>
    [MaxLength(7)]
    public string? ColorCode { get; set; }
    
    /// <summary>
    /// Role icon for UI display
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// Maximum number of users that can have this role (null = unlimited)
    /// </summary>
    public int? MaxUsers { get; set; }
    
    /// <summary>
    /// Role-specific settings and configurations (stored as JSON)
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation Properties
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Permission entity for granular access control
/// </summary>
public class Permission : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Permission category for grouping
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }
    
    /// <summary>
    /// Is this a system permission (cannot be deleted)
    /// </summary>
    public bool IsSystemPermission { get; set; } = false;
    
    /// <summary>
    /// Permission level (1 = Basic, 2 = Intermediate, 3 = Advanced, 4 = Admin)
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Dependencies - other permissions required for this permission
    /// </summary>
    public string? Dependencies { get; set; }
    
    /// <summary>
    /// Is this permission currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Junction table linking roles to permissions
/// </summary>
public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    
    /// <summary>
    /// Permission grant date
    /// </summary>
    public DateTime GrantedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Who granted this permission
    /// </summary>
    public int? GrantedBy { get; set; }
    
    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}