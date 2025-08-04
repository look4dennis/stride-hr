namespace StrideHR.Infrastructure.DTOs;

public class CreateDSRDto
{
    public DateTime Date { get; set; }
    public string TasksCompleted { get; set; } = string.Empty;
    public string TasksPlanned { get; set; } = string.Empty;
    public string? Issues { get; set; }
    public string? Notes { get; set; }
    public int ProjectId { get; set; }
    public int TaskId { get; set; }
    public decimal HoursWorked { get; set; }
    public string Description { get; set; } = string.Empty;
}