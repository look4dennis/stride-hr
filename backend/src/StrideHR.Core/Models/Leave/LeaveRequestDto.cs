using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeavePolicyId { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RequestedDays { get; set; }
    public decimal ApprovedDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public LeaveStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsEmergency { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LeaveApprovalHistoryDto> ApprovalHistory { get; set; } = new();
}