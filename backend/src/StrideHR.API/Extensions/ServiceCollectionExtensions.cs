using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StrideHR.API.Filters;
using StrideHR.Core.Entities;
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

            // Asset management policies
            options.AddPolicy("Permission:Asset.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("Asset.View")));
            options.AddPolicy("Permission:Asset.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("Asset.Create")));
            options.AddPolicy("Permission:Asset.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("Asset.Update")));
            options.AddPolicy("Permission:Asset.Delete", policy => 
                policy.Requirements.Add(new PermissionRequirement("Asset.Delete")));
            options.AddPolicy("Permission:Asset.Read", policy => 
                policy.Requirements.Add(new PermissionRequirement("Asset.Read")));
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

            // Document template policies
            options.AddPolicy("Permission:DocumentTemplate.View", policy => 
                policy.Requirements.Add(new PermissionRequirement("DocumentTemplate.View")));
            options.AddPolicy("Permission:DocumentTemplate.Create", policy => 
                policy.Requirements.Add(new PermissionRequirement("DocumentTemplate.Create")));
            options.AddPolicy("Permission:DocumentTemplate.Update", policy => 
                policy.Requirements.Add(new PermissionRequirement("DocumentTemplate.Update")));
            options.AddPolicy("Permission:DocumentTemplate.Delete", policy => 
                policy.Requirements.Add(new PermissionRequirement("DocumentTemplate.Delete")));
            options.AddPolicy("Permission:DocumentTemplate.Read", policy => 
                policy.Requirements.Add(new PermissionRequirement("DocumentTemplate.Read")));

            // Training management policies
            options.AddPolicy("CanManageTraining", policy => 
                policy.Requirements.Add(new PermissionRequirement("Training.Manage")));
            options.AddPolicy("CanAssignTraining", policy => 
                policy.Requirements.Add(new PermissionRequirement("Training.Assign")));
            options.AddPolicy("CanViewTrainingReports", policy => 
                policy.Requirements.Add(new PermissionRequirement("Training.ViewReports")));
            options.AddPolicy("CanIssueCertifications", policy => 
                policy.Requirements.Add(new PermissionRequirement("Training.IssueCertifications")));
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
        services.AddScoped<IPermissionRepository, PermissionRepository>();
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
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        
        // Register leave management repositories
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeavePolicyRepository, LeavePolicyRepository>();
        services.AddScoped<ILeaveApprovalHistoryRepository, LeaveApprovalHistoryRepository>();
        services.AddScoped<ILeaveCalendarRepository, LeaveCalendarRepository>();
        services.AddScoped<ILeaveAccrualRepository, LeaveAccrualRepository>();
        services.AddScoped<ILeaveEncashmentRepository, LeaveEncashmentRepository>();
        services.AddScoped<ILeaveAccrualRuleRepository, LeaveAccrualRuleRepository>();
        
        // Register email management repositories
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();
        services.AddScoped<IEmailCampaignRepository, EmailCampaignRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();

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
        services.AddScoped<IFinancialReportingService, FinancialReportingService>();
        
        // Register leave management services
        services.AddScoped<ILeaveManagementService, LeaveManagementService>();
        
        // Register performance management repositories
        services.AddScoped<IPerformanceGoalRepository, PerformanceGoalRepository>();
        services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<IPerformanceFeedbackRepository, PerformanceFeedbackRepository>();
        services.AddScoped<IPerformanceImprovementPlanRepository, PerformanceImprovementPlanRepository>();
        
        // Register performance management services
        services.AddScoped<IPerformanceManagementService, PerformanceManagementService>();
        services.AddScoped<IPIPManagementService, PIPManagementService>();
        
        // Register training repositories
        services.AddScoped<ITrainingModuleRepository, TrainingModuleRepository>();
        services.AddScoped<ITrainingAssignmentRepository, TrainingAssignmentRepository>();
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<IAssessmentAttemptRepository, AssessmentAttemptRepository>();
        services.AddScoped<ICertificationRepository, CertificationRepository>();
        
        // Register training services
        services.AddScoped<ITrainingService, TrainingService>();
        
        // Register asset management repositories
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();
        services.AddScoped<IAssetMaintenanceRepository, AssetMaintenanceRepository>();
        services.AddScoped<IAssetHandoverRepository, AssetHandoverRepository>();
        
        // Register asset management services
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();
        services.AddScoped<IAssetMaintenanceService, AssetMaintenanceService>();
        services.AddScoped<IAssetHandoverService, AssetHandoverService>();
        
        // Register support ticket repositories
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ISupportTicketCommentRepository, SupportTicketCommentRepository>();
        
        // Register support ticket services
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        
        // Register notification repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();
        
        // Register notification services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRealTimeNotificationService, StrideHR.API.Services.SignalRNotificationService>();
        
        // Register email management services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        // Register chatbot repositories
        services.AddScoped<IChatbotConversationRepository, ChatbotConversationRepository>();
        services.AddScoped<IChatbotMessageRepository, ChatbotMessageRepository>();
        services.AddScoped<IChatbotKnowledgeBaseRepository, ChatbotKnowledgeBaseRepository>();
        services.AddScoped<IChatbotLearningDataRepository, ChatbotLearningDataRepository>();
        
        // Register chatbot services
        services.AddScoped<IChatbotService, ChatbotService>();
        services.AddScoped<INaturalLanguageProcessingService, NaturalLanguageProcessingService>();
        services.AddScoped<IChatbotKnowledgeBaseService, ChatbotKnowledgeBaseService>();
        services.AddScoped<IChatbotLearningService, ChatbotLearningService>();
        
        // Register AI Analytics service
        services.AddScoped<IAIAnalyticsService, AIAnalyticsService>();
        
        // Register survey management repositories
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
        services.AddScoped<ISurveyResponseRepository, SurveyResponseRepository>();
        services.AddScoped<ISurveyDistributionRepository, SurveyDistributionRepository>();
        services.AddScoped<ISurveyAnalyticsRepository, SurveyAnalyticsRepository>();
        
        // Register survey management services
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<ISurveyResponseService, SurveyResponseService>();
        services.AddScoped<ISurveyAnalyticsService, SurveyAnalyticsService>();
        
        // Register grievance management repositories
        services.AddScoped<IGrievanceRepository, GrievanceRepository>();
        services.AddScoped<IGrievanceCommentRepository, GrievanceCommentRepository>();
        services.AddScoped<IGrievanceFollowUpRepository, GrievanceFollowUpRepository>();
        
        // Register grievance management services
        services.AddScoped<IGrievanceService, GrievanceService>();
        
        // Register document template repositories
        services.AddScoped<IDocumentTemplateRepository, DocumentTemplateRepository>();
        services.AddScoped<IGeneratedDocumentRepository, GeneratedDocumentRepository>();
        services.AddScoped<IDocumentRetentionPolicyRepository, DocumentRetentionPolicyRepository>();
        
        // Register document template services
        services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();
        
        // Register data import/export services
        services.AddScoped<IDataImportExportService, DataImportExportService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<ICsvService, CsvService>();
        
        // Register integration repositories
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();
        services.AddScoped<ICalendarIntegrationRepository, CalendarIntegrationRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
        services.AddScoped<IExternalIntegrationRepository, ExternalIntegrationRepository>();
        services.AddScoped<IIntegrationLogRepository, IntegrationLogRepository>();
        
        // Register integration services
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<ICalendarIntegrationService, CalendarIntegrationService>();
        services.AddScoped<IExternalIntegrationService, ExternalIntegrationService>();
        
        // Add SignalR
        services.AddSignalR();
        
        // Add background services
        services.AddHostedService<StrideHR.API.Services.NotificationBackgroundService>();
        
        // Register employee repository (generic)
        services.AddScoped<IRepository<Employee>, Repository<Employee>>();

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
        
        // Add HttpClient for integrations
        services.AddHttpClient();

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
                Version = "v1.0",
                Description = @"
# StrideHR API Documentation

A comprehensive Human Resource Management System API designed for global organizations with multi-branch support.

## Features

### Core HR Management
- **Employee Management**: Complete lifecycle from onboarding to exit
- **Attendance Tracking**: Real-time location-based check-in/out with break management
- **Leave Management**: Multi-level approval workflows with balance tracking
- **Performance Management**: Including PIP (Performance Improvement Plans)

### Advanced Features
- **Payroll System**: Advanced payroll with custom formulas and multi-currency support
- **Project Management**: Kanban boards with integrated time tracking
- **Asset Management**: Complete asset lifecycle tracking
- **Training & Certification**: Module-based training with assessments

### Integration & APIs
- **Webhook Support**: Real-time event notifications to external systems
- **Calendar Integration**: Google Calendar and Outlook integration
- **External Systems**: Payroll and accounting system integrations
- **Data Import/Export**: Bulk operations with Excel/CSV support

### Security & Compliance
- **Role-based Access Control**: Granular permissions system
- **Multi-branch Data Isolation**: Organization-level data separation
- **Audit Trails**: Comprehensive logging and monitoring
- **International Compliance**: Multi-currency, timezone, and regulatory support

## Authentication

All API endpoints require JWT Bearer token authentication unless otherwise specified.

## Rate Limiting

API requests are rate-limited to ensure system stability. Please refer to response headers for current limits.

## Support

For API support, contact: support@stridehr.com
",
                Contact = new OpenApiContact
                {
                    Name = "StrideHR Development Team",
                    Email = "support@stridehr.com",
                    Url = new Uri("https://stridehr.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "StrideHR License",
                    Url = new Uri("https://stridehr.com/license")
                },
                TermsOfService = new Uri("https://stridehr.com/terms")
            });

            // Add multiple API versions if needed
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "StrideHR API", Version = "v1.0" });

            // Add JWT Authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Group endpoints by tags
            c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            c.DocInclusionPredicate((name, api) => true);

            // Add custom operation filters
            c.OperationFilter<SwaggerDefaultValues>();
            c.DocumentFilter<SwaggerDocumentFilter>();

            // Include XML comments for better documentation
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                if (Path.GetFileNameWithoutExtension(xmlFile).StartsWith("StrideHR"))
                {
                    c.IncludeXmlComments(xmlFile);
                }
            }

            // Add examples for common request/response models
            c.SchemaFilter<SwaggerSchemaExampleFilter>();

            // Note: EnableAnnotations() is not available in this version of Swashbuckle
            // Annotations can be enabled through other means if needed

            // Add servers information
            c.AddServer(new OpenApiServer
            {
                Url = "https://api.stridehr.com",
                Description = "Production Server"
            });
            c.AddServer(new OpenApiServer
            {
                Url = "https://staging-api.stridehr.com",
                Description = "Staging Server"
            });
            c.AddServer(new OpenApiServer
            {
                Url = "http://localhost:5000",
                Description = "Development Server"
            });
        });

        return services;
    }
}