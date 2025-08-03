using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ICalendarIntegrationRepository : IRepository<CalendarIntegration>
{
    Task<List<CalendarIntegration>> GetByEmployeeIdAsync(int employeeId);
    Task<CalendarIntegration?> GetByEmployeeAndProviderAsync(int employeeId, CalendarProvider provider);
    Task<List<CalendarIntegration>> GetActiveIntegrationsAsync();
}

public interface ICalendarEventRepository : IRepository<CalendarEvent>
{
    Task<List<CalendarEvent>> GetByIntegrationIdAsync(int integrationId);
    Task<List<CalendarEvent>> GetByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<CalendarEvent?> GetByProviderEventIdAsync(string providerEventId);
}