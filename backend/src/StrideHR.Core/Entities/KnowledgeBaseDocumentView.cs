namespace StrideHR.Core.Entities;

public class KnowledgeBaseDocumentView : BaseEntity
{
    public int DocumentId { get; set; }
    public int? ViewedBy { get; set; } // Nullable for anonymous views
    public DateTime ViewedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public TimeSpan? ReadDuration { get; set; }
    public bool IsUniqueView { get; set; } = true; // First view by this user

    // Navigation Properties
    public virtual KnowledgeBaseDocument Document { get; set; } = null!;
    public virtual Employee? ViewedByEmployee { get; set; }
}