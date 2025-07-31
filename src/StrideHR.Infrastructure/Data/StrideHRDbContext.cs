using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;

namespace StrideHR.Infrastructure.Data;

/// <summary>
/// Main database context for StrideHR application
/// </summary>
public class StrideHRDbContext : DbContext
{
    public StrideHRDbContext(DbContextOptions<StrideHRDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        ApplyEntityConfigurations(modelBuilder);
        
        // Apply global query filters for soft delete
        ApplyGlobalQueryFilters(modelBuilder);
        
        // Seed initial data
        SeedInitialData(modelBuilder);
    }
    
    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.LogoPath).HasMaxLength(500);
            entity.Property(e => e.NormalWorkingHours).HasPrecision(4, 2);
            entity.Property(e => e.OvertimeRate).HasPrecision(4, 2);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Email);
        });
        
        // Branch configuration
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmployeeIdPattern).HasMaxLength(50);
            
            // Relationships
            entity.HasOne(e => e.Organization)
                  .WithMany(o => o.Branches)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.OrganizationId, e.Name });
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.IsActive);
        });
    }
    
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
    
    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed default organization
        modelBuilder.Entity<Organization>().HasData(
            new Organization
            {
                Id = 1,
                Name = "StrideHR Demo Organization",
                Address = "123 Business Street, Tech City, TC 12345",
                Email = "admin@stridehr.com",
                Phone = "+1-555-0123",
                NormalWorkingHours = 8.0m,
                OvertimeRate = 1.5m,
                ProductiveHoursThreshold = 6,
                BranchIsolationEnabled = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
        
        // Seed default branch
        modelBuilder.Entity<Branch>().HasData(
            new Branch
            {
                Id = 1,
                OrganizationId = 1,
                Name = "Head Office",
                Country = "United States",
                Currency = "USD",
                TimeZone = "America/New_York",
                Address = "123 Business Street, Tech City, TC 12345",
                City = "Tech City",
                State = "TC",
                PostalCode = "12345",
                Phone = "+1-555-0123",
                Email = "headoffice@stridehr.com",
                EmployeeIdPattern = "HO-{YYYY}-{###}",
                WorkingHoursStart = new TimeSpan(9, 0, 0),
                WorkingHoursEnd = new TimeSpan(17, 0, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields before saving
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                    
                case EntityState.Deleted:
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}