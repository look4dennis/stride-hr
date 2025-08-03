using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class GeneratedDocumentRepository : Repository<GeneratedDocument>, IGeneratedDocumentRepository
{
    public GeneratedDocumentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GeneratedDocument>> GetDocumentsByEmployeeAsync(int employeeId)
    {
        return await _context.GeneratedDocuments
            .Where(d => d.EmployeeId == employeeId)
            .Include(d => d.DocumentTemplate)
            .Include(d => d.GeneratedByEmployee)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedDocument>> GetDocumentsByTemplateAsync(int templateId)
    {
        return await _context.GeneratedDocuments
            .Where(d => d.DocumentTemplateId == templateId)
            .Include(d => d.Employee)
            .Include(d => d.GeneratedByEmployee)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedDocument>> GetDocumentsByStatusAsync(DocumentStatus status)
    {
        return await _context.GeneratedDocuments
            .Where(d => d.Status == status)
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.GeneratedByEmployee)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync();
    }

    public async Task<GeneratedDocument?> GetDocumentWithSignaturesAsync(int id)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.GeneratedByEmployee)
            .Include(d => d.Signatures.OrderBy(s => s.SignatureOrder))
                .ThenInclude(s => s.Signer)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<GeneratedDocument?> GetDocumentWithApprovalsAsync(int id)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.GeneratedByEmployee)
            .Include(d => d.Approvals.OrderBy(a => a.ApprovalOrder))
                .ThenInclude(a => a.Approver)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<GeneratedDocument?> GetDocumentByNumberAsync(string documentNumber)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.GeneratedByEmployee)
            .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber);
    }

    public async Task<IEnumerable<GeneratedDocument>> GetExpiredDocumentsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.GeneratedDocuments
            .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value.Date <= today)
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedDocument>> GetDocumentsRequiringSignatureAsync(int employeeId)
    {
        return await _context.GeneratedDocuments
            .Where(d => d.RequiresSignature && 
                       d.Signatures.Any(s => s.SignerId == employeeId && s.Action == ApprovalAction.Pending))
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.Signatures.Where(s => s.SignerId == employeeId))
            .OrderBy(d => d.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneratedDocument>> GetDocumentsRequiringApprovalAsync(int approverId)
    {
        return await _context.GeneratedDocuments
            .Where(d => d.Approvals.Any(a => a.ApproverId == approverId && a.Action == ApprovalAction.Pending))
            .Include(d => d.DocumentTemplate)
            .Include(d => d.Employee)
            .Include(d => d.Approvals.Where(a => a.ApproverId == approverId))
            .OrderBy(d => d.GeneratedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateDocumentNumberAsync(DocumentType type)
    {
        var prefix = GetDocumentPrefix(type);
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        var lastNumber = await _context.GeneratedDocuments
            .Where(d => d.DocumentTemplate.Type == type && 
                       d.DocumentNumber.StartsWith($"{prefix}-{year:D4}-{month:D2}"))
            .OrderByDescending(d => d.DocumentNumber)
            .Select(d => d.DocumentNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 4 && int.TryParse(parts[3], out int lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}-{year:D4}-{month:D2}-{nextSequence:D4}";
    }

    private static string GetDocumentPrefix(DocumentType type)
    {
        return type switch
        {
            DocumentType.OfferLetter => "OL",
            DocumentType.Contract => "CT",
            DocumentType.Policy => "PL",
            DocumentType.Certificate => "CR",
            DocumentType.Report => "RP",
            DocumentType.Form => "FM",
            DocumentType.Memo => "MM",
            DocumentType.Notice => "NT",
            _ => "DOC"
        };
    }
}