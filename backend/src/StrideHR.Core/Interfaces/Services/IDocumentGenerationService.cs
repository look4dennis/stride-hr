using StrideHR.Core.Models.DocumentTemplate;

namespace StrideHR.Core.Interfaces.Services;

public interface IDocumentGenerationService
{
    Task<GeneratedDocumentDto> GenerateDocumentAsync(GenerateDocumentDto dto, int userId);
    Task<GeneratedDocumentDto?> GetGeneratedDocumentAsync(int id);
    Task<IEnumerable<GeneratedDocumentDto>> GetDocumentsByEmployeeAsync(int employeeId);
    Task<IEnumerable<GeneratedDocumentDto>> GetDocumentsByTemplateAsync(int templateId);
    Task<IEnumerable<GeneratedDocumentDto>> GetDocumentsRequiringSignatureAsync(int employeeId);
    Task<IEnumerable<GeneratedDocumentDto>> GetDocumentsRequiringApprovalAsync(int approverId);
    Task<byte[]> GetDocumentContentAsync(int id);
    Task<bool> SignDocumentAsync(int id, SignDocumentDto dto, int userId);
    Task<bool> ApproveDocumentAsync(int id, ApproveDocumentDto dto, int userId);
    Task<bool> RegenerateDocumentAsync(int id, Dictionary<string, object>? newMergeData, int userId);
    Task<bool> VoidDocumentAsync(int id, string reason, int userId);
    Task<string> GetDocumentDownloadUrlAsync(int id);
    Task LogDocumentAccessAsync(int documentId, int userId, string action, string details);
}