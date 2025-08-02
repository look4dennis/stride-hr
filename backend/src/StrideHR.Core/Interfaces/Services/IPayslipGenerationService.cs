using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayslipGenerationService
{
    /// <summary>
    /// Generates a payslip for a specific payroll record
    /// </summary>
    Task<PayslipGenerationDto> GeneratePayslipAsync(CreatePayslipGenerationRequest request, int generatedBy);
    
    /// <summary>
    /// Generates payslips for multiple payroll records
    /// </summary>
    Task<List<PayslipGenerationDto>> GenerateBulkPayslipsAsync(BulkPayslipGenerationRequest request, int generatedBy);
    
    /// <summary>
    /// Gets payslip generation by ID
    /// </summary>
    Task<PayslipGenerationDto?> GetPayslipGenerationAsync(int payslipGenerationId);
    
    /// <summary>
    /// Gets payslip generations pending approval
    /// </summary>
    Task<List<PayslipGenerationDto>> GetPendingApprovalsAsync(PayslipApprovalLevel approvalLevel);
    
    /// <summary>
    /// Approves or rejects a payslip
    /// </summary>
    Task<bool> ProcessApprovalAsync(PayslipApprovalRequest request, int actionBy);
    
    /// <summary>
    /// Releases approved payslips to employees
    /// </summary>
    Task<bool> ReleasePayslipsAsync(PayslipReleaseRequest request, int releasedBy);
    
    /// <summary>
    /// Gets payslip approval summary for a branch and period
    /// </summary>
    Task<PayslipApprovalSummary> GetApprovalSummaryAsync(int branchId, int year, int month);
    
    /// <summary>
    /// Gets payslips for an employee
    /// </summary>
    Task<List<PayslipGenerationDto>> GetEmployeePayslipsAsync(int employeeId, int year, int? month = null);
    
    /// <summary>
    /// Regenerates a payslip (creates new version)
    /// </summary>
    Task<PayslipGenerationDto> RegeneratePayslipAsync(int payslipGenerationId, string reason, int regeneratedBy);
    
    /// <summary>
    /// Downloads payslip file
    /// </summary>
    Task<(byte[] fileContent, string fileName, string contentType)> DownloadPayslipAsync(int payslipGenerationId);
}