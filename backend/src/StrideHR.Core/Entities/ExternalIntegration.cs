using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExternalIntegration : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public string SystemType { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty; // JSON configuration
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<IntegrationLog> Logs { get; set; } = new List<IntegrationLog>();
}

public class IntegrationLog : BaseEntity
{
    public int ExternalIntegrationId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Navigation properties
    public virtual ExternalIntegration ExternalIntegration { get; set; } = null!;
}