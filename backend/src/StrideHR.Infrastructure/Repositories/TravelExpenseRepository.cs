using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class TravelExpenseRepository : Repository<TravelExpense>, ITravelExpenseRepository
{
    public TravelExpenseRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<TravelExpense?> GetByExpenseClaimIdAsync(int expenseClaimId)
    {
        return await _context.TravelExpenses
            .Include(te => te.TravelItems)
            .Include(te => te.ExpenseClaim)
            .Include(te => te.Project)
            .FirstOrDefaultAsync(te => te.ExpenseClaimId == expenseClaimId);
    }

    public async Task<IEnumerable<TravelExpense>> GetByEmployeeIdAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TravelExpenses
            .Include(te => te.TravelItems)
            .Include(te => te.ExpenseClaim)
            .Include(te => te.Project)
            .Where(te => te.ExpenseClaim.EmployeeId == employeeId);

        if (startDate.HasValue)
        {
            query = query.Where(te => te.DepartureDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(te => te.ReturnDate <= endDate.Value);
        }

        return await query.OrderByDescending(te => te.DepartureDate).ToListAsync();
    }

    public async Task<IEnumerable<TravelExpense>> GetByProjectIdAsync(int projectId)
    {
        return await _context.TravelExpenses
            .Include(te => te.TravelItems)
            .Include(te => te.ExpenseClaim)
            .Include(te => te.Project)
            .Where(te => te.ProjectId == projectId)
            .OrderByDescending(te => te.DepartureDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalMileageByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _context.TravelExpenses
            .Where(te => te.ExpenseClaim.EmployeeId == employeeId &&
                        te.DepartureDate >= startDate &&
                        te.ReturnDate <= endDate &&
                        te.MileageDistance.HasValue)
            .SumAsync(te => te.MileageDistance ?? 0);
    }

    public async Task<decimal> GetTotalTravelExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _context.TravelExpenses
            .Where(te => te.ExpenseClaim.EmployeeId == employeeId &&
                        te.DepartureDate >= startDate &&
                        te.ReturnDate <= endDate)
            .SelectMany(te => te.TravelItems)
            .SumAsync(ti => ti.Amount);
    }

    public async Task<IEnumerable<TravelExpense>> GetByTravelModeAsync(TravelMode travelMode, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TravelExpenses
            .Include(te => te.TravelItems)
            .Include(te => te.ExpenseClaim)
            .Include(te => te.Project)
            .Where(te => te.TravelMode == travelMode);

        if (startDate.HasValue)
        {
            query = query.Where(te => te.DepartureDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(te => te.ReturnDate <= endDate.Value);
        }

        return await query.OrderByDescending(te => te.DepartureDate).ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetPopularRoutesAsync(DateTime startDate, DateTime endDate, int limit = 10)
    {
        var routes = await _context.TravelExpenses
            .Where(te => te.DepartureDate >= startDate && te.ReturnDate <= endDate)
            .GroupBy(te => new { te.FromLocation, te.ToLocation })
            .Select(g => new { Route = $"{g.Key.FromLocation} → {g.Key.ToLocation}", Count = g.Count() })
            .OrderByDescending(r => r.Count)
            .Take(limit)
            .ToListAsync();

        return routes.ToDictionary(r => r.Route, r => r.Count);
    }

    public async Task<decimal> CalculateMileageAmountAsync(decimal distance, decimal rate, bool isRoundTrip)
    {
        var totalDistance = isRoundTrip ? distance * 2 : distance;
        return totalDistance * rate;
    }
}