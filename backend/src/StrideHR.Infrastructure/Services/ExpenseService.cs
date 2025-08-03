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
    private readonly ITravelExpenseRepository _travelExpenseRepository;
    private readonly IExpenseBudgetRepository _expenseBudgetRepository;
    private readonly IExpenseComplianceViolationRepository _complianceViolationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseService> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public ExpenseService(
        IExpenseClaimRepository expenseClaimRepository,
        IExpenseCategoryRepository expenseCategoryRepository,
        IExpenseDocumentRepository expenseDocumentRepository,
        ITravelExpenseRepository travelExpenseRepository,
        IExpenseBudgetRepository expenseBudgetRepository,
        IExpenseComplianceViolationRepository complianceViolationRepository,
        IMapper mapper,
        ILogger<ExpenseService> logger,
        IFileStorageService fileStorageService,
        INotificationService notificationService)
    {
        _expenseClaimRepository = expenseClaimRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
        _expenseDocumentRepository = expenseDocumentRepository;
        _travelExpenseRepository = travelExpenseRepository;
        _expenseBudgetRepository = expenseBudgetRepository;
        _complianceViolationRepository = complianceViolationRepository;
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

    // Travel Expense Management Implementation
    public async Task<TravelExpenseDto> CreateTravelExpenseAsync(int expenseClaimId, CreateTravelExpenseDto dto)
    {
        _logger.LogInformation("Creating travel expense for expense claim {ExpenseClaimId}", expenseClaimId);

        var expenseClaim = await _expenseClaimRepository.GetByIdAsync(expenseClaimId);
        if (expenseClaim == null)
            throw new ArgumentException("Expense claim not found");

        var travelExpense = new TravelExpense
        {
            ExpenseClaimId = expenseClaimId,
            TravelPurpose = dto.TravelPurpose,
            FromLocation = dto.FromLocation,
            ToLocation = dto.ToLocation,
            DepartureDate = dto.DepartureDate,
            ReturnDate = dto.ReturnDate,
            TravelMode = dto.TravelMode,
            MileageDistance = dto.MileageDistance,
            MileageRate = dto.MileageRate,
            VehicleDetails = dto.VehicleDetails,
            RouteDetails = dto.RouteDetails,
            IsRoundTrip = dto.IsRoundTrip,
            ProjectId = dto.ProjectId
        };

        // Calculate mileage amount if applicable
        if (dto.MileageDistance.HasValue && dto.MileageRate.HasValue)
        {
            travelExpense.CalculatedMileageAmount = await _travelExpenseRepository.CalculateMileageAmountAsync(
                dto.MileageDistance.Value, dto.MileageRate.Value, dto.IsRoundTrip);
        }

        // Add travel items
        foreach (var itemDto in dto.TravelItems)
        {
            var travelItem = new TravelExpenseItem
            {
                ExpenseType = itemDto.ExpenseType,
                Description = itemDto.Description,
                Amount = itemDto.Amount,
                Currency = itemDto.Currency,
                ExpenseDate = itemDto.ExpenseDate,
                Vendor = itemDto.Vendor,
                Location = itemDto.Location,
                Notes = itemDto.Notes,
                RequiresReceipt = GetExpenseTypeReceiptRequirement(itemDto.ExpenseType),
                HasReceipt = false
            };

            travelExpense.TravelItems.Add(travelItem);
        }

        await _travelExpenseRepository.AddAsync(travelExpense);
        await _travelExpenseRepository.SaveChangesAsync();

        _logger.LogInformation("Created travel expense {Id} for expense claim {ExpenseClaimId}", travelExpense.Id, expenseClaimId);

        return await GetTravelExpenseByIdAsync(travelExpense.Id) ?? throw new InvalidOperationException("Failed to retrieve created travel expense");
    }

    public async Task<TravelExpenseDto> UpdateTravelExpenseAsync(int id, CreateTravelExpenseDto dto)
    {
        _logger.LogInformation("Updating travel expense {Id}", id);

        var travelExpense = await _travelExpenseRepository.GetByIdAsync(id);
        if (travelExpense == null)
            throw new ArgumentException("Travel expense not found");

        // Update basic properties
        travelExpense.TravelPurpose = dto.TravelPurpose;
        travelExpense.FromLocation = dto.FromLocation;
        travelExpense.ToLocation = dto.ToLocation;
        travelExpense.DepartureDate = dto.DepartureDate;
        travelExpense.ReturnDate = dto.ReturnDate;
        travelExpense.TravelMode = dto.TravelMode;
        travelExpense.MileageDistance = dto.MileageDistance;
        travelExpense.MileageRate = dto.MileageRate;
        travelExpense.VehicleDetails = dto.VehicleDetails;
        travelExpense.RouteDetails = dto.RouteDetails;
        travelExpense.IsRoundTrip = dto.IsRoundTrip;
        travelExpense.ProjectId = dto.ProjectId;

        // Recalculate mileage amount
        if (dto.MileageDistance.HasValue && dto.MileageRate.HasValue)
        {
            travelExpense.CalculatedMileageAmount = await _travelExpenseRepository.CalculateMileageAmountAsync(
                dto.MileageDistance.Value, dto.MileageRate.Value, dto.IsRoundTrip);
        }

        await _travelExpenseRepository.UpdateAsync(travelExpense);
        await _travelExpenseRepository.SaveChangesAsync();

        _logger.LogInformation("Updated travel expense {Id}", id);

        return await GetTravelExpenseByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated travel expense");
    }

    public async Task<TravelExpenseDto?> GetTravelExpenseByIdAsync(int id)
    {
        var travelExpense = await _travelExpenseRepository.GetByIdAsync(id);
        if (travelExpense == null)
            return null;

        return _mapper.Map<TravelExpenseDto>(travelExpense);
    }

    public async Task<TravelExpenseDto?> GetTravelExpenseByClaimIdAsync(int expenseClaimId)
    {
        var travelExpense = await _travelExpenseRepository.GetByExpenseClaimIdAsync(expenseClaimId);
        if (travelExpense == null)
            return null;

        return _mapper.Map<TravelExpenseDto>(travelExpense);
    }

    public async Task<IEnumerable<TravelExpenseDto>> GetTravelExpensesByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var travelExpenses = await _travelExpenseRepository.GetByEmployeeIdAsync(employeeId, startDate, endDate);
        return _mapper.Map<IEnumerable<TravelExpenseDto>>(travelExpenses);
    }

    public async Task<bool> DeleteTravelExpenseAsync(int id)
    {
        var travelExpense = await _travelExpenseRepository.GetByIdAsync(id);
        if (travelExpense == null)
            return false;

        await _travelExpenseRepository.DeleteAsync(travelExpense);
        return await _travelExpenseRepository.SaveChangesAsync();
    }

    // Mileage Calculation Implementation
    public async Task<MileageCalculationResultDto> CalculateMileageAsync(MileageCalculationDto dto)
    {
        var totalAmount = await _travelExpenseRepository.CalculateMileageAmountAsync(dto.Distance, dto.Rate, dto.IsRoundTrip);
        var totalDistance = dto.IsRoundTrip ? dto.Distance * 2 : dto.Distance;

        return new MileageCalculationResultDto
        {
            TotalDistance = totalDistance,
            Rate = dto.Rate,
            TotalAmount = totalAmount,
            IsRoundTrip = dto.IsRoundTrip,
            CalculationDetails = $"Distance: {dto.Distance} miles, Rate: ${dto.Rate}/mile, Round Trip: {dto.IsRoundTrip}",
            CalculatedAt = DateTime.UtcNow
        };
    }

    public async Task<decimal> GetMileageRateAsync(int organizationId, TravelMode travelMode)
    {
        // This would typically be stored in a configuration table
        // For now, return default rates based on travel mode
        return travelMode switch
        {
            TravelMode.PersonalVehicle => 0.65m, // Standard IRS rate
            TravelMode.Car => 0.65m,
            TravelMode.Motorcycle => 0.58m,
            _ => 0.00m
        };
    }

    public async Task<bool> UpdateMileageRateAsync(int organizationId, TravelMode travelMode, decimal rate)
    {
        // This would update the configuration table
        // For now, just return true as if it was updated
        _logger.LogInformation("Updated mileage rate for organization {OrganizationId}, travel mode {TravelMode} to {Rate}", 
            organizationId, travelMode, rate);
        return true;
    }

    // Expense Analytics Implementation
    public async Task<ExpenseAnalyticsDto> GetExpenseAnalyticsAsync(int organizationId, ExpenseAnalyticsPeriod period, DateTime? startDate = null, DateTime? endDate = null)
    {
        var (start, end) = GetPeriodDates(period, startDate, endDate);

        var analytics = new ExpenseAnalyticsDto
        {
            Period = period,
            StartDate = start,
            EndDate = end
        };

        // Get basic expense metrics
        var claims = await _expenseClaimRepository.GetByDateRangeAsync(start, end);
        var organizationClaims = claims.Where(c => c.Employee.Branch.OrganizationId == organizationId);

        analytics.TotalExpenses = organizationClaims.Sum(c => c.TotalAmount);
        analytics.TotalClaims = organizationClaims.Count();
        analytics.ApprovedClaims = organizationClaims.Count(c => c.Status == ExpenseClaimStatus.Approved || c.Status == ExpenseClaimStatus.Reimbursed);
        analytics.PendingClaims = organizationClaims.Count(c => c.Status == ExpenseClaimStatus.Submitted || c.Status == ExpenseClaimStatus.UnderReview);
        analytics.RejectedClaims = organizationClaims.Count(c => c.Status == ExpenseClaimStatus.Rejected);
        analytics.AverageClaimAmount = analytics.TotalClaims > 0 ? analytics.TotalExpenses / analytics.TotalClaims : 0;

        // Get travel-specific metrics
        var travelExpenses = await _travelExpenseRepository.GetByEmployeeIdAsync(0, start, end); // This would need to be organization-based
        analytics.TotalTravelExpenses = travelExpenses.SelectMany(te => te.TravelItems).Sum(ti => ti.Amount);
        analytics.TotalMileageExpenses = travelExpenses.Where(te => te.CalculatedMileageAmount.HasValue).Sum(te => te.CalculatedMileageAmount ?? 0);

        // Get category breakdown
        analytics.CategoryBreakdown = (await GetCategoryAnalyticsAsync(organizationId, start, end)).ToList();

        // Get employee breakdown
        analytics.EmployeeBreakdown = (await GetEmployeeAnalyticsAsync(organizationId, start, end)).ToList();

        // Get monthly trends
        analytics.MonthlyTrends = (await GetMonthlyTrendsAsync(organizationId, 12)).ToList();

        // Get travel analytics
        analytics.TravelAnalytics = (await GetTravelAnalyticsAsync(organizationId, start, end)).ToList();

        return analytics;
    }

    public async Task<IEnumerable<ExpenseCategoryAnalyticsDto>> GetCategoryAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate)
    {
        var categories = await _expenseCategoryRepository.GetActiveByOrganizationAsync(organizationId);
        var claims = await _expenseClaimRepository.GetByDateRangeAsync(startDate, endDate);
        var organizationClaims = claims.Where(c => c.Employee.Branch.OrganizationId == organizationId);

        var totalExpenses = organizationClaims.Sum(c => c.TotalAmount);

        var categoryAnalytics = new List<ExpenseCategoryAnalyticsDto>();

        foreach (var category in categories)
        {
            var categoryItems = organizationClaims
                .SelectMany(c => c.ExpenseItems)
                .Where(ei => ei.ExpenseCategoryId == category.Id);

            var categoryTotal = categoryItems.Sum(ei => ei.Amount);
            var categoryCount = categoryItems.Count();

            categoryAnalytics.Add(new ExpenseCategoryAnalyticsDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryCode = category.Code,
                TotalAmount = categoryTotal,
                ClaimCount = categoryCount,
                AverageAmount = categoryCount > 0 ? categoryTotal / categoryCount : 0,
                PercentageOfTotal = totalExpenses > 0 ? (categoryTotal / totalExpenses) * 100 : 0,
                BudgetLimit = category.MonthlyLimit ?? 0,
                BudgetUtilization = category.MonthlyLimit.HasValue && category.MonthlyLimit > 0 ? (categoryTotal / category.MonthlyLimit.Value) * 100 : 0,
                IsOverBudget = category.MonthlyLimit.HasValue && categoryTotal > category.MonthlyLimit.Value
            });
        }

        return categoryAnalytics.OrderByDescending(ca => ca.TotalAmount);
    }

    public async Task<IEnumerable<EmployeeExpenseAnalyticsDto>> GetEmployeeAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate)
    {
        var claims = await _expenseClaimRepository.GetByDateRangeAsync(startDate, endDate);
        var organizationClaims = claims.Where(c => c.Employee.Branch.OrganizationId == organizationId);

        var employeeAnalytics = organizationClaims
            .GroupBy(c => c.Employee)
            .Select(g => new EmployeeExpenseAnalyticsDto
            {
                EmployeeId = g.Key.Id,
                EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                Department = g.Key.Department,
                TotalExpenses = g.Sum(c => c.TotalAmount),
                ClaimCount = g.Count(),
                AverageClaimAmount = g.Average(c => c.TotalAmount),
                TravelExpenses = 0, // Would need to calculate from travel expenses
                MileageExpenses = 0, // Would need to calculate from mileage
                BudgetLimit = 0, // Would need to get from budget configuration
                BudgetUtilization = 0,
                IsOverBudget = false
            })
            .OrderByDescending(ea => ea.TotalExpenses);

        return employeeAnalytics;
    }

    public async Task<IEnumerable<MonthlyExpenseTrendDto>> GetMonthlyTrendsAsync(int organizationId, int months = 12)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddMonths(-months);

        var claims = await _expenseClaimRepository.GetByDateRangeAsync(startDate, endDate);
        var organizationClaims = claims.Where(c => c.Employee.Branch.OrganizationId == organizationId);

        var monthlyTrends = organizationClaims
            .GroupBy(c => new { c.ExpenseDate.Year, c.ExpenseDate.Month })
            .Select(g => new MonthlyExpenseTrendDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                TotalAmount = g.Sum(c => c.TotalAmount),
                ClaimCount = g.Count(),
                TravelAmount = 0, // Would need to calculate from travel expenses
                MileageAmount = 0, // Would need to calculate from mileage
                AverageClaimAmount = g.Average(c => c.TotalAmount)
            })
            .OrderBy(mt => mt.Year)
            .ThenBy(mt => mt.Month);

        return monthlyTrends;
    }

    public async Task<IEnumerable<TravelExpenseAnalyticsDto>> GetTravelAnalyticsAsync(int organizationId, DateTime startDate, DateTime endDate)
    {
        // This would need to be implemented with proper organization filtering
        var travelExpenses = await _travelExpenseRepository.GetByEmployeeIdAsync(0, startDate, endDate);

        var travelAnalytics = travelExpenses
            .GroupBy(te => te.TravelMode)
            .Select(g => new TravelExpenseAnalyticsDto
            {
                TravelMode = g.Key,
                TravelModeText = g.Key.ToString(),
                TotalAmount = g.SelectMany(te => te.TravelItems).Sum(ti => ti.Amount),
                TripCount = g.Count(),
                AverageTripCost = g.SelectMany(te => te.TravelItems).Any() ? g.SelectMany(te => te.TravelItems).Average(ti => ti.Amount) : 0,
                TotalMileage = g.Where(te => te.MileageDistance.HasValue).Sum(te => te.MileageDistance ?? 0),
                AverageMileageRate = g.Where(te => te.MileageRate.HasValue).Any() ? g.Where(te => te.MileageRate.HasValue).Average(te => te.MileageRate ?? 0) : 0,
                PopularRoutes = new List<string>()
            });

        return travelAnalytics;
    }

    // Budget Tracking Implementation
    public async Task<ExpenseBudgetTrackingDto> GetBudgetTrackingAsync(int organizationId, int? departmentId = null, int? employeeId = null)
    {
        var budget = await _expenseBudgetRepository.GetActiveBudgetAsync(organizationId, departmentId, employeeId, null, DateTime.Today);
        if (budget == null)
        {
            return new ExpenseBudgetTrackingDto
            {
                OrganizationId = organizationId,
                DepartmentId = departmentId,
                EmployeeId = employeeId,
                Period = ExpenseAnalyticsPeriod.Monthly,
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,
                BudgetLimit = 0,
                ActualExpenses = 0,
                RemainingBudget = 0,
                BudgetUtilization = 0,
                IsOverBudget = false,
                ProjectedExpenses = 0
            };
        }

        var actualExpenses = await _expenseBudgetRepository.GetBudgetUtilizationAsync(budget.Id);
        var remainingBudget = budget.BudgetLimit - actualExpenses;
        var utilizationPercentage = budget.BudgetLimit > 0 ? (actualExpenses / budget.BudgetLimit) * 100 : 0;

        return new ExpenseBudgetTrackingDto
        {
            OrganizationId = organizationId,
            DepartmentId = departmentId,
            EmployeeId = employeeId,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            BudgetLimit = budget.BudgetLimit,
            ActualExpenses = actualExpenses,
            RemainingBudget = remainingBudget,
            BudgetUtilization = utilizationPercentage,
            IsOverBudget = actualExpenses > budget.BudgetLimit,
            ProjectedExpenses = actualExpenses * 1.1m // Simple projection
        };
    }

    public async Task<IEnumerable<ExpenseBudgetTrackingDto>> GetBudgetTrackingByPeriodAsync(int organizationId, ExpenseAnalyticsPeriod period)
    {
        var budgets = await _expenseBudgetRepository.GetByOrganizationIdAsync(organizationId);
        var periodBudgets = budgets.Where(b => b.Period == period && b.IsActive);

        var trackingList = new List<ExpenseBudgetTrackingDto>();

        foreach (var budget in periodBudgets)
        {
            var tracking = await GetBudgetTrackingAsync(organizationId, null, budget.EmployeeId);
            trackingList.Add(tracking);
        }

        return trackingList;
    }

    public async Task<bool> CheckBudgetComplianceAsync(int expenseClaimId)
    {
        var expenseClaim = await _expenseClaimRepository.GetByIdWithDetailsAsync(expenseClaimId);
        if (expenseClaim == null)
            return true; // No claim, no violation

        var budget = await _expenseBudgetRepository.GetActiveBudgetAsync(
            expenseClaim.Employee.Branch.OrganizationId, 
            null, 
            expenseClaim.EmployeeId, 
            null, 
            expenseClaim.ExpenseDate);

        if (budget == null)
            return true; // No budget, no violation

        return !await _expenseBudgetRepository.IsBudgetExceededAsync(budget.Id, expenseClaim.TotalAmount);
    }

    public async Task<IEnumerable<ExpenseBudgetAlert>> GetBudgetAlertsAsync(int organizationId, bool unresolved = true)
    {
        var budgets = await _expenseBudgetRepository.GetByOrganizationIdAsync(organizationId);
        var alerts = budgets.SelectMany(b => b.BudgetAlerts);

        if (unresolved)
        {
            alerts = alerts.Where(a => !a.IsResolved);
        }

        return alerts.OrderByDescending(a => a.AlertDate);
    }

    // Policy Compliance Implementation
    public async Task<ExpenseComplianceReportDto> GetComplianceReportAsync(int organizationId, DateTime startDate, DateTime endDate)
    {
        var violations = await _complianceViolationRepository.GetByEmployeeIdAsync(0, startDate, endDate); // Would need organization filtering
        var totalClaims = await _expenseClaimRepository.GetByDateRangeAsync(startDate, endDate);
        var orgClaims = totalClaims.Where(c => c.Employee.Branch.OrganizationId == organizationId);

        var totalClaimsCount = orgClaims.Count();
        var violatingClaims = violations.Select(v => v.ExpenseClaimId).Distinct().Count();
        var compliantClaims = totalClaimsCount - violatingClaims;

        return new ExpenseComplianceReportDto
        {
            ReportDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            TotalClaims = totalClaimsCount,
            CompliantClaims = compliantClaims,
            NonCompliantClaims = violatingClaims,
            ComplianceRate = totalClaimsCount > 0 ? (decimal)compliantClaims / totalClaimsCount * 100 : 100,
            Violations = _mapper.Map<List<ExpenseComplianceViolationDto>>(violations),
            PolicyCompliance = new List<ExpensePolicyComplianceDto>()
        };
    }

    public async Task<IEnumerable<ExpenseComplianceViolationDto>> GetComplianceViolationsAsync(int organizationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var violations = await _complianceViolationRepository.GetByEmployeeIdAsync(0, startDate, endDate); // Would need organization filtering
        return _mapper.Map<IEnumerable<ExpenseComplianceViolationDto>>(violations);
    }

    public async Task<bool> ValidateExpenseComplianceAsync(int expenseClaimId)
    {
        var violations = await _complianceViolationRepository.GetByExpenseClaimIdAsync(expenseClaimId);
        return !violations.Any(v => !v.IsResolved && !v.IsWaived);
    }

    public async Task<bool> ResolveComplianceViolationAsync(int violationId, int resolvedBy, string resolutionNotes)
    {
        var violation = await _complianceViolationRepository.GetByIdAsync(violationId);
        if (violation == null)
            return false;

        violation.IsResolved = true;
        violation.ResolvedDate = DateTime.UtcNow;
        violation.ResolvedBy = resolvedBy;
        violation.ResolutionNotes = resolutionNotes;

        await _complianceViolationRepository.UpdateAsync(violation);
        return await _complianceViolationRepository.SaveChangesAsync();
    }

    public async Task<bool> WaiveComplianceViolationAsync(int violationId, int waivedBy, string waiverReason)
    {
        var violation = await _complianceViolationRepository.GetByIdAsync(violationId);
        if (violation == null)
            return false;

        violation.IsWaived = true;
        violation.WaivedDate = DateTime.UtcNow;
        violation.WaivedBy = waivedBy;
        violation.WaiverReason = waiverReason;

        await _complianceViolationRepository.UpdateAsync(violation);
        return await _complianceViolationRepository.SaveChangesAsync();
    }

    // Advanced Reporting Implementation
    public async Task<IEnumerable<ExpenseClaimDto>> GetAdvancedExpenseReportAsync(ExpenseReportFilterDto filter)
    {
        var claims = await _expenseClaimRepository.GetByDateRangeAsync(
            filter.StartDate ?? DateTime.Today.AddMonths(-1), 
            filter.EndDate ?? DateTime.Today);

        // Apply filters
        if (filter.EmployeeId.HasValue)
        {
            claims = claims.Where(c => c.EmployeeId == filter.EmployeeId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            claims = claims.Where(c => c.ExpenseItems.Any(ei => ei.ExpenseCategoryId == filter.CategoryId.Value));
        }

        if (filter.Status.HasValue)
        {
            claims = claims.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            claims = claims.Where(c => c.TotalAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            claims = claims.Where(c => c.TotalAmount <= filter.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            claims = claims.Where(c => c.Title.Contains(filter.SearchTerm) || 
                                     c.Description.Contains(filter.SearchTerm));
        }

        // Apply sorting
        claims = filter.SortBy.ToLower() switch
        {
            "amount" => filter.SortDescending ? claims.OrderByDescending(c => c.TotalAmount) : claims.OrderBy(c => c.TotalAmount),
            "employee" => filter.SortDescending ? claims.OrderByDescending(c => c.Employee.FirstName) : claims.OrderBy(c => c.Employee.FirstName),
            "status" => filter.SortDescending ? claims.OrderByDescending(c => c.Status) : claims.OrderBy(c => c.Status),
            _ => filter.SortDescending ? claims.OrderByDescending(c => c.ExpenseDate) : claims.OrderBy(c => c.ExpenseDate)
        };

        // Apply pagination
        var pagedClaims = claims.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

        return _mapper.Map<IEnumerable<ExpenseClaimDto>>(pagedClaims);
    }

    public async Task<byte[]> ExportExpenseReportAsync(ExpenseReportFilterDto filter, string format = "xlsx")
    {
        var claims = await GetAdvancedExpenseReportAsync(filter);
        
        // This would implement actual export functionality
        // For now, return empty byte array
        return Array.Empty<byte>();
    }

    public async Task<Dictionary<string, object>> GetExpenseDashboardDataAsync(int organizationId, int? employeeId = null)
    {
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        var analytics = await GetExpenseAnalyticsAsync(organizationId, ExpenseAnalyticsPeriod.Monthly, startDate, endDate);
        var budgetTracking = await GetBudgetTrackingAsync(organizationId, null, employeeId);
        var complianceReport = await GetComplianceReportAsync(organizationId, startDate, endDate);

        return new Dictionary<string, object>
        {
            ["analytics"] = analytics,
            ["budgetTracking"] = budgetTracking,
            ["complianceReport"] = complianceReport,
            ["lastUpdated"] = DateTime.UtcNow
        };
    }

    // Helper Methods
    private static (DateTime start, DateTime end) GetPeriodDates(ExpenseAnalyticsPeriod period, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
            return (startDate.Value, endDate.Value);

        var today = DateTime.Today;
        return period switch
        {
            ExpenseAnalyticsPeriod.Daily => (today, today),
            ExpenseAnalyticsPeriod.Weekly => (today.AddDays(-7), today),
            ExpenseAnalyticsPeriod.Monthly => (today.AddMonths(-1), today),
            ExpenseAnalyticsPeriod.Quarterly => (today.AddMonths(-3), today),
            ExpenseAnalyticsPeriod.Yearly => (today.AddYears(-1), today),
            _ => (today.AddMonths(-1), today)
        };
    }

    private static bool GetExpenseTypeReceiptRequirement(TravelExpenseType expenseType)
    {
        return expenseType switch
        {
            TravelExpenseType.Accommodation => true,
            TravelExpenseType.FlightTicket => true,
            TravelExpenseType.TrainTicket => true,
            TravelExpenseType.CarRental => true,
            TravelExpenseType.Fuel => true,
            TravelExpenseType.Meals => false,
            TravelExpenseType.Mileage => false,
            _ => true
        };
    }
}