namespace StrideHR.Core.Enums;

public enum NotificationType
{
    // Attendance Related
    AttendanceCheckIn,
    AttendanceCheckOut,
    AttendanceLate,
    AttendanceMissed,
    BreakStarted,
    BreakEnded,
    OvertimeAlert,
    
    // Leave Related
    LeaveRequested,
    LeaveApproved,
    LeaveRejected,
    LeaveBalanceLow,
    
    // Payroll Related
    PayrollGenerated,
    PayslipAvailable,
    PayrollApprovalRequired,
    
    // Project Related
    TaskAssigned,
    TaskCompleted,
    ProjectDeadlineApproaching,
    ProjectOverdue,
    DSRReminder,
    DSRSubmitted,
    
    // Performance Related
    PerformanceReviewDue,
    PIPAssigned,
    PIPReviewDue,
    GoalDeadlineApproaching,
    
    // Birthday and Celebrations
    BirthdayToday,
    BirthdayWishes,
    WorkAnniversary,
    
    // System Alerts
    SystemMaintenance,
    SecurityAlert,
    PolicyUpdate,
    
    // General
    Announcement,
    Reminder,
    Welcome,
    Congratulations
}