using StrideHR.Core.Models.Expense;
using StrideHR.Core.Enums;
using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Services;

public interface IExpenseService
{
    // Expense Claim Management
    Task<ExpenseClaimDto> CreateExpenseClaimAsync(int employeeId, CreateExpenseClaimDto dto);
    Task<ExpenseClaimDto> UpdateExpenseClaimAsync(int id, UpdateExpenseClaimDto dto, int employeeId);
    Task<ExpenseClaimDto?> GetExpenseClaimByIdAsync(int id);
    Task<IEnumerable<ExpenseClaimDto>> GetExpenseClaimsByEmployeeAsync(int employeeId);
    Task<IEnumerable<ExpenseClaimDto>> GetPendingApprovalsAsync(int approverId);
    Task<bool> DeleteExpenseClaimAsync(int id, int employeeId);
    Task<bool> SubmitExpenseClaimAsync(int id, int employeeId);
    Task<bool> WithdrawExpenseClaimAsync(int id, int employeeId);

    // Approval Workflow
    Task<bool> ApproveExpenseClaimAsync(int id, int approverId, ExpenseApprovalDto dto);
    Task<bool> RejectExpenseClaimAsync(int id, int approverId, ExpenseApprovalDto dto);
    Task<IEnumerable<ExpenseClaimDto>> BulkApproveExpenseClaimsAsync(BulkExpenseApprovalDto dto, int approverId);

    // Document Management
    Task<bool> UploadDocumentAsync(int expenseClaimId, int? expenseItemId, byte[] fileData, string fileName, string contentType, DocumentType documentType, string? description, int uploadedBy);
    Task<bool> DeleteDocumentAsync(int documentId, int employeeId);
    Task<Stream?> DownloadDocumentAsync(int documentId);

    // Policy Validation
    Task<List<string>> ValidateExpenseClaimAsync(CreateExpenseClaimDto dto, int employeeId);
    Task<List<string>> ValidateExpenseItemAsync(CreateExpenseItemDto dto, int employeeId);

    // Reimbursement
    Task<bool> MarkAsReimbursedAsync(int id, string reimbursementReference, int processedBy);
    Task<IEnumerable<ExpenseClaimDto>> GetExpensesForReimbursementAsync();

    // Categories
    Task<IEnumerable<ExpenseCategoryDto>> GetExpenseCategoriesAsync(int organizationId);
    Task<ExpenseCategoryDto> CreateExpenseCategoryAsync(CreateExpenseCategoryDto dto, int organizationId);
    Task<ExpenseCategoryDto> UpdateExpenseCategoryAsync(int id, CreateExpenseCategoryDto dto);
    Task<bool> DeleteExpenseCategoryAsync(int id);

    // Reports and Analytics
    Task<decimal> GetTotalExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, decimal>> GetExpensesByCategory(int organizationId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ExpenseClaimDto>> GetExpenseReportAsync(DateTime startDate, DateTime endDate, int? employeeId = null, ExpenseClaimStatus? status = null);

    // Travel Expense Management
    Task<TravelExpenseDto> CreateTravelExpenseAsync(int expenseClaimId, CreateTravelExpenseDto dto);
    Task<TravelExpenseDto> UpdateTravelExpenseAsync(int id, CreateTravelExpenseDto dto);
    Task<TravelExpenseDto?> GetTravelExpenseByIdAsync(int id);
    Task<TravelExpenseDto?> GetTravelExpenseByClaimIdAsync(int expenseClaimId);
    Task<IEnumerable<TravelExpenseDto>> GetTravelExpensesByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> DeleteTravelExpenseAsync(int id);

    // Mileage Calculation
    Task<MileageCalculationResultDto> CalculateMileageAsync(MileageCalculationDto dto);
    Task<decimal> GetMileageRateAsync(int organizationId, TravelMode travelMode);
    Task<bool> UpdateMileageRateAsync(int organizationId, TravelMode travelMode, decimal rate);

    // Expense Analytics
    Task<ExpenseAnalyticsDto> GetExpenseAnalyticsAsync(int organizationId, ExpenseAnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ExpenseCategoryAnalyticsDto>> GetCategoryAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<EmployeeExpenseAnalyticsDto>> GetEmployeeAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MonthlyExpenseTrendDto>> GetMonthlyTrendsAsync(int organizationId, int months = 12);
    Task<IEnumerable<TravelExpenseAnalyticsDto>> GetTravelAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate);

    // Budget Tracking
    Task<ExpenseBudgetTrackingDto> GetBudgetTrackingAsync(int organizationId, int? departmentId = null, int? employeeId = null);
    Task<IEnumerable<ExpenseBudgetTrackingDto>> GetBudgetTrackingByPeriodAsync(int organizationId, ExpenseAnalyticsPeriod period);
    Task<bool> CheckBudgetComplianceAsync(int expenseClaimId);
    Task<IEnumerable<ExpenseBudgetAlert>> GetBudgetAlertsAsync(int organizationId, bool unresolved = true);

    // Policy Compliance
    Task<ExpenseComplianceReportDto> GetComplianceReportAsync(int organizationId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ExpenseComplianceViolationDto>> GetComplianceViolationsAsync(int organizationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> ValidateExpenseComplianceAsync(int expenseClaimId);
    Task<bool> ResolveComplianceViolationAsync(int violationId, int resolvedBy, string resolutionNotes);
    Task<bool> WaiveComplianceViolationAsync(int violationId, int waivedBy, string waiverReason);

    // Advanced Reporting
    Task<IEnumerable<ExpenseClaimDto>> GetAdvancedExpenseReportAsync(ExpenseReportFilterDto filter);
    Task<byte[]> ExportExpenseReportAsync(ExpenseReportFilterDto filter, string format = "xlsx");
    Task<Dictionary<string, object>> GetExpenseDashboardDataAsync(int organizationId, int? employeeId = null);
}

