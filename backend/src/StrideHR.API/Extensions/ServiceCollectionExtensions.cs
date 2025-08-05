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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            // Handle SignalR connections and add comprehensive error handling
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
                    var remoteIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = context.Request.Headers.UserAgent.ToString();
                    
                    // Set appropriate response headers based on error type
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                        context.Response.Headers["WWW-Authenticate"] = "Bearer error=\"invalid_token\", error_description=\"The access token expired\"";
                        
                        logger.LogWarning("Security Event: JWT token expired for request to {Path} from {RemoteIpAddress} using {UserAgent}", 
                            context.Request.Path, remoteIp, userAgent);
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenInvalidSignatureException))
                    {
                        context.Response.Headers["WWW-Authenticate"] = "Bearer error=\"invalid_token\", error_description=\"The access token signature is invalid\"";
                        
                        logger.LogWarning("Security Event: JWT token has invalid signature for request to {Path} from {RemoteIpAddress} using {UserAgent}", 
                            context.Request.Path, remoteIp, userAgent);
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenValidationException))
                    {
                        context.Response.Headers["WWW-Authenticate"] = "Bearer error=\"invalid_token\", error_description=\"The access token is invalid\"";
                        
                        logger.LogWarning("Security Event: JWT token validation failed for request to {Path} from {RemoteIpAddress} using {UserAgent}: {Exception}", 
                            context.Request.Path, remoteIp, userAgent, context.Exception.Message);
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenMalformedException))
                    {
                        context.Response.Headers["WWW-Authenticate"] = "Bearer error=\"invalid_token\", error_description=\"The access token is malformed\"";
                        
                        logger.LogWarning("Security Event: Malformed JWT token for request to {Path} from {RemoteIpAddress} using {UserAgent}", 
                            context.Request.Path, remoteIp, userAgent);
                    }
                    else
                    {
                        context.Response.Headers["WWW-Authenticate"] = "Bearer error=\"invalid_token\", error_description=\"Authentication failed\"";
                        
                        logger.LogError(context.Exception, "Security Event: JWT authentication failed for request to {Path} from {RemoteIpAddress} using {UserAgent}", 
                            context.Request.Path, remoteIp, userAgent);
                    }
                    
                    // Let the default authentication handling proceed
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                    var remoteIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = context.Request.Headers.UserAgent.ToString();
                    
                    var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var employeeIdClaim = context.Principal?.FindFirst("EmployeeId")?.Value;
                    var organizationIdClaim = context.Principal?.FindFirst("OrganizationId")?.Value;
                    var branchIdClaim = context.Principal?.FindFirst("BranchId")?.Value;
                    
                    // Validate required claims are present
                    if (string.IsNullOrEmpty(employeeIdClaim))
                    {
                        logger.LogWarning("Security Event: JWT token missing required EmployeeId claim for User {UserId} from {RemoteIpAddress}", 
                            userIdClaim, remoteIp);
                        context.Fail("Missing required EmployeeId claim");
                        return Task.CompletedTask;
                    }
                    
                    logger.LogInformation("Security Event: JWT token validated successfully for User {UserId}, Employee {EmployeeId}, Organization {OrganizationId}, Branch {BranchId} from {RemoteIpAddress} using {UserAgent}", 
                        userIdClaim, employeeIdClaim, organizationIdClaim, branchIdClaim, remoteIp, userAgent);
                    
                    // Add user info to context items for easy access
                    if (context.Principal != null)
                    {
                        context.HttpContext.Items["User"] = context.Principal;
                        if (userIdClaim != null) context.HttpContext.Items["UserId"] = userIdClaim;
                        if (employeeIdClaim != null) context.HttpContext.Items["EmployeeId"] = employeeIdClaim;
                        if (branchIdClaim != null) context.HttpContext.Items["BranchId"] = branchIdClaim;
                        if (organizationIdClaim != null) context.HttpContext.Items["OrganizationId"] = organizationIdClaim;
                        
                        // Add roles and permissions for easy access
                        var roles = context.Principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                        var permissions = context.Principal.FindAll("permission").Select(c => c.Value).ToList();
                        context.HttpContext.Items["UserRoles"] = roles;
                        context.HttpContext.Items["UserPermissions"] = permissions;
                    }
                    
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                    var remoteIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = context.Request.Headers.UserAgent.ToString();
                    
                    logger.LogWarning("Security Event: JWT authentication challenge for request to {Path} from {RemoteIpAddress} using {UserAgent}: {Error} - {ErrorDescription}", 
                        context.Request.Path, remoteIp, userAgent, context.Error, context.ErrorDescription);
                    
                    // Customize challenge response
                    if (string.IsNullOrEmpty(context.Error))
                    {
                        context.Error = "invalid_token";
                        context.ErrorDescription = "Authentication required";
                    }
                    
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                    var remoteIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var userAgent = context.Request.Headers.UserAgent.ToString();
                    var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var employeeIdClaim = context.Principal?.FindFirst("EmployeeId")?.Value;
                    
                    logger.LogWarning("Security Event: Access forbidden for User {UserId}, Employee {EmployeeId} to {Path} from {RemoteIpAddress} using {UserAgent}", 
                        userIdClaim, employeeIdClaim, context.Request.Path, remoteIp, userAgent);
                    
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
                new MySqlServerVersion(new Version(8, 0, 33)),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                    mysqlOptions.CommandTimeout(60);
                }));

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
        services.AddScoped<IPayrollAuditTrailRepository, PayrollAuditTrailRepository>();
        services.AddScoped<IPayrollErrorCorrectionRepository, PayrollErrorCorrectionRepository>();
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
        services.AddScoped<ISecurityEventService, SecurityEventService>();
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
        services.AddScoped<IPayrollReportingService, PayrollReportingService>();
        services.AddScoped<IPayrollErrorCorrectionService, PayrollErrorCorrectionService>();
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
        
        // Add HTTP context accessor for authorization handlers
        services.AddHttpContextAccessor();

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

## Getting Started

### 1. Authentication
All API endpoints require JWT Bearer token authentication. Obtain a token by calling the `/api/auth/login` endpoint with valid credentials.

### 2. Authorization Header
Include the JWT token in the Authorization header for all requests:
```
Authorization: Bearer your-jwt-token-here
```

### 3. Response Format
All API responses follow a consistent format:
```json
{
  ""success"": true,
  ""data"": { /* response data */ },
  ""message"": ""Operation completed successfully""
}
```

### 4. Error Handling
Error responses include detailed information:
```json
{
  ""success"": false,
  ""message"": ""Error description"",
  ""errors"": [""Detailed error message""]
}
```

## Rate Limiting

API requests are rate-limited to ensure system stability:
- Standard endpoints: 1000 requests per hour per user
- Bulk operations: 100 requests per hour per user
- Authentication endpoints: 10 requests per minute per IP

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1638360000
```

## Pagination

List endpoints support pagination with the following parameters:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `sortBy`: Field to sort by
- `sortOrder`: 'asc' or 'desc' (default: 'asc')

## Filtering and Search

Many endpoints support filtering and search:
- Use query parameters for basic filtering
- Use the `search` parameter for text-based searches
- Date ranges can be specified with `startDate` and `endDate`

## Webhooks

StrideHR supports webhooks for real-time event notifications. Available events include:
- Employee lifecycle events (created, updated, deleted)
- Attendance events (check-in, check-out)
- Leave request events (submitted, approved, rejected)
- Payroll events (processed, approved)
- Project events (created, completed)

## SDKs and Libraries

Official SDKs are available for:
- JavaScript/Node.js: `npm install @stridehr/api-client`
- Python: `pip install stridehr-api`
- C#: `dotnet add package StrideHR.ApiClient`


## Changelog

### v1.0.0 (Current)
- Initial API release with full HR management capabilities
- Comprehensive authentication and authorization system
- Real-time notifications via SignalR
- Webhook support for external integrations
- Multi-branch and multi-currency support
",
                Contact = new OpenApiContact
                {
                    Name = "StrideHR Development Team",
                    Email = "support@stridehr.com",
                    Url = new Uri("https://stridehr.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://stridehr.com/terms")
            });

            // Add JWT Authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            // Include XML comments for better documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add custom schema filters
            c.SchemaFilter<SwaggerSchemaExampleFilter>();
            c.DocumentFilter<SwaggerDocumentFilter>();
            c.OperationFilter<SwaggerDefaultValues>();

            // Group endpoints by tags
            c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            c.DocInclusionPredicate((name, api) => true);

            // Add examples for common response types
            c.MapType<DateTime>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Example = new Microsoft.OpenApi.Any.OpenApiString("2024-12-01T10:00:00Z")
            });

            c.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Example = new Microsoft.OpenApi.Any.OpenApiString("08:30:00")
            });

            // Add servers for different environments
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