using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data.Configurations;

namespace StrideHR.Infrastructure.Data
{
    public class StrideHRDbContext : DbContext
    {
        public StrideHRDbContext(DbContextOptions<StrideHRDbContext> options) : base(options)
        {
        }

        // Employee Management
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeOnboarding> EmployeeOnboardings { get; set; }
        public DbSet<EmployeeOnboardingTask> EmployeeOnboardingTasks { get; set; }
        public DbSet<EmployeeExit> EmployeeExits { get; set; }
        public DbSet<EmployeeExitTask> EmployeeExitTasks { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        // Attendance & DSR
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<AttendanceAlert> AttendanceAlerts { get; set; }
        public DbSet<BreakRecord> BreakRecords { get; set; }
        public DbSet<DSR> DSRs { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
        public DbSet<ShiftSwapRequest> ShiftSwapRequests { get; set; }
        public DbSet<ShiftSwapResponse> ShiftSwapResponses { get; set; }
        public DbSet<ShiftCoverageRequest> ShiftCoverageRequests { get; set; }
        public DbSet<ShiftCoverageResponse> ShiftCoverageResponses { get; set; }

        // Leave Management
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveAccrual> LeaveAccruals { get; set; }
        public DbSet<LeaveAccrualRule> LeaveAccrualRules { get; set; }
        public DbSet<LeaveApprovalHistory> LeaveApprovalHistories { get; set; }
        public DbSet<LeaveCalendar> LeaveCalendars { get; set; }
        public DbSet<LeaveEncashment> LeaveEncashments { get; set; }

        // Payroll Management
        public DbSet<PayrollRecord> PayrollRecords { get; set; }
        public DbSet<PayrollAdjustment> PayrollAdjustments { get; set; }
        public DbSet<PayrollFormula> PayrollFormulas { get; set; }
        public DbSet<PayslipGeneration> PayslipGenerations { get; set; }
        public DbSet<PayslipTemplate> PayslipTemplates { get; set; }
        public DbSet<PayslipApprovalHistory> PayslipApprovalHistories { get; set; }
        public DbSet<PayrollAuditTrail> PayrollAuditTrails { get; set; }
        public DbSet<PayrollErrorCorrection> PayrollErrorCorrections { get; set; }

        // Expense Management
        public DbSet<ExpenseClaim> ExpenseClaims { get; set; }
        public DbSet<ExpenseItem> ExpenseItems { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ExpenseDocument> ExpenseDocuments { get; set; }
        public DbSet<ExpensePolicyRule> ExpensePolicyRules { get; set; }
        public DbSet<ExpenseApprovalHistory> ExpenseApprovalHistories { get; set; }
        public DbSet<ExpenseComplianceViolation> ExpenseComplianceViolations { get; set; }
        public DbSet<ExpenseBudget> ExpenseBudgets { get; set; }
        public DbSet<TravelExpense> TravelExpenses { get; set; }
        public DbSet<TravelExpenseItem> TravelExpenseItems { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        // Performance Management
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<PerformanceGoal> PerformanceGoals { get; set; }
        public DbSet<PerformanceFeedback> PerformanceFeedbacks { get; set; }
        public DbSet<PerformanceImprovementPlan> PerformanceImprovementPlans { get; set; }
        public DbSet<PIPGoal> PIPGoals { get; set; }
        public DbSet<PIPReview> PIPReviews { get; set; }
        public DbSet<PerformanceGoalCheckIn> PerformanceGoalCheckIns { get; set; }

        // Project Management
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
        public DbSet<ProjectComment> ProjectComments { get; set; }
        public DbSet<ProjectCommentReply> ProjectCommentReplies { get; set; }
        public DbSet<ProjectActivity> ProjectActivities { get; set; }
        public DbSet<ProjectRisk> ProjectRisks { get; set; }
        public DbSet<ProjectAlert> ProjectAlerts { get; set; }

        // Training Management
        public DbSet<TrainingModule> TrainingModules { get; set; }
        public DbSet<TrainingAssignment> TrainingAssignments { get; set; }
        public DbSet<TrainingProgress> TrainingProgresses { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
        public DbSet<Certification> Certifications { get; set; }

        // Asset Management
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetAssignment> AssetAssignments { get; set; }
        public DbSet<AssetHandover> AssetHandovers { get; set; }
        public DbSet<AssetMaintenance> AssetMaintenances { get; set; }

        // Communication
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

        // Document Management
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }
        public DbSet<DocumentApproval> DocumentApprovals { get; set; }
        public DbSet<DocumentSignature> DocumentSignatures { get; set; }
        public DbSet<DocumentAuditLog> DocumentAuditLogs { get; set; }
        public DbSet<DocumentRetentionPolicy> DocumentRetentionPolicies { get; set; }
        public DbSet<DocumentRetentionExecution> DocumentRetentionExecutions { get; set; }

        // Reports
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }
        public DbSet<ReportExecution> ReportExecutions { get; set; }
        public DbSet<ReportSchedule> ReportSchedules { get; set; }
        public DbSet<ReportShare> ReportShares { get; set; }

        // Surveys
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyQuestionOption> SurveyQuestionOptions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public DbSet<SurveyDistribution> SurveyDistributions { get; set; }
        public DbSet<SurveyAnalytics> SurveyAnalytics { get; set; }

        // Support & Grievances
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketComment> SupportTicketComments { get; set; }
        public DbSet<SupportTicketStatusHistory> SupportTicketStatusHistories { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<GrievanceComment> GrievanceComments { get; set; }
        public DbSet<GrievanceFollowUp> GrievanceFollowUps { get; set; }
        public DbSet<GrievanceStatusHistory> GrievanceStatusHistories { get; set; }
        public DbSet<GrievanceEscalation> GrievanceEscalations { get; set; }

        // Budget & Financial
        public DbSet<Budget> Budgets { get; set; }

        // Chatbot
        public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
        public DbSet<ChatbotMessage> ChatbotMessages { get; set; }
        public DbSet<ChatbotKnowledgeBase> ChatbotKnowledgeBases { get; set; }
        public DbSet<ChatbotKnowledgeBaseFeedback> ChatbotKnowledgeBaseFeedbacks { get; set; }
        public DbSet<ChatbotLearningData> ChatbotLearningDatas { get; set; }
        public DbSet<ChatbotLearningData> ChatbotLearningData { get; set; }

        // Knowledge Base
        public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
        public DbSet<KnowledgeBaseDocument> KnowledgeBaseDocuments { get; set; }
        public DbSet<KnowledgeBaseDocumentAttachment> KnowledgeBaseDocumentAttachments { get; set; }
        public DbSet<KnowledgeBaseDocumentComment> KnowledgeBaseDocumentComments { get; set; }
        public DbSet<KnowledgeBaseDocumentApproval> KnowledgeBaseDocumentApprovals { get; set; }
        public DbSet<KnowledgeBaseDocumentView> KnowledgeBaseDocumentViews { get; set; }

        // Calendar & Integration
        public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        // External Integrations
        public DbSet<ExternalIntegration> ExternalIntegrations { get; set; }
        public DbSet<IntegrationLog> IntegrationLogs { get; set; }

        // Webhooks
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }

        // Task Management
        public DbSet<TaskAssignment> TaskAssignments { get; set; }



        // Audit
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrideHRDbContext).Assembly);

            // Global query filters for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);
                    
                    entityType.SetQueryFilter(lambda);
                }
            }

            // Configure decimal precision for financial data
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditable && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var auditable = (IAuditable)entry.Entity;
                
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = DateTime.UtcNow;
                }
                
                auditable.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
