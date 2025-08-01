using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Date)
            .IsRequired();
            
        builder.Property(a => a.Location)
            .HasMaxLength(200);
            
        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        // Composite index for employee and date
        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        // Relationships
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.BreakRecords)
            .WithOne(b => b.AttendanceRecord)
            .HasForeignKey(b => b.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BreakRecordConfiguration : IEntityTypeConfiguration<BreakRecord>
{
    public void Configure(EntityTypeBuilder<BreakRecord> builder)
    {
        builder.HasKey(b => b.Id);

        // Relationships
        builder.HasOne(b => b.AttendanceRecord)
            .WithMany(a => a.BreakRecords)
            .HasForeignKey(b => b.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}