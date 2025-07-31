using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Organization entity representing the main company/organization
/// </summary>
public class Organization : AuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    /// <summary>
    /// Organization logo file path
    /// </summary>
    [MaxLength(500)]
    public string? LogoPath { get; set; }
    
    /// <summary>
    /// Normal working hours per day (in hours)
    /// </summary>
    public decimal NormalWorkingHours { get; set; } = 8.0m;
    
    /// <summary>
    /// Overtime pay rate multiplier (e.g., 1.5 for 150%)
    /// </summary>
    public decimal OvertimeRate { get; set; } = 1.5m;
    
    /// <summary>
    /// Productive hours threshold for employee productivity tracking
    /// </summary>
    public int ProductiveHoursThreshold { get; set; } = 6;
    
    /// <summary>
    /// Enable/disable branch-based data isolation
    /// </summary>
    public bool BranchIsolationEnabled { get; set; } = false;
    
    /// <summary>
    /// Organization-wide settings stored as JSON
    /// </summary>
    public string? Settings { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}