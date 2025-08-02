using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Models.Payroll;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class PayrollErrorCorrectionService : IPayrollErrorCorrectionService
{
    private readonly IPayrollErrorCorrectionRepository _errorCorrectionRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly IPayrollService _payrollService;
    private readonly IPayrollAuditTrailRepository _auditTrailRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PayrollErrorCorrectionService> _logger;

    public PayrollErrorCorrectionService(
        IPayrollErrorCorrectionRepository errorCorrectionRepository,
        IPayrollRepository payrollRepository,
        IPayrollService payrollService,
        IPayrollAuditTrailRepository auditTrailRepository,
        IUnitOfWork unitOfWork,
        ILogger<PayrollErrorCorrectionService> logger)
    {
        _errorCorrectionRepository = errorCorrectionRepository;
        _payrollRepository = payrollRepository;
        _payrollService = payrollService;
        _auditTrailRepository = auditTrailRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PayrollErrorCorrectionResult> CreateErrorCorrectionAsync(PayrollErrorCorrectionRequest request)
    {
        try
        {
            _logger.LogInformation("Creating error correction for payroll record: {PayrollRecordId}", request.PayrollRecordId);

            // Validate the request
            var validationErrors = await ValidateErrorCorrectionAsync(request);
            if (validationErrors.Any())
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
            }

            // Get the original payroll record
            var originalRecord = await _payrollRepository.GetByIdAsync(request.PayrollRecordId);
            if (originalRecord == null)
            {
                throw new ArgumentException($"Payroll record {request.PayrollRecordId} not found");
            }

            // Create the error correction entity
            var errorCorrection = new PayrollErrorCorrection
            {
                PayrollRecordId = request.PayrollRecordId,
                ErrorType = request.ErrorType,
                ErrorDescription = request.ErrorDescription,
                CorrectionData = JsonSerializer.Serialize(request.CorrectionData),
                Status = PayrollCorrectionStatus.Pending,
                RequestedBy = request.RequestedBy,
                RequestedAt = DateTime.UtcNow,
                Reason = request.Reason,
                OriginalValues = SerializePayrollRecord(originalRecord)
            };

            await _errorCorrectionRepository.AddAsync(errorCorrection);
            await _unitOfWork.SaveChangesAsync();

            // Create audit trail entry
            await CreateAuditTrailEntryAsync(originalRecord.EmployeeId, originalRecord.Id, 
                PayrollAuditAction.ErrorCorrected, "Error correction request created", request.RequestedBy);

            // Calculate the corrected values for preview
            var correctedCalculation = await CalculateCorrectedValuesAsync(originalRecord, request.CorrectionData);

            var result = new PayrollErrorCorrectionResult
            {
                CorrectionId = errorCorrection.Id,
                PayrollRecordId = request.PayrollRecordId,
                ErrorType = request.ErrorType,
                Status = PayrollCorrectionStatus.Pending.ToString(),
                OriginalCalculation = MapToCalculationResult(originalRecord),
                CorrectedCalculation = correctedCalculation,
                Changes = CalculateChanges(originalRecord, correctedCalculation),
                ApprovalRequired = request.RequiresApproval ? "Yes" : "No",
                CreatedAt = errorCorrection.RequestedAt,
                CreatedBy = request.RequestedBy.ToString()
            };

            _logger.LogInformation("Successfully created error correction with ID: {CorrectionId}", errorCorrection.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating error correction for payroll record: {PayrollRecordId}", request.PayrollRecordId);
            throw;
        }
    }

    public async Task<bool> ApproveErrorCorrectionAsync(int correctionId, int approvedBy, string? notes = null)
    {
        try
        {
            var correction = await _errorCorrectionRepository.GetByIdAsync(correctionId);
            if (correction == null)
            {
                return false;
            }

            correction.Status = PayrollCorrectionStatus.Approved;
            correction.ApprovedBy = approvedBy;
            correction.ApprovedAt = DateTime.UtcNow;
            correction.ApprovalNotes = notes;

            await _errorCorrectionRepository.UpdateAsync(correction);
            await _unitOfWork.SaveChangesAsync();

            // Create audit trail entry
            await CreateAuditTrailEntryAsync(correction.PayrollRecord.EmployeeId, correction.PayrollRecordId,
                PayrollAuditAction.Approved, "Error correction approved", approvedBy);

            _logger.LogInformation("Error correction {CorrectionId} approved by user {ApprovedBy}", correctionId, approvedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving error correction: {CorrectionId}", correctionId);
            throw;
        }
    }

    public async Task<bool> RejectErrorCorrectionAsync(int correctionId, int rejectedBy, string reason)
    {
        try
        {
            var correction = await _errorCorrectionRepository.GetByIdAsync(correctionId);
            if (correction == null)
            {
                return false;
            }

            correction.Status = PayrollCorrectionStatus.Rejected;
            correction.ApprovedBy = rejectedBy;
            correction.ApprovedAt = DateTime.UtcNow;
            correction.ApprovalNotes = reason;

            await _errorCorrectionRepository.UpdateAsync(correction);
            await _unitOfWork.SaveChangesAsync();

            // Create audit trail entry
            await CreateAuditTrailEntryAsync(correction.PayrollRecord.EmployeeId, correction.PayrollRecordId,
                PayrollAuditAction.Rejected, "Error correction rejected", rejectedBy);

            _logger.LogInformation("Error correction {CorrectionId} rejected by user {RejectedBy}", correctionId, rejectedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting error correction: {CorrectionId}", correctionId);
            throw;
        }
    }

    public async Task<PayrollCalculationResult> ProcessErrorCorrectionAsync(int correctionId, int processedBy)
    {
        try
        {
            var correction = await _errorCorrectionRepository.GetByIdAsync(correctionId);
            if (correction == null || correction.Status != PayrollCorrectionStatus.Approved)
            {
                throw new InvalidOperationException("Error correction not found or not approved");
            }

            // Get the payroll record
            var payrollRecord = await _payrollRepository.GetByIdAsync(correction.PayrollRecordId);
            if (payrollRecord == null)
            {
                throw new InvalidOperationException("Payroll record not found");
            }

            // Apply the corrections
            var correctionData = JsonSerializer.Deserialize<Dictionary<string, object>>(correction.CorrectionData);
            if (correctionData != null)
            {
                ApplyCorrectionsToPayrollRecord(payrollRecord, correctionData);
            }

            // Update the payroll record
            await _payrollRepository.UpdateAsync(payrollRecord);

            // Update correction status
            correction.Status = PayrollCorrectionStatus.Processed;
            correction.ProcessedBy = processedBy;
            correction.ProcessedAt = DateTime.UtcNow;
            correction.CorrectedValues = SerializePayrollRecord(payrollRecord);

            await _errorCorrectionRepository.UpdateAsync(correction);
            await _unitOfWork.SaveChangesAsync();

            // Create audit trail entry
            await CreateAuditTrailEntryAsync(payrollRecord.EmployeeId, payrollRecord.Id,
                PayrollAuditAction.ErrorCorrected, "Error correction processed", processedBy);

            var result = MapToCalculationResult(payrollRecord);
            _logger.LogInformation("Error correction {CorrectionId} processed successfully", correctionId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing error correction: {CorrectionId}", correctionId);
            throw;
        }
    }

    public async Task<PayrollErrorCorrection?> GetErrorCorrectionAsync(int correctionId)
    {
        return await _errorCorrectionRepository.GetByIdAsync(correctionId);
    }

    public async Task<List<PayrollErrorCorrection>> GetPayrollErrorCorrectionsAsync(int payrollRecordId)
    {
        return await _errorCorrectionRepository.GetByPayrollRecordIdAsync(payrollRecordId);
    }

    public async Task<List<PayrollErrorCorrection>> GetPendingErrorCorrectionsAsync(int? branchId = null)
    {
        return await _errorCorrectionRepository.GetPendingCorrectionsAsync(branchId);
    }

    public async Task<List<string>> ValidateErrorCorrectionAsync(PayrollErrorCorrectionRequest request)
    {
        var errors = new List<string>();

        // Check if payroll record exists
        var payrollRecord = await _payrollRepository.GetByIdAsync(request.PayrollRecordId);
        if (payrollRecord == null)
        {
            errors.Add("Payroll record not found");
        }

        // Validate correction data based on error type
        if (request.CorrectionData == null || !request.CorrectionData.Any())
        {
            errors.Add("Correction data is required");
        }

        // Add more validation logic based on error type
        switch (request.ErrorType)
        {
            case PayrollErrorType.CalculationError:
                ValidateCalculationErrorData(request.CorrectionData, errors);
                break;
            case PayrollErrorType.DataEntryError:
                ValidateDataEntryErrorData(request.CorrectionData, errors);
                break;
            // Add more validation cases
        }

        return errors;
    }

    public async Task<bool> CancelErrorCorrectionAsync(int correctionId, int cancelledBy, string reason)
    {
        try
        {
            var correction = await _errorCorrectionRepository.GetByIdAsync(correctionId);
            if (correction == null)
            {
                return false;
            }

            correction.Status = PayrollCorrectionStatus.Cancelled;
            correction.ProcessedBy = cancelledBy;
            correction.ProcessedAt = DateTime.UtcNow;
            correction.ProcessingNotes = reason;

            await _errorCorrectionRepository.UpdateAsync(correction);
            await _unitOfWork.SaveChangesAsync();

            // Create audit trail entry
            await CreateAuditTrailEntryAsync(correction.PayrollRecord.EmployeeId, correction.PayrollRecordId,
                PayrollAuditAction.Cancelled, "Error correction cancelled", cancelledBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling error correction: {CorrectionId}", correctionId);
            throw;
        }
    }

    // Private helper methods
    private async Task<PayrollCalculationResult> CalculateCorrectedValuesAsync(PayrollRecord originalRecord, Dictionary<string, object> correctionData)
    {
        // Create a copy of the original record and apply corrections
        var correctedRecord = ClonePayrollRecord(originalRecord);
        ApplyCorrectionsToPayrollRecord(correctedRecord, correctionData);
        
        return MapToCalculationResult(correctedRecord);
    }

    private PayrollRecord ClonePayrollRecord(PayrollRecord original)
    {
        return new PayrollRecord
        {
            EmployeeId = original.EmployeeId,
            PayrollPeriodStart = original.PayrollPeriodStart,
            PayrollPeriodEnd = original.PayrollPeriodEnd,
            PayrollMonth = original.PayrollMonth,
            PayrollYear = original.PayrollYear,
            BasicSalary = original.BasicSalary,
            GrossSalary = original.GrossSalary,
            NetSalary = original.NetSalary,
            TotalAllowances = original.TotalAllowances,
            TotalDeductions = original.TotalDeductions,
            OvertimeAmount = original.OvertimeAmount,
            Currency = original.Currency,
            ExchangeRate = original.ExchangeRate
        };
    }

    private void ApplyCorrectionsToPayrollRecord(PayrollRecord record, Dictionary<string, object> correctionData)
    {
        foreach (var correction in correctionData)
        {
            switch (correction.Key.ToLower())
            {
                case "basicsalary":
                    if (decimal.TryParse(correction.Value.ToString(), out var basicSalary))
                        record.BasicSalary = basicSalary;
                    break;
                case "grosssalary":
                    if (decimal.TryParse(correction.Value.ToString(), out var grossSalary))
                        record.GrossSalary = grossSalary;
                    break;
                case "netsalary":
                    if (decimal.TryParse(correction.Value.ToString(), out var netSalary))
                        record.NetSalary = netSalary;
                    break;
                // Add more correction fields as needed
            }
        }
    }

    private PayrollCalculationResult MapToCalculationResult(PayrollRecord record)
    {
        return new PayrollCalculationResult
        {
            EmployeeId = record.EmployeeId,
            EmployeeName = record.Employee?.FirstName + " " + record.Employee?.LastName ?? "",
            BasicSalary = record.BasicSalary,
            GrossSalary = record.GrossSalary,
            NetSalary = record.NetSalary,
            TotalAllowances = record.TotalAllowances,
            TotalDeductions = record.TotalDeductions,
            OvertimeAmount = record.OvertimeAmount,
            Currency = record.Currency,
            ExchangeRate = record.ExchangeRate,
            PayrollMonth = record.PayrollMonth,
            PayrollYear = record.PayrollYear
        };
    }

    private List<PayrollCorrectionChange> CalculateChanges(PayrollRecord original, PayrollCalculationResult corrected)
    {
        var changes = new List<PayrollCorrectionChange>();

        if (original.BasicSalary != corrected.BasicSalary)
        {
            changes.Add(new PayrollCorrectionChange
            {
                FieldName = "BasicSalary",
                FieldDisplayName = "Basic Salary",
                OldValue = original.BasicSalary,
                NewValue = corrected.BasicSalary,
                ImpactAmount = corrected.BasicSalary - original.BasicSalary
            });
        }

        if (original.GrossSalary != corrected.GrossSalary)
        {
            changes.Add(new PayrollCorrectionChange
            {
                FieldName = "GrossSalary",
                FieldDisplayName = "Gross Salary",
                OldValue = original.GrossSalary,
                NewValue = corrected.GrossSalary,
                ImpactAmount = corrected.GrossSalary - original.GrossSalary
            });
        }

        if (original.NetSalary != corrected.NetSalary)
        {
            changes.Add(new PayrollCorrectionChange
            {
                FieldName = "NetSalary",
                FieldDisplayName = "Net Salary",
                OldValue = original.NetSalary,
                NewValue = corrected.NetSalary,
                ImpactAmount = corrected.NetSalary - original.NetSalary
            });
        }

        return changes;
    }

    private string SerializePayrollRecord(PayrollRecord record)
    {
        var data = new
        {
            record.BasicSalary,
            record.GrossSalary,
            record.NetSalary,
            record.TotalAllowances,
            record.TotalDeductions,
            record.OvertimeAmount,
            record.Currency,
            record.ExchangeRate
        };
        return JsonSerializer.Serialize(data);
    }

    private async Task CreateAuditTrailEntryAsync(int employeeId, int payrollRecordId, PayrollAuditAction action, string description, int userId)
    {
        var auditTrail = new PayrollAuditTrail
        {
            PayrollRecordId = payrollRecordId,
            EmployeeId = employeeId,
            Action = action,
            ActionDescription = description,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        await _auditTrailRepository.AddAsync(auditTrail);
    }

    private void ValidateCalculationErrorData(Dictionary<string, object> correctionData, List<string> errors)
    {
        // Add specific validation for calculation errors
        if (!correctionData.ContainsKey("BasicSalary") && !correctionData.ContainsKey("GrossSalary"))
        {
            errors.Add("At least one salary field must be corrected for calculation errors");
        }
    }

    private void ValidateDataEntryErrorData(Dictionary<string, object> correctionData, List<string> errors)
    {
        // Add specific validation for data entry errors
        if (correctionData.Count == 0)
        {
            errors.Add("Correction data is required for data entry errors");
        }
    }
}