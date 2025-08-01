namespace StrideHR.Core.Models.Project;

public class ProjectTeamMemberDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public bool IsTeamLead { get; set; }
    public DateTime AssignedDate { get; set; }
    public string? Role { get; set; }
    public decimal? HourlyRate { get; set; }
}