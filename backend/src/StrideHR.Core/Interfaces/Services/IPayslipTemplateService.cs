using StrideHR.Core.Models.Payroll;

namespace StrideHR.Core.Interfaces.Services;

public interface IPayslipTemplateService
{
    /// <summary>
    /// Creates a new payslip template
    /// </summary>
    Task<PayslipTemplateDto> CreateTemplateAsync(PayslipTemplateDto templateDto, int createdBy);
    
    /// <summary>
    /// Updates an existing payslip template
    /// </summary>
    Task<PayslipTemplateDto> UpdateTemplateAsync(int templateId, PayslipTemplateDto templateDto, int modifiedBy);
    
    /// <summary>
    /// Gets payslip template by ID
    /// </summary>
    Task<PayslipTemplateDto?> GetTemplateAsync(int templateId);
    
    /// <summary>
    /// Gets all templates for an organization
    /// </summary>
    Task<List<PayslipTemplateDto>> GetOrganizationTemplatesAsync(int organizationId);
    
    /// <summary>
    /// Gets templates available for a branch
    /// </summary>
    Task<List<PayslipTemplateDto>> GetBranchTemplatesAsync(int branchId);
    
    /// <summary>
    /// Gets the default template for an organization/branch
    /// </summary>
    Task<PayslipTemplateDto?> GetDefaultTemplateAsync(int organizationId, int? branchId = null);
    
    /// <summary>
    /// Sets a template as default
    /// </summary>
    Task<bool> SetAsDefaultAsync(int templateId, int organizationId, int? branchId = null);
    
    /// <summary>
    /// Deactivates a template
    /// </summary>
    Task<bool> DeactivateTemplateAsync(int templateId);
    
    /// <summary>
    /// Validates template configuration
    /// </summary>
    Task<(bool isValid, List<string> errors)> ValidateTemplateAsync(PayslipTemplateDto templateDto);
}