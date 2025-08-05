using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;
using System;
using System.Threading.Tasks;
using Xunit;

namespace StrideHR.Tests.TestConfiguration;

public abstract class TestBase : IDisposable
{
    protected readonly StrideHRDbContext DbContext;
    protected readonly Mock<ILogger> MockLogger;
    protected readonly ServiceProvider ServiceProvider;
    protected readonly TestDatabaseProvider DatabaseProvider;

    protected TestBase(DatabaseProviderType providerType = DatabaseProviderType.InMemory)
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Create database provider
        var logger = services.BuildServiceProvider().GetService<ILogger<TestDatabaseProvider>>();
        DatabaseProvider = new TestDatabaseProvider(providerType, logger: logger);
        
        // Configure database
        DatabaseProvider.ConfigureDbContext(services);

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        MockLogger = new Mock<ILogger>();

        // Initialize database
        InitializeDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            await DatabaseProvider.CreateContextAsync(ServiceProvider);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize test database", ex);
        }
    }

    protected async Task SeedTestDataAsync()
    {
        var seeder = new TestDataSeeder(DbContext);
        await seeder.SeedAllAsync();
    }

    protected async Task CleanupTestDataAsync()
    {
        try
        {
            var seeder = new TestDataSeeder(DbContext);
            await seeder.ClearAllAsync();
            await DatabaseProvider.CleanupAsync(DbContext);
        }
        catch (Exception ex)
        {
            // Log but don't throw to avoid masking test failures
            Console.WriteLine($"Warning: Failed to cleanup test data: {ex.Message}");
        }
    }

    public virtual void Dispose()
    {
        try
        {
            CleanupTestDataAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore cleanup errors during disposal
        }
        
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class IntegrationTestBase : TestBase
{
    protected IntegrationTestBase(DatabaseProviderType providerType = DatabaseProviderType.InMemory) 
        : base(providerType)
    {
        // Additional setup for integration tests
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        if (!DatabaseProvider.SupportsTransactions)
        {
            // For in-memory database, just execute the action
            return await action();
        }

        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    protected async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        if (!DatabaseProvider.SupportsTransactions)
        {
            // For in-memory database, just execute the action
            await action();
            return;
        }

        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}