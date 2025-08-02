namespace StrideHR.Core.Models.DSR;

public class ProductivitySummary
{
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTeamMembers { get; set; }
    public decimal AverageProductivity { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public int TotalDSRsSubmitted { get; set; }
    public int ExpectedDSRs { get; set; }
    public decimal DSRSubmissionRate { get; set; }
    public List<EmployeeProductivitySummary> TeamMemberProductivity { get; set; } = new();
}

public class EmployeeProductivitySummary
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public decimal WorkingHours { get; set; }
    public decimal ProductivityPercentage { get; set; }
    public int DSRsSubmitted { get; set; }
    public int ExpectedDSRs { get; set; }
    public string Status { get; set; } = string.Empty; // High, Medium, Low productivity
}