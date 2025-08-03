using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Calendar;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class CalendarIntegrationRepository : Repository<CalendarIntegration>, ICalendarIntegrationRepository
{
    public CalendarIntegrationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<CalendarIntegration>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.CalendarIntegrations
            .Where(c => c.EmployeeId == employeeId)
            .Include(c => c.CalendarEvents)
            .ToListAsync();
    }

    public async Task<CalendarIntegration?> GetByEmployeeAndProviderAsync(int employeeId, CalendarProvider provider)
    {
        return await _context.CalendarIntegrations
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Provider == provider);
    }

    public async Task<List<CalendarIntegration>> GetActiveIntegrationsAsync()
    {
        return await _context.CalendarIntegrations
            .Where(c => c.IsActive)
            .Include(c => c.Employee)
            .ToListAsync();
    }
}

public class CalendarEventRepository : Repository<CalendarEvent>, ICalendarEventRepository
{
    public CalendarEventRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<CalendarEvent>> GetByIntegrationIdAsync(int integrationId)
    {
        return await _context.CalendarEvents
            .Where(e => e.CalendarIntegrationId == integrationId)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<List<CalendarEvent>> GetByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _context.CalendarEvents
            .Include(e => e.CalendarIntegration)
            .Where(e => e.CalendarIntegration.EmployeeId == employeeId &&
                       e.StartTime >= startDate &&
                       e.EndTime <= endDate)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<CalendarEvent?> GetByProviderEventIdAsync(string providerEventId)
    {
        return await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.ProviderEventId == providerEventId);
    }
}