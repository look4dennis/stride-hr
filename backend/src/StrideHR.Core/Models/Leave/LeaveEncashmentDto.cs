using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveEncashmentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal EncashedDays { get; set; }
    public decimal EncashmentRate { get; set; }
    public decimal EncashmentAmount { get; set; }
    public DateTime EncashmentDate { get; set; }
    public EncashmentStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Reason { get; set; }
    public string? Comments { get; set; }
}

public class CreateLeaveEncashmentDto
{
    public int EmployeeId { get; set; }
    public int LeavePolicyId { get; set; }
    public int Year { get; set; }
    public decimal EncashedDays { get; set; }
    public string? Reason { get; set; }
    public string? Comments { get; set; }
}

public class ApproveLeaveEncashmentDto
{
    public decimal? ApprovedDays { get; set; }
    public string? Comments { get; set; }
}