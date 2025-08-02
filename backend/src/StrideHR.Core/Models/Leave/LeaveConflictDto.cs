namespace StrideHR.Core.Models.Leave;

public class LeaveConflictDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime ConflictDate { get; set; }
    public string ConflictReason { get; set; } = string.Empty;
    public int ConflictingRequestId { get; set; }
}