using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveCalendarDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveRequestId { get; set; }
    public DateTime Date { get; set; }
    public bool IsFullDay { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
}