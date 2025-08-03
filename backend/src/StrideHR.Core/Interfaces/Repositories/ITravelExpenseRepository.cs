using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ITravelExpenseRepository : IRepository<TravelExpense>
{
    Task<TravelExpense?> GetByExpenseClaimIdAsync(int expenseClaimId);
    Task<IEnumerable<TravelExpense>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<TravelExpense>> GetByProjectIdAsync(int projectId);
    Task<decimal> GetTotalMileageByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalTravelExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TravelExpense>> GetByTravelModeAsync(TravelMode travelMode, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetPopularRoutesAsync(DateTime startDate, DateTime endDate, int limit = 10);
    Task<decimal> CalculateMileageAmountAsync(decimal distance, decimal rate, bool isRoundTrip);
}