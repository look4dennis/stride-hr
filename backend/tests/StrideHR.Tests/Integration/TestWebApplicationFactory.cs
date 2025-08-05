using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StrideHR.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Hosting;
using StrideHR.API;
using Microsoft.AspNetCore.Hosting;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Tests.Integration
{
    public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StrideHRDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add test database
                services.AddDbContext<StrideHRDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Configure test authentication
                services.AddAuthentication(defaultScheme: "Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "Test", options => { });

                // Replace the policy evaluator
                services.AddSingleton<IPolicyEvaluator, TestPolicyEvaluator>();
            });
        }
    }
}
