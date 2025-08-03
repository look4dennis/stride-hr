namespace StrideHR.Core.Entities;

public class GrievanceFollowUp : BaseEntity
{
    public int GrievanceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public int ScheduledById { get; set; }
    public int? CompletedById { get; set; }
    public string? CompletionNotes { get; set; }
    
    // Navigation Properties
    public virtual Grievance Grievance { get; set; } = null!;
    public virtual Employee ScheduledBy { get; set; } = null!;
    public virtual Employee? CompletedBy { get; set; }
}