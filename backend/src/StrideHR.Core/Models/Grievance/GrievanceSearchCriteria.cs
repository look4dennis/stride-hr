using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Grievance;

public class GrievanceSearchCriteria
{
    public string? SearchTerm { get; set; }
    public GrievanceStatus? Status { get; set; }
    public GrievanceCategory? Category { get; set; }
    public GrievancePriority? Priority { get; set; }
    public EscalationLevel? EscalationLevel { get; set; }
    public int? SubmittedById { get; set; }
    public int? AssignedToId { get; set; }
    public bool? IsAnonymous { get; set; }
    public bool? IsEscalated { get; set; }
    public bool? RequiresInvestigation { get; set; }
    public bool? IsOverdue { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}