using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IExternalIntegrationRepository : IRepository<ExternalIntegration>
{
    Task<List<ExternalIntegration>> GetByOrganizationIdAsync(int organizationId);
    Task<List<ExternalIntegration>> GetByTypeAsync(IntegrationType type);
    Task<ExternalIntegration?> GetByOrganizationAndTypeAsync(int organizationId, IntegrationType type, string systemType);
}

public interface IIntegrationLogRepository : IRepository<IntegrationLog>
{
    Task<List<IntegrationLog>> GetByIntegrationIdAsync(int integrationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<IntegrationLog>> GetFailedOperationsAsync(int integrationId);
}