using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Entities;

public class PayrollErrorCorrection : BaseEntity
{
    public int PayrollRecordId { get; set; }
    public PayrollErrorType ErrorType { get; set; }
    public string ErrorDescription { get; set; } = string.Empty;
    public string CorrectionData { get; set; } = string.Empty; // JSON
    public PayrollCorrectionStatus Status { get; set; } = PayrollCorrectionStatus.Pending;
    public int RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public int? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingNotes { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? OriginalValues { get; set; } // JSON
    public string? CorrectedValues { get; set; } // JSON
    public decimal? ImpactAmount { get; set; }
    
    // Navigation Properties
    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
    public virtual User RequestedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
    public virtual User? ProcessedByUser { get; set; }
}