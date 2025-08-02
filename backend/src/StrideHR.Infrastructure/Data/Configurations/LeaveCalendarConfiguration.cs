using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class LeaveCalendarConfiguration : IEntityTypeConfiguration<LeaveCalendar>
{
    public void Configure(EntityTypeBuilder<LeaveCalendar> builder)
    {
        builder.HasKey(lc => lc.Id);
        
        builder.Property(lc => lc.Date)
            .IsRequired()
            .HasColumnType("date");
            
        builder.Property(lc => lc.IsFullDay)
            .IsRequired();
            
        builder.Property(lc => lc.StartTime)
            .HasColumnType("time");
            
        builder.Property(lc => lc.EndTime)
            .HasColumnType("time");
            
        builder.Property(lc => lc.LeaveType)
            .IsRequired()
            .HasConversion<int>();
            
        // Relationships
        builder.HasOne(lc => lc.Employee)
            .WithMany(e => e.LeaveCalendarEntries)
            .HasForeignKey(lc => lc.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(lc => lc.LeaveRequest)
            .WithMany()
            .HasForeignKey(lc => lc.LeaveRequestId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(lc => lc.Date);
        builder.HasIndex(lc => lc.EmployeeId);
        builder.HasIndex(lc => new { lc.Date, lc.EmployeeId });
    }
}