using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class LeaveCalendarRepository : Repository<LeaveCalendar>, ILeaveCalendarRepository
{
    public LeaveCalendarRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveCalendar>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int branchId)
    {
        return await _dbSet
            .Include(lc => lc.Employee)
            .Include(lc => lc.LeaveRequest)
                .ThenInclude(lr => lr.LeavePolicy)
            .Where(lc => lc.Date >= startDate &&
                        lc.Date <= endDate &&
                        lc.Employee.BranchId == branchId &&
                        !lc.IsDeleted)
            .OrderBy(lc => lc.Date)
            .ThenBy(lc => lc.Employee.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveCalendar>> GetByEmployeeAndDateRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(lc => lc.Employee)
            .Include(lc => lc.LeaveRequest)
                .ThenInclude(lr => lr.LeavePolicy)
            .Where(lc => lc.EmployeeId == employeeId &&
                        lc.Date >= startDate &&
                        lc.Date <= endDate &&
                        !lc.IsDeleted)
            .OrderBy(lc => lc.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveCalendar>> GetTeamCalendarAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(lc => lc.Employee)
            .Include(lc => lc.LeaveRequest)
                .ThenInclude(lr => lr.LeavePolicy)
            .Where(lc => lc.Employee.ReportingManagerId == managerId &&
                        lc.Date >= startDate &&
                        lc.Date <= endDate &&
                        !lc.IsDeleted)
            .OrderBy(lc => lc.Date)
            .ThenBy(lc => lc.Employee.FirstName)
            .ToListAsync();
    }
}