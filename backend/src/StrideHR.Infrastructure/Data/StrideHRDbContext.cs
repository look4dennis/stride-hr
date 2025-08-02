using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using System.Linq.Expressions;

namespace StrideHR.Infrastructure.Data;

public class StrideHRDbContext : DbContext
{
    public StrideHRDbContext(DbContextOptions<StrideHRDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<BreakRecord> BreakRecords { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<AttendancePolicy> AttendancePolicies { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Project Management DbSets
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<DSR> DSRs { get; set; }
    public DbSet<ProjectAlert> ProjectAlerts { get; set; }
    
    // Payroll Management DbSets
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<PayrollFormula> PayrollFormulas { get; set; }
    public DbSet<PayrollAdjustment> PayrollAdjustments { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    // Payslip Management DbSets
    public DbSet<PayslipTemplate> PayslipTemplates { get; set; }
    public DbSet<PayslipGeneration> PayslipGenerations { get; set; }
    public DbSet<PayslipApprovalHistory> PayslipApprovalHistories { get; set; }
    
    // Leave Management DbSets
    public DbSet<LeavePolicy> LeavePolicies { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveApprovalHistory> LeaveApprovalHistories { get; set; }
    public DbSet<LeaveCalendar> LeaveCalendars { get; set; }
    public DbSet<LeaveAccrual> LeaveAccruals { get; set; }
    public DbSet<LeaveEncashment> LeaveEncashments { get; set; }
    public DbSet<LeaveAccrualRule> LeaveAccrualRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrideHRDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}