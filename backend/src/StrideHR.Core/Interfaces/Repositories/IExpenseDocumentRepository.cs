using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExpenseDocumentRepository : IRepository<ExpenseDocument>
{
    Task<IEnumerable<ExpenseDocument>> GetByExpenseClaimIdAsync(int expenseClaimId);
    Task<IEnumerable<ExpenseDocument>> GetByExpenseItemIdAsync(int expenseItemId);
    Task<ExpenseDocument?> GetByFileNameAsync(string fileName);
    Task<bool> DeleteDocumentAsync(int documentId);
}