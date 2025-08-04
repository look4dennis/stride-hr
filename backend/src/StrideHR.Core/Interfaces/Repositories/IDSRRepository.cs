using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IDSRRepository : IRepository<DSR>
{
    Task<IEnumerable<DSR>> GetDSRsByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<DSR>> GetDSRsByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<DSR>> GetDSRsByStatusAsync(DSRStatus status);
    Task<DSR?> GetDSRByEmployeeAndDateAsync(int employeeId, DateTime date);
    Task<IEnumerable<DSR>> GetPendingDSRsForReviewAsync(int reviewerId);
    Task<decimal> GetTotalHoursByProjectAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalHoursByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<DSR>> GetProjectDSRsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
}