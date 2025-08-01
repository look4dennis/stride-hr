using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(u => u.Username)
            .IsUnique();
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(u => u.PasswordSalt)
            .HasMaxLength(500);

        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(100);

        builder.Property(u => u.SecurityQuestion)
            .HasMaxLength(500);

        builder.Property(u => u.SecurityAnswerHash)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(u => u.Employee)
            .WithOne()
            .HasForeignKey<User>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserSessions)
            .WithOne(us => us.User)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(rt => rt.CreatedByIp)
            .IsRequired()
            .HasMaxLength(50);

        // Ignore computed properties
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);

        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(us => us.Id);
        
        builder.Property(us => us.SessionId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(us => us.SessionId)
            .IsUnique();

        builder.Property(us => us.IpAddress)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(us => us.UserAgent)
            .HasMaxLength(500);

        builder.Property(us => us.DeviceInfo)
            .HasMaxLength(200);

        builder.Property(us => us.Location)
            .HasMaxLength(200);

        // Relationships
        builder.HasOne(us => us.User)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}