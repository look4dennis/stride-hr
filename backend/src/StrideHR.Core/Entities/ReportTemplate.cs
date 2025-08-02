using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ReportTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty; // JSON configuration
    public string DefaultFilters { get; set; } = string.Empty; // JSON filters
    public string DefaultColumns { get; set; } = string.Empty; // JSON column definitions
    public string DefaultChartConfiguration { get; set; } = string.Empty; // JSON chart config
    public bool IsSystemTemplate { get; set; }
    public bool IsActive { get; set; }
    public new int? CreatedBy { get; set; }

    // Navigation Properties
    public virtual Employee? CreatedByEmployee { get; set; }
}