using StrideHR.Core.Models.Email;

namespace StrideHR.Core.Interfaces.Services;

public interface IEmailTemplateService
{
    // Template rendering
    Task<string> RenderTemplateAsync(string templateContent, Dictionary<string, object> parameters);
    Task<string> RenderSubjectAsync(string subjectTemplate, Dictionary<string, object> parameters);
    Task<EmailRenderResultDto> RenderEmailTemplateAsync(int templateId, Dictionary<string, object> parameters);
    
    // Template validation
    Task<TemplateValidationResultDto> ValidateTemplateAsync(string templateContent, List<string> requiredParameters);
    Task<List<string>> ExtractParametersFromTemplateAsync(string templateContent);
    
    // Template utilities
    Task<string> ConvertToHtmlAsync(string textContent);
    Task<string> ConvertToTextAsync(string htmlContent);
    Task<string> SanitizeHtmlAsync(string htmlContent);
    
    // Template preview
    Task<EmailPreviewDto> GeneratePreviewAsync(int templateId, Dictionary<string, object>? sampleData = null);
    Task<string> GeneratePreviewHtmlAsync(string templateContent, Dictionary<string, object> sampleData);
}