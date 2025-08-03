using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using System.Linq.Expressions;

namespace StrideHR.Infrastructure.Data;

public class StrideHRDbContext : DbContext
{
    public StrideHRDbContext(DbContextOptions<StrideHRDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<BreakRecord> BreakRecords { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
    public DbSet<ShiftSwapRequest> ShiftSwapRequests { get; set; }
    public DbSet<ShiftSwapResponse> ShiftSwapResponses { get; set; }
    public DbSet<ShiftCoverageRequest> ShiftCoverageRequests { get; set; }
    public DbSet<ShiftCoverageResponse> ShiftCoverageResponses { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<AttendancePolicy> AttendancePolicies { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Project Management DbSets
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<DSR> DSRs { get; set; }
    public DbSet<ProjectAlert> ProjectAlerts { get; set; }
    
    // Payroll Management DbSets
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<PayrollFormula> PayrollFormulas { get; set; }
    public DbSet<PayrollAdjustment> PayrollAdjustments { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    // Payslip Management DbSets
    public DbSet<PayslipTemplate> PayslipTemplates { get; set; }
    public DbSet<PayslipGeneration> PayslipGenerations { get; set; }
    public DbSet<PayslipApprovalHistory> PayslipApprovalHistories { get; set; }
    
    // Leave Management DbSets
    public DbSet<LeavePolicy> LeavePolicies { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveApprovalHistory> LeaveApprovalHistories { get; set; }
    public DbSet<LeaveCalendar> LeaveCalendars { get; set; }
    public DbSet<LeaveAccrual> LeaveAccruals { get; set; }
    public DbSet<LeaveEncashment> LeaveEncashments { get; set; }
    public DbSet<LeaveAccrualRule> LeaveAccrualRules { get; set; }
    
    // Performance Management DbSets
    public DbSet<PerformanceGoal> PerformanceGoals { get; set; }
    public DbSet<PerformanceGoalCheckIn> PerformanceGoalCheckIns { get; set; }
    public DbSet<PerformanceReview> PerformanceReviews { get; set; }
    public DbSet<PerformanceFeedback> PerformanceFeedbacks { get; set; }
    public DbSet<PerformanceImprovementPlan> PerformanceImprovementPlans { get; set; }
    public DbSet<PIPGoal> PIPGoals { get; set; }
    public DbSet<PIPReview> PIPReviews { get; set; }
    
    // Training Management DbSets
    public DbSet<TrainingModule> TrainingModules { get; set; }
    public DbSet<TrainingAssignment> TrainingAssignments { get; set; }
    public DbSet<TrainingProgress> TrainingProgresses { get; set; }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
    public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }
    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    
    // Asset Management DbSets
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<AssetMaintenance> AssetMaintenances { get; set; }
    public DbSet<AssetHandover> AssetHandovers { get; set; }
    
    // Support Ticket DbSets
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<SupportTicketComment> SupportTicketComments { get; set; }
    public DbSet<SupportTicketStatusHistory> SupportTicketStatusHistories { get; set; }
    
    // Notification DbSets
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }
    
    // Chatbot DbSets
    public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
    public DbSet<ChatbotMessage> ChatbotMessages { get; set; }
    public DbSet<ChatbotKnowledgeBase> ChatbotKnowledgeBases { get; set; }
    
    // Knowledge Base DbSets
    public DbSet<KnowledgeBaseDocument> KnowledgeBaseDocuments { get; set; }
    public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
    public DbSet<KnowledgeBaseDocumentApproval> KnowledgeBaseDocumentApprovals { get; set; }
    public DbSet<KnowledgeBaseDocumentAttachment> KnowledgeBaseDocumentAttachments { get; set; }
    public DbSet<KnowledgeBaseDocumentView> KnowledgeBaseDocumentViews { get; set; }
    public DbSet<KnowledgeBaseDocumentComment> KnowledgeBaseDocumentComments { get; set; }
    
    // Document Template DbSets
    public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
    public DbSet<DocumentTemplateVersion> DocumentTemplateVersions { get; set; }
    public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }
    public DbSet<DocumentSignature> DocumentSignatures { get; set; }
    public DbSet<DocumentApproval> DocumentApprovals { get; set; }
    public DbSet<DocumentAuditLog> DocumentAuditLogs { get; set; }
    public DbSet<DocumentRetentionPolicy> DocumentRetentionPolicies { get; set; }
    public DbSet<DocumentRetentionExecution> DocumentRetentionExecutions { get; set; }
    
    // Report Management DbSets
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportShare> ReportShares { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public DbSet<ChatbotKnowledgeBaseFeedback> ChatbotKnowledgeBaseFeedbacks { get; set; }
    public DbSet<ChatbotLearningData> ChatbotLearningData { get; set; }
    
    // Email Management DbSets
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<EmailCampaign> EmailCampaigns { get; set; }
    
    // Survey Management DbSets
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyQuestionOption> SurveyQuestionOptions { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
    public DbSet<SurveyDistribution> SurveyDistributions { get; set; }
    public DbSet<SurveyAnalytics> SurveyAnalytics { get; set; }
    
    // Grievance Management DbSets
    public DbSet<Grievance> Grievances { get; set; }
    public DbSet<GrievanceComment> GrievanceComments { get; set; }
    public DbSet<GrievanceStatusHistory> GrievanceStatusHistories { get; set; }
    public DbSet<GrievanceEscalation> GrievanceEscalations { get; set; }
    public DbSet<GrievanceFollowUp> GrievanceFollowUps { get; set; }
    
    // Expense Management DbSets
    public DbSet<ExpenseClaim> ExpenseClaims { get; set; }
    public DbSet<ExpenseItem> ExpenseItems { get; set; }
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<ExpenseDocument> ExpenseDocuments { get; set; }
    public DbSet<ExpenseApprovalHistory> ExpenseApprovalHistories { get; set; }
    public DbSet<ExpensePolicyRule> ExpensePolicyRules { get; set; }
    public DbSet<TravelExpense> TravelExpenses { get; set; }
    public DbSet<TravelExpenseItem> TravelExpenseItems { get; set; }
    public DbSet<ExpenseBudget> ExpenseBudgets { get; set; }
    public DbSet<ExpenseBudgetAlert> ExpenseBudgetAlerts { get; set; }
    public DbSet<ExpenseComplianceViolation> ExpenseComplianceViolations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrideHRDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}