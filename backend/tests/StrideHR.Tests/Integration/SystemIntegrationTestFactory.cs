using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrideHR.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using StrideHR.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using StrideHR.Tests.TestConfiguration;

namespace StrideHR.Tests.Integration
{
    // Test authentication handler for bypassing JWT authentication in tests
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("employeeId", "1"),
                new Claim("organizationId", "1"),
                new Claim("branchId", "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    // Test policy evaluator that always allows access
    public class TestPolicyEvaluator : IPolicyEvaluator
    {
        public virtual async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var testScheme = "Test";
            var principal = new ClaimsPrincipal();
            
            principal.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("employeeId", "1"),
                new Claim("organizationId", "1"),
                new Claim("branchId", "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, testScheme));

            return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, testScheme)));
        }

        public virtual async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
            AuthenticateResult authenticationResult, HttpContext context, object resource)
        {
            return await Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }

    public class SystemIntegrationTestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing configuration
                config.Sources.Clear();
                
                // Use test configuration
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                    ["JwtSettings:SecretKey"] = "test-super-secret-jwt-key-for-integration-testing-that-is-long-enough",
                    ["JwtSettings:Issuer"] = "StrideHR-Test",
                    ["JwtSettings:Audience"] = "StrideHR-Test-Users",
                    ["JwtSettings:ExpirationHours"] = "24",
                    ["JwtSettings:ValidateIssuer"] = "false",
                    ["JwtSettings:ValidateAudience"] = "false",
                    ["JwtSettings:ValidateLifetime"] = "false",
                    ["JwtSettings:ValidateIssuerSigningKey"] = "false",
                    ["JwtSettings:ClockSkewMinutes"] = "5",
                    ["EncryptionSettings:MasterKey"] = "test-encryption-key-for-integration-testing-that-is-long-enough",
                    ["EncryptionSettings:Salt"] = "test-salt-value",
                    ["EncryptionSettings:EnableEncryption"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove existing authentication services
                var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPolicyEvaluator));
                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                // Add test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
                
                // Add test policy evaluator
                services.AddSingleton<IPolicyEvaluator, TestPolicyEvaluator>();

                // Configure test database provider
                var databaseName = $"StrideHR_Test_{Guid.NewGuid()}";
                var tempServiceProvider = services.BuildServiceProvider();
                var logger = tempServiceProvider.GetService<ILogger<TestDatabaseProvider>>();
                var databaseProvider = new TestDatabaseProvider(DatabaseProviderType.InMemory, logger: logger);
                
                databaseProvider.ConfigureDbContext(services, databaseName);

                // Build service provider to initialize database
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StrideHRDbContext>();
                var factoryLogger = scope.ServiceProvider.GetRequiredService<ILogger<SystemIntegrationTestFactory>>();
                
                try
                {
                    // Initialize database using provider (synchronous version for ConfigureServices)
                    context.Database.EnsureCreated();
                    
                    // Seed test data
                    SeedTestData(context);
                    
                    factoryLogger.LogInformation("Test database initialized successfully with {OrganizationCount} organizations", 
                        context.Organizations.Count());
                }
                catch (Exception ex)
                {
                    factoryLogger.LogError(ex, "An error occurred while setting up the test database");
                    throw; // Re-throw to fail the test setup if database initialization fails
                }
                
                tempServiceProvider.Dispose();
            });

            builder.UseEnvironment("Testing");
        }

        private static void SeedTestData(StrideHRDbContext context)
        {
            try
            {
                var seeder = new TestDataSeeder(context);
                seeder.SeedAllAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to seed test data", ex);
            }
        }
    }
}