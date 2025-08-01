namespace StrideHR.Core.Models.Project;

public class TaskAssignmentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}