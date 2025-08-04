using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.DTOs;

public class CreateLeaveRequestDto
{
    public int LeavePolicyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public string? Notes { get; set; }
    public bool IsEmergency { get; set; }
}

public class ApproveLeaveRequestDto
{
    public LeaveApprovalStatus Status { get; set; }
    public string? Notes { get; set; }
    public LeaveApprovalStatus ApprovalStatus { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime ApprovalDate { get; set; }
}