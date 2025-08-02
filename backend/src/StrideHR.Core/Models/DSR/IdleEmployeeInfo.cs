namespace StrideHR.Core.Models.DSR;

public class IdleEmployeeInfo
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal WorkingHours { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal IdleHours { get; set; }
    public decimal IdlePercentage { get; set; }
    public string Reason { get; set; } = string.Empty; // No DSR, Insufficient hours, etc.
    public bool HasDSR { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
}