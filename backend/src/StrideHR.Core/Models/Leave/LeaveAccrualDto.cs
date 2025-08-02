using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveAccrualDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal AccrualRate { get; set; }
    public AccrualType AccrualType { get; set; }
    public DateTime AccrualDate { get; set; }
    public string? Notes { get; set; }
    public bool IsProcessed { get; set; }
}

public class CreateLeaveAccrualDto
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal AccruedDays { get; set; }
    public decimal AccrualRate { get; set; }
    public AccrualType AccrualType { get; set; }
    public DateTime AccrualDate { get; set; }
    public string? Notes { get; set; }
}