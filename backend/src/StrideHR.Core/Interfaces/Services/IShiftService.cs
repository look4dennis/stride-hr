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
    
    // Shift Swapping
    Task<ShiftSwapRequestDto> CreateShiftSwapRequestAsync(int requesterId, CreateShiftSwapRequestDto createDto);
    Task<ShiftSwapRequestDto> RespondToShiftSwapRequestAsync(int responderId, CreateShiftSwapResponseDto responseDto);
    Task<ShiftSwapRequestDto> ApproveShiftSwapRequestAsync(int requestId, int approverId, ApproveShiftSwapDto approvalDto);
    Task<bool> CancelShiftSwapRequestAsync(int requestId, int userId);
    Task<IEnumerable<ShiftSwapRequestDto>> GetShiftSwapRequestsAsync(int employeeId);
    Task<IEnumerable<ShiftSwapRequestDto>> GetPendingShiftSwapApprovalsAsync(int managerId);
    Task<(IEnumerable<ShiftSwapRequestDto> Requests, int TotalCount)> SearchShiftSwapRequestsAsync(ShiftSwapSearchCriteria criteria);
    
    // Shift Coverage
    Task<ShiftCoverageRequestDto> CreateShiftCoverageRequestAsync(int requesterId, CreateShiftCoverageRequestDto createDto);
    Task<ShiftCoverageRequestDto> RespondToShiftCoverageRequestAsync(int responderId, CreateShiftCoverageResponseDto responseDto);
    Task<ShiftCoverageRequestDto> ApproveShiftCoverageRequestAsync(int requestId, int approverId, ApproveShiftCoverageDto approvalDto);
    Task<bool> CancelShiftCoverageRequestAsync(int requestId, int userId);
    Task<IEnumerable<ShiftCoverageRequestDto>> GetShiftCoverageRequestsAsync(int employeeId);
    Task<IEnumerable<ShiftCoverageRequestDto>> GetPendingShiftCoverageApprovalsAsync(int managerId);
    Task<(IEnumerable<ShiftCoverageRequestDto> Requests, int TotalCount)> SearchShiftCoverageRequestsAsync(ShiftCoverageSearchCriteria criteria);
    
    // Emergency Coverage Broadcasting
    Task<List<ShiftCoverageRequestDto>> BroadcastEmergencyShiftCoverageAsync(int broadcasterId, EmergencyShiftCoverageBroadcastDto broadcastDto);
    Task<IEnumerable<ShiftCoverageRequestDto>> GetEmergencyShiftCoverageRequestsAsync(int branchId);
    
    // Reporting and Analytics
    Task<IEnumerable<ShiftAssignmentDto>> GetUpcomingShiftAssignmentsAsync(int employeeId, int days = 7);
    Task<Dictionary<string, object>> GetShiftAnalyticsAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<ShiftAnalyticsDto> GetDetailedShiftAnalyticsAsync(ShiftAnalyticsSearchCriteria criteria);
}