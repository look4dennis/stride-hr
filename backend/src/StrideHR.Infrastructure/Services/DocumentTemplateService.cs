using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DocumentTemplate;

namespace StrideHR.Infrastructure.Services;

public class DocumentTemplateService : IDocumentTemplateService
{
    private readonly IDocumentTemplateRepository _templateRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentTemplateService(
        IDocumentTemplateRepository templateRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DocumentTemplateDto>> GetAllTemplatesAsync()
    {
        var templates = await _templateRepository.GetAllAsync();
        var templateDtos = _mapper.Map<IEnumerable<DocumentTemplateDto>>(templates);
        
        // Add usage count for each template
        foreach (var dto in templateDtos)
        {
            dto.UsageCount = await _templateRepository.GetUsageCountAsync(dto.Id);
        }
        
        return templateDtos;
    }

    public async Task<IEnumerable<DocumentTemplateDto>> GetActiveTemplatesAsync()
    {
        var templates = await _templateRepository.GetActiveTemplatesAsync();
        var templateDtos = _mapper.Map<IEnumerable<DocumentTemplateDto>>(templates);
        
        foreach (var dto in templateDtos)
        {
            dto.UsageCount = await _templateRepository.GetUsageCountAsync(dto.Id);
        }
        
        return templateDtos;
    }

    public async Task<IEnumerable<DocumentTemplateDto>> GetTemplatesByTypeAsync(DocumentType type)
    {
        var templates = await _templateRepository.GetTemplatesByTypeAsync(type);
        return _mapper.Map<IEnumerable<DocumentTemplateDto>>(templates);
    }

    public async Task<IEnumerable<DocumentTemplateDto>> GetTemplatesByCategoryAsync(string category)
    {
        var templates = await _templateRepository.GetTemplatesByCategoryAsync(category);
        return _mapper.Map<IEnumerable<DocumentTemplateDto>>(templates);
    }

    public async Task<DocumentTemplateDto?> GetTemplateByIdAsync(int id)
    {
        var template = await _templateRepository.GetTemplateWithVersionsAsync(id);
        if (template == null) return null;
        
        var dto = _mapper.Map<DocumentTemplateDto>(template);
        dto.UsageCount = await _templateRepository.GetUsageCountAsync(id);
        
        return dto;
    }

    public async Task<DocumentTemplateDto> CreateTemplateAsync(CreateDocumentTemplateDto dto, int userId)
    {
        // Validate unique name
        if (!await _templateRepository.IsTemplateNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Template with name '{dto.Name}' already exists.");
        }

        var template = _mapper.Map<DocumentTemplate>(dto);
        template.CreatedBy = userId;
        template.CreatedAt = DateTime.UtcNow;

        await _templateRepository.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DocumentTemplateDto>(template);
    }

    public async Task<DocumentTemplateDto> UpdateTemplateAsync(int id, UpdateDocumentTemplateDto dto, int userId)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null)
        {
            throw new InvalidOperationException("Template not found.");
        }

        if (template.IsSystemTemplate)
        {
            throw new InvalidOperationException("System templates cannot be modified.");
        }

        // Validate unique name
        if (!await _templateRepository.IsTemplateNameUniqueAsync(dto.Name, id))
        {
            throw new InvalidOperationException($"Template with name '{dto.Name}' already exists.");
        }

        // Create new version if content changed
        if (template.Content != dto.Content)
        {
            var version = new DocumentTemplateVersion
            {
                DocumentTemplateId = id,
                VersionNumber = template.Versions.Count + 1,
                Content = template.Content,
                MergeFields = template.MergeFields,
                ChangeLog = dto.ChangeLog,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            };
            
            template.Versions.Add(version);
        }

        // Update template
        _mapper.Map(dto, template);
        template.LastModifiedBy = userId;
        template.LastModifiedAt = DateTime.UtcNow;

        await _templateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DocumentTemplateDto>(template);
    }

    public async Task<bool> DeleteTemplateAsync(int id, int userId)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null) return false;

        if (template.IsSystemTemplate)
        {
            throw new InvalidOperationException("System templates cannot be deleted.");
        }

        // Check if template is in use
        var usageCount = await _templateRepository.GetUsageCountAsync(id);
        if (usageCount > 0)
        {
            throw new InvalidOperationException("Cannot delete template that has been used to generate documents.");
        }

        await _templateRepository.DeleteAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateTemplateAsync(int id, int userId)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null) return false;

        template.IsActive = true;
        template.LastModifiedBy = userId;
        template.LastModifiedAt = DateTime.UtcNow;

        await _templateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateTemplateAsync(int id, int userId)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null) return false;

        if (template.IsSystemTemplate)
        {
            throw new InvalidOperationException("System templates cannot be deactivated.");
        }

        template.IsActive = false;
        template.LastModifiedBy = userId;
        template.LastModifiedAt = DateTime.UtcNow;

        await _templateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<DocumentTemplatePreviewDto> PreviewTemplateAsync(int id, Dictionary<string, object>? sampleData = null)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null)
        {
            throw new InvalidOperationException("Template not found.");
        }

        var mergeData = sampleData ?? GetSampleMergeData(template.Type);
        var previewHtml = ProcessMergeFields(template.Content, mergeData);

        return new DocumentTemplatePreviewDto
        {
            PreviewHtml = previewHtml,
            SampleData = mergeData
        };
    }

    public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
    {
        return await _templateRepository.GetAvailableCategoriesAsync();
    }

    public async Task<Dictionary<string, object>> GetAvailableMergeFieldsAsync(DocumentType type)
    {
        return GetAvailableMergeFieldsByType(type);
    }

    public async Task<bool> ValidateTemplateAsync(int id)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null) return false;

        // Validate merge fields exist in content
        foreach (var field in template.MergeFields)
        {
            if (!template.Content.Contains($"{{{{{field}}}}}"))
            {
                return false;
            }
        }

        // Validate required fields are present
        foreach (var field in template.RequiredFields)
        {
            if (!template.MergeFields.Contains(field))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<DocumentTemplateDto> CloneTemplateAsync(int id, string newName, int userId)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template == null)
        {
            throw new InvalidOperationException("Template not found.");
        }

        if (!await _templateRepository.IsTemplateNameUniqueAsync(newName))
        {
            throw new InvalidOperationException($"Template with name '{newName}' already exists.");
        }

        var clonedTemplate = new DocumentTemplate
        {
            Name = newName,
            Description = $"Copy of {template.Description}",
            Type = template.Type,
            Content = template.Content,
            MergeFields = template.MergeFields,
            Category = template.Category,
            Settings = new Dictionary<string, object>(template.Settings),
            RequiredFields = template.RequiredFields,
            OptionalFields = template.OptionalFields,
            RequiresApproval = template.RequiresApproval,
            ApprovalWorkflow = template.ApprovalWorkflow,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsSystemTemplate = false
        };

        await _templateRepository.AddAsync(clonedTemplate);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DocumentTemplateDto>(clonedTemplate);
    }

    private static string ProcessMergeFields(string content, Dictionary<string, object> mergeData)
    {
        var processedContent = content;
        
        foreach (var kvp in mergeData)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            var value = kvp.Value?.ToString() ?? "";
            processedContent = processedContent.Replace(placeholder, value);
        }

        return processedContent;
    }

    private static Dictionary<string, object> GetSampleMergeData(DocumentType type)
    {
        return type switch
        {
            DocumentType.OfferLetter => new Dictionary<string, object>
            {
                ["EmployeeName"] = "John Doe",
                ["Position"] = "Software Developer",
                ["Salary"] = "$75,000",
                ["StartDate"] = DateTime.Now.AddDays(14).ToString("MMMM dd, yyyy"),
                ["CompanyName"] = "StrideHR Inc.",
                ["ManagerName"] = "Jane Smith"
            },
            DocumentType.Contract => new Dictionary<string, object>
            {
                ["EmployeeName"] = "John Doe",
                ["EmployeeId"] = "EMP-2025-001",
                ["Position"] = "Software Developer",
                ["Department"] = "Engineering",
                ["ContractDate"] = DateTime.Now.ToString("MMMM dd, yyyy"),
                ["CompanyName"] = "StrideHR Inc."
            },
            _ => new Dictionary<string, object>
            {
                ["EmployeeName"] = "John Doe",
                ["Date"] = DateTime.Now.ToString("MMMM dd, yyyy"),
                ["CompanyName"] = "StrideHR Inc."
            }
        };
    }

    private static Dictionary<string, object> GetAvailableMergeFieldsByType(DocumentType type)
    {
        var commonFields = new Dictionary<string, object>
        {
            ["EmployeeName"] = "Employee's full name",
            ["EmployeeId"] = "Employee ID",
            ["Date"] = "Current date",
            ["CompanyName"] = "Organization name",
            ["CompanyAddress"] = "Organization address"
        };

        var typeSpecificFields = type switch
        {
            DocumentType.OfferLetter => new Dictionary<string, object>
            {
                ["Position"] = "Job position",
                ["Salary"] = "Offered salary",
                ["StartDate"] = "Employment start date",
                ["ManagerName"] = "Reporting manager name",
                ["Department"] = "Department name"
            },
            DocumentType.Contract => new Dictionary<string, object>
            {
                ["Position"] = "Job position",
                ["Department"] = "Department name",
                ["ContractDate"] = "Contract signing date",
                ["ContractDuration"] = "Contract duration",
                ["Salary"] = "Base salary"
            },
            DocumentType.Certificate => new Dictionary<string, object>
            {
                ["CertificateName"] = "Certificate title",
                ["CompletionDate"] = "Completion date",
                ["ValidUntil"] = "Certificate expiry date",
                ["CertificateNumber"] = "Certificate number"
            },
            _ => new Dictionary<string, object>()
        };

        return commonFields.Concat(typeSpecificFields).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}