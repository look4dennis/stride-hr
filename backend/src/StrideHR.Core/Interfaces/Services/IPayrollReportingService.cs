using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayrollReportingService
{
    /// <summary>
    /// Generates payroll reports with currency conversion
    /// </summary>
    Task<PayrollReportResult> GeneratePayrollReportAsync(PayrollReportRequest request);
    
    /// <summary>
    /// Generates compliance reports for statutory requirements
    /// </summary>
    Task<ComplianceReportResult> GenerateComplianceReportAsync(ComplianceReportRequest request);
    
    /// <summary>
    /// Generates payroll analytics reports
    /// </summary>
    Task<PayrollAnalyticsResult> GenerateAnalyticsReportAsync(PayrollAnalyticsRequest request);
    
    /// <summary>
    /// Generates budget variance reports
    /// </summary>
    Task<BudgetVarianceResult> GenerateBudgetVarianceReportAsync(BudgetVarianceRequest request);
    
    /// <summary>
    /// Gets payroll audit trail
    /// </summary>
    Task<PayrollAuditTrailResult> GetPayrollAuditTrailAsync(PayrollAuditTrailRequest request);
    
    /// <summary>
    /// Exports report to various formats (PDF, Excel, CSV)
    /// </summary>
    Task<byte[]> ExportReportAsync(object reportResult, string format);
    
    /// <summary>
    /// Validates compliance requirements
    /// </summary>
    Task<List<ComplianceViolation>> ValidateComplianceAsync(int branchId, DateTime startDate, DateTime endDate);
}