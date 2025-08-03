namespace StrideHR.Core.Models.Grievance;

public class GrievanceFollowUpDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ScheduledById { get; set; }
    public string ScheduledByName { get; set; } = string.Empty;
    public int? CompletedById { get; set; }
    public string? CompletedByName { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGrievanceFollowUpDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
}

public class CompleteGrievanceFollowUpDto
{
    public string? CompletionNotes { get; set; }
}