using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Expense;

namespace StrideHR.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseClaimRepository _expenseClaimRepository;
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;
    private readonly IExpenseDocumentRepository _expenseDocumentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseService> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public ExpenseService(
        IExpenseClaimRepository expenseClaimRepository,
        IExpenseCategoryRepository expenseCategoryRepository,
        IExpenseDocumentRepository expenseDocumentRepository,
        IMapper mapper,
        ILogger<ExpenseService> logger,
        IFileStorageService fileStorageService,
        INotificationService notificationService)
    {
        _expenseClaimRepository = expenseClaimRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
        _expenseDocumentRepository = expenseDocumentRepository;
        _mapper = mapper;
        _logger = logger;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<ExpenseClaimDto> CreateExpenseClaimAsync(int employeeId, CreateExpenseClaimDto dto)
    {
        _logger.LogInformation("Creating expense claim for employee {EmployeeId}", employeeId);

        // Validate the expense claim
        var validationErrors = await ValidateExpenseClaimAsync(dto, employeeId);
        if (validationErrors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
        }

        var claimNumber = await _expenseClaimRepository.GenerateClaimNumberAsync();
        
        var expenseClaim = new ExpenseClaim
        {
            EmployeeId = employeeId,
            ClaimNumber = claimNumber,
            Title = dto.Title,
            Description = dto.Description,
            ExpenseDate = dto.ExpenseDate,
            SubmissionDate = DateTime.UtcNow,
            Currency = dto.Currency,
            Status = ExpenseClaimStatus.Draft,
            IsAdvanceClaim = dto.IsAdvanceClaim,
            AdvanceAmount = dto.AdvanceAmount,
            Notes = dto.Notes
        };

        // Add expense items
        foreach (var itemDto in dto.ExpenseItems)
        {
            var expenseItem = new ExpenseItem
            {
                ExpenseCategoryId = itemDto.ExpenseCategoryId,
                Description = itemDto.Description,
                Amount = itemDto.Amount,
                Currency = itemDto.Currency,
                ExpenseDate = itemDto.ExpenseDate,
                Vendor = itemDto.Vendor,
                Location = itemDto.Location,
                IsBillable = itemDto.IsBillable,
                ProjectId = itemDto.ProjectId,
                Notes = itemDto.Notes,
                MileageDistance = itemDto.MileageDistance,
                MileageRate = itemDto.MileageRate
            };

            expenseClaim.ExpenseItems.Add(expenseItem);
        }

        // Calculate total amount
        expenseClaim.TotalAmount = expenseClaim.ExpenseItems.Sum(ei => ei.Amount);

        await _expenseClaimRepository.AddAsync(expenseClaim);
        await _expenseClaimRepository.SaveChangesAsync();

        _logger.LogInformation("Created expense claim {ClaimNumber} for employee {EmployeeId}", claimNumber, employeeId);

        return await GetExpenseClaimByIdAsync(expenseClaim.Id) ?? throw new InvalidOperationException("Failed to retrieve created expense claim");
    }

    public async Task<ExpenseClaimDto> UpdateExpenseClaimAsync(int id, UpdateExpenseClaimDto dto, int employeeId)
    {
        _logger.LogInformation("Updating expense claim {Id} for employee {EmployeeId}", id, employeeId);

        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(id);
        if (expenseClaim == null)
            throw new ArgumentException("Expense claim not found");

        if (expenseClaim.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only update your own expense claims");

        if (expenseClaim.Status != ExpenseClaimStatus.Draft)
            throw new InvalidOperationException("Only draft expense claims can be updated");

        // Update basic properties
        expenseClaim.Title = dto.Title;
        expenseClaim.Description = dto.Description;
        expenseClaim.ExpenseDate = dto.ExpenseDate;
        expenseClaim.Currency = dto.Currency;
        expenseClaim.IsAdvanceClaim = dto.IsAdvanceClaim;
        expenseClaim.AdvanceAmount = dto.AdvanceAmount;
        expenseClaim.Notes = dto.Notes;

        // Update expense items
        var existingItemIds = expenseClaim.ExpenseItems.Select(ei => ei.Id).ToList();
        var updatedItemIds = dto.ExpenseItems.Where(ei => ei.Id.HasValue).Select(ei => ei.Id!.Value).ToList();

        // Remove deleted items
        var itemsToRemove = expenseClaim.ExpenseItems.Where(ei => !updatedItemIds.Contains(ei.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            expenseClaim.ExpenseItems.Remove(item);
        }

        // Update existing items and add new ones
        foreach (var itemDto in dto.ExpenseItems)
        {
            if (itemDto.Id.HasValue)
            {
                // Update existing item
                var existingItem = expenseClaim.ExpenseItems.FirstOrDefault(ei => ei.Id == itemDto.Id.Value);
                if (existingItem != null)
                {
                    existingItem.ExpenseCategoryId = itemDto.ExpenseCategoryId;
                    existingItem.Description = itemDto.Description;
                    existingItem.Amount = itemDto.Amount;
                    existingItem.Currency = itemDto.Currency;
                    existingItem.ExpenseDate = itemDto.ExpenseDate;
                    existingItem.Vendor = itemDto.Vendor;
                    existingItem.Location = itemDto.Location;
                    existingItem.IsBillable = itemDto.IsBillable;
                    existingItem.ProjectId = itemDto.ProjectId;
                    existingItem.Notes = itemDto.Notes;
                    existingItem.MileageDistance = itemDto.MileageDistance;
                    existingItem.MileageRate = itemDto.MileageRate;
                }
            }
            else
            {
                // Add new item
                var newItem = new ExpenseItem
                {
                    ExpenseCategoryId = itemDto.ExpenseCategoryId,
                    Description = itemDto.Description,
                    Amount = itemDto.Amount,
                    Currency = itemDto.Currency,
                    ExpenseDate = itemDto.ExpenseDate,
                    Vendor = itemDto.Vendor,
                    Location = itemDto.Location,
                    IsBillable = itemDto.IsBillable,
                    ProjectId = itemDto.ProjectId,
                    Notes = itemDto.Notes,
                    MileageDistance = itemDto.MileageDistance,
                    MileageRate = itemDto.MileageRate
                };

                expenseClaim.ExpenseItems.Add(newItem);
            }
        }

        // Recalculate total amount
        expenseClaim.TotalAmount = expenseClaim.ExpenseItems.Sum(ei => ei.Amount);

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        await _expenseClaimRepository.SaveChangesAsync();

        _logger.LogInformation("Updated expense claim {Id}", id);

        return await GetExpenseClaimByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated expense claim");
    }

    public async Task<ExpenseClaimDto?> GetExpenseClaimByIdAsync(int id)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(id);
        if (expenseClaim == null)
            return null;

        return _mapper.Map<ExpenseClaimDto>(expenseClaim);
    }

    public async Task<IEnumerable<ExpenseClaimDto>> GetExpenseClaimsByEmployeeAsync(int employeeId)
    {
        var expenseClaims = await _expenseClaimRepository.GetByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<ExpenseClaimDto>>(expenseClaims);
    }

    public async Task<IEnumerable<ExpenseClaimDto>> GetPendingApprovalsAsync(int approverId)
    {
        var expenseClaims = await _expenseClaimRepository.GetPendingApprovalsAsync(approverId);
        return _mapper.Map<IEnumerable<ExpenseClaimDto>>(expenseClaims);
    }

    public async Task<bool> DeleteExpenseClaimAsync(int id, int employeeId)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdAsync(id);
        if (expenseClaim == null)
            return false;

        if (expenseClaim.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only delete your own expense claims");

        if (expenseClaim.Status != ExpenseClaimStatus.Draft)
            throw new InvalidOperationException("Only draft expense claims can be deleted");

        await _expenseClaimRepository.DeleteAsync(expenseClaim);
        return await _expenseClaimRepository.SaveChangesAsync();
    }

    public async Task<bool> SubmitExpenseClaimAsync(int id, int employeeId)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(id);
        if (expenseClaim == null)
            return false;

        if (expenseClaim.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only submit your own expense claims");

        if (expenseClaim.Status != ExpenseClaimStatus.Draft)
            throw new InvalidOperationException("Only draft expense claims can be submitted");

        // Validate before submission
        var validationErrors = await ValidateExpenseClaimAsync(_mapper.Map<CreateExpenseClaimDto>(expenseClaim), employeeId);
        if (validationErrors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
        }

        expenseClaim.Status = ExpenseClaimStatus.Submitted;
        expenseClaim.SubmissionDate = DateTime.UtcNow;

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        var result = await _expenseClaimRepository.SaveChangesAsync();

        if (result)
        {
            // Send notification to approvers
            await _notificationService.CreateFromTemplateAsync("ExpenseClaimSubmitted", expenseClaim.EmployeeId, 
                new Dictionary<string, object> { { "ClaimNumber", expenseClaim.ClaimNumber } });
            _logger.LogInformation("Submitted expense claim {ClaimNumber}", expenseClaim.ClaimNumber);
        }

        return result;
    }

    public async Task<bool> WithdrawExpenseClaimAsync(int id, int employeeId)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdAsync(id);
        if (expenseClaim == null)
            return false;

        if (expenseClaim.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only withdraw your own expense claims");

        if (expenseClaim.Status != ExpenseClaimStatus.Submitted && expenseClaim.Status != ExpenseClaimStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or under review expense claims can be withdrawn");

        expenseClaim.Status = ExpenseClaimStatus.Draft;

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        return await _expenseClaimRepository.SaveChangesAsync();
    }

    public async Task<bool> ApproveExpenseClaimAsync(int id, int approverId, ExpenseApprovalDto dto)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(id);
        if (expenseClaim == null)
            return false;

        // Add approval history
        var approvalHistory = new ExpenseApprovalHistory
        {
            ExpenseClaimId = id,
            ApproverId = approverId,
            ApprovalLevel = ApprovalLevel.Manager, // This should be determined based on approver role
            Action = dto.Action,
            Comments = dto.Comments,
            ActionDate = DateTime.UtcNow,
            ApprovedAmount = dto.ApprovedAmount ?? expenseClaim.TotalAmount
        };

        expenseClaim.ApprovalHistory.Add(approvalHistory);

        // Update status based on approval level
        if (dto.Action == ApprovalAction.Approved)
        {
            expenseClaim.Status = ExpenseClaimStatus.Approved;
            expenseClaim.ApprovedDate = DateTime.UtcNow;
            expenseClaim.ApprovedBy = approverId;
        }

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        var result = await _expenseClaimRepository.SaveChangesAsync();

        if (result)
        {
            await _notificationService.CreateFromTemplateAsync("ExpenseClaimApproved", expenseClaim.EmployeeId, 
                new Dictionary<string, object> { { "ClaimNumber", expenseClaim.ClaimNumber } });
            _logger.LogInformation("Approved expense claim {ClaimNumber} by approver {ApproverId}", expenseClaim.ClaimNumber, approverId);
        }

        return result;
    }

    public async Task<bool> RejectExpenseClaimAsync(int id, int approverId, ExpenseApprovalDto dto)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(id);
        if (expenseClaim == null)
            return false;

        // Add approval history
        var approvalHistory = new ExpenseApprovalHistory
        {
            ExpenseClaimId = id,
            ApproverId = approverId,
            ApprovalLevel = ApprovalLevel.Manager,
            Action = ApprovalAction.Rejected,
            Comments = dto.Comments,
            ActionDate = DateTime.UtcNow,
            RejectionReason = dto.RejectionReason
        };

        expenseClaim.ApprovalHistory.Add(approvalHistory);
        expenseClaim.Status = ExpenseClaimStatus.Rejected;
        expenseClaim.RejectionReason = dto.RejectionReason;

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        var result = await _expenseClaimRepository.SaveChangesAsync();

        if (result)
        {
            await _notificationService.CreateFromTemplateAsync("ExpenseClaimRejected", expenseClaim.EmployeeId, 
                new Dictionary<string, object> { { "ClaimNumber", expenseClaim.ClaimNumber }, { "RejectionReason", dto.RejectionReason ?? "" } });
            _logger.LogInformation("Rejected expense claim {ClaimNumber} by approver {ApproverId}", expenseClaim.ClaimNumber, approverId);
        }

        return result;
    }

    public async Task<IEnumerable<ExpenseClaimDto>> BulkApproveExpenseClaimsAsync(BulkExpenseApprovalDto dto, int approverId)
    {
        var approvedClaims = new List<ExpenseClaimDto>();

        foreach (var claimId in dto.ExpenseClaimIds)
        {
            var approvalDto = new ExpenseApprovalDto
            {
                Action = dto.Action,
                Comments = dto.Comments,
                RejectionReason = dto.RejectionReason
            };

            if (dto.Action == ApprovalAction.Approved)
            {
                await ApproveExpenseClaimAsync(claimId, approverId, approvalDto);
            }
            else if (dto.Action == ApprovalAction.Rejected)
            {
                await RejectExpenseClaimAsync(claimId, approverId, approvalDto);
            }

            var updatedClaim = await GetExpenseClaimByIdAsync(claimId);
            if (updatedClaim != null)
            {
                approvedClaims.Add(updatedClaim);
            }
        }

        return approvedClaims;
    }

    public async Task<bool> UploadDocumentAsync(int expenseClaimId, int? expenseItemId, byte[] fileData, string fileName, string contentType, DocumentType documentType, string? description, int uploadedBy)
    {
        var savedFileName = await _fileStorageService.SaveFileAsync(fileData, fileName, "expense-documents");
        
        var document = new ExpenseDocument
        {
            ExpenseClaimId = expenseClaimId,
            ExpenseItemId = expenseItemId,
            FileName = savedFileName,
            OriginalFileName = fileName,
            FilePath = $"expense-documents/{savedFileName}",
            ContentType = contentType,
            FileSize = fileData.Length,
            DocumentType = documentType,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            Description = description
        };

        await _expenseDocumentRepository.AddAsync(document);
        return await _expenseDocumentRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int employeeId)
    {
        var document = await _expenseDocumentRepository.GetByIdAsync(documentId);
        if (document == null)
            return false;

        // Check if user has permission to delete this document
        if (document.UploadedBy != employeeId)
            throw new UnauthorizedAccessException("You can only delete documents you uploaded");

        // Delete physical file
        await _fileStorageService.DeleteFileAsync(document.FilePath);

        return await _expenseDocumentRepository.DeleteDocumentAsync(documentId);
    }

    public async Task<Stream?> DownloadDocumentAsync(int documentId)
    {
        var document = await _expenseDocumentRepository.GetByIdAsync(documentId);
        if (document == null)
            return null;

        var fileData = await _fileStorageService.GetFileAsync(document.FilePath);
        if (fileData == null)
            return null;

        return new MemoryStream(fileData);
    }

    public async Task<List<string>> ValidateExpenseClaimAsync(CreateExpenseClaimDto dto, int employeeId)
    {
        var errors = new List<string>();

        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Title))
            errors.Add("Title is required");

        if (dto.ExpenseDate > DateTime.Today)
            errors.Add("Expense date cannot be in the future");

        if (!dto.ExpenseItems.Any())
            errors.Add("At least one expense item is required");

        // Validate each expense item
        foreach (var item in dto.ExpenseItems)
        {
            var itemErrors = await ValidateExpenseItemAsync(item, employeeId);
            errors.AddRange(itemErrors);
        }

        return errors;
    }

    public async Task<List<string>> ValidateExpenseItemAsync(CreateExpenseItemDto dto, int employeeId)
    {
        var errors = new List<string>();

        // Check if category exists and is active
        var category = await _expenseCategoryRepository.GetByIdAsync(dto.ExpenseCategoryId);
        if (category == null || !category.IsActive)
        {
            errors.Add($"Invalid expense category for item: {dto.Description}");
            return errors;
        }

        // Validate amount limits
        if (category.MaxAmount.HasValue && dto.Amount > category.MaxAmount.Value)
        {
            errors.Add($"Amount {dto.Amount:C} exceeds maximum allowed {category.MaxAmount.Value:C} for category {category.Name}");
        }

        // Validate daily limits
        if (category.DailyLimit.HasValue)
        {
            var dailyTotal = await GetDailyExpenseTotal(employeeId, dto.ExpenseCategoryId, dto.ExpenseDate);
            if (dailyTotal + dto.Amount > category.DailyLimit.Value)
            {
                errors.Add($"Daily limit of {category.DailyLimit.Value:C} would be exceeded for category {category.Name}");
            }
        }

        // Validate monthly limits
        if (category.MonthlyLimit.HasValue)
        {
            var monthlyTotal = await GetMonthlyExpenseTotal(employeeId, dto.ExpenseCategoryId, dto.ExpenseDate);
            if (monthlyTotal + dto.Amount > category.MonthlyLimit.Value)
            {
                errors.Add($"Monthly limit of {category.MonthlyLimit.Value:C} would be exceeded for category {category.Name}");
            }
        }

        // Validate mileage-based expenses
        if (category.IsMileageBased)
        {
            if (!dto.MileageDistance.HasValue || dto.MileageDistance.Value <= 0)
            {
                errors.Add($"Mileage distance is required for category {category.Name}");
            }

            if (!dto.MileageRate.HasValue || dto.MileageRate.Value <= 0)
            {
                errors.Add($"Mileage rate is required for category {category.Name}");
            }
        }

        return errors;
    }

    public async Task<bool> MarkAsReimbursedAsync(int id, string reimbursementReference, int processedBy)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdAsync(id);
        if (expenseClaim == null)
            return false;

        if (expenseClaim.Status != ExpenseClaimStatus.Approved)
            throw new InvalidOperationException("Only approved expense claims can be marked as reimbursed");

        expenseClaim.Status = ExpenseClaimStatus.Reimbursed;
        expenseClaim.ReimbursedDate = DateTime.UtcNow;
        expenseClaim.ReimbursementReference = reimbursementReference;

        await _expenseClaimRepository.UpdateAsync(expenseClaim);
        var result = await _expenseClaimRepository.SaveChangesAsync();

        if (result)
        {
            await _notificationService.CreateFromTemplateAsync("ExpenseClaimReimbursed", expenseClaim.EmployeeId, 
                new Dictionary<string, object> { { "ClaimNumber", expenseClaim.ClaimNumber }, { "ReimbursementReference", reimbursementReference } });
            _logger.LogInformation("Marked expense claim {ClaimNumber} as reimbursed", expenseClaim.ClaimNumber);
        }

        return result;
    }

    public async Task<IEnumerable<ExpenseClaimDto>> GetExpensesForReimbursementAsync()
    {
        var expenseClaims = await _expenseClaimRepository.GetExpensesForReimbursementAsync();
        return _mapper.Map<IEnumerable<ExpenseClaimDto>>(expenseClaims);
    }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetExpenseCategoriesAsync(int organizationId)
    {
        var categories = await _expenseCategoryRepository.GetActiveByOrganizationAsync(organizationId);
        return _mapper.Map<IEnumerable<ExpenseCategoryDto>>(categories);
    }

    public async Task<ExpenseCategoryDto> CreateExpenseCategoryAsync(CreateExpenseCategoryDto dto, int organizationId)
    {
        // Check if code is unique
        var isUnique = await _expenseCategoryRepository.IsCodeUniqueAsync(dto.Code, organizationId);
        if (!isUnique)
            throw new ArgumentException($"Category code '{dto.Code}' already exists");

        var category = new ExpenseCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Code = dto.Code,
            RequiresReceipt = dto.RequiresReceipt,
            MaxAmount = dto.MaxAmount,
            DailyLimit = dto.DailyLimit,
            MonthlyLimit = dto.MonthlyLimit,
            RequiresApproval = dto.RequiresApproval,
            DefaultApprovalLevel = dto.DefaultApprovalLevel,
            IsMileageBased = dto.IsMileageBased,
            MileageRate = dto.MileageRate,
            PolicyDescription = dto.PolicyDescription,
            OrganizationId = organizationId,
            IsActive = true
        };

        await _expenseCategoryRepository.AddAsync(category);
        await _expenseCategoryRepository.SaveChangesAsync();

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<ExpenseCategoryDto> UpdateExpenseCategoryAsync(int id, CreateExpenseCategoryDto dto)
    {
        var category = await _expenseCategoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new ArgumentException("Category not found");

        // Check if code is unique (excluding current category)
        var isUnique = await _expenseCategoryRepository.IsCodeUniqueAsync(dto.Code, category.OrganizationId, id);
        if (!isUnique)
            throw new ArgumentException($"Category code '{dto.Code}' already exists");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Code = dto.Code;
        category.RequiresReceipt = dto.RequiresReceipt;
        category.MaxAmount = dto.MaxAmount;
        category.DailyLimit = dto.DailyLimit;
        category.MonthlyLimit = dto.MonthlyLimit;
        category.RequiresApproval = dto.RequiresApproval;
        category.DefaultApprovalLevel = dto.DefaultApprovalLevel;
        category.IsMileageBased = dto.IsMileageBased;
        category.MileageRate = dto.MileageRate;
        category.PolicyDescription = dto.PolicyDescription;

        await _expenseCategoryRepository.UpdateAsync(category);
        await _expenseCategoryRepository.SaveChangesAsync();

        return _mapper.Map<ExpenseCategoryDto>(category);
    }

    public async Task<bool> DeleteExpenseCategoryAsync(int id)
    {
        var category = await _expenseCategoryRepository.GetByIdAsync(id);
        if (category == null)
            return false;

        // Soft delete by marking as inactive
        category.IsActive = false;

        await _expenseCategoryRepository.UpdateAsync(category);
        return await _expenseCategoryRepository.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalExpensesByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        return await _expenseClaimRepository.GetTotalExpensesByEmployeeAsync(employeeId, startDate, endDate);
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategory(int organizationId, DateTime startDate, DateTime endDate)
    {
        // This would need to be implemented with a more complex query
        // For now, returning empty dictionary
        return new Dictionary<string, decimal>();
    }

    public async Task<IEnumerable<ExpenseClaimDto>> GetExpenseReportAsync(DateTime startDate, DateTime endDate, int? employeeId = null, ExpenseClaimStatus? status = null)
    {
        var expenseClaims = await _expenseClaimRepository.GetByDateRangeAsync(startDate, endDate);
        
        if (employeeId.HasValue)
        {
            expenseClaims = expenseClaims.Where(e => e.EmployeeId == employeeId.Value);
        }

        if (status.HasValue)
        {
            expenseClaims = expenseClaims.Where(e => e.Status == status.Value);
        }

        return _mapper.Map<IEnumerable<ExpenseClaimDto>>(expenseClaims);
    }

    private async Task<decimal> GetDailyExpenseTotal(int employeeId, int categoryId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var claims = await _expenseClaimRepository.FindAsync(
            e => e.EmployeeId == employeeId && 
                 e.ExpenseDate >= startOfDay && 
                 e.ExpenseDate <= endOfDay &&
                 (e.Status == ExpenseClaimStatus.Approved || e.Status == ExpenseClaimStatus.Reimbursed));

        return claims.SelectMany(c => c.ExpenseItems)
                    .Where(ei => ei.ExpenseCategoryId == categoryId)
                    .Sum(ei => ei.Amount);
    }

    private async Task<decimal> GetMonthlyExpenseTotal(int employeeId, int categoryId, DateTime date)
    {
        var startOfMonth = new DateTime(date.Year, date.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var claims = await _expenseClaimRepository.FindAsync(
            e => e.EmployeeId == employeeId && 
                 e.ExpenseDate >= startOfMonth && 
                 e.ExpenseDate <= endOfMonth &&
                 (e.Status == ExpenseClaimStatus.Approved || e.Status == ExpenseClaimStatus.Reimbursed));

        return claims.SelectMany(c => c.ExpenseItems)
                    .Where(ei => ei.ExpenseCategoryId == categoryId)
                    .Sum(ei => ei.Amount);
    }
}