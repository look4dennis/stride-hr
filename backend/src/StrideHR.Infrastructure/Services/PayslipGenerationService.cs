using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Infrastructure.Services;

public class PayslipGenerationService : IPayslipGenerationService
{
    private readonly IPayslipGenerationRepository _payslipGenerationRepository;
    private readonly IPayslipTemplateService _payslipTemplateService;
    private readonly IPayslipDesignerService _payslipDesignerService;
    private readonly IPayrollService _payrollService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PayslipGenerationService> _logger;

    public PayslipGenerationService(
        IPayslipGenerationRepository payslipGenerationRepository,
        IPayslipTemplateService payslipTemplateService,
        IPayslipDesignerService payslipDesignerService,
        IPayrollService payrollService,
        IFileStorageService fileStorageService,
        ILogger<PayslipGenerationService> logger)
    {
        _payslipGenerationRepository = payslipGenerationRepository;
        _payslipTemplateService = payslipTemplateService;
        _payslipDesignerService = payslipDesignerService;
        _payrollService = payrollService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<PayslipGenerationDto> GeneratePayslipAsync(CreatePayslipGenerationRequest request, int generatedBy)
    {
        try
        {
            // Get payroll record
            var payrollRecord = await _payrollService.GetPayrollRecordAsync(request.PayrollRecordId);
            if (payrollRecord == null)
            {
                throw new ArgumentException($"Payroll record with ID {request.PayrollRecordId} not found");
            }

            // Check if payslip already exists
            var existingPayslip = await _payslipGenerationRepository.GetByPayrollRecordAsync(request.PayrollRecordId);
            if (existingPayslip != null)
            {
                throw new InvalidOperationException($"Payslip already exists for payroll record {request.PayrollRecordId}");
            }

            // Get template
            var template = await _payslipTemplateService.GetTemplateAsync(request.PayslipTemplateId);
            if (template == null)
            {
                throw new ArgumentException($"Payslip template with ID {request.PayslipTemplateId} not found");
            }

            // Create payroll calculation request for payslip data
            var payrollCalculationRequest = new PayrollCalculationRequest
            {
                EmployeeId = payrollRecord.EmployeeId,
                PayrollPeriodStart = payrollRecord.PayrollPeriodStart,
                PayrollPeriodEnd = payrollRecord.PayrollPeriodEnd,
                PayrollMonth = payrollRecord.PayrollMonth,
                PayrollYear = payrollRecord.PayrollYear,
                IncludeCustomFormulas = true
            };

            var payrollData = await _payrollService.CalculatePayrollAsync(payrollCalculationRequest);

            // Generate PDF payslip
            var (pdfContent, fileName) = await _payslipDesignerService.GeneratePdfPayslipAsync(template, payrollData);

            // Save PDF file
            var filePath = await _fileStorageService.SaveFileAsync(
                pdfContent, 
                fileName, 
                $"payslips/{payrollRecord.PayrollYear}/{payrollRecord.PayrollMonth:D2}");

            // Create payslip generation record
            var payslipGeneration = new PayslipGeneration
            {
                PayrollRecordId = request.PayrollRecordId,
                PayslipTemplateId = request.PayslipTemplateId,
                PayslipPath = filePath,
                PayslipFileName = fileName,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = generatedBy,
                Status = request.AutoSubmitForApproval ? PayslipStatus.PendingHRApproval : PayslipStatus.Generated,
                Version = 1
            };

            await _payslipGenerationRepository.AddAsync(payslipGeneration);
            await _payslipGenerationRepository.SaveChangesAsync();

            // Create approval history entry
            if (request.AutoSubmitForApproval)
            {
                var approvalHistory = new PayslipApprovalHistory
                {
                    PayslipGenerationId = payslipGeneration.Id,
                    ApprovalLevel = PayslipApprovalLevel.HR,
                    Action = PayslipApprovalAction.RequestedChanges, // Submitted for approval
                    ActionBy = generatedBy,
                    ActionAt = DateTime.UtcNow,
                    Notes = "Automatically submitted for HR approval",
                    PreviousStatus = PayslipStatus.Generated,
                    NewStatus = PayslipStatus.PendingHRApproval
                };

                payslipGeneration.ApprovalHistory.Add(approvalHistory);
                await _payslipGenerationRepository.SaveChangesAsync();
            }

            _logger.LogInformation("Payslip generated for payroll record {PayrollRecordId} by user {GeneratedBy}", 
                request.PayrollRecordId, generatedBy);

            return await GetPayslipGenerationAsync(payslipGeneration.Id) ?? new PayslipGenerationDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payslip for payroll record {PayrollRecordId}", request.PayrollRecordId);
            throw;
        }
    }

    public async Task<List<PayslipGenerationDto>> GenerateBulkPayslipsAsync(BulkPayslipGenerationRequest request, int generatedBy)
    {
        var results = new List<PayslipGenerationDto>();

        foreach (var payrollRecordId in request.PayrollRecordIds)
        {
            try
            {
                var singleRequest = new CreatePayslipGenerationRequest
                {
                    PayrollRecordId = payrollRecordId,
                    PayslipTemplateId = request.PayslipTemplateId,
                    AutoSubmitForApproval = request.AutoSubmitForApproval
                };

                var result = await GeneratePayslipAsync(singleRequest, generatedBy);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payslip for payroll record {PayrollRecordId} in bulk operation", payrollRecordId);
                
                // Add error result
                results.Add(new PayslipGenerationDto
                {
                    PayrollRecordId = payrollRecordId,
                    Status = PayslipStatus.Cancelled,
                    GeneratedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }

    public async Task<PayslipGenerationDto?> GetPayslipGenerationAsync(int payslipGenerationId)
    {
        var payslipGeneration = await _payslipGenerationRepository.GetByIdAsync(payslipGenerationId);
        if (payslipGeneration == null)
            return null;

        return MapToDto(payslipGeneration);
    }

    public async Task<List<PayslipGenerationDto>> GetPendingApprovalsAsync(PayslipApprovalLevel approvalLevel)
    {
        var payslipGenerations = await _payslipGenerationRepository.GetPendingApprovalsAsync(approvalLevel);
        return payslipGenerations.Select(MapToDto).ToList();
    }

    public async Task<bool> ProcessApprovalAsync(PayslipApprovalRequest request, int actionBy)
    {
        try
        {
            var payslipGeneration = await _payslipGenerationRepository.GetByIdAsync(request.PayslipGenerationId);
            if (payslipGeneration == null)
                return false;

            var previousStatus = payslipGeneration.Status;
            PayslipStatus newStatus;

            // Determine new status based on approval level and action
            switch (request.ApprovalLevel)
            {
                case PayslipApprovalLevel.HR:
                    if (request.Action == PayslipApprovalAction.Approved)
                    {
                        payslipGeneration.HRApprovedBy = actionBy;
                        payslipGeneration.HRApprovedAt = DateTime.UtcNow;
                        payslipGeneration.HRApprovalNotes = request.Notes;
                        newStatus = PayslipStatus.PendingFinanceApproval;
                    }
                    else
                    {
                        newStatus = PayslipStatus.HRRejected;
                    }
                    break;

                case PayslipApprovalLevel.Finance:
                    if (request.Action == PayslipApprovalAction.Approved)
                    {
                        payslipGeneration.FinanceApprovedBy = actionBy;
                        payslipGeneration.FinanceApprovedAt = DateTime.UtcNow;
                        payslipGeneration.FinanceApprovalNotes = request.Notes;
                        newStatus = PayslipStatus.FinanceApproved;
                    }
                    else
                    {
                        newStatus = PayslipStatus.FinanceRejected;
                    }
                    break;

                default:
                    return false;
            }

            payslipGeneration.Status = newStatus;

            // Create approval history entry
            var approvalHistory = new PayslipApprovalHistory
            {
                PayslipGenerationId = request.PayslipGenerationId,
                ApprovalLevel = request.ApprovalLevel,
                Action = request.Action,
                ActionBy = actionBy,
                ActionAt = DateTime.UtcNow,
                Notes = request.Notes,
                RejectionReason = request.RejectionReason,
                PreviousStatus = previousStatus,
                NewStatus = newStatus
            };

            payslipGeneration.ApprovalHistory.Add(approvalHistory);

            await _payslipGenerationRepository.UpdateAsync(payslipGeneration);
            await _payslipGenerationRepository.SaveChangesAsync();

            _logger.LogInformation("Payslip {PayslipGenerationId} {Action} by {ActionBy} at {ApprovalLevel} level", 
                request.PayslipGenerationId, request.Action, actionBy, request.ApprovalLevel);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval for payslip {PayslipGenerationId}", request.PayslipGenerationId);
            return false;
        }
    }

    public async Task<bool> ReleasePayslipsAsync(PayslipReleaseRequest request, int releasedBy)
    {
        try
        {
            var releasedCount = 0;

            foreach (var payslipGenerationId in request.PayslipGenerationIds)
            {
                var payslipGeneration = await _payslipGenerationRepository.GetByIdAsync(payslipGenerationId);
                if (payslipGeneration == null || payslipGeneration.Status != PayslipStatus.FinanceApproved)
                    continue;

                // Update payslip status to released
                payslipGeneration.Status = PayslipStatus.Released;
                payslipGeneration.ReleasedBy = releasedBy;
                payslipGeneration.ReleasedAt = DateTime.UtcNow;

                // Create approval history entry
                var approvalHistory = new PayslipApprovalHistory
                {
                    PayslipGenerationId = payslipGenerationId,
                    ApprovalLevel = PayslipApprovalLevel.Finance,
                    Action = PayslipApprovalAction.Released,
                    ActionBy = releasedBy,
                    ActionAt = DateTime.UtcNow,
                    Notes = request.ReleaseNotes,
                    PreviousStatus = PayslipStatus.FinanceApproved,
                    NewStatus = PayslipStatus.Released
                };

                payslipGeneration.ApprovalHistory.Add(approvalHistory);

                await _payslipGenerationRepository.UpdateAsync(payslipGeneration);
                releasedCount++;

                // Send notification to employee if requested
                if (request.SendNotifications)
                {
                    // TODO: Implement notification service call
                    payslipGeneration.IsNotificationSent = true;
                    payslipGeneration.NotificationSentAt = DateTime.UtcNow;
                }
            }

            await _payslipGenerationRepository.SaveChangesAsync();

            _logger.LogInformation("{ReleasedCount} payslips released by user {ReleasedBy}", releasedCount, releasedBy);

            return releasedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing payslips");
            return false;
        }
    }

    public async Task<PayslipApprovalSummary> GetApprovalSummaryAsync(int branchId, int year, int month)
    {
        var payslips = await _payslipGenerationRepository.GetByBranchAndPeriodAsync(branchId, year, month);

        var summary = new PayslipApprovalSummary
        {
            TotalPayslips = payslips.Count,
            PendingHRApproval = payslips.Count(p => p.Status == PayslipStatus.PendingHRApproval),
            PendingFinanceApproval = payslips.Count(p => p.Status == PayslipStatus.PendingFinanceApproval),
            Approved = payslips.Count(p => p.Status == PayslipStatus.FinanceApproved),
            Released = payslips.Count(p => p.Status == PayslipStatus.Released),
            Rejected = payslips.Count(p => p.Status == PayslipStatus.HRRejected || p.Status == PayslipStatus.FinanceRejected),
            TotalPayrollAmount = payslips.Sum(p => p.PayrollRecord.NetSalary),
            Currency = payslips.FirstOrDefault()?.PayrollRecord.Currency ?? "USD"
        };

        return summary;
    }

    public async Task<List<PayslipGenerationDto>> GetEmployeePayslipsAsync(int employeeId, int year, int? month = null)
    {
        var payslips = await _payslipGenerationRepository.GetByEmployeeAsync(employeeId, year, month);
        return payslips.Select(MapToDto).ToList();
    }

    public async Task<PayslipGenerationDto> RegeneratePayslipAsync(int payslipGenerationId, string reason, int regeneratedBy)
    {
        var existingPayslip = await _payslipGenerationRepository.GetByIdAsync(payslipGenerationId);
        if (existingPayslip == null)
        {
            throw new ArgumentException($"Payslip generation with ID {payslipGenerationId} not found");
        }

        // Create new version
        var request = new CreatePayslipGenerationRequest
        {
            PayrollRecordId = existingPayslip.PayrollRecordId,
            PayslipTemplateId = existingPayslip.PayslipTemplateId,
            AutoSubmitForApproval = false
        };

        // Cancel existing payslip
        existingPayslip.Status = PayslipStatus.Cancelled;
        await _payslipGenerationRepository.UpdateAsync(existingPayslip);

        // Generate new payslip
        var newPayslip = await GeneratePayslipAsync(request, regeneratedBy);
        
        // Update version and reason
        var newPayslipEntity = await _payslipGenerationRepository.GetByIdAsync(newPayslip.Id);
        if (newPayslipEntity != null)
        {
            newPayslipEntity.Version = existingPayslip.Version + 1;
            newPayslipEntity.RegenerationReason = reason;
            await _payslipGenerationRepository.UpdateAsync(newPayslipEntity);
            await _payslipGenerationRepository.SaveChangesAsync();
        }

        return newPayslip;
    }

    public async Task<(byte[] fileContent, string fileName, string contentType)> DownloadPayslipAsync(int payslipGenerationId)
    {
        var payslipGeneration = await _payslipGenerationRepository.GetByIdAsync(payslipGenerationId);
        if (payslipGeneration == null)
        {
            throw new ArgumentException($"Payslip generation with ID {payslipGenerationId} not found");
        }

        if (string.IsNullOrEmpty(payslipGeneration.PayslipPath))
        {
            throw new InvalidOperationException("Payslip file path is not available");
        }

        var fileContent = await _fileStorageService.GetFileAsync(payslipGeneration.PayslipPath);
        if (fileContent == null)
        {
            throw new InvalidOperationException("Payslip file not found");
        }
        
        return (fileContent, payslipGeneration.PayslipFileName, "application/pdf");
    }

    private static PayslipGenerationDto MapToDto(PayslipGeneration payslipGeneration)
    {
        return new PayslipGenerationDto
        {
            Id = payslipGeneration.Id,
            PayrollRecordId = payslipGeneration.PayrollRecordId,
            PayslipTemplateId = payslipGeneration.PayslipTemplateId,
            PayslipPath = payslipGeneration.PayslipPath,
            PayslipFileName = payslipGeneration.PayslipFileName,
            Status = payslipGeneration.Status,
            GeneratedAt = payslipGeneration.GeneratedAt,
            GeneratedByName = payslipGeneration.GeneratedByEmployee?.FullName ?? "System",
            EmployeeName = payslipGeneration.PayrollRecord?.Employee?.FullName ?? "",
            EmployeeId = payslipGeneration.PayrollRecord?.Employee?.EmployeeId ?? "",
            Department = payslipGeneration.PayrollRecord?.Employee?.Department ?? "",
            Designation = payslipGeneration.PayrollRecord?.Employee?.Designation ?? "",
            PayrollMonth = payslipGeneration.PayrollRecord?.PayrollMonth ?? 0,
            PayrollYear = payslipGeneration.PayrollRecord?.PayrollYear ?? 0,
            NetSalary = payslipGeneration.PayrollRecord?.NetSalary ?? 0,
            Currency = payslipGeneration.PayrollRecord?.Currency ?? "USD",
            HRApproval = payslipGeneration.HRApprovedBy.HasValue ? new PayslipApprovalInfo
            {
                ApprovedByName = payslipGeneration.HRApprovedByEmployee?.FullName ?? "",
                ApprovedAt = payslipGeneration.HRApprovedAt ?? DateTime.MinValue,
                Notes = payslipGeneration.HRApprovalNotes
            } : null,
            FinanceApproval = payslipGeneration.FinanceApprovedBy.HasValue ? new PayslipApprovalInfo
            {
                ApprovedByName = payslipGeneration.FinanceApprovedByEmployee?.FullName ?? "",
                ApprovedAt = payslipGeneration.FinanceApprovedAt ?? DateTime.MinValue,
                Notes = payslipGeneration.FinanceApprovalNotes
            } : null,
            ReleasedAt = payslipGeneration.ReleasedAt,
            ReleasedByName = payslipGeneration.ReleasedByEmployee?.FullName,
            IsNotificationSent = payslipGeneration.IsNotificationSent,
            Version = payslipGeneration.Version,
            RegenerationReason = payslipGeneration.RegenerationReason
        };
    }
}