namespace StrideHR.Core.Models.Project;

public class ProjectHoursReportDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal TotalHoursWorked { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal HoursVariance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DailyHoursDto> DailyHours { get; set; } = new List<DailyHoursDto>();
}