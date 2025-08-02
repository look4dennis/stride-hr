using StrideHR.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Leave;

public class LeaveApprovalDto
{
    [Required]
    public int LeaveRequestId { get; set; }
    
    [Required]
    public ApprovalAction Action { get; set; }
    
    [StringLength(1000)]
    public string? Comments { get; set; }
    
    public decimal? ApprovedDays { get; set; }
    
    public int? EscalateToId { get; set; }
}