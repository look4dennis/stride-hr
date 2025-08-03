using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExpenseDocumentRepository : Repository<ExpenseDocument>, IExpenseDocumentRepository
{
    public ExpenseDocumentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExpenseDocument>> GetByExpenseClaimIdAsync(int expenseClaimId)
    {
        return await _dbSet
            .Include(d => d.UploadedByEmployee)
            .Where(d => d.ExpenseClaimId == expenseClaimId)
            .OrderBy(d => d.UploadedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseDocument>> GetByExpenseItemIdAsync(int expenseItemId)
    {
        return await _dbSet
            .Include(d => d.UploadedByEmployee)
            .Where(d => d.ExpenseItemId == expenseItemId)
            .OrderBy(d => d.UploadedDate)
            .ToListAsync();
    }

    public async Task<ExpenseDocument?> GetByFileNameAsync(string fileName)
    {
        return await _dbSet
            .Include(d => d.UploadedByEmployee)
            .FirstOrDefaultAsync(d => d.FileName == fileName);
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        var document = await GetByIdAsync(documentId);
        if (document == null)
            return false;

        await DeleteAsync(document);
        return await SaveChangesAsync();
    }
}