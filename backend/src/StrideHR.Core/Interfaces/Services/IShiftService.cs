using StrideHR.Core.Models.Shift;

namespace StrideHR.Core.Interfaces.Services;

public interface IShiftService
{
    // Shift Management
    Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto);
    Task<ShiftDto> UpdateShiftAsync(int shiftId, UpdateShiftDto updateShiftDto);
    Task<bool> DeleteShiftAsync(int shiftId);
    Task<ShiftDto?> GetShiftByIdAsync(int shiftId);
    Task<IEnumerable<ShiftDto>> GetShiftsByOrganizationAsync(int organizationId);
    Task<IEnumerable<ShiftDto>> GetShiftsByBranchAsync(int branchId);
    Task<IEnumerable<ShiftDto>> GetActiveShiftsAsync(int organizationId);
    Task<(IEnumerable<ShiftDto> Shifts, int TotalCount)> SearchShiftsAsync(ShiftSearchCriteria criteria);
    
    // Shift Templates
    Task<IEnumerable<ShiftTemplateDto>> GetShiftTemplatesAsync(int organizationId);
    Task<ShiftDto> CreateShiftFromTemplateAsync(int templateId, CreateShiftDto createShiftDto);
    
    // Shift Assignment Management
    Task<ShiftAssignmentDto> AssignEmployeeToShiftAsync(CreateShiftAssignmentDto assignmentDto);
    Task<IEnumerable<ShiftAssignmentDto>> BulkAssignEmployeesToShiftAsync(BulkShiftAssignmentDto bulkAssignmentDto);
    Task<ShiftAssignmentDto> UpdateShiftAssignmentAsync(int assignmentId, UpdateShiftAssignmentDto updateDto);
    Task<bool> RemoveEmployeeFromShiftAsync(int assignmentId);
    Task<IEnumerable<ShiftAssignmentDto>> GetEmployeeShiftAssignmentsAsync(int employeeId);
    Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsAsync(int shiftId);
    Task<ShiftAssignmentDto?> GetCurrentEmployeeShiftAsync(int employeeId, DateTime date);
    
    // Shift Coverage and Conflict Detection
    Task<IEnumerable<ShiftCoverageDto>> GetShiftCoverageAsync(int branchId, DateTime date);
    Task<IEnumerable<ShiftConflictDto>> DetectShiftConflictsAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate);
    Task<IEnumerable<ShiftConflictDto>> GetAllShiftConflictsAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<bool> ValidateShiftAssignmentAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate);
    
    // Shift Pattern Management
    Task<IEnumerable<ShiftDto>> GetShiftsByPatternAsync(int organizationId, Enums.ShiftType shiftType);
    Task<IEnumerable<ShiftAssignmentDto>> GenerateRotatingShiftScheduleAsync(int branchId, List<int> employeeIds, List<int> shiftIds, DateTime startDate, int weeks);
    
    // Reporting and Analytics
    Task<IEnumerable<ShiftAssignmentDto>> GetUpcomingShiftAssignmentsAsync(int employeeId, int days = 7);
    Task<Dictionary<string, object>> GetShiftAnalyticsAsync(int branchId, DateTime startDate, DateTime endDate);
}