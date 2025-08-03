using StrideHR.Core.Entities;
using StrideHR.Core.Models.Shift;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IShiftRepository : IRepository<Shift>
{
    Task<IEnumerable<Shift>> GetByOrganizationIdAsync(int organizationId);
    Task<IEnumerable<Shift>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<Shift>> GetActiveShiftsAsync(int organizationId);
    Task<IEnumerable<Shift>> SearchShiftsAsync(ShiftSearchCriteria criteria);
    Task<int> GetTotalCountAsync(ShiftSearchCriteria criteria);
    Task<Shift?> GetShiftWithAssignmentsAsync(int shiftId);
    Task<IEnumerable<Shift>> GetShiftTemplatesAsync(int organizationId);
    Task<bool> IsShiftNameUniqueAsync(int organizationId, string name, int? excludeId = null);
    Task<IEnumerable<Shift>> GetShiftsByTypeAsync(int organizationId, Enums.ShiftType shiftType);
    Task<IEnumerable<Shift>> GetOverlappingShiftsAsync(int organizationId, TimeSpan startTime, TimeSpan endTime, int? excludeShiftId = null);
}