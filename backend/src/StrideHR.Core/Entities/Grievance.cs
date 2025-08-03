using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class Grievance : BaseEntity
{
    public string GrievanceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GrievanceCategory Category { get; set; }
    public GrievancePriority Priority { get; set; }
    public GrievanceStatus Status { get; set; } = GrievanceStatus.Submitted;
    public bool IsAnonymous { get; set; } = false;
    public int SubmittedById { get; set; }
    public int? AssignedToId { get; set; }
    public EscalationLevel CurrentEscalationLevel { get; set; } = EscalationLevel.Level1_DirectManager;
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }
    public int? ResolvedById { get; set; }
    public string? AttachmentPath { get; set; }
    public bool RequiresInvestigation { get; set; } = false;
    public string? InvestigationNotes { get; set; }
    public int? SatisfactionRating { get; set; } // 1-5 scale
    public string? FeedbackComments { get; set; }
    public bool IsEscalated { get; set; } = false;
    public DateTime? EscalatedAt { get; set; }
    public int? EscalatedById { get; set; }
    public string? EscalationReason { get; set; }
    
    // Navigation Properties
    public virtual Employee SubmittedBy { get; set; } = null!;
    public virtual Employee? AssignedTo { get; set; }
    public virtual Employee? ResolvedBy { get; set; }
    public virtual Employee? EscalatedBy { get; set; }
    public virtual ICollection<GrievanceComment> Comments { get; set; } = new List<GrievanceComment>();
    public virtual ICollection<GrievanceStatusHistory> StatusHistory { get; set; } = new List<GrievanceStatusHistory>();
    public virtual ICollection<GrievanceEscalation> Escalations { get; set; } = new List<GrievanceEscalation>();
    public virtual ICollection<GrievanceFollowUp> FollowUps { get; set; } = new List<GrievanceFollowUp>();
}