using Serilog;
using StrideHR.API.Extensions;
using StrideHR.API.Middleware;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting StrideHR API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();
    
    // Add database services
    builder.Services.AddScoped<DatabaseInitializationService>();
    builder.Services.AddScoped<DatabaseHealthCheckService>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        });
    });

    var app = builder.Build();

    // Initialize database on startup
    using (var scope = app.Services.CreateScope())
    {
        var dbInitService = scope.ServiceProvider.GetRequiredService<DatabaseInitializationService>();
        var initResult = await dbInitService.InitializeDatabaseAsync();
        if (!initResult)
        {
            Log.Fatal("Failed to initialize database. Application will not start.");
            return;
        }
    }

    // Configure the HTTP request pipeline
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Enable Swagger in all environments with proper security
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/v1/swagger.json", "StrideHR API V1");
        c.RoutePrefix = "api-docs"; // Set Swagger UI at /api-docs
        c.DocumentTitle = "StrideHR API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide models section by default
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        
        // Custom CSS for better appearance
        c.InjectStylesheet("/swagger-ui/custom.css");
        
        // Add custom JavaScript for enhanced functionality
        c.InjectJavascript("/swagger-ui/custom.js");
        
        // OAuth configuration for production
        if (!app.Environment.IsDevelopment())
        {
            c.OAuthClientId("stridehr-swagger-ui");
            c.OAuthAppName("StrideHR API Documentation");
            c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        }
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // Correct middleware pipeline ordering for authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    // Map SignalR hubs
    app.MapHub<StrideHR.API.Hubs.NotificationHub>("/hubs/notification");

    // Legacy health check endpoint for backward compatibility
    app.MapGet("/health", async (StrideHR.API.Services.HealthCheckService healthService, DatabaseHealthCheckService dbHealthService) =>
    {
        var health = await healthService.GetSystemHealthAsync();
        var dbHealth = await dbHealthService.CheckHealthAsync();
        
        var overallHealthy = health.Status == StrideHR.API.Services.HealthStatus.Healthy && dbHealth.IsHealthy;
        
        return overallHealthy 
            ? Results.Ok(new { 
                Status = "Healthy", 
                Timestamp = DateTime.UtcNow,
                Database = new { Status = "Healthy", CheckedAt = dbHealth.CheckedAt },
                System = health
            })
            : Results.Problem("System is unhealthy", statusCode: 503);
    })
    .WithName("HealthCheck")
    .WithOpenApi();

    Log.Information("StrideHR API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "StrideHR API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
