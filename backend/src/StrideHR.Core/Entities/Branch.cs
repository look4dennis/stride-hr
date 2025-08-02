namespace StrideHR.Core.Entities;

public class Branch : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty; // ISO country code
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string LocalHolidays { get; set; } = "[]"; // JSON string
    public string ComplianceSettings { get; set; } = "{}"; // JSON string for local labor laws, tax settings, etc.
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<LeavePolicy> LeavePolicies { get; set; } = new List<LeavePolicy>();
}