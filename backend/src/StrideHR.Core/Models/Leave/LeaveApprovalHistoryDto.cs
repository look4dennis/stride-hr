using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveApprovalHistoryDto
{
    public int Id { get; set; }
    public int LeaveRequestId { get; set; }
    public int ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalLevel Level { get; set; }
    public ApprovalAction Action { get; set; }
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
    public int? EscalatedToId { get; set; }
    public string? EscalatedToName { get; set; }
}