using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data;

/// <summary>
/// Database performance optimizations for StrideHR system
/// </summary>
public static class DatabaseOptimizations
{
    /// <summary>
    /// Configure additional indexes for performance optimization
    /// </summary>
    public static void ConfigurePerformanceIndexes(ModelBuilder modelBuilder)
    {
        // Employee performance indexes
        ConfigureEmployeeIndexes(modelBuilder);
        
        // Attendance performance indexes
        ConfigureAttendanceIndexes(modelBuilder);
        
        // Payroll performance indexes
        ConfigurePayrollIndexes(modelBuilder);
        
        // Project management indexes
        ConfigureProjectIndexes(modelBuilder);
        
        // Audit and logging indexes
        ConfigureAuditIndexes(modelBuilder);
        
        // Notification indexes
        ConfigureNotificationIndexes(modelBuilder);
        
        // Leave management indexes
        ConfigureLeaveIndexes(modelBuilder);
        
        // Performance management indexes
        ConfigurePerformanceManagementIndexes(modelBuilder);
    }

    private static void ConfigureEmployeeIndexes(ModelBuilder modelBuilder)
    {
        // Employee search and filtering indexes
        modelBuilder.Entity<Employee>()
            .HasIndex(e => new { e.BranchId, e.Status })
            .HasDatabaseName("IX_Employee_Branch_Status");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => new { e.Department, e.Status })
            .HasDatabaseName("IX_Employee_Department_Status");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => new { e.ReportingManagerId, e.Status })
            .HasDatabaseName("IX_Employee_Manager_Status");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.JoiningDate)
            .HasDatabaseName("IX_Employee_JoiningDate");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => new { e.FirstName, e.LastName })
            .HasDatabaseName("IX_Employee_FullName");

        // User authentication indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.IsActive })
            .HasDatabaseName("IX_User_Email_Active");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.LastLoginAt)
            .HasDatabaseName("IX_User_LastLogin");

        // Role and permission indexes
        modelBuilder.Entity<EmployeeRole>()
            .HasIndex(er => new { er.EmployeeId, er.IsActive })
            .HasDatabaseName("IX_EmployeeRole_Employee_Active");

        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.IsGranted })
            .HasDatabaseName("IX_RolePermission_Role_Granted");
    }

    private static void ConfigureAttendanceIndexes(ModelBuilder modelBuilder)
    {
        // Attendance reporting indexes
        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.EmployeeId, a.Date, a.Status })
            .HasDatabaseName("IX_Attendance_Employee_Date_Status");

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.Date, a.Status })
            .HasDatabaseName("IX_Attendance_Date_Status");

        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.EmployeeId, a.CreatedAt })
            .HasDatabaseName("IX_Attendance_Employee_Created");

        // Break records indexes
        modelBuilder.Entity<BreakRecord>()
            .HasIndex(b => new { b.AttendanceRecordId, b.StartTime })
            .HasDatabaseName("IX_BreakRecord_Attendance_Start");

        // Shift management indexes
        modelBuilder.Entity<ShiftAssignment>()
            .HasIndex(sa => new { sa.EmployeeId, sa.StartDate })
            .HasDatabaseName("IX_ShiftAssignment_Employee_StartDate");

        modelBuilder.Entity<ShiftSwapRequest>()
            .HasIndex(ssr => new { ssr.RequesterId, ssr.Status })
            .HasDatabaseName("IX_ShiftSwap_Employee_Status");
    }

    private static void ConfigurePayrollIndexes(ModelBuilder modelBuilder)
    {
        // Payroll processing indexes
        modelBuilder.Entity<PayrollRecord>()
            .HasIndex(p => new { p.PayrollYear, p.PayrollMonth, p.Status })
            .HasDatabaseName("IX_Payroll_Period_Status");

        modelBuilder.Entity<PayrollRecord>()
            .HasIndex(p => new { p.EmployeeId, p.Status })
            .HasDatabaseName("IX_Payroll_Employee_Status");

        modelBuilder.Entity<PayrollRecord>()
            .HasIndex(p => p.ProcessedAt)
            .HasDatabaseName("IX_Payroll_ProcessedAt");

        // Payroll adjustments
        modelBuilder.Entity<PayrollAdjustment>()
            .HasIndex(pa => new { pa.PayrollRecordId, pa.Type })
            .HasDatabaseName("IX_PayrollAdjustment_Record_Type");

        // Exchange rates for multi-currency
        modelBuilder.Entity<ExchangeRate>()
            .HasIndex(er => new { er.FromCurrency, er.ToCurrency, er.EffectiveDate })
            .HasDatabaseName("IX_ExchangeRate_Currencies_EffectiveDate");
    }

    private static void ConfigureProjectIndexes(ModelBuilder modelBuilder)
    {
        // Project management indexes
        modelBuilder.Entity<Project>()
            .HasIndex(p => new { p.Status, p.Priority })
            .HasDatabaseName("IX_Project_Status_Priority");

        modelBuilder.Entity<Project>()
            .HasIndex(p => new { p.StartDate, p.EndDate })
            .HasDatabaseName("IX_Project_DateRange");

        modelBuilder.Entity<ProjectTask>()
            .HasIndex(pt => new { pt.ProjectId, pt.Status })
            .HasDatabaseName("IX_ProjectTask_Project_Status");

        modelBuilder.Entity<ProjectTask>()
            .HasIndex(pt => new { pt.AssignedToEmployeeId, pt.Status })
            .HasDatabaseName("IX_ProjectTask_Assignee_Status");

        modelBuilder.Entity<ProjectAssignment>()
            .HasIndex(pa => new { pa.EmployeeId, pa.UnassignedDate })
            .HasDatabaseName("IX_ProjectAssignment_Employee_Unassigned");

        // DSR (Daily Status Report) indexes
        modelBuilder.Entity<DSR>()
            .HasIndex(d => new { d.EmployeeId, d.Date })
            .HasDatabaseName("IX_DSR_Employee_Date");

        modelBuilder.Entity<DSR>()
            .HasIndex(d => new { d.ProjectId, d.Date })
            .HasDatabaseName("IX_DSR_Project_Date");
    }

    private static void ConfigureAuditIndexes(ModelBuilder modelBuilder)
    {
        // Audit log indexes for compliance and monitoring
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.UserId, al.Timestamp })
            .HasDatabaseName("IX_AuditLog_User_Timestamp");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => new { al.EventType, al.Timestamp })
            .HasDatabaseName("IX_AuditLog_EventType_Timestamp");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");
    }

    private static void ConfigureNotificationIndexes(ModelBuilder modelBuilder)
    {
        // Notification delivery indexes
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notification_User_Read");

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.Type, n.CreatedAt })
            .HasDatabaseName("IX_Notification_Type_Created");

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.IsRead, n.CreatedAt })
            .HasDatabaseName("IX_Notification_Read_Created");
    }

    private static void ConfigureLeaveIndexes(ModelBuilder modelBuilder)
    {
        // Leave management indexes
        modelBuilder.Entity<LeaveRequest>()
            .HasIndex(lr => new { lr.EmployeeId, lr.Status })
            .HasDatabaseName("IX_LeaveRequest_Employee_Status");

        modelBuilder.Entity<LeaveRequest>()
            .HasIndex(lr => new { lr.StartDate, lr.EndDate })
            .HasDatabaseName("IX_LeaveRequest_DateRange");

        modelBuilder.Entity<LeaveBalance>()
            .HasIndex(lb => new { lb.EmployeeId, lb.LeavePolicyId, lb.Year })
            .HasDatabaseName("IX_LeaveBalance_Employee_Policy_Year");

        modelBuilder.Entity<LeaveApprovalHistory>()
            .HasIndex(lah => new { lah.LeaveRequestId, lah.Level })
            .HasDatabaseName("IX_LeaveApproval_Request_Level");
    }

    private static void ConfigurePerformanceManagementIndexes(ModelBuilder modelBuilder)
    {
        // Performance management indexes
        modelBuilder.Entity<PerformanceReview>()
            .HasIndex(pr => new { pr.EmployeeId, pr.ReviewPeriod })
            .HasDatabaseName("IX_PerformanceReview_Employee_Period");

        modelBuilder.Entity<PerformanceGoal>()
            .HasIndex(pg => new { pg.EmployeeId, pg.Status })
            .HasDatabaseName("IX_PerformanceGoal_Employee_Status");

        modelBuilder.Entity<PerformanceImprovementPlan>()
            .HasIndex(pip => new { pip.EmployeeId, pip.Status })
            .HasDatabaseName("IX_PIP_Employee_Status");
    }

    /// <summary>
    /// Configure database connection optimizations
    /// </summary>
    public static void ConfigureConnectionOptimizations(IServiceCollection services)
    {
        services.Configure<DbContextOptions>(options =>
        {
            // Connection pool settings will be configured in startup
        });
    }

    /// <summary>
    /// Configure query optimization settings
    /// </summary>
    public static void ConfigureQueryOptimizations(DbContextOptionsBuilder options)
    {
        // Enable sensitive data logging only in development
        #if DEBUG
        options.EnableSensitiveDataLogging();
        #endif

        // Enable detailed errors in development
        #if DEBUG
        options.EnableDetailedErrors();
        #endif

        // Configure query tracking behavior
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);

        // Configure command timeout - this is typically set in connection string or DbContext configuration
    }

    /// <summary>
    /// Get optimized query hints for common scenarios
    /// </summary>
    public static class QueryHints
    {
        public const string EmployeeSearch = "OPTION (OPTIMIZE FOR (@Department = 'IT', @Status = 1))";
        public const string AttendanceReport = "OPTION (OPTIMIZE FOR (@StartDate = '2024-01-01', @EndDate = '2024-12-31'))";
        public const string PayrollProcessing = "OPTION (OPTIMIZE FOR (@PayrollMonth = 1, @PayrollYear = 2024))";
    }

    /// <summary>
    /// Common query patterns for performance optimization
    /// </summary>
    public static class OptimizedQueries
    {
        public static IQueryable<Employee> GetActiveEmployeesByBranch(DbContext context, int branchId)
        {
            return context.Set<Employee>()
                .Where(e => e.BranchId == branchId && e.Status == Core.Enums.EmployeeStatus.Active)
                .AsNoTracking(); // Use AsNoTracking for read-only queries
        }

        public static IQueryable<AttendanceRecord> GetAttendanceByDateRange(DbContext context, int employeeId, DateTime startDate, DateTime endDate)
        {
            return context.Set<AttendanceRecord>()
                .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .AsNoTracking();
        }

        public static IQueryable<PayrollRecord> GetPayrollByPeriod(DbContext context, int year, int month)
        {
            return context.Set<PayrollRecord>()
                .Where(p => p.PayrollYear == year && p.PayrollMonth == month)
                .Include(p => p.Employee)
                .AsNoTracking();
        }
    }
}