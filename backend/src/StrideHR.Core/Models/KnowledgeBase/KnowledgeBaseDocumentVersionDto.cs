using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.KnowledgeBase;

public class KnowledgeBaseDocumentVersionDto
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCurrentVersion { get; set; }
    public string? VersionNotes { get; set; }
}