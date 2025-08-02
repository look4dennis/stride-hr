namespace StrideHR.Core.Enums;

public enum EmailTemplateType
{
    // Authentication & Onboarding
    Welcome,
    PasswordReset,
    AccountActivation,
    OnboardingReminder,
    
    // Attendance
    AttendanceAlert,
    LateArrivalNotification,
    AbsenceNotification,
    OvertimeApproval,
    
    // Leave Management
    LeaveRequestSubmitted,
    LeaveRequestApproved,
    LeaveRequestRejected,
    LeaveBalanceAlert,
    LeaveReminder,
    
    // Payroll
    PayslipGenerated,
    PayrollProcessed,
    PayrollApprovalRequired,
    SalaryRevision,
    
    // Performance
    PerformanceReviewReminder,
    PIPAssignment,
    GoalDeadlineReminder,
    PerformanceFeedback,
    
    // Project Management
    TaskAssignment,
    ProjectDeadline,
    DSRReminder,
    ProjectCompletion,
    
    // Training & Development
    TrainingAssignment,
    CertificationReminder,
    TrainingCompletion,
    
    // Birthday & Celebrations
    BirthdayWishes,
    WorkAnniversary,
    Achievement,
    
    // System & Administrative
    SystemMaintenance,
    PolicyUpdate,
    SecurityAlert,
    Announcement,
    
    // Support & IT
    TicketCreated,
    TicketResolved,
    AssetAssignment,
    
    // General
    CustomTemplate,
    BulkAnnouncement
}