namespace StrideHR.Core.Models.Project;

public class DailyHoursDto
{
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? TaskId { get; set; }
    public string? TaskTitle { get; set; }
}