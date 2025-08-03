using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<IEnumerable<ExpenseCategory>> GetActiveByOrganizationAsync(int organizationId);
    Task<ExpenseCategory?> GetByCodeAsync(string code, int organizationId);
    Task<IEnumerable<ExpenseCategory>> GetMileageBasedCategoriesAsync(int organizationId);
    Task<bool> IsCodeUniqueAsync(string code, int organizationId, int? excludeId = null);
}