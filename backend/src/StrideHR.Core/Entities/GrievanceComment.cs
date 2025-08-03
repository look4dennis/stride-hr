namespace StrideHR.Core.Entities;

public class GrievanceComment : BaseEntity
{
    public int GrievanceId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public bool IsInternal { get; set; } = false; // Internal comments only visible to HR/Admin
    public string? AttachmentPath { get; set; }
    
    // Navigation Properties
    public virtual Grievance Grievance { get; set; } = null!;
    public virtual Employee Author { get; set; } = null!;
}