using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(r => r.Name)
            .IsUnique();
            
        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.EmployeeRoles)
            .WithOne(er => er.Role)
            .HasForeignKey(er => er.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(p => p.Resource)
            .IsRequired()
            .HasMaxLength(50);

        // Composite unique index
        builder.HasIndex(p => new { p.Module, p.Action, p.Resource })
            .IsUnique();

        // Relationships
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => rp.Id);

        // Composite unique index
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeRoleConfiguration : IEntityTypeConfiguration<EmployeeRole>
{
    public void Configure(EntityTypeBuilder<EmployeeRole> builder)
    {
        builder.ToTable("EmployeeRoles");
        
        builder.HasKey(er => er.Id);
        
        // Configure properties
        builder.Property(er => er.AssignedDate)
            .IsRequired();
            
        builder.Property(er => er.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(er => er.Notes)
            .HasMaxLength(1000);

        // Configure relationships
        
        // Employee relationship (the employee who has the role)
        builder.HasOne(er => er.Employee)
            .WithMany(e => e.EmployeeRoles)
            .HasForeignKey(er => er.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Role relationship
        builder.HasOne(er => er.Role)
            .WithMany(r => r.EmployeeRoles)
            .HasForeignKey(er => er.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // AssignedBy relationship (the employee who assigned the role)
        builder.HasOne(er => er.AssignedByEmployee)
            .WithMany()
            .HasForeignKey(er => er.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // RevokedBy relationship (the employee who revoked the role)
        builder.HasOne(er => er.RevokedByEmployee)
            .WithMany()
            .HasForeignKey(er => er.RevokedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(er => er.EmployeeId);
        builder.HasIndex(er => er.RoleId);
        builder.HasIndex(er => er.AssignedBy);
        builder.HasIndex(er => er.RevokedBy);
        builder.HasIndex(er => new { er.EmployeeId, er.RoleId, er.IsActive });
    }
}