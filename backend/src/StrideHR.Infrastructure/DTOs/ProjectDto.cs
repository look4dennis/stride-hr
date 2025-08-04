using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.DTOs;

public class AssignTeamMembersDto
{
    public List<int> EmployeeIds { get; set; } = new();
    public int ProjectId { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StrideHR.Core.Enums.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
}

public class AssignTaskDto
{
    public int EmployeeId { get; set; }
    public int TaskId { get; set; }
    public int AssignedToId { get; set; }
}