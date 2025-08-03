using StrideHR.Core.Models.DocumentTemplate;

namespace StrideHR.Core.Interfaces.Services;

public interface IDocumentRetentionService
{
    Task<IEnumerable<DocumentRetentionPolicyDto>> GetAllPoliciesAsync();
    Task<IEnumerable<DocumentRetentionPolicyDto>> GetActivePoliciesAsync();
    Task<DocumentRetentionPolicyDto?> GetPolicyByIdAsync(int id);
    Task<DocumentRetentionPolicyDto> CreatePolicyAsync(CreateDocumentRetentionPolicyDto dto, int userId);
    Task<DocumentRetentionPolicyDto> UpdatePolicyAsync(int id, UpdateDocumentRetentionPolicyDto dto, int userId);
    Task<bool> DeletePolicyAsync(int id, int userId);
    Task<bool> ActivatePolicyAsync(int id, int userId);
    Task<bool> DeactivatePolicyAsync(int id, int userId);
    Task<IEnumerable<DocumentRetentionExecutionDto>> GetScheduledExecutionsAsync();
    Task<IEnumerable<DocumentRetentionExecutionDto>> GetPendingApprovalsAsync();
    Task<bool> ApproveRetentionExecutionAsync(int executionId, string comments, int userId);
    Task<bool> RejectRetentionExecutionAsync(int executionId, string reason, int userId);
    Task ProcessScheduledRetentionsAsync();
    Task<int> GetAffectedDocumentsCountAsync(int policyId);
}