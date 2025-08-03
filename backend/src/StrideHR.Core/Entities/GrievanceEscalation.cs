using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class GrievanceEscalation : BaseEntity
{
    public int GrievanceId { get; set; }
    public EscalationLevel FromLevel { get; set; }
    public EscalationLevel ToLevel { get; set; }
    public int EscalatedById { get; set; }
    public int? EscalatedToId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAutoEscalation { get; set; } = false; // True if escalated due to SLA breach
    
    // Navigation Properties
    public virtual Grievance Grievance { get; set; } = null!;
    public virtual Employee EscalatedBy { get; set; } = null!;
    public virtual Employee? EscalatedTo { get; set; }
}