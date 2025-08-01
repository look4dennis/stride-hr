using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces;

/// <summary>
/// Repository interface for AttendanceRecord entity
/// </summary>
public interface IAttendanceRepository : IRepository<AttendanceRecord>
{
    // Today's attendance
    Task<AttendanceRecord?> GetTodayRecordAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetTodayBranchRecordsAsync(int branchId, CancellationToken cancellationToken = default);
    
    // Employee attendance queries
    Task<IEnumerable<AttendanceRecord>> GetEmployeeAttendanceAsync(int employeeId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetEmployeeAttendanceByDateAsync(int employeeId, DateTime date, CancellationToken cancellationToken = default);
    
    // Branch attendance queries
    Task<IEnumerable<AttendanceRecord>> GetBranchAttendanceAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetBranchAttendanceByDateAsync(int branchId, DateTime date, CancellationToken cancellationToken = default);
    
    // Search and filtering
    Task<(IEnumerable<AttendanceRecord> Records, int TotalCount)> SearchAsync(AttendanceSearchCriteria criteria, CancellationToken cancellationToken = default);
    
    // Status queries
    Task<IEnumerable<AttendanceRecord>> GetByStatusAsync(AttendanceStatus status, int? branchId = null, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetLateArrivalsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceRecord>> GetOvertimeRecordsAsync(int branchId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // Include related data
    Task<AttendanceRecord?> GetWithBreaksAsync(int id, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetWithCorrectionsAsync(int id, CancellationToken cancellationToken = default);
    Task<AttendanceRecord?> GetWithAllDetailsAsync(int id, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<int> GetPresentCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default);
    Task<int> GetAbsentCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default);
    Task<int> GetLateCountAsync(int branchId, DateTime date, CancellationToken cancellationToken = default);
    Task<int> GetOnBreakCountAsync(int branchId, CancellationToken cancellationToken = default);
    
    // Manual entries
    Task<IEnumerable<AttendanceRecord>> GetManualEntriesAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for BreakRecord entity
/// </summary>
public interface IBreakRecordRepository : IRepository<BreakRecord>
{
    Task<IEnumerable<BreakRecord>> GetByAttendanceRecordAsync(int attendanceRecordId, CancellationToken cancellationToken = default);
    Task<BreakRecord?> GetActiveBreakAsync(int attendanceRecordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BreakRecord>> GetActiveBreaksByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BreakRecord>> GetBreaksByTypeAsync(BreakType type, int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<BreakRecord>> GetExceededBreaksAsync(int? branchId = null, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<BreakRecord>> GetPendingApprovalBreaksAsync(int? branchId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for AttendanceCorrection entity
/// </summary>
public interface IAttendanceCorrectionRepository : IRepository<AttendanceCorrection>
{
    Task<IEnumerable<AttendanceCorrection>> GetByAttendanceRecordAsync(int attendanceRecordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceCorrection>> GetByStatusAsync(CorrectionStatus status, int? branchId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceCorrection>> GetPendingCorrectionsAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceCorrection>> GetByRequestedByAsync(int requestedBy, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceCorrection>> GetByApprovedByAsync(int approvedBy, CancellationToken cancellationToken = default);
}