using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;
using System.Text;
using HtmlAgilityPack;

namespace StrideHR.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        IEmailTemplateRepository templateRepository,
        IMapper mapper,
        ILogger<EmailTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<string> RenderTemplateAsync(string templateContent, Dictionary<string, object> parameters)
    {
        try
        {
            var rendered = templateContent;
            
            // First, extract all parameters from the template
            var allParametersInTemplate = await ExtractParametersFromTemplateAsync(templateContent);
            
            // Replace all parameters, including missing ones with empty strings
            foreach (var paramName in allParametersInTemplate)
            {
                var placeholder = $"{{{{{paramName}}}}}";
                var value = parameters.ContainsKey(paramName) 
                    ? parameters[paramName]?.ToString() ?? string.Empty
                    : string.Empty;
                rendered = rendered.Replace(placeholder, value);
            }

            // Handle conditional blocks
            rendered = ProcessConditionalBlocks(rendered, parameters);
            
            // Handle loops
            rendered = ProcessLoops(rendered, parameters);

            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template");
            throw;
        }
    }

    public async Task<string> RenderSubjectAsync(string subjectTemplate, Dictionary<string, object> parameters)
    {
        return await RenderTemplateAsync(subjectTemplate, parameters);
    }

    public async Task<EmailRenderResultDto> RenderEmailTemplateAsync(int templateId, Dictionary<string, object> parameters)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                return new EmailRenderResultDto
                {
                    IsValid = false,
                    Errors = new List<string> { "Template not found" }
                };
            }

            var missingParameters = new List<string>();
            foreach (var requiredParam in template.RequiredParameters)
            {
                if (!parameters.ContainsKey(requiredParam))
                {
                    missingParameters.Add(requiredParam);
                }
            }

            if (missingParameters.Any())
            {
                return new EmailRenderResultDto
                {
                    IsValid = false,
                    Errors = new List<string> { "Missing required parameters" },
                    MissingParameters = missingParameters
                };
            }

            // Merge with default parameters
            var allParameters = new Dictionary<string, object>(template.DefaultParameters ?? new Dictionary<string, object>());
            foreach (var param in parameters)
            {
                allParameters[param.Key] = param.Value;
            }

            var renderedSubject = await RenderSubjectAsync(template.Subject, allParameters);
            var renderedHtmlBody = await RenderTemplateAsync(template.HtmlBody, allParameters);
            var renderedTextBody = !string.IsNullOrEmpty(template.TextBody) 
                ? await RenderTemplateAsync(template.TextBody, allParameters) 
                : null;

            return new EmailRenderResultDto
            {
                Subject = renderedSubject,
                HtmlBody = renderedHtmlBody,
                TextBody = renderedTextBody,
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering email template {TemplateId}", templateId);
            return new EmailRenderResultDto
            {
                IsValid = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<TemplateValidationResultDto> ValidateTemplateAsync(string templateContent, List<string> requiredParameters)
    {
        var result = new TemplateValidationResultDto();
        
        try
        {
            // Extract parameters from template
            var extractedParams = await ExtractParametersFromTemplateAsync(templateContent);
            result.ExtractedParameters = extractedParams;

            // Check for missing required parameters
            result.MissingRequiredParameters = requiredParameters
                .Where(rp => !extractedParams.Contains(rp))
                .ToList();

            // Validate HTML structure if it contains HTML
            if (templateContent.Contains("<") && templateContent.Contains(">"))
            {
                var htmlValidation = ValidateHtml(templateContent);
                result.Errors.AddRange(htmlValidation.Errors);
                result.Warnings.AddRange(htmlValidation.Warnings);
            }

            // Check for common template issues
            ValidateTemplateStructure(templateContent, result);

            result.IsValid = !result.Errors.Any() && !result.MissingRequiredParameters.Any();
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    public async Task<List<string>> ExtractParametersFromTemplateAsync(string templateContent)
    {
        var parameters = new HashSet<string>();
        
        // Extract {{parameter}} style parameters
        var regex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.IgnoreCase);
        var matches = regex.Matches(templateContent);
        
        foreach (Match match in matches)
        {
            var paramName = match.Groups[1].Value.Trim();
            parameters.Add(paramName);
        }

        return parameters.ToList();
    }

    public async Task<string> ConvertToHtmlAsync(string textContent)
    {
        if (string.IsNullOrWhiteSpace(textContent))
            return string.Empty;

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\">");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        var lines = textContent.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                html.AppendLine("<br>");
            }
            else
            {
                html.AppendLine($"<p>{System.Web.HttpUtility.HtmlEncode(line.Trim())}</p>");
            }
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    public async Task<string> ConvertToTextAsync(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            return string.Empty;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            
            return doc.DocumentNode.InnerText;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error converting HTML to text, returning original content");
            return htmlContent;
        }
    }

    public async Task<string> SanitizeHtmlAsync(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            return string.Empty;

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Remove dangerous elements
            var dangerousElements = new[] { "script", "iframe", "object", "embed", "form" };
            foreach (var tagName in dangerousElements)
            {
                var elements = doc.DocumentNode.SelectNodes($"//{tagName}");
                if (elements != null)
                {
                    foreach (var element in elements.ToList())
                    {
                        element.Remove();
                    }
                }
            }

            // Remove dangerous attributes
            var dangerousAttributes = new[] { "onclick", "onload", "onerror", "onmouseover", "javascript:" };
            var allElements = doc.DocumentNode.SelectNodes("//*[@*]");
            if (allElements != null)
            {
                foreach (var element in allElements)
                {
                    var attributesToRemove = new List<HtmlAttribute>();
                    foreach (var attr in element.Attributes)
                    {
                        if (dangerousAttributes.Any(da => attr.Name.Contains(da) || attr.Value.Contains(da)))
                        {
                            attributesToRemove.Add(attr);
                        }
                    }
                    
                    foreach (var attr in attributesToRemove)
                    {
                        element.Attributes.Remove(attr);
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error sanitizing HTML, returning original content");
            return htmlContent;
        }
    }

    public async Task<EmailPreviewDto> GeneratePreviewAsync(int templateId, Dictionary<string, object>? sampleData = null)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new ArgumentException("Template not found", nameof(templateId));
        }

        var previewData = sampleData ?? GenerateSampleData(template.RequiredParameters);
        var renderResult = await RenderEmailTemplateAsync(templateId, previewData);

        return new EmailPreviewDto
        {
            Subject = renderResult.Subject,
            HtmlBody = renderResult.HtmlBody,
            TextBody = renderResult.TextBody,
            SampleData = previewData,
            PreviewUrl = $"/api/email-templates/{templateId}/preview"
        };
    }

    public async Task<string> GeneratePreviewHtmlAsync(string templateContent, Dictionary<string, object> sampleData)
    {
        return await RenderTemplateAsync(templateContent, sampleData);
    }

    private string ProcessConditionalBlocks(string content, Dictionary<string, object> parameters)
    {
        // Process {{#if condition}} blocks
        var ifRegex = new Regex(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        return ifRegex.Replace(content, match =>
        {
            var condition = match.Groups[1].Value;
            var blockContent = match.Groups[2].Value;
            
            if (parameters.ContainsKey(condition) && 
                parameters[condition] != null && 
                !parameters[condition].Equals(false) &&
                !parameters[condition].Equals(""))
            {
                return blockContent;
            }
            
            return string.Empty;
        });
    }

    private string ProcessLoops(string content, Dictionary<string, object> parameters)
    {
        // Process {{#each items}} blocks
        var eachRegex = new Regex(@"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        return eachRegex.Replace(content, match =>
        {
            var arrayName = match.Groups[1].Value;
            var blockContent = match.Groups[2].Value;
            
            if (parameters.ContainsKey(arrayName) && parameters[arrayName] is IEnumerable<object> items)
            {
                var result = new StringBuilder();
                foreach (var item in items)
                {
                    var itemContent = blockContent;
                    if (item is Dictionary<string, object> itemDict)
                    {
                        foreach (var prop in itemDict)
                        {
                            itemContent = itemContent.Replace($"{{{{this.{prop.Key}}}}}", prop.Value?.ToString() ?? "");
                        }
                    }
                    result.Append(itemContent);
                }
                return result.ToString();
            }
            
            return string.Empty;
        });
    }

    private (List<string> Errors, List<string> Warnings) ValidateHtml(string htmlContent)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            if (doc.ParseErrors.Any())
            {
                errors.AddRange(doc.ParseErrors.Select(e => e.Reason));
            }

            // Check for common email HTML issues
            if (!htmlContent.Contains("<!DOCTYPE"))
            {
                warnings.Add("Missing DOCTYPE declaration");
            }

            if (!htmlContent.Contains("<meta") || !htmlContent.Contains("charset"))
            {
                warnings.Add("Missing charset meta tag");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"HTML parsing error: {ex.Message}");
        }

        return (errors, warnings);
    }

    private void ValidateTemplateStructure(string templateContent, TemplateValidationResultDto result)
    {
        // Check for unmatched template tags
        var openTags = Regex.Matches(templateContent, @"\{\{#\w+").Count;
        var closeTags = Regex.Matches(templateContent, @"\{\{/\w+").Count;
        
        if (openTags != closeTags)
        {
            result.Errors.Add("Unmatched template tags detected");
        }

        // Check for common issues
        if (templateContent.Contains("{{") && !templateContent.Contains("}}"))
        {
            result.Errors.Add("Unclosed template parameter detected");
        }
    }

    private Dictionary<string, object> GenerateSampleData(List<string> requiredParameters)
    {
        var sampleData = new Dictionary<string, object>();
        
        foreach (var param in requiredParameters)
        {
            sampleData[param] = param switch
            {
                "EmployeeName" or "UserName" or "Name" => "John Doe",
                "Email" => "john.doe@company.com",
                "Date" => DateTime.Now.ToString("yyyy-MM-dd"),
                "Time" => DateTime.Now.ToString("HH:mm"),
                "CompanyName" => "StrideHR Company",
                "Amount" => "1000.00",
                "Currency" => "USD",
                _ => $"Sample {param}"
            };
        }

        return sampleData;
    }
}