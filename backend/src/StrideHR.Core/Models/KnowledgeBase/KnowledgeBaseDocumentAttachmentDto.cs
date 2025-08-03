namespace StrideHR.Core.Models.KnowledgeBase;

public class KnowledgeBaseDocumentAttachmentDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public int UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int DownloadCount { get; set; }
}