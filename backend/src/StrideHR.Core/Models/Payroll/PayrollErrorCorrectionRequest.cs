namespace StrideHR.Core.Models.Payroll;

public class PayrollErrorCorrectionRequest
{
    public int PayrollRecordId { get; set; }
    public string ErrorDescription { get; set; } = string.Empty;
    public PayrollErrorType ErrorType { get; set; }
    public Dictionary<string, object> CorrectionData { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; } = true;
    public int RequestedBy { get; set; }
}

public enum PayrollErrorType
{
    CalculationError,
    DataEntryError,
    FormulaError,
    AttendanceError,
    DeductionError,
    AllowanceError,
    TaxCalculationError,
    CurrencyConversionError,
    SystemError
}

public class PayrollErrorCorrectionResult
{
    public int CorrectionId { get; set; }
    public int PayrollRecordId { get; set; }
    public PayrollErrorType ErrorType { get; set; }
    public string Status { get; set; } = string.Empty;
    public PayrollCalculationResult? OriginalCalculation { get; set; }
    public PayrollCalculationResult? CorrectedCalculation { get; set; }
    public List<PayrollCorrectionChange> Changes { get; set; } = new();
    public string? ApprovalRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class PayrollCorrectionChange
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldDisplayName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public decimal? ImpactAmount { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
}

public class PayrollErrorCorrectionWorkflow
{
    public int Id { get; set; }
    public int PayrollRecordId { get; set; }
    public PayrollErrorType ErrorType { get; set; }
    public string ErrorDescription { get; set; } = string.Empty;
    public string CorrectionData { get; set; } = string.Empty; // JSON
    public PayrollCorrectionStatus Status { get; set; }
    public int RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public int? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingNotes { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public enum PayrollCorrectionStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Processed,
    Cancelled
}