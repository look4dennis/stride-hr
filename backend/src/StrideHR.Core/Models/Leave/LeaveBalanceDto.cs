using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
    public decimal EncashedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal SickLeaveUsed { get; set; }
    public decimal EmergencyLeaveUsed { get; set; }
}