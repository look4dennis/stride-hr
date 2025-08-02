using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ProjectAlert : BaseEntity
{
    public int ProjectId { get; set; }
    public ProjectAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public bool IsResolved { get; set; }
    public int? ResolvedByEmployeeId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual Employee? ResolvedByEmployee { get; set; }
}