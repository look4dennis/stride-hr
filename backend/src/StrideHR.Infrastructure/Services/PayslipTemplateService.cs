using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class PayslipTemplateService : IPayslipTemplateService
{
    private readonly IPayslipTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PayslipTemplateService> _logger;

    public PayslipTemplateService(
        IPayslipTemplateRepository templateRepository,
        IMapper mapper,
        ILogger<PayslipTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PayslipTemplateDto> CreateTemplateAsync(PayslipTemplateDto templateDto, int createdBy)
    {
        try
        {
            // Validate template
            var (isValid, errors) = await ValidateTemplateAsync(templateDto);
            if (!isValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join(", ", errors)}");
            }

            // Check if template with same name exists
            var existingTemplate = await _templateRepository.GetActiveTemplateByNameAsync(
                templateDto.OrganizationId, templateDto.Name);
            
            if (existingTemplate != null)
            {
                throw new InvalidOperationException($"Template with name '{templateDto.Name}' already exists");
            }

            var template = new PayslipTemplate
            {
                Name = templateDto.Name,
                Description = templateDto.Description,
                OrganizationId = templateDto.OrganizationId,
                BranchId = templateDto.BranchId,
                TemplateConfig = JsonSerializer.Serialize(templateDto.TemplateConfig),
                ShowOrganizationLogo = templateDto.HeaderConfig.ShowOrganizationLogo,
                HeaderText = templateDto.HeaderConfig.HeaderText,
                HeaderColor = templateDto.HeaderConfig.HeaderColor,
                FooterText = templateDto.FooterConfig.FooterText,
                ShowDigitalSignature = templateDto.FooterConfig.ShowDigitalSignature,
                VisibleFields = JsonSerializer.Serialize(templateDto.FieldConfig.VisibleFields),
                FieldLabels = JsonSerializer.Serialize(templateDto.FieldConfig.FieldLabels),
                PrimaryColor = templateDto.StylingConfig.PrimaryColor,
                SecondaryColor = templateDto.StylingConfig.SecondaryColor,
                FontFamily = templateDto.StylingConfig.FontFamily,
                FontSize = templateDto.StylingConfig.FontSize,
                IsActive = templateDto.IsActive,
                IsDefault = templateDto.IsDefault,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            await _templateRepository.AddAsync(template);
            await _templateRepository.SaveChangesAsync();

            // If this is set as default, update other templates
            if (templateDto.IsDefault)
            {
                await _templateRepository.SetAsDefaultAsync(template.Id, templateDto.OrganizationId, templateDto.BranchId);
            }

            _logger.LogInformation("Payslip template created: {TemplateName} by user {CreatedBy}", 
                templateDto.Name, createdBy);

            return await GetTemplateAsync(template.Id) ?? templateDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payslip template: {TemplateName}", templateDto.Name);
            throw;
        }
    }

    public async Task<PayslipTemplateDto> UpdateTemplateAsync(int templateId, PayslipTemplateDto templateDto, int modifiedBy)
    {
        try
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template with ID {templateId} not found");
            }

            // Validate template
            var (isValid, errors) = await ValidateTemplateAsync(templateDto);
            if (!isValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join(", ", errors)}");
            }

            // Check if another template with same name exists
            var existingTemplate = await _templateRepository.GetActiveTemplateByNameAsync(
                templateDto.OrganizationId, templateDto.Name);
            
            if (existingTemplate != null && existingTemplate.Id != templateId)
            {
                throw new InvalidOperationException($"Another template with name '{templateDto.Name}' already exists");
            }

            // Update template properties
            template.Name = templateDto.Name;
            template.Description = templateDto.Description;
            template.TemplateConfig = JsonSerializer.Serialize(templateDto.TemplateConfig);
            template.ShowOrganizationLogo = templateDto.HeaderConfig.ShowOrganizationLogo;
            template.HeaderText = templateDto.HeaderConfig.HeaderText;
            template.HeaderColor = templateDto.HeaderConfig.HeaderColor;
            template.FooterText = templateDto.FooterConfig.FooterText;
            template.ShowDigitalSignature = templateDto.FooterConfig.ShowDigitalSignature;
            template.VisibleFields = JsonSerializer.Serialize(templateDto.FieldConfig.VisibleFields);
            template.FieldLabels = JsonSerializer.Serialize(templateDto.FieldConfig.FieldLabels);
            template.PrimaryColor = templateDto.StylingConfig.PrimaryColor;
            template.SecondaryColor = templateDto.StylingConfig.SecondaryColor;
            template.FontFamily = templateDto.StylingConfig.FontFamily;
            template.FontSize = templateDto.StylingConfig.FontSize;
            template.IsActive = templateDto.IsActive;
            template.IsDefault = templateDto.IsDefault;
            template.LastModifiedBy = modifiedBy;
            template.LastModifiedAt = DateTime.UtcNow;

            await _templateRepository.UpdateAsync(template);
            await _templateRepository.SaveChangesAsync();

            // If this is set as default, update other templates
            if (templateDto.IsDefault)
            {
                await _templateRepository.SetAsDefaultAsync(templateId, templateDto.OrganizationId, templateDto.BranchId);
            }

            _logger.LogInformation("Payslip template updated: {TemplateName} by user {ModifiedBy}", 
                templateDto.Name, modifiedBy);

            return await GetTemplateAsync(templateId) ?? templateDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payslip template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<PayslipTemplateDto?> GetTemplateAsync(int templateId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
            return null;

        return MapToDto(template);
    }

    public async Task<List<PayslipTemplateDto>> GetOrganizationTemplatesAsync(int organizationId)
    {
        var templates = await _templateRepository.GetByOrganizationAsync(organizationId);
        return templates.Select(MapToDto).ToList();
    }

    public async Task<List<PayslipTemplateDto>> GetBranchTemplatesAsync(int branchId)
    {
        var templates = await _templateRepository.GetByBranchAsync(branchId);
        return templates.Select(MapToDto).ToList();
    }

    public async Task<PayslipTemplateDto?> GetDefaultTemplateAsync(int organizationId, int? branchId = null)
    {
        var template = await _templateRepository.GetDefaultTemplateAsync(organizationId, branchId);
        return template != null ? MapToDto(template) : null;
    }

    public async Task<bool> SetAsDefaultAsync(int templateId, int organizationId, int? branchId = null)
    {
        return await _templateRepository.SetAsDefaultAsync(templateId, organizationId, branchId);
    }

    public async Task<bool> DeactivateTemplateAsync(int templateId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
            return false;

        template.IsActive = false;
        template.LastModifiedAt = DateTime.UtcNow;

        await _templateRepository.UpdateAsync(template);
        await _templateRepository.SaveChangesAsync();

        return true;
    }

    public async Task<(bool isValid, List<string> errors)> ValidateTemplateAsync(PayslipTemplateDto templateDto)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(templateDto.Name))
            errors.Add("Template name is required");

        if (templateDto.OrganizationId <= 0)
            errors.Add("Valid organization ID is required");

        // Validate template configuration
        if (templateDto.TemplateConfig?.Sections == null || !templateDto.TemplateConfig.Sections.Any())
            errors.Add("Template must have at least one section");

        // Validate colors
        if (!IsValidHexColor(templateDto.StylingConfig.PrimaryColor))
            errors.Add("Primary color must be a valid hex color");

        if (!IsValidHexColor(templateDto.StylingConfig.SecondaryColor))
            errors.Add("Secondary color must be a valid hex color");

        // Validate font size
        if (templateDto.StylingConfig.FontSize < 8 || templateDto.StylingConfig.FontSize > 24)
            errors.Add("Font size must be between 8 and 24");

        return (errors.Count == 0, errors);
    }

    private static PayslipTemplateDto MapToDto(PayslipTemplate template)
    {
        var templateConfig = string.IsNullOrEmpty(template.TemplateConfig) 
            ? new PayslipTemplateConfig() 
            : JsonSerializer.Deserialize<PayslipTemplateConfig>(template.TemplateConfig) ?? new PayslipTemplateConfig();

        var visibleFields = string.IsNullOrEmpty(template.VisibleFields) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(template.VisibleFields) ?? new List<string>();

        var fieldLabels = string.IsNullOrEmpty(template.FieldLabels) 
            ? new Dictionary<string, string>() 
            : JsonSerializer.Deserialize<Dictionary<string, string>>(template.FieldLabels) ?? new Dictionary<string, string>();

        return new PayslipTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            OrganizationId = template.OrganizationId,
            BranchId = template.BranchId,
            TemplateConfig = templateConfig,
            HeaderConfig = new PayslipHeaderConfig
            {
                ShowOrganizationLogo = template.ShowOrganizationLogo,
                HeaderText = template.HeaderText,
                HeaderColor = template.HeaderColor
            },
            FooterConfig = new PayslipFooterConfig
            {
                FooterText = template.FooterText,
                ShowDigitalSignature = template.ShowDigitalSignature
            },
            FieldConfig = new PayslipFieldConfig
            {
                VisibleFields = visibleFields,
                FieldLabels = fieldLabels
            },
            StylingConfig = new PayslipStylingConfig
            {
                PrimaryColor = template.PrimaryColor,
                SecondaryColor = template.SecondaryColor,
                FontFamily = template.FontFamily,
                FontSize = template.FontSize
            },
            IsActive = template.IsActive,
            IsDefault = template.IsDefault,
            CreatedAt = template.CreatedAt,
            CreatedByName = template.CreatedByEmployee?.FullName ?? "System"
        };
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        return color.StartsWith("#") && color.Length == 7 && 
               color.Skip(1).All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
    }
}