using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Shift;

public class ShiftSwapRequestDto
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmployeeId { get; set; } = string.Empty;
    public int RequesterShiftAssignmentId { get; set; }
    public string RequesterShiftName { get; set; } = string.Empty;
    public DateTime RequesterShiftDate { get; set; }
    public TimeSpan RequesterShiftStartTime { get; set; }
    public TimeSpan RequesterShiftEndTime { get; set; }
    public int? TargetEmployeeId { get; set; }
    public string? TargetEmployeeName { get; set; }
    public string? TargetEmployeeId_Display { get; set; }
    public int? TargetShiftAssignmentId { get; set; }
    public string? TargetShiftName { get; set; }
    public DateTime? TargetShiftDate { get; set; }
    public TimeSpan? TargetShiftStartTime { get; set; }
    public TimeSpan? TargetShiftEndTime { get; set; }
    public DateTime RequestedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ShiftSwapStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsEmergency { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<ShiftSwapResponseDto> SwapResponses { get; set; } = new();
}

public class CreateShiftSwapRequestDto
{
    public int RequesterShiftAssignmentId { get; set; }
    public int? TargetEmployeeId { get; set; }
    public int? TargetShiftAssignmentId { get; set; }
    public DateTime RequestedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsEmergency { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
}

public class ShiftSwapResponseDto
{
    public int Id { get; set; }
    public int ShiftSwapRequestId { get; set; }
    public int ResponderId { get; set; }
    public string ResponderName { get; set; } = string.Empty;
    public string ResponderEmployeeId { get; set; } = string.Empty;
    public int ResponderShiftAssignmentId { get; set; }
    public string ResponderShiftName { get; set; } = string.Empty;
    public DateTime ResponderShiftDate { get; set; }
    public TimeSpan ResponderShiftStartTime { get; set; }
    public TimeSpan ResponderShiftEndTime { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
    public DateTime RespondedAt { get; set; }
}

public class CreateShiftSwapResponseDto
{
    public int ShiftSwapRequestId { get; set; }
    public int ResponderShiftAssignmentId { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
}

public class ApproveShiftSwapDto
{
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
}

public class ShiftSwapSearchCriteria
{
    public int? RequesterId { get; set; }
    public int? TargetEmployeeId { get; set; }
    public int? BranchId { get; set; }
    public ShiftSwapStatus? Status { get; set; }
    public bool? IsEmergency { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}