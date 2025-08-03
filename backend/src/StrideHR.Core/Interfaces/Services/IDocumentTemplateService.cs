using StrideHR.Core.Models.DocumentTemplate;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IDocumentTemplateService
{
    Task<IEnumerable<DocumentTemplateDto>> GetAllTemplatesAsync();
    Task<IEnumerable<DocumentTemplateDto>> GetActiveTemplatesAsync();
    Task<IEnumerable<DocumentTemplateDto>> GetTemplatesByTypeAsync(DocumentType type);
    Task<IEnumerable<DocumentTemplateDto>> GetTemplatesByCategoryAsync(string category);
    Task<DocumentTemplateDto?> GetTemplateByIdAsync(int id);
    Task<DocumentTemplateDto> CreateTemplateAsync(CreateDocumentTemplateDto dto, int userId);
    Task<DocumentTemplateDto> UpdateTemplateAsync(int id, UpdateDocumentTemplateDto dto, int userId);
    Task<bool> DeleteTemplateAsync(int id, int userId);
    Task<bool> ActivateTemplateAsync(int id, int userId);
    Task<bool> DeactivateTemplateAsync(int id, int userId);
    Task<DocumentTemplatePreviewDto> PreviewTemplateAsync(int id, Dictionary<string, object>? sampleData = null);
    Task<IEnumerable<string>> GetAvailableCategoriesAsync();
    Task<Dictionary<string, object>> GetAvailableMergeFieldsAsync(DocumentType type);
    Task<bool> ValidateTemplateAsync(int id);
    Task<DocumentTemplateDto> CloneTemplateAsync(int id, string newName, int userId);
}