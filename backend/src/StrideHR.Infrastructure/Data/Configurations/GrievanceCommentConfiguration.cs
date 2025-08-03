using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class GrievanceCommentConfiguration : IEntityTypeConfiguration<GrievanceComment>
{
    public void Configure(EntityTypeBuilder<GrievanceComment> builder)
    {
        builder.ToTable("GrievanceComments");

        builder.HasKey(gc => gc.Id);

        builder.Property(gc => gc.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(gc => gc.AttachmentPath)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(gc => gc.Grievance)
            .WithMany(g => g.Comments)
            .HasForeignKey(gc => gc.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gc => gc.Author)
            .WithMany()
            .HasForeignKey(gc => gc.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}