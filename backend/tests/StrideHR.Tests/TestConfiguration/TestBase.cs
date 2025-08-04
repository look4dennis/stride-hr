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

    protected TestBase()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database
        services.AddDbContext<StrideHRDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<StrideHRDbContext>();
        
        MockLogger = new Mock<ILogger>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    protected async Task SeedTestDataAsync()
    {
        // Add common test data here
        await DbContext.SaveChangesAsync();
    }

    protected async Task CleanupTestDataAsync()
    {
        // Clean up test data
        DbContext.ChangeTracker.Clear();
        await DbContext.Database.EnsureDeletedAsync();
    }

    public virtual void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class IntegrationTestBase : TestBase
{
    protected IntegrationTestBase() : base()
    {
        // Additional setup for integration tests
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
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