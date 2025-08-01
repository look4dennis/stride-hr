using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using StrideHR.API.Authorization;
using StrideHR.Core.Models;

namespace StrideHR.API.Extensions;

/// <summary>
/// Extension methods for configuring authentication
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwtSettings == null)
            throw new InvalidOperationException("JWT settings not found in configuration");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = jwtSettings.SaveToken;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = jwtSettings.RequireExpirationTime
            };

            // Handle SignalR connections
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                    logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                    var userId = context.Principal?.FindFirst("UserId")?.Value;
                    logger.LogDebug("JWT token validated for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Configure authorization policies
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, BranchAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SameBranchAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AllPermissionsAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Basic policies
            options.AddPolicy("RequireAuthentication", policy =>
                policy.RequireAuthenticatedUser());

            // Role-based policies
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy("RequireHRRole", policy =>
                policy.RequireRole("HR", "Admin", "SuperAdmin"));

            options.AddPolicy("RequireManagerRole", policy =>
                policy.RequireRole("Manager", "HR", "Admin", "SuperAdmin"));

            // Permission-based policies
            options.AddPolicy("CanManageEmployees", policy =>
                policy.RequireClaim("permission", "Employee.Create", "Employee.Update", "Employee.Delete"));

            options.AddPolicy("CanViewReports", policy =>
                policy.RequireClaim("permission", "Reports.View"));

            options.AddPolicy("CanManagePayroll", policy =>
                policy.RequireClaim("permission", "Payroll.Create", "Payroll.Update", "Payroll.Process"));

            options.AddPolicy("CanManageAttendance", policy =>
                policy.RequireClaim("permission", "Attendance.Manage"));

            // Branch-based policies (custom requirement)
            options.AddPolicy("SameBranchOnly", policy =>
                policy.Requirements.Add(new SameBranchRequirement()));

            options.AddPolicy("CanAccessBranch", policy =>
                policy.Requirements.Add(new BranchAccessRequirement()));

            // Dynamic policy provider for permission-based policies
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add policy provider for dynamic policies
        services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();

        return services;
    }
}

/// <summary>
/// Custom authorization requirement for same branch access
/// </summary>
public class SameBranchRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Custom authorization requirement for branch access
/// </summary>
public class BranchAccessRequirement : IAuthorizationRequirement
{
    public int? BranchId { get; set; }

    public BranchAccessRequirement(int? branchId = null)
    {
        BranchId = branchId;
    }
}