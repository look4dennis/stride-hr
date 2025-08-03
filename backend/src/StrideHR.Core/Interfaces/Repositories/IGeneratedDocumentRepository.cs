using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IGeneratedDocumentRepository : IRepository<GeneratedDocument>
{
    Task<IEnumerable<GeneratedDocument>> GetDocumentsByEmployeeAsync(int employeeId);
    Task<IEnumerable<GeneratedDocument>> GetDocumentsByTemplateAsync(int templateId);
    Task<IEnumerable<GeneratedDocument>> GetDocumentsByStatusAsync(DocumentStatus status);
    Task<GeneratedDocument?> GetDocumentWithSignaturesAsync(int id);
    Task<GeneratedDocument?> GetDocumentWithApprovalsAsync(int id);
    Task<GeneratedDocument?> GetDocumentByNumberAsync(string documentNumber);
    Task<IEnumerable<GeneratedDocument>> GetExpiredDocumentsAsync();
    Task<IEnumerable<GeneratedDocument>> GetDocumentsRequiringSignatureAsync(int employeeId);
    Task<IEnumerable<GeneratedDocument>> GetDocumentsRequiringApprovalAsync(int approverId);
    Task<string> GenerateDocumentNumberAsync(DocumentType type);
}