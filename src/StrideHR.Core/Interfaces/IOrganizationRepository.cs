using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Repository interface for Organization entity
/// </summary>
public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Organization?> GetWithBranchesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Branch entity
/// </summary>
public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetActiveBranchesAsync(int? organizationId = null, CancellationToken cancellationToken = default);
    Task<Branch?> GetByNameAsync(string name, int organizationId, CancellationToken cancellationToken = default);
    Task<Branch?> GetWithEmployeesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string name, int organizationId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDistinctCountriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDistinctCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDistinctTimeZonesAsync(CancellationToken cancellationToken = default);
}