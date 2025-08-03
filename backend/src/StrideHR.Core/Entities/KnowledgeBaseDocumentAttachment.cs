namespace StrideHR.Core.Entities;

public class KnowledgeBaseDocumentAttachment : BaseEntity
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public int DownloadCount { get; set; } = 0;

    // Navigation Properties
    public virtual KnowledgeBaseDocument Document { get; set; } = null!;
    public virtual Employee UploadedByEmployee { get; set; } = null!;
}