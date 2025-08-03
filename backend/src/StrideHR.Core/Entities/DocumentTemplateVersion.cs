namespace StrideHR.Core.Entities;

public class DocumentTemplateVersion : BaseEntity
{
    public int DocumentTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public string[] MergeFields { get; set; } = Array.Empty<string>();
    public string ChangeLog { get; set; } = string.Empty;
    public new int CreatedBy { get; set; }
    public bool IsActive { get; set; } = false;

    // Navigation Properties
    public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
}