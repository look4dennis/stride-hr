using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IDocumentRetentionPolicyRepository : IRepository<DocumentRetentionPolicy>
{
    Task<IEnumerable<DocumentRetentionPolicy>> GetActivePoliciesAsync();
    Task<DocumentRetentionPolicy?> GetPolicyByDocumentTypeAsync(DocumentType documentType);
    Task<IEnumerable<DocumentRetentionExecution>> GetScheduledExecutionsAsync();
    Task<IEnumerable<DocumentRetentionExecution>> GetPendingApprovalsAsync();
    Task<int> GetAffectedDocumentsCountAsync(int policyId);
}