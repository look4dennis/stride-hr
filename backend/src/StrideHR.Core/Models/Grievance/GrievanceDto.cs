using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Grievance;

public class GrievanceDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GrievanceCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public GrievancePriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public GrievanceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public int SubmittedById { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public string SubmitterEmail { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public EscalationLevel CurrentEscalationLevel { get; set; }
    public string EscalationLevelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Resolution { get; set; }
    public string? ResolutionNotes { get; set; }
    public int? ResolvedById { get; set; }
    public string? ResolvedByName { get; set; }
    public string? AttachmentPath { get; set; }
    public bool RequiresInvestigation { get; set; }
    public string? InvestigationNotes { get; set; }
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public int? EscalatedById { get; set; }
    public string? EscalatedByName { get; set; }
    public string? EscalationReason { get; set; }
    public TimeSpan? ResolutionTime { get; set; }
    public int CommentsCount { get; set; }
    public int EscalationsCount { get; set; }
    public int FollowUpsCount { get; set; }
    public bool IsOverdue { get; set; }
}