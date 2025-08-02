using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;
using System.Text.Json;

namespace StrideHR.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.TargetRole)
            .HasMaxLength(100);

        builder.Property(n => n.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null));

        builder.Property(n => n.ExpiresAt)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.IsGlobal)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Branch)
            .WithMany()
            .HasForeignKey(n => n.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.BranchId);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.ExpiresAt);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(nt => nt.Id);

        builder.Property(nt => nt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(nt => nt.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(nt => nt.TitleTemplate)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(nt => nt.MessageTemplate)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(nt => nt.DefaultChannel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(nt => nt.DefaultPriority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(nt => nt.IsActive)
            .HasDefaultValue(true);

        builder.Property(nt => nt.DefaultMetadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null));

        // Indexes
        builder.HasIndex(nt => nt.Name)
            .IsUnique();
        builder.HasIndex(nt => nt.Type);
        builder.HasIndex(nt => nt.IsActive);
    }
}

public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("UserNotificationPreferences");

        builder.HasKey(unp => unp.Id);

        builder.Property(unp => unp.NotificationType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(unp => unp.Channel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(unp => unp.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(unp => unp.WeekendNotifications)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(unp => unp.User)
            .WithMany()
            .HasForeignKey(unp => unp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(unp => unp.UserId);
        builder.HasIndex(unp => new { unp.UserId, unp.NotificationType, unp.Channel })
            .IsUnique();
    }
}