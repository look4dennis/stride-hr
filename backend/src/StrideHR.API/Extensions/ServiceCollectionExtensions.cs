using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Authorization;
using StrideHR.Core.Models.Configuration;
using StrideHR.Infrastructure.Authorization;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;
using StrideHR.Infrastructure.Services;
using System.Text;

namespace StrideHR.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JWT settings not found in configuration");
        }

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        // Configure encryption settings
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));

        var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
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
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Basic role-based policies
            options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
            options.AddPolicy("HRManager", policy => policy.RequireRole("SuperAdmin", "HRManager"));
            options.AddPolicy("Manager", policy => policy.RequireRole("SuperAdmin", "HRManager", "Manager"));
            options.AddPolicy("Employee", policy => policy.RequireRole("SuperAdmin", "HRManager", "Manager", "Employee"));

            // Permission-based policies using custom authorization handlers
            options.AddPolicy("CanManageEmployees", policy => 
                policy.Requirements.Add(new PermissionRequirement("Employee.Manage")));
            
            options.AddPolicy("CanViewReports", policy => 
                policy.Requirements.Add(new PermissionRequirement("Report.View")));
            
            options.AddPolicy("CanManagePayroll", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Manage")));

            // Branch-based policies using custom authorization handlers
            options.AddPolicy("BranchAccess", policy => 
                policy.Requirements.Add(new BranchAccessRequirement()));

            options.AddPolicy("SameBranch", policy => 
                policy.Requirements.Add(new BranchAccessRequirement(true)));

            // Role hierarchy policies
            options.AddPolicy("ManagerLevel", policy => 
                policy.Requirements.Add(new RoleHierarchyRequirement(3)));

            options.AddPolicy("HRLevel", policy => 
                policy.Requirements.Add(new RoleHierarchyRequirement(4)));

            options.AddPolicy("AdminLevel", policy => 
                policy.Requirements.Add(new RoleHierarchyRequirement(5)));

            // Dynamic permission policies
            options.AddPolicy("Permission:Employee.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Employee.View")));
            options.AddPolicy("Permission:Employee.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Employee.Create")));
            options.AddPolicy("Permission:Employee.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("Employee.Update")));
            options.AddPolicy("Permission:Employee.Delete", policy => 
                policy.Requirements.Add(new PermissionRequirement("Employee.Delete")));

            options.AddPolicy("Permission:Role.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Role.View")));
            options.AddPolicy("Permission:Role.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Role.Create")));
            options.AddPolicy("Permission:Role.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("Role.Update")));
            options.AddPolicy("Permission:Role.Delete", policy => 
                policy.Requirements.Add(new PermissionRequirement("Role.Delete")));
            options.AddPolicy("Permission:Role.Assign", policy => 
                policy.Requirements.Add(new PermissionRequirement("Role.Assign")));

            options.AddPolicy("Permission:Payroll.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.View")));
            options.AddPolicy("Permission:Payroll.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Create")));
            options.AddPolicy("Permission:Payroll.Process", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Process")));
            options.AddPolicy("Permission:Payroll.Calculate", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Calculate")));
            options.AddPolicy("Permission:Payroll.Approve", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Approve")));
            
            // Payslip policies
            options.AddPolicy("Permission:Payroll.Templates.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Templates.View")));
            options.AddPolicy("Permission:Payroll.Templates.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Templates.Create")));
            options.AddPolicy("Permission:Payroll.Templates.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Templates.Update")));
            options.AddPolicy("Permission:Payroll.Templates.Delete", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Templates.Delete")));
            options.AddPolicy("Permission:Payroll.Templates.Manage", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Templates.Manage")));
            
            options.AddPolicy("Permission:Payroll.Payslips.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.View")));
            options.AddPolicy("Permission:Payroll.Payslips.Generate", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.Generate")));
            options.AddPolicy("Permission:Payroll.Payslips.Approve", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.Approve")));
            options.AddPolicy("Permission:Payroll.Payslips.Release", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.Release")));
            options.AddPolicy("Permission:Payroll.Payslips.Download", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.Download")));
            options.AddPolicy("Permission:Payroll.Payslips.Regenerate", policy => 
                policy.Requirements.Add(new PermissionRequirement("Payroll.Payslips.Regenerate")));

            options.AddPolicy("Permission:Report.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Report.View")));
            options.AddPolicy("Permission:Report.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Report.Create")));

            // User management policies
            options.AddPolicy("Permission:User.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.View")));
            options.AddPolicy("Permission:User.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.Create")));
            options.AddPolicy("Permission:User.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.Update")));
            options.AddPolicy("Permission:User.Deactivate", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.Deactivate")));
            options.AddPolicy("Permission:User.Activate", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.Activate")));
            options.AddPolicy("Permission:User.ForcePasswordChange", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.ForcePasswordChange")));
            options.AddPolicy("Permission:User.Unlock", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.Unlock")));
            options.AddPolicy("Permission:User.ViewSessions", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.ViewSessions")));
            options.AddPolicy("Permission:User.TerminateSession", policy => 
                policy.Requirements.Add(new PermissionRequirement("User.TerminateSession")));

            // Audit log policies
            options.AddPolicy("Permission:AuditLog.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("AuditLog.View")));
            options.AddPolicy("Permission:AuditLog.ViewSecurity", policy => 
                policy.Requirements.Add(new PermissionRequirement("AuditLog.ViewSecurity")));
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<StrideHRDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

        // Add JWT Authentication
        services.AddJwtAuthentication(configuration);
        
        // Add Authorization Policies
        services.AddAuthorizationPolicies();

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        
        // Register project management repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        services.AddScoped<IProjectAssignmentRepository, ProjectAssignmentRepository>();
        services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
        services.AddScoped<IDSRRepository, DSRRepository>();
        services.AddScoped<IProjectAlertRepository, ProjectAlertRepository>();
        
        // Register payroll repositories
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IPayrollFormulaRepository, PayrollFormulaRepository>();
        services.AddScoped<IPayslipTemplateRepository, PayslipTemplateRepository>();
        services.AddScoped<IPayslipGenerationRepository, PayslipGenerationRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        
        // Register leave management repositories
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeavePolicyRepository, LeavePolicyRepository>();
        services.AddScoped<ILeaveApprovalHistoryRepository, LeaveApprovalHistoryRepository>();
        services.AddScoped<ILeaveCalendarRepository, LeaveCalendarRepository>();

        // Register services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDataEncryptionService, DataEncryptionService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        
        // Register project management services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDSRService, DSRService>();
        services.AddScoped<IProjectMonitoringService, ProjectMonitoringService>();
        
        // Register payroll services
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IPayrollFormulaEngine, PayrollFormulaEngine>();
        services.AddScoped<IPayslipTemplateService, PayslipTemplateService>();
        services.AddScoped<IPayslipGenerationService, PayslipGenerationService>();
        services.AddScoped<IPayslipDesignerService, PayslipDesignerService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ITimeZoneService, TimeZoneService>();
        
        // Register leave management services
        services.AddScoped<ILeaveManagementService, LeaveManagementService>();

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, BranchAccessAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleHierarchyAuthorizationHandler>();

        // Add HttpContextAccessor for authorization handlers
        services.AddHttpContextAccessor();

        // Add AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Add FluentValidation
        services.AddFluentValidationAutoValidation();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StrideHR API",
                Version = "v1",
                Description = "A comprehensive Human Resource Management System API",
                Contact = new OpenApiContact
                {
                    Name = "StrideHR Team",
                    Email = "support@stridehr.com"
                }
            });

            // Add JWT Authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}