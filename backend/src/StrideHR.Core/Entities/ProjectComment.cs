namespace StrideHR.Core.Entities;

public class ProjectComment : BaseEntity
{
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int EmployeeId { get; set; }
    public string Comment { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual ProjectTask? Task { get; set; }
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<ProjectCommentReply> Replies { get; set; } = new List<ProjectCommentReply>();
}

public class ProjectCommentReply : BaseEntity
{
    public int CommentId { get; set; }
    public int EmployeeId { get; set; }
    public string Reply { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual ProjectComment Comment { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}

public class ProjectActivity : BaseEntity
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}

public class ProjectRisk : BaseEntity
{
    public int ProjectId { get; set; }
    public string RiskType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
    public string MitigationPlan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedTo { get; set; }
    public DateTime IdentifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual Employee? AssignedToEmployee { get; set; }
}