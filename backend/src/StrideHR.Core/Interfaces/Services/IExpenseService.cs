using StrideHR.Core.Models.Expense;
using StrideHR.Core.Enums;

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
}

