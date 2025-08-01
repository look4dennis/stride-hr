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

        builder.Property(a => a.CheckInLocation)
            .HasMaxLength(200);

        builder.Property(a => a.CheckOutLocation)
            .HasMaxLength(200);

        builder.Property(a => a.CheckInTimeZone)
            .HasMaxLength(100);

        builder.Property(a => a.CheckOutTimeZone)
            .HasMaxLength(100);

        builder.Property(a => a.DeviceInfo)
            .HasMaxLength(500);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);
            
        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CorrectionReason)
            .HasMaxLength(500);

        // Precision for coordinates
        builder.Property(a => a.CheckInLatitude)
            .HasPrecision(10, 8);

        builder.Property(a => a.CheckInLongitude)
            .HasPrecision(11, 8);

        builder.Property(a => a.CheckOutLatitude)
            .HasPrecision(10, 8);

        builder.Property(a => a.CheckOutLongitude)
            .HasPrecision(11, 8);

        // Composite index for employee and date
        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        // Relationships
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Shift)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.CorrectedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.CorrectedBy)
            .OnDelete(DeleteBehavior.SetNull);

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

        builder.Property(b => b.Location)
            .HasMaxLength(200);

        builder.Property(b => b.TimeZone)
            .HasMaxLength(100);

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        // Precision for coordinates
        builder.Property(b => b.Latitude)
            .HasPrecision(10, 8);

        builder.Property(b => b.Longitude)
            .HasPrecision(11, 8);

        // Relationships
        builder.HasOne(b => b.AttendanceRecord)
            .WithMany(a => a.BreakRecords)
            .HasForeignKey(b => b.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}