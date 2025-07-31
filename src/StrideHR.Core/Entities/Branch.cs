using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Entities;

/// <summary>
/// Branch entity representing different office locations/branches
/// </summary>
public class Branch : AuditableEntity
{
    public int OrganizationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    [MaxLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(50)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Local holidays specific to this branch (stored as JSON array)
    /// </summary>
    public string? LocalHolidays { get; set; }
    
    /// <summary>
    /// Compliance settings specific to this branch/country (stored as JSON)
    /// </summary>
    public string? ComplianceSettings { get; set; }
    
    /// <summary>
    /// Employee ID pattern for this branch (e.g., "NYC-HR-{YYYY}-{###}")
    /// </summary>
    [MaxLength(50)]
    public string? EmployeeIdPattern { get; set; }
    
    /// <summary>
    /// Branch-specific working hours
    /// </summary>
    public TimeSpan? WorkingHoursStart { get; set; }
    
    public TimeSpan? WorkingHoursEnd { get; set; }
    
    /// <summary>
    /// Is this branch currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}