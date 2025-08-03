namespace StrideHR.Core.Models.Grievance;

public class GrievanceCommentDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGrievanceCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
    public string? AttachmentPath { get; set; }
}