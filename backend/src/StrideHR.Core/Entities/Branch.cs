namespace StrideHR.Core.Entities;

public class Branch : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LocalHolidays { get; set; } = "[]"; // JSON string
    public string ComplianceSettings { get; set; } = "{}"; // JSON string
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}