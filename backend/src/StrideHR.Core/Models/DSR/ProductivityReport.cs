namespace StrideHR.Core.Models.DSR;

public class ProductivityReport
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal ProductiveHours { get; set; }
    public decimal IdleHours { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public int TotalDSRsSubmitted { get; set; }
    public int TotalWorkingDays { get; set; }
    public List<DailyProductivity> DailyBreakdown { get; set; } = new();
}

public class DailyProductivity
{
    public DateTime Date { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal WorkingHours { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public bool HasDSR { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Absent, Leave, etc.
}