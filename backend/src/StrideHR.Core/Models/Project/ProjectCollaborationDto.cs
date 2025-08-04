namespace StrideHR.Core.Models.Project;

public class ProjectCollaborationDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public List<ProjectCommentDto> Comments { get; set; } = new();
    public List<ProjectActivityDto> Activities { get; set; } = new();
    public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();
    public ProjectCommunicationStatsDto CommunicationStats { get; set; } = new();
}

public class ProjectCommentDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeePhoto { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProjectCommentReplyDto> Replies { get; set; } = new();
}

public class ProjectCommentReplyDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeePhoto { get; set; } = string.Empty;
    public string Reply { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProjectActivityDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProjectCommunicationStatsDto
{
    public int TotalComments { get; set; }
    public int TotalActivities { get; set; }
    public int ActiveTeamMembers { get; set; }
    public DateTime LastActivity { get; set; }
    public List<TeamMemberActivityDto> MemberActivities { get; set; } = new();
}

public class TeamMemberActivityDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CommentsCount { get; set; }
    public int ActivitiesCount { get; set; }
    public DateTime LastActivity { get; set; }
}

public class CreateProjectCommentDto
{
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class CreateCommentReplyDto
{
    public int CommentId { get; set; }
    public string Reply { get; set; } = string.Empty;
}