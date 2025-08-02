using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class PIPGoalConfiguration : IEntityTypeConfiguration<PIPGoal>
{
    public void Configure(EntityTypeBuilder<PIPGoal> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(g => g.MeasurableObjective)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(g => g.ProgressPercentage)
            .HasPrecision(5, 2);
            
        builder.Property(g => g.EmployeeComments)
            .HasMaxLength(1000);
            
        builder.Property(g => g.ManagerComments)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(g => g.PIP)
            .WithMany(p => p.Goals)
            .HasForeignKey(g => g.PIPId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}