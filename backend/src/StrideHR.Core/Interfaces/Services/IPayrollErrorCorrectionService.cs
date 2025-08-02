using StrideHR.Core.Models.Payroll;
using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayrollErrorCorrectionService
{
    /// <summary>
    /// Creates a new error correction request
    /// </summary>
    Task<PayrollErrorCorrectionResult> CreateErrorCorrectionAsync(PayrollErrorCorrectionRequest request);
    
    /// <summary>
    /// Approves an error correction request
    /// </summary>
    Task<bool> ApproveErrorCorrectionAsync(int correctionId, int approvedBy, string? notes = null);
    
    /// <summary>
    /// Rejects an error correction request
    /// </summary>
    Task<bool> RejectErrorCorrectionAsync(int correctionId, int rejectedBy, string reason);
    
    /// <summary>
    /// Processes an approved error correction
    /// </summary>
    Task<PayrollCalculationResult> ProcessErrorCorrectionAsync(int correctionId, int processedBy);
    
    /// <summary>
    /// Gets error correction by ID
    /// </summary>
    Task<PayrollErrorCorrection?> GetErrorCorrectionAsync(int correctionId);
    
    /// <summary>
    /// Gets error corrections for a payroll record
    /// </summary>
    Task<List<PayrollErrorCorrection>> GetPayrollErrorCorrectionsAsync(int payrollRecordId);
    
    /// <summary>
    /// Gets pending error corrections for approval
    /// </summary>
    Task<List<PayrollErrorCorrection>> GetPendingErrorCorrectionsAsync(int? branchId = null);
    
    /// <summary>
    /// Validates error correction data
    /// </summary>
    Task<List<string>> ValidateErrorCorrectionAsync(PayrollErrorCorrectionRequest request);
    
    /// <summary>
    /// Cancels an error correction request
    /// </summary>
    Task<bool> CancelErrorCorrectionAsync(int correctionId, int cancelledBy, string reason);
}