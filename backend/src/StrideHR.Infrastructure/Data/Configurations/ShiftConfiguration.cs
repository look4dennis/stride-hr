using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.TimeZone)
            .HasMaxLength(100);

        builder.Property(s => s.WorkingDays)
            .HasMaxLength(50);

        builder.Property(s => s.OvertimeMultiplier)
            .HasPrecision(5, 2);

        // Relationships
        builder.HasOne(s => s.Organization)
            .WithMany(o => o.Shifts)
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.ShiftAssignments)
            .WithOne(sa => sa.Shift)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.AttendanceRecords)
            .WithOne(ar => ar.Shift)
            .HasForeignKey(ar => ar.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AssignedBy)
            .HasMaxLength(100);

        builder.Property(sa => sa.Notes)
            .HasMaxLength(500);

        // Composite index to prevent overlapping assignments
        builder.HasIndex(sa => new { sa.EmployeeId, sa.ShiftId, sa.StartDate })
            .IsUnique();

        // Relationships
        builder.HasOne(sa => sa.Employee)
            .WithMany()
            .HasForeignKey(sa => sa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Shift)
            .WithMany(s => s.ShiftAssignments)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.HasKey(wh => wh.Id);
        
        builder.Property(wh => wh.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(wh => wh.Description)
            .HasMaxLength(500);

        builder.Property(wh => wh.TimeZone)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(wh => wh.Branch)
            .WithMany()
            .HasForeignKey(wh => wh.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(h => h.Description)
            .HasMaxLength(500);

        // Index for efficient date queries
        builder.HasIndex(h => new { h.BranchId, h.Date });

        // Relationships
        builder.HasOne(h => h.Branch)
            .WithMany()
            .HasForeignKey(h => h.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AttendancePolicyConfiguration : IEntityTypeConfiguration<AttendancePolicy>
{
    public void Configure(EntityTypeBuilder<AttendancePolicy> builder)
    {
        builder.HasKey(ap => ap.Id);
        
        builder.Property(ap => ap.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(ap => ap.Description)
            .HasMaxLength(500);

        builder.Property(ap => ap.LocationRadius)
            .HasPrecision(10, 2);

        builder.Property(ap => ap.OvertimeRate)
            .HasPrecision(5, 2);

        // Relationships
        builder.HasOne(ap => ap.Branch)
            .WithMany()
            .HasForeignKey(ap => ap.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}