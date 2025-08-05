using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StrideHR.Infrastructure.Data;

public class StrideHRDbContextFactory : IDesignTimeDbContextFactory<StrideHRDbContext>
{
    public StrideHRDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../StrideHR.API"))
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<StrideHRDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33)), options =>
        {
            options.EnableRetryOnFailure();
            options.CommandTimeout(60);
        });

        return new StrideHRDbContext(optionsBuilder.Options);
    }
}