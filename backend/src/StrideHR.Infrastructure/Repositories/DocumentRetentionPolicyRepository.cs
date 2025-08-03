using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class DocumentRetentionPolicyRepository : Repository<DocumentRetentionPolicy>, IDocumentRetentionPolicyRepository
{
    public DocumentRetentionPolicyRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DocumentRetentionPolicy>> GetActivePoliciesAsync()
    {
        return await _context.DocumentRetentionPolicies
            .Where(p => p.IsActive)
            .Include(p => p.CreatedByEmployee)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<DocumentRetentionPolicy?> GetPolicyByDocumentTypeAsync(DocumentType documentType)
    {
        return await _context.DocumentRetentionPolicies
            .Where(p => p.DocumentType == documentType && p.IsActive)
            .Include(p => p.CreatedByEmployee)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<DocumentRetentionExecution>> GetScheduledExecutionsAsync()
    {
        return await _context.DocumentRetentionExecutions
            .Where(e => e.Status == "Scheduled" && e.ScheduledDate <= DateTime.UtcNow)
            .Include(e => e.DocumentRetentionPolicy)
            .Include(e => e.GeneratedDocument)
                .ThenInclude(d => d.Employee)
            .OrderBy(e => e.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentRetentionExecution>> GetPendingApprovalsAsync()
    {
        return await _context.DocumentRetentionExecutions
            .Where(e => e.RequiredApproval && !e.IsApproved && e.Status == "Scheduled")
            .Include(e => e.DocumentRetentionPolicy)
            .Include(e => e.GeneratedDocument)
                .ThenInclude(d => d.Employee)
            .OrderBy(e => e.ScheduledDate)
            .ToListAsync();
    }

    public async Task<int> GetAffectedDocumentsCountAsync(int policyId)
    {
        var policy = await _context.DocumentRetentionPolicies.FindAsync(policyId);
        if (policy == null) return 0;

        var cutoffDate = DateTime.UtcNow.AddMonths(-policy.RetentionPeriodMonths);
        
        return await _context.GeneratedDocuments
            .Where(d => d.DocumentTemplate.Type == policy.DocumentType && 
                       d.GeneratedAt <= cutoffDate)
            .CountAsync();
    }
}