namespace StrideHR.Core.Entities;

public class KnowledgeBaseDocumentComment : BaseEntity
{
    public int DocumentId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public bool IsInternal { get; set; } = false; // Internal comments for reviewers
    public DateTime PostedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public new bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public virtual KnowledgeBaseDocument Document { get; set; } = null!;
    public virtual Employee Author { get; set; } = null!;
    public virtual KnowledgeBaseDocumentComment? ParentComment { get; set; }
    public virtual ICollection<KnowledgeBaseDocumentComment> Replies { get; set; } = new List<KnowledgeBaseDocumentComment>();
}