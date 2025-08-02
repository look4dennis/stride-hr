using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayslipDesignerService
{
    /// <summary>
    /// Generates PDF payslip from template and payroll data
    /// </summary>
    Task<(byte[] pdfContent, string fileName)> GeneratePdfPayslipAsync(PayslipTemplateDto template, PayrollCalculationResult payrollData);
    
    /// <summary>
    /// Generates HTML preview of payslip
    /// </summary>
    Task<string> GenerateHtmlPreviewAsync(PayslipTemplateDto template, PayrollCalculationResult payrollData);
    
    /// <summary>
    /// Gets available field mappings for payslip design
    /// </summary>
    Task<Dictionary<string, string>> GetAvailableFieldMappingsAsync();
    
    /// <summary>
    /// Validates template design configuration
    /// </summary>
    Task<(bool isValid, List<string> errors)> ValidateTemplateDesignAsync(PayslipTemplateConfig templateConfig);
    
    /// <summary>
    /// Gets default template configuration
    /// </summary>
    Task<PayslipTemplateConfig> GetDefaultTemplateConfigAsync();
}