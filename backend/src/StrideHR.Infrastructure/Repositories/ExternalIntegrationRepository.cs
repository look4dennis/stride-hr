using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ExternalIntegrationRepository : Repository<ExternalIntegration>, IExternalIntegrationRepository
{
    public ExternalIntegrationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ExternalIntegration>> GetByOrganizationIdAsync(int organizationId)
    {
        return await _context.ExternalIntegrations
            .Where(i => i.OrganizationId == organizationId)
            .Include(i => i.Logs.OrderByDescending(l => l.CreatedAt).Take(10))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ExternalIntegration>> GetByTypeAsync(IntegrationType type)
    {
        return await _context.ExternalIntegrations
            .Where(i => i.Type == type && i.IsActive)
            .ToListAsync();
    }

    public async Task<ExternalIntegration?> GetByOrganizationAndTypeAsync(int organizationId, IntegrationType type, string systemType)
    {
        return await _context.ExternalIntegrations
            .FirstOrDefaultAsync(i => i.OrganizationId == organizationId && 
                                     i.Type == type && 
                                     i.SystemType == systemType);
    }
}

public class IntegrationLogRepository : Repository<IntegrationLog>, IIntegrationLogRepository
{
    public IntegrationLogRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<IntegrationLog>> GetByIntegrationIdAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.IntegrationLogs
            .Where(l => l.ExternalIntegrationId == integrationId);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<IntegrationLog>> GetFailedOperationsAsync(int integrationId)
    {
        return await _context.IntegrationLogs
            .Where(l => l.ExternalIntegrationId == integrationId && l.Status == "Failed")
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}