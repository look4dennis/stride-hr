using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Shift;

public class ShiftCoverageRequestDto
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmployeeId { get; set; } = string.Empty;
    public int ShiftAssignmentId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateTime ShiftDate { get; set; }
    public TimeSpan ShiftStartTime { get; set; }
    public TimeSpan ShiftEndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ShiftCoverageRequestStatus Status { get; set; }
    public bool IsEmergency { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? AcceptedBy { get; set; }
    public string? AcceptedByName { get; set; }
    public string? AcceptedByEmployeeId { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptanceNotes { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<ShiftCoverageResponseDto> CoverageResponses { get; set; } = new();
}

public class CreateShiftCoverageRequestDto
{
    public int ShiftAssignmentId { get; set; }
    public DateTime ShiftDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsEmergency { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
}

public class ShiftCoverageResponseDto
{
    public int Id { get; set; }
    public int ShiftCoverageRequestId { get; set; }
    public int ResponderId { get; set; }
    public string ResponderName { get; set; } = string.Empty;
    public string ResponderEmployeeId { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
    public DateTime RespondedAt { get; set; }
}

public class CreateShiftCoverageResponseDto
{
    public int ShiftCoverageRequestId { get; set; }
    public bool IsAccepted { get; set; }
    public string? Notes { get; set; }
}

public class ApproveShiftCoverageDto
{
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
}

public class ShiftCoverageSearchCriteria
{
    public int? RequesterId { get; set; }
    public int? AcceptedBy { get; set; }
    public int? BranchId { get; set; }
    public ShiftCoverageRequestStatus? Status { get; set; }
    public bool? IsEmergency { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class EmergencyShiftCoverageBroadcastDto
{
    public int BranchId { get; set; }
    public int ShiftId { get; set; }
    public DateTime ShiftDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<int>? TargetEmployeeIds { get; set; } // If null, broadcast to all eligible employees
}