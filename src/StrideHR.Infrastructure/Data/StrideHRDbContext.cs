using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using System.Linq.Expressions;

namespace StrideHR.Infrastructure.Data;

/// <summary>
/// Main database context for StrideHR application
/// </summary>
public class StrideHRDbContext : DbContext
{
    public StrideHRDbContext(DbContextOptions<StrideHRDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
    
    // Authentication and Authorization entities
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Attendance entities
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<BreakRecord> BreakRecords { get; set; }
    public DbSet<AttendanceCorrection> AttendanceCorrections { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    
    // Leave management entities
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    
    // Payroll entities
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    
    // Project management entities
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    
    // DSR entities
    public DbSet<DSR> DSRs { get; set; }
    
    // Branch isolation entities
    public DbSet<UserBranchAccess> UserBranchAccess { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        ApplyEntityConfigurations(modelBuilder);
        
        // Apply global query filters for soft delete
        ApplyGlobalQueryFilters(modelBuilder);
        
        // Seed initial data
        SeedInitialData(modelBuilder);
    }
    
    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.LogoPath).HasMaxLength(500);
            entity.Property(e => e.NormalWorkingHours).HasPrecision(4, 2);
            entity.Property(e => e.OvertimeRate).HasPrecision(4, 2);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Email);
        });
        
        // Branch configuration
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeIdPattern).HasMaxLength(50);
            
            // Relationships
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.Branches)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.IsActive);
        });
        
        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.AlternatePhone).HasMaxLength(20);
            entity.Property(e => e.ProfilePhotoPath).HasMaxLength(500);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.VisaStatus).HasMaxLength(100);
            entity.Property(e => e.TerminationReason).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.Branch)
                  .WithMany(b => b.Employees)
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.ReportingManager)
                  .WithMany(e => e.Subordinates)
                  .HasForeignKey(e => e.ReportingManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.BranchId, e.Status });
            entity.HasIndex(e => e.Department);
            entity.HasIndex(e => e.ReportingManagerId);
        });
        
        // Shift configuration
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.WorkingHours).HasPrecision(4, 2);
            entity.Property(e => e.OvertimeRate).HasPrecision(4, 2);
            entity.Property(e => e.ShiftAllowance).HasPrecision(18, 2);
            entity.Property(e => e.ColorCode).HasMaxLength(7);
            
            // Relationships
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.Shifts)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
        });
        
        // ShiftAssignment configuration
        modelBuilder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.ShiftAssignments)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Shift)
                  .WithMany(s => s.ShiftAssignments)
                  .HasForeignKey(e => e.ShiftId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.StartDate });
            entity.HasIndex(e => e.IsActive);
        });
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastLoginIp).HasMaxLength(45);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(500);
            entity.Property(e => e.EmailVerificationToken).HasMaxLength(500);
            entity.Property(e => e.TwoFactorSecretKey).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithOne()
                  .HasForeignKey<User>(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastLoginAt);
        });
        
        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ColorCode).HasMaxLength(7);
            entity.Property(e => e.Icon).HasMaxLength(50);
            
            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.HierarchyLevel);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsSystemRole);
        });
        
        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => new { e.Module, e.Action, e.Resource }).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.IsActive);
        });
        
        // RolePermission configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });
        
        // EmployeeRole configuration
        modelBuilder.Entity<EmployeeRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.EmployeeRoles)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.EmployeeRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.RoleId, e.IsActive });
        });
        
        // AttendanceRecord configuration
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CheckInLocation).HasMaxLength(200);
            entity.Property(e => e.CheckOutLocation).HasMaxLength(200);
            entity.Property(e => e.CheckInIpAddress).HasMaxLength(45);
            entity.Property(e => e.CheckOutIpAddress).HasMaxLength(45);
            entity.Property(e => e.CheckInDevice).HasMaxLength(500);
            entity.Property(e => e.CheckOutDevice).HasMaxLength(500);
            entity.Property(e => e.ManualEntryReason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.AttendanceRecords)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Shift)
                  .WithMany()
                  .HasForeignKey(e => e.ShiftId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.ManualEntryByEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.ManualEntryBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.Date }).IsUnique();
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ShiftId);
            entity.HasIndex(e => e.IsManualEntry);
        });
        
        // BreakRecord configuration
        modelBuilder.Entity<BreakRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Reason).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.AttendanceRecord)
                  .WithMany(a => a.BreakRecords)
                  .HasForeignKey(e => e.AttendanceRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.ApprovedByEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.AttendanceRecordId, e.StartTime });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ApprovalStatus);
            entity.HasIndex(e => e.IsExceeding);
        });
        
        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ApprovalComments).HasMaxLength(500);
            entity.Property(e => e.Days).HasPrecision(4, 2);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.LeaveRequests)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Approver)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.StartDate });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);
        });
        
        // PayrollRecord configuration
        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.Allowances).HasPrecision(18, 2);
            entity.Property(e => e.OvertimeAmount).HasPrecision(18, 2);
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
            entity.Property(e => e.Deductions).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.PayrollRecords)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Approver)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.Period }).IsUnique();
            entity.HasIndex(e => e.Status);
        });
        
        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            
            // Relationships
            entity.HasOne(e => e.CreatedByEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });
        
        // ProjectAssignment configuration
        modelBuilder.Entity<ProjectAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(100);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.ProjectAssignments)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.ProjectAssignments)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.ProjectId, e.Status });
        });
        
        // ProjectTask configuration
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            // Relationships
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.ProjectId, e.Status });
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.DueDate);
        });
        
        // TaskAssignment configuration
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relationships
            entity.HasOne(e => e.Task)
                  .WithMany(t => t.TaskAssignments)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Employee)
                  .WithMany()
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.TaskId, e.EmployeeId }).IsUnique();
        });
        
        // DSR configuration
        modelBuilder.Entity<DSR>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReviewComments).HasMaxLength(500);
            entity.Property(e => e.HoursWorked).HasPrecision(4, 2);
            
            // Relationships
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.DSRs)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Project)
                  .WithMany()
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Task)
                  .WithMany()
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Reviewer)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.EmployeeId, e.Date });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ProjectId);
        });
        
        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RevocationReason).HasMaxLength(200);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            
            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });
            entity.HasIndex(e => e.ExpiryDate);
        });
        
        // UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.TerminationReason).HasMaxLength(200);
            
            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserSessions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.SessionToken).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.LastActivity);
        });
        
        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.RequestId).HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            
            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => new { e.EntityName, e.EntityId });
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.RequestId);
        });
        
        // AttendanceCorrection configuration
        modelBuilder.Entity<AttendanceCorrection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalValue).HasMaxLength(200);
            entity.Property(e => e.CorrectedValue).HasMaxLength(200);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ApprovalComments).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.AttendanceRecord)
                  .WithMany(a => a.AttendanceCorrections)
                  .HasForeignKey(e => e.AttendanceRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.RequestedByEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.RequestedBy)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.ApprovedByEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => new { e.AttendanceRecordId, e.Type });
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedBy);
        });
        
        // WorkingHours configuration
        modelBuilder.Entity<WorkingHours>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WeeklyHours).HasPrecision(5, 2);
            entity.Property(e => e.OvertimeThreshold).HasPrecision(4, 2);
            entity.Property(e => e.FlexibleMinHours).HasPrecision(4, 2);
            entity.Property(e => e.FlexibleMaxHours).HasPrecision(4, 2);
            
            // Relationships
            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Employee)
                  .WithMany()
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.BranchId, e.IsActive });
            entity.HasIndex(e => new { e.EmployeeId, e.IsActive });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => new { e.EffectiveFrom, e.EffectiveTo });
        });
        
        // Holiday configuration
        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RecurrencePattern).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.BranchId, e.Date });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsRecurring);
        });
        
        // UserBranchAccess configuration
        modelBuilder.Entity<UserBranchAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.GrantedBy).IsRequired().HasMaxLength(450);
            entity.Property(e => e.RevokedBy).HasMaxLength(450);
            
            // Relationships
            entity.HasOne(e => e.Branch)
                  .WithMany()
                  .HasForeignKey(e => e.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.UserId, e.BranchId, e.IsActive });
            entity.HasIndex(e => new { e.UserId, e.IsPrimary });
            entity.HasIndex(e => e.BranchId);
            entity.HasIndex(e => e.GrantedAt);
        });
    }
    
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
    
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed default organization
        modelBuilder.Entity<Organization>().HasData(
            new Organization
            {
                Id = 1,
                Name = "StrideHR Demo Organization",
                Address = "123 Business Street, Tech City, TC 12345",
                Email = "admin@stridehr.com",
                Phone = "+1-555-0123",
                NormalWorkingHours = 8.0m,
                OvertimeRate = 1.5m,
                ProductiveHoursThreshold = 6,
                BranchIsolationEnabled = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
        
        // Seed default branch
        modelBuilder.Entity<Branch>().HasData(
            new Branch
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Head Office",
                Country = "United States",
                Currency = "USD",
                TimeZone = "America/New_York",
                Address = "123 Business Street, Tech City, TC 12345",
                City = "Tech City",
                State = "TC",
                PostalCode = "12345",
                Phone = "+1-555-0123",
                Email = "headoffice@stridehr.com",
                EmployeeIdPattern = "HO-{YYYY}-{###}",
                WorkingHoursStart = new TimeSpan(9, 0, 0),
                WorkingHoursEnd = new TimeSpan(17, 0, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields before saving
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
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
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}