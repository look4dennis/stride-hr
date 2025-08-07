using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;

namespace StrideHR.API.Controllers;

[Authorize]
public class ExpenseController : BaseController
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpenseController> _logger;

    public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new expense claim
    /// </summary>
    [HttpPost("claims")]
    [RequirePermission("expense", "create")]
    public async Task<IActionResult> CreateExpenseClaim([FromBody] CreateExpenseClaimDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.CreateExpenseClaimAsync(employeeId, dto);
            return Success(result, "Expense claim created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense claim");
            return Error("An error occurred while creating the expense claim");
        }
    }

    /// <summary>
    /// Update an existing expense claim
    /// </summary>
    [HttpPut("claims/{id}")]
    [RequirePermission("expense", "update")]
    public async Task<IActionResult> UpdateExpenseClaim(int id, [FromBody] UpdateExpenseClaimDto dto)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.UpdateExpenseClaimAsync(id, dto, employeeId);
            return Success(result, "Expense claim updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense claim {Id}", id);
            return Error("An error occurred while updating the expense claim");
        }
    }

    /// <summary>
    /// Get expense claim by ID
    /// </summary>
    [HttpGet("claims/{id}")]
    [RequirePermission("expense", "read")]
    public async Task<IActionResult> GetExpenseClaim(int id)
    {
        try
        {
            var result = await _expenseService.GetExpenseClaimByIdAsync(id);
            if (result == null)
                return NotFound("Expense claim not found");

            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense claim {Id}", id);
            return Error("An error occurred while retrieving the expense claim");
        }
    }

    /// <summary>
    /// Get expense claims for current employee
    /// </summary>
    [HttpGet("claims/my")]
    [RequirePermission("expense", "read")]
    public async Task<IActionResult> GetMyExpenseClaims()
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.GetExpenseClaimsByEmployeeAsync(employeeId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense claims for employee");
            return Error("An error occurred while retrieving expense claims");
        }
    }

    /// <summary>
    /// Get pending expense claims for approval
    /// </summary>
    [HttpGet("claims/pending-approvals")]
    [RequirePermission("expense", "approve")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _expenseService.GetPendingApprovalsAsync(approverId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approvals");
            return Error("An error occurred while retrieving pending approvals");
        }
    }

    /// <summary>
    /// Submit expense claim for approval
    /// </summary>
    [HttpPost("claims/{id}/submit")]
    [RequirePermission("expense", "submit")]
    public async Task<IActionResult> SubmitExpenseClaim(int id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.SubmitExpenseClaimAsync(id, employeeId);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim submitted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting expense claim {Id}", id);
            return Error("An error occurred while submitting the expense claim");
        }
    }

    /// <summary>
    /// Withdraw expense claim
    /// </summary>
    [HttpPost("claims/{id}/withdraw")]
    [RequirePermission("expense", "withdraw")]
    public async Task<IActionResult> WithdrawExpenseClaim(int id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.WithdrawExpenseClaimAsync(id, employeeId);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim withdrawn successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing expense claim {Id}", id);
            return Error("An error occurred while withdrawing the expense claim");
        }
    }

    /// <summary>
    /// Approve expense claim
    /// </summary>
    [HttpPost("claims/{id}/approve")]
    [RequirePermission("expense", "approve")]
    public async Task<IActionResult> ApproveExpenseClaim(int id, [FromBody] ExpenseApprovalDto dto)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _expenseService.ApproveExpenseClaimAsync(id, approverId, dto);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim approved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense claim {Id}", id);
            return Error("An error occurred while approving the expense claim");
        }
    }

    /// <summary>
    /// Reject expense claim
    /// </summary>
    [HttpPost("claims/{id}/reject")]
    [RequirePermission("expense", "approve")]
    public async Task<IActionResult> RejectExpenseClaim(int id, [FromBody] ExpenseApprovalDto dto)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _expenseService.RejectExpenseClaimAsync(id, approverId, dto);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim rejected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting expense claim {Id}", id);
            return Error("An error occurred while rejecting the expense claim");
        }
    }

    /// <summary>
    /// Bulk approve/reject expense claims
    /// </summary>
    [HttpPost("claims/bulk-approval")]
    [RequirePermission("expense", "approve")]
    public async Task<IActionResult> BulkApproveExpenseClaims([FromBody] BulkExpenseApprovalDto dto)
    {
        try
        {
            var approverId = GetCurrentEmployeeId();
            var result = await _expenseService.BulkApproveExpenseClaimsAsync(dto, approverId);
            return Success(result, $"Bulk {dto.Action.ToString().ToLower()} completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk approving expense claims");
            return Error("An error occurred while processing bulk approval");
        }
    }

    /// <summary>
    /// Delete expense claim
    /// </summary>
    [HttpDelete("claims/{id}")]
    [RequirePermission("expense", "delete")]
    public async Task<IActionResult> DeleteExpenseClaim(int id)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.DeleteExpenseClaimAsync(id, employeeId);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense claim {Id}", id);
            return Error("An error occurred while deleting the expense claim");
        }
    }

    /// <summary>
    /// Upload expense receipt or document
    /// </summary>
    /// <param name="claimId">The expense claim ID</param>
    /// <param name="file">The file to upload (PDF, JPEG, PNG, DOC, DOCX supported, max 10MB)</param>
    /// <param name="expenseItemId">Optional expense item ID to associate the document with</param>
    /// <param name="documentType">Type of document being uploaded</param>
    /// <param name="description">Optional description of the document</param>
    /// <returns>Success response with document information</returns>
    [HttpPost("claims/{claimId}/documents")]
    [RequirePermission("expense", "upload")]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)] // Temporarily exclude from Swagger due to complex IFormFile handling
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(413)]
    [ProducesResponseType(415)]
    public async Task<IActionResult> UploadDocument(
        int claimId, 
        [FromForm] IFormFile file, 
        [FromForm] int? expenseItemId = null,
        [FromForm] DocumentType documentType = DocumentType.Receipt,
        [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Error("No file provided");

            // Convert IFormFile to byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.UploadDocumentAsync(claimId, expenseItemId, fileData, file.FileName, file.ContentType, documentType, description, employeeId);
            
            if (!result)
                return Error("Failed to upload document");

            return Success("Document uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for expense claim {ClaimId}", claimId);
            return Error("An error occurred while uploading the document");
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    [HttpDelete("documents/{documentId}")]
    [RequirePermission("expense", "delete")]
    public async Task<IActionResult> DeleteDocument(int documentId)
    {
        try
        {
            var employeeId = GetCurrentEmployeeId();
            var result = await _expenseService.DeleteDocumentAsync(documentId, employeeId);
            
            if (!result)
                return NotFound("Document not found");

            return Success("Document deleted successfully");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return Error("An error occurred while deleting the document");
        }
    }

    /// <summary>
    /// Download document
    /// </summary>
    [HttpGet("documents/{documentId}/download")]
    [RequirePermission("expense", "read")]
    public async Task<IActionResult> DownloadDocument(int documentId)
    {
        try
        {
            var stream = await _expenseService.DownloadDocumentAsync(documentId);
            if (stream == null)
                return NotFound("Document not found");

            return File(stream, "application/octet-stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            return Error("An error occurred while downloading the document");
        }
    }

    /// <summary>
    /// Get expense categories
    /// </summary>
    [HttpGet("categories")]
    [RequirePermission("expense", "read")]
    public async Task<IActionResult> GetExpenseCategories([FromQuery] int organizationId)
    {
        try
        {
            var result = await _expenseService.GetExpenseCategoriesAsync(organizationId);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense categories");
            return Error("An error occurred while retrieving expense categories");
        }
    }

    /// <summary>
    /// Create expense category
    /// </summary>
    [HttpPost("categories")]
    [RequirePermission("expense_category", "create")]
    public async Task<IActionResult> CreateExpenseCategory([FromBody] CreateExpenseCategoryDto dto, [FromQuery] int organizationId)
    {
        try
        {
            var result = await _expenseService.CreateExpenseCategoryAsync(dto, organizationId);
            return Success(result, "Expense category created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense category");
            return Error("An error occurred while creating the expense category");
        }
    }

    /// <summary>
    /// Update expense category
    /// </summary>
    [HttpPut("categories/{id}")]
    [RequirePermission("expense_category", "update")]
    public async Task<IActionResult> UpdateExpenseCategory(int id, [FromBody] CreateExpenseCategoryDto dto)
    {
        try
        {
            var result = await _expenseService.UpdateExpenseCategoryAsync(id, dto);
            return Success(result, "Expense category updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense category {Id}", id);
            return Error("An error occurred while updating the expense category");
        }
    }

    /// <summary>
    /// Delete expense category
    /// </summary>
    [HttpDelete("categories/{id}")]
    [RequirePermission("expense_category", "delete")]
    public async Task<IActionResult> DeleteExpenseCategory(int id)
    {
        try
        {
            var result = await _expenseService.DeleteExpenseCategoryAsync(id);
            
            if (!result)
                return NotFound("Expense category not found");

            return Success("Expense category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense category {Id}", id);
            return Error("An error occurred while deleting the expense category");
        }
    }

    /// <summary>
    /// Mark expense claim as reimbursed
    /// </summary>
    [HttpPost("claims/{id}/reimburse")]
    [RequirePermission("expense", "reimburse")]
    public async Task<IActionResult> MarkAsReimbursed(int id, [FromBody] ReimbursementDto dto)
    {
        try
        {
            var processedBy = GetCurrentEmployeeId();
            var result = await _expenseService.MarkAsReimbursedAsync(id, dto.ReimbursementReference, processedBy);
            
            if (!result)
                return NotFound("Expense claim not found");

            return Success("Expense claim marked as reimbursed successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking expense claim {Id} as reimbursed", id);
            return Error("An error occurred while marking the expense claim as reimbursed");
        }
    }

    /// <summary>
    /// Get expenses for reimbursement
    /// </summary>
    [HttpGet("claims/for-reimbursement")]
    [RequirePermission("expense", "reimburse")]
    public async Task<IActionResult> GetExpensesForReimbursement()
    {
        try
        {
            var result = await _expenseService.GetExpensesForReimbursementAsync();
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses for reimbursement");
            return Error("An error occurred while retrieving expenses for reimbursement");
        }
    }

    /// <summary>
    /// Get expense report
    /// </summary>
    [HttpGet("reports")]
    [RequirePermission("expense", "report")]
    public async Task<IActionResult> GetExpenseReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? employeeId = null,
        [FromQuery] ExpenseClaimStatus? status = null)
    {
        try
        {
            var result = await _expenseService.GetExpenseReportAsync(startDate, endDate, employeeId, status);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating expense report");
            return Error("An error occurred while generating the expense report");
        }
    }

    /// <summary>
    /// Get total expenses by employee
    /// </summary>
    [HttpGet("employees/{employeeId}/total")]
    [RequirePermission("expense", "read")]
    public async Task<IActionResult> GetTotalExpensesByEmployee(
        int employeeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var result = await _expenseService.GetTotalExpensesByEmployeeAsync(employeeId, startDate, endDate);
            return Success(new { TotalAmount = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total expenses for employee {EmployeeId}", employeeId);
            return Error("An error occurred while retrieving total expenses");
        }
    }
}

public class ReimbursementDto
{
    public string ReimbursementReference { get; set; } = string.Empty;
}