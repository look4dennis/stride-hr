using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class LeaveCalendar : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveRequestId { get; set; }
    public DateTime Date { get; set; }
    public bool IsFullDay { get; set; } = true;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public LeaveType LeaveType { get; set; }
    
    // Navigation Properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual LeaveRequest LeaveRequest { get; set; } = null!;
}