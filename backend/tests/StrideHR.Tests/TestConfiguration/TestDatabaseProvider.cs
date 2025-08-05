using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace StrideHR.Tests.TestConfiguration;

public enum DatabaseProviderType
{
    InMemory,
    SqlServer,
    MySql
}

public class TestDatabaseProvider
{
    private readonly DatabaseProviderType _providerType;
    private readonly string _connectionString;
    private readonly ILogger<TestDatabaseProvider>? _logger;

    public TestDatabaseProvider(DatabaseProviderType providerType, string? connectionString = null, ILogger<TestDatabaseProvider>? logger = null)
    {
        _providerType = providerType;
        _connectionString = connectionString ?? GenerateConnectionString(providerType);
        _logger = logger;
    }

    public void ConfigureDbContext(IServiceCollection services, string? databaseName = null)
    {
        // Remove existing DbContext registration
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        switch (_providerType)
        {
            case DatabaseProviderType.InMemory:
                ConfigureInMemoryProvider(services, databaseName);
                break;
            case DatabaseProviderType.SqlServer:
                ConfigureSqlServerProvider(services);
                break;
            case DatabaseProviderType.MySql:
                ConfigureMySqlProvider(services);
                break;
            default:
                throw new ArgumentException($"Unsupported database provider type: {_providerType}");
        }
    }

    public async Task<StrideHRDbContext> CreateContextAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<StrideHRDbContext>();
        
        try
        {
            if (_providerType == DatabaseProviderType.InMemory)
            {
                // For in-memory database, ensure it's created
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                // For SQL databases, apply migrations
                await context.Database.MigrateAsync();
            }

            _logger?.LogInformation("Database context created successfully using {ProviderType}", _providerType);
            return context;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create database context using {ProviderType}", _providerType);
            throw;
        }
    }

    public async Task CleanupAsync(StrideHRDbContext context)
    {
        try
        {
            if (_providerType == DatabaseProviderType.InMemory)
            {
                // For in-memory database, just clear the data
                await context.Database.EnsureDeletedAsync();
            }
            else
            {
                // For SQL databases, truncate tables or delete test data
                await TruncateTablesAsync(context);
            }

            _logger?.LogInformation("Database cleanup completed for {ProviderType}", _providerType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to cleanup database for {ProviderType}", _providerType);
            throw;
        }
    }

    private void ConfigureInMemoryProvider(IServiceCollection services, string? databaseName)
    {
        var dbName = databaseName ?? $"StrideHR_Test_{Guid.NewGuid()}";
        
        services.AddDbContext<StrideHRDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(message => _logger?.LogDebug(message));
        });

        _logger?.LogInformation("Configured In-Memory database provider with name: {DatabaseName}", dbName);
    }

    private void ConfigureSqlServerProvider(IServiceCollection services)
    {
        // For now, we'll use in-memory for SQL Server tests too since we don't have SQL Server provider configured
        // In a real implementation, you would add Microsoft.EntityFrameworkCore.SqlServer package
        ConfigureInMemoryProvider(services, "SqlServer_Test_" + Guid.NewGuid().ToString());
        _logger?.LogInformation("Configured SQL Server database provider (using in-memory for testing)");
    }

    private void ConfigureMySqlProvider(IServiceCollection services)
    {
        services.AddDbContext<StrideHRDbContext>(options =>
        {
            options.UseMySql(_connectionString, new MySqlServerVersion(new Version(8, 0, 33)), mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                mysqlOptions.CommandTimeout(60);
            });
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(message => _logger?.LogDebug(message));
        });

        _logger?.LogInformation("Configured MySQL database provider");
    }

    private static string GenerateConnectionString(DatabaseProviderType providerType)
    {
        return providerType switch
        {
            DatabaseProviderType.InMemory => "DataSource=:memory:",
            DatabaseProviderType.SqlServer => "Server=(localdb)\\mssqllocaldb;Database=StrideHR_Test;Trusted_Connection=true;MultipleActiveResultSets=true",
            DatabaseProviderType.MySql => "Server=localhost;Database=StrideHR_Test;Uid=root;Pwd=password;",
            _ => throw new ArgumentException($"Unsupported database provider type: {providerType}")
        };
    }

    private async Task TruncateTablesAsync(StrideHRDbContext context)
    {
        // Get all table names (this is a simplified approach)
        var tableNames = new[]
        {
            "UserRoles", "Users", "Employees", "Branches", "Organizations", "Roles",
            "Permissions", "RolePermissions", "AuditLogs", "Notifications"
        };

        // Disable foreign key constraints temporarily
        if (_providerType == DatabaseProviderType.SqlServer)
        {
            await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
        }
        else if (_providerType == DatabaseProviderType.MySql)
        {
            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0");
        }

        // Truncate tables
        foreach (var tableName in tableNames)
        {
            try
            {
                if (_providerType == DatabaseProviderType.SqlServer)
                {
                    await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");
                    await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{tableName}', RESEED, 0)");
                }
                else if (_providerType == DatabaseProviderType.MySql)
                {
                    await context.Database.ExecuteSqlRawAsync($"DELETE FROM `{tableName}`");
                    await context.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` AUTO_INCREMENT = 1");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to truncate table {TableName}", tableName);
            }
        }

        // Re-enable foreign key constraints
        if (_providerType == DatabaseProviderType.SqlServer)
        {
            await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
        }
        else if (_providerType == DatabaseProviderType.MySql)
        {
            await context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1");
        }
    }

    public bool SupportsTransactions => _providerType != DatabaseProviderType.InMemory;

    public bool SupportsMigrations => _providerType != DatabaseProviderType.InMemory;

    public DatabaseProviderType ProviderType => _providerType;
}