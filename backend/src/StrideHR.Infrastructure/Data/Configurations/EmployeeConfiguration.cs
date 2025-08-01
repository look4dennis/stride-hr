using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(e => e.EmployeeId)
            .IsUnique();
            
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.Email)
            .IsUnique();
            
        builder.Property(e => e.Phone)
            .HasMaxLength(20);
            
        builder.Property(e => e.ProfilePhoto)
            .HasMaxLength(500);
            
        builder.Property(e => e.Designation)
            .HasMaxLength(100);
            
        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.Property(e => e.BasicSalary)
            .HasPrecision(18, 2);

        // Relationships
        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Employees)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReportingManager)
            .WithMany(e => e.Subordinates)
            .HasForeignKey(e => e.ReportingManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AttendanceRecords)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EmployeeRoles)
            .WithOne(er => er.Employee)
            .HasForeignKey(er => er.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}