using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayslipTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; } // Null means organization-wide template
    
    // Template Design Configuration (JSON)
    public string TemplateConfig { get; set; } = "{}"; // Stores drag-and-drop layout configuration
    
    // Header Configuration
    public bool ShowOrganizationLogo { get; set; } = true;
    public string HeaderText { get; set; } = string.Empty;
    public string HeaderColor { get; set; } = "#3b82f6";
    
    // Footer Configuration
    public string FooterText { get; set; } = string.Empty;
    public bool ShowDigitalSignature { get; set; } = true;
    
    // Field Visibility Configuration
    public string VisibleFields { get; set; } = "{}"; // JSON array of visible field names
    public string FieldLabels { get; set; } = "{}"; // JSON object for custom field labels
    
    // Styling Configuration
    public string PrimaryColor { get; set; } = "#3b82f6";
    public string SecondaryColor { get; set; } = "#6b7280";
    public string FontFamily { get; set; } = "Inter";
    public int FontSize { get; set; } = 12;
    
    // Status and Metadata
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public new int CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int? LastModifiedBy { get; set; }
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual Employee? LastModifiedByEmployee { get; set; }
    public virtual ICollection<PayslipGeneration> PayslipGenerations { get; set; } = new List<PayslipGeneration>();
}