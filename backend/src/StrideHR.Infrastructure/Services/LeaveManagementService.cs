using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Leave;

namespace StrideHR.Infrastructure.Services;

public class LeaveManagementService : ILeaveManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LeaveManagementService> _logger;

    public LeaveManagementService(
        IUnitOfWork unitOfWork,
        ILogger<LeaveManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Leave Request Management

    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(int employeeId, CreateLeaveRequestDto request)
    {
        try
        {
            // Validate employee exists
            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found", nameof(employeeId));

            // Validate leave policy
            var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(request.LeavePolicyId);
            if (leavePolicy == null)
                throw new ArgumentException("Leave policy not found", nameof(request.LeavePolicyId));

            // Validate dates
            if (request.StartDate > request.EndDate)
                throw new ArgumentException("Start date cannot be after end date");

            if (request.StartDate < DateTime.Today && !request.IsEmergency)
                throw new ArgumentException("Cannot request leave for past dates unless it's an emergency");

            // Check advance notice requirement
            var daysDifference = (request.StartDate - DateTime.Today).Days;
            if (daysDifference < leavePolicy.MinAdvanceNoticeDays && !request.IsEmergency)
                throw new ArgumentException($"Leave request requires at least {leavePolicy.MinAdvanceNoticeDays} days advance notice");

            // Calculate requested days
            var requestedDays = await CalculateLeaveDaysAsync(request.StartDate, request.EndDate, employee.BranchId);

            // Validate maximum consecutive days
            if (requestedDays > leavePolicy.MaxConsecutiveDays)
                throw new ArgumentException($"Cannot request more than {leavePolicy.MaxConsecutiveDays} consecutive days for this leave type");

            // Check for overlapping requests
            var hasOverlapping = await _unitOfWork.LeaveRequests.HasOverlappingRequestsAsync(
                employeeId, request.StartDate, request.EndDate);
            if (hasOverlapping)
                throw new ArgumentException("You already have a leave request for the selected dates");

            // Validate leave balance
            var currentYear = DateTime.Now.Year;
            var isBalanceValid = await ValidateLeaveBalanceAsync(employeeId, request.LeavePolicyId, requestedDays, currentYear);
            if (!isBalanceValid)
                throw new ArgumentException("Insufficient leave balance");

            // Create leave request
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeavePolicyId = request.LeavePolicyId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedDays = requestedDays,
                ApprovedDays = 0,
                Reason = request.Reason,
                Comments = request.Comments,
                Status = LeaveStatus.Pending,
                IsEmergency = request.IsEmergency,
                AttachmentPath = request.AttachmentPath,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = employeeId.ToString()
            };

            await _unitOfWork.LeaveRequests.AddAsync(leaveRequest);

            // Create initial approval history entry
            var approvalHistory = new LeaveApprovalHistory
            {
                LeaveRequestId = leaveRequest.Id,
                ApproverId = employee.ReportingManagerId ?? employeeId, // If no manager, assign to self (will be handled by HR)
                Level = ApprovalLevel.Manager,
                Action = ApprovalAction.Pending,
                ActionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = employeeId.ToString()
            };

            await _unitOfWork.LeaveApprovalHistory.AddAsync(approvalHistory);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Leave request created successfully for employee {EmployeeId}", employeeId);

            return await MapToLeaveRequestDto(leaveRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<LeaveRequestDto> GetLeaveRequestAsync(int id)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(id);
        if (leaveRequest == null)
            throw new ArgumentException("Leave request not found", nameof(id));

        return await MapToLeaveRequestDto(leaveRequest);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetEmployeeLeaveRequestsAsync(int employeeId)
    {
        var leaveRequests = await _unitOfWork.LeaveRequests.GetByEmployeeIdAsync(employeeId);
        var result = new List<LeaveRequestDto>();

        foreach (var request in leaveRequests)
        {
            result.Add(await MapToLeaveRequestDto(request));
        }

        return result;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync(int branchId)
    {
        var leaveRequests = await _unitOfWork.LeaveRequests.GetPendingRequestsAsync(branchId);
        var result = new List<LeaveRequestDto>();

        foreach (var request in leaveRequests)
        {
            result.Add(await MapToLeaveRequestDto(request));
        }

        return result;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetRequestsForApprovalAsync(int approverId)
    {
        var leaveRequests = await _unitOfWork.LeaveRequests.GetRequestsForApprovalAsync(approverId);
        var result = new List<LeaveRequestDto>();

        foreach (var request in leaveRequests)
        {
            result.Add(await MapToLeaveRequestDto(request));
        }

        return result;
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestAsync(int id, CreateLeaveRequestDto request)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(id);
        if (leaveRequest == null)
            throw new ArgumentException("Leave request not found", nameof(id));

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Cannot update a leave request that is not pending");

        // Validate the update similar to create
        var requestedDays = await CalculateLeaveDaysAsync(request.StartDate, request.EndDate, leaveRequest.Employee.BranchId);
        
        var hasOverlapping = await _unitOfWork.LeaveRequests.HasOverlappingRequestsAsync(
            leaveRequest.EmployeeId, request.StartDate, request.EndDate, id);
        if (hasOverlapping)
            throw new ArgumentException("You already have a leave request for the selected dates");

        var currentYear = DateTime.Now.Year;
        var isBalanceValid = await ValidateLeaveBalanceAsync(leaveRequest.EmployeeId, request.LeavePolicyId, requestedDays, currentYear);
        if (!isBalanceValid)
            throw new ArgumentException("Insufficient leave balance");

        // Update the request
        leaveRequest.LeavePolicyId = request.LeavePolicyId;
        leaveRequest.StartDate = request.StartDate;
        leaveRequest.EndDate = request.EndDate;
        leaveRequest.RequestedDays = requestedDays;
        leaveRequest.Reason = request.Reason;
        leaveRequest.Comments = request.Comments;
        leaveRequest.IsEmergency = request.IsEmergency;
        leaveRequest.AttachmentPath = request.AttachmentPath;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return await MapToLeaveRequestDto(leaveRequest);
    }

    public async Task<bool> CancelLeaveRequestAsync(int id, int employeeId)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(id);
        if (leaveRequest == null)
            return false;

        if (leaveRequest.EmployeeId != employeeId)
            throw new UnauthorizedAccessException("You can only cancel your own leave requests");

        if (leaveRequest.Status == LeaveStatus.Cancelled)
            return true;

        leaveRequest.Status = LeaveStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedBy = employeeId.ToString();

        await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Leave Approval Workflow

    public async Task<LeaveRequestDto> ApproveLeaveRequestAsync(int requestId, LeaveApprovalDto approval, int approverId)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(requestId);
        if (leaveRequest == null)
            throw new ArgumentException("Leave request not found", nameof(requestId));

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Leave request is not pending approval");

        // Validate approver has permission
        var canApprove = leaveRequest.Employee.ReportingManagerId == approverId ||
                        leaveRequest.ApprovalHistory.Any(ah => ah.ApproverId == approverId && ah.Action == ApprovalAction.Pending);
        
        if (!canApprove)
            throw new UnauthorizedAccessException("You don't have permission to approve this leave request");

        var approvedDays = approval.ApprovedDays ?? leaveRequest.RequestedDays;

        // Update leave request
        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedDays = approvedDays;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApprovedBy = approverId;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedBy = approverId.ToString();

        // Add approval history
        var approvalHistory = new LeaveApprovalHistory
        {
            LeaveRequestId = requestId,
            ApproverId = approverId,
            Level = ApprovalLevel.Manager, // This could be dynamic based on approver role
            Action = ApprovalAction.Approved,
            Comments = approval.Comments,
            ActionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = approverId.ToString()
        };

        await _unitOfWork.LeaveApprovalHistory.AddAsync(approvalHistory);

        // Update leave balance
        await UpdateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeavePolicyId, approvedDays, DateTime.Now.Year);

        // Create calendar entries
        await CreateLeaveCalendarEntriesAsync(leaveRequest);

        await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} approved by {ApproverId}", requestId, approverId);

        return await MapToLeaveRequestDto(leaveRequest);
    }

    public async Task<LeaveRequestDto> RejectLeaveRequestAsync(int requestId, LeaveApprovalDto rejection, int approverId)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(requestId);
        if (leaveRequest == null)
            throw new ArgumentException("Leave request not found", nameof(requestId));

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Leave request is not pending approval");

        // Validate approver has permission
        var canApprove = leaveRequest.Employee.ReportingManagerId == approverId ||
                        leaveRequest.ApprovalHistory.Any(ah => ah.ApproverId == approverId && ah.Action == ApprovalAction.Pending);
        
        if (!canApprove)
            throw new UnauthorizedAccessException("You don't have permission to reject this leave request");

        // Update leave request
        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.RejectionReason = rejection.Comments;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedBy = approverId.ToString();

        // Add approval history
        var approvalHistory = new LeaveApprovalHistory
        {
            LeaveRequestId = requestId,
            ApproverId = approverId,
            Level = ApprovalLevel.Manager,
            Action = ApprovalAction.Rejected,
            Comments = rejection.Comments,
            ActionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = approverId.ToString()
        };

        await _unitOfWork.LeaveApprovalHistory.AddAsync(approvalHistory);
        await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} rejected by {ApproverId}", requestId, approverId);

        return await MapToLeaveRequestDto(leaveRequest);
    }

    public async Task<LeaveRequestDto> EscalateLeaveRequestAsync(int requestId, int escalateToId, int approverId, string? comments = null)
    {
        var leaveRequest = await _unitOfWork.LeaveRequests.GetWithDetailsAsync(requestId);
        if (leaveRequest == null)
            throw new ArgumentException("Leave request not found", nameof(requestId));

        // Add escalation history
        var approvalHistory = new LeaveApprovalHistory
        {
            LeaveRequestId = requestId,
            ApproverId = approverId,
            Level = ApprovalLevel.Manager,
            Action = ApprovalAction.Escalated,
            Comments = comments,
            EscalatedToId = escalateToId,
            ActionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = approverId.ToString()
        };

        await _unitOfWork.LeaveApprovalHistory.AddAsync(approvalHistory);

        // Create new pending approval for escalated person
        var newApprovalHistory = new LeaveApprovalHistory
        {
            LeaveRequestId = requestId,
            ApproverId = escalateToId,
            Level = ApprovalLevel.HR, // Escalated requests go to HR level
            Action = ApprovalAction.Pending,
            ActionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = approverId.ToString()
        };

        await _unitOfWork.LeaveApprovalHistory.AddAsync(newApprovalHistory);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} escalated from {ApproverId} to {EscalateToId}", requestId, approverId, escalateToId);

        return await MapToLeaveRequestDto(leaveRequest);
    }

    #endregion

    #region Leave Balance Management

    public async Task<IEnumerable<LeaveBalanceDto>> GetEmployeeLeaveBalancesAsync(int employeeId)
    {
        var leaveBalances = await _unitOfWork.LeaveBalances.GetByEmployeeIdAsync(employeeId);
        var result = new List<LeaveBalanceDto>();

        foreach (var balance in leaveBalances)
        {
            result.Add(new LeaveBalanceDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                LeavePolicyId = balance.LeavePolicyId,
                LeaveType = balance.LeavePolicy.LeaveType,
                LeaveTypeName = balance.LeavePolicy.Name,
                Year = balance.Year,
                AllocatedDays = balance.AllocatedDays,
                UsedDays = balance.UsedDays,
                CarriedForwardDays = balance.CarriedForwardDays,
                EncashedDays = balance.EncashedDays,
                RemainingDays = balance.RemainingDays
            });
        }

        return result;
    }

    public async Task<LeaveBalanceDto> GetLeaveBalanceAsync(int employeeId, int leavePolicyId, int year)
    {
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(employeeId, leavePolicyId, year);
        if (balance == null)
        {
            // Create initial balance if it doesn't exist
            var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(leavePolicyId);
            if (leavePolicy == null)
                throw new ArgumentException("Leave policy not found");

            balance = new LeaveBalance
            {
                EmployeeId = employeeId,
                LeavePolicyId = leavePolicyId,
                Year = year,
                AllocatedDays = leavePolicy.AnnualAllocation,
                UsedDays = 0,
                CarriedForwardDays = 0,
                EncashedDays = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveBalances.AddAsync(balance);
            await _unitOfWork.SaveChangesAsync();
        }

        return new LeaveBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            LeavePolicyId = balance.LeavePolicyId,
            LeaveType = balance.LeavePolicy.LeaveType,
            LeaveTypeName = balance.LeavePolicy.Name,
            Year = balance.Year,
            AllocatedDays = balance.AllocatedDays,
            UsedDays = balance.UsedDays,
            CarriedForwardDays = balance.CarriedForwardDays,
            EncashedDays = balance.EncashedDays,
            RemainingDays = balance.RemainingDays
        };
    }

    public async Task<bool> ValidateLeaveBalanceAsync(int employeeId, int leavePolicyId, decimal requestedDays, int year)
    {
        var remainingBalance = await _unitOfWork.LeaveBalances.GetRemainingBalanceAsync(employeeId, leavePolicyId, year);
        
        // If no balance exists, create it
        if (remainingBalance == 0)
        {
            var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(leavePolicyId);
            if (leavePolicy != null)
            {
                remainingBalance = leavePolicy.AnnualAllocation;
            }
        }

        return remainingBalance >= requestedDays;
    }

    #endregion

    #region Leave Policy Management

    public async Task<IEnumerable<LeavePolicyDto>> GetBranchLeavePoliciesAsync(int branchId)
    {
        var policies = await _unitOfWork.LeavePolicies.GetActiveByBranchIdAsync(branchId);
        var result = new List<LeavePolicyDto>();

        foreach (var policy in policies)
        {
            result.Add(new LeavePolicyDto
            {
                Id = policy.Id,
                BranchId = policy.BranchId,
                LeaveType = policy.LeaveType,
                Name = policy.Name,
                Description = policy.Description,
                AnnualAllocation = policy.AnnualAllocation,
                MaxConsecutiveDays = policy.MaxConsecutiveDays,
                MinAdvanceNoticeDays = policy.MinAdvanceNoticeDays,
                RequiresApproval = policy.RequiresApproval,
                IsCarryForwardAllowed = policy.IsCarryForwardAllowed,
                MaxCarryForwardDays = policy.MaxCarryForwardDays,
                IsEncashmentAllowed = policy.IsEncashmentAllowed,
                EncashmentRate = policy.EncashmentRate,
                IsActive = policy.IsActive
            });
        }

        return result;
    }

    public async Task<LeavePolicyDto> GetLeavePolicyAsync(int id)
    {
        var policy = await _unitOfWork.LeavePolicies.GetByIdAsync(id);
        if (policy == null)
            throw new ArgumentException("Leave policy not found", nameof(id));

        return new LeavePolicyDto
        {
            Id = policy.Id,
            BranchId = policy.BranchId,
            LeaveType = policy.LeaveType,
            Name = policy.Name,
            Description = policy.Description,
            AnnualAllocation = policy.AnnualAllocation,
            MaxConsecutiveDays = policy.MaxConsecutiveDays,
            MinAdvanceNoticeDays = policy.MinAdvanceNoticeDays,
            RequiresApproval = policy.RequiresApproval,
            IsCarryForwardAllowed = policy.IsCarryForwardAllowed,
            MaxCarryForwardDays = policy.MaxCarryForwardDays,
            IsEncashmentAllowed = policy.IsEncashmentAllowed,
            EncashmentRate = policy.EncashmentRate,
            IsActive = policy.IsActive
        };
    }

    #endregion

    #region Leave Calendar and Conflict Detection

    public async Task<IEnumerable<LeaveCalendarDto>> GetLeaveCalendarAsync(DateTime startDate, DateTime endDate, int branchId)
    {
        var calendarEntries = await _unitOfWork.LeaveCalendar.GetByDateRangeAsync(startDate, endDate, branchId);
        var result = new List<LeaveCalendarDto>();

        foreach (var entry in calendarEntries)
        {
            result.Add(new LeaveCalendarDto
            {
                Id = entry.Id,
                EmployeeId = entry.EmployeeId,
                EmployeeName = entry.Employee.FullName,
                LeaveRequestId = entry.LeaveRequestId,
                Date = entry.Date,
                IsFullDay = entry.IsFullDay,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                LeaveType = entry.LeaveType,
                LeaveTypeName = entry.LeaveRequest.LeavePolicy.Name,
                Status = entry.LeaveRequest.Status
            });
        }

        return result;
    }

    public async Task<IEnumerable<LeaveCalendarDto>> GetEmployeeLeaveCalendarAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var calendarEntries = await _unitOfWork.LeaveCalendar.GetByEmployeeAndDateRangeAsync(employeeId, startDate, endDate);
        var result = new List<LeaveCalendarDto>();

        foreach (var entry in calendarEntries)
        {
            result.Add(new LeaveCalendarDto
            {
                Id = entry.Id,
                EmployeeId = entry.EmployeeId,
                EmployeeName = entry.Employee.FullName,
                LeaveRequestId = entry.LeaveRequestId,
                Date = entry.Date,
                IsFullDay = entry.IsFullDay,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                LeaveType = entry.LeaveType,
                LeaveTypeName = entry.LeaveRequest.LeavePolicy.Name,
                Status = entry.LeaveRequest.Status
            });
        }

        return result;
    }

    public async Task<IEnumerable<LeaveCalendarDto>> GetTeamLeaveCalendarAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        var calendarEntries = await _unitOfWork.LeaveCalendar.GetTeamCalendarAsync(managerId, startDate, endDate);
        var result = new List<LeaveCalendarDto>();

        foreach (var entry in calendarEntries)
        {
            result.Add(new LeaveCalendarDto
            {
                Id = entry.Id,
                EmployeeId = entry.EmployeeId,
                EmployeeName = entry.Employee.FullName,
                LeaveRequestId = entry.LeaveRequestId,
                Date = entry.Date,
                IsFullDay = entry.IsFullDay,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                LeaveType = entry.LeaveType,
                LeaveTypeName = entry.LeaveRequest.LeavePolicy.Name,
                Status = entry.LeaveRequest.Status
            });
        }

        return result;
    }

    public async Task<IEnumerable<LeaveConflictDto>> DetectLeaveConflictsAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            return new List<LeaveConflictDto>();

        // Get team members (same department or reporting to same manager)
        var teamMembers = await _unitOfWork.Employees.FindAsync(e => 
            e.BranchId == employee.BranchId && 
            (e.Department == employee.Department || e.ReportingManagerId == employee.ReportingManagerId) &&
            e.Id != employeeId);

        var conflicts = new List<LeaveConflictDto>();

        foreach (var teamMember in teamMembers)
        {
            var conflictingRequests = await _unitOfWork.LeaveRequests.GetConflictingRequestsAsync(
                teamMember.Id, startDate, endDate);

            foreach (var request in conflictingRequests.Where(r => r.Status == LeaveStatus.Approved))
            {
                // Check each day for conflicts
                for (var date = Math.Max(startDate.Ticks, request.StartDate.Ticks); 
                     date <= Math.Min(endDate.Ticks, request.EndDate.Ticks); 
                     date += TimeSpan.TicksPerDay)
                {
                    conflicts.Add(new LeaveConflictDto
                    {
                        EmployeeId = teamMember.Id,
                        EmployeeName = teamMember.FullName,
                        Department = teamMember.Department,
                        ConflictDate = new DateTime(date),
                        ConflictReason = $"Already on {request.LeavePolicy.Name}",
                        ConflictingRequestId = request.Id
                    });
                }
            }
        }

        return conflicts;
    }

    public async Task<IEnumerable<LeaveConflictDto>> GetTeamLeaveConflictsAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        var teamRequests = await _unitOfWork.LeaveRequests.GetTeamRequestsAsync(managerId, startDate, endDate);
        var conflicts = new List<LeaveConflictDto>();

        // Group by date to find conflicts
        var requestsByDate = new Dictionary<DateTime, List<LeaveRequest>>();

        foreach (var request in teamRequests)
        {
            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
            {
                if (!requestsByDate.ContainsKey(date))
                    requestsByDate[date] = new List<LeaveRequest>();
                
                requestsByDate[date].Add(request);
            }
        }

        // Find dates with multiple people on leave
        foreach (var dateGroup in requestsByDate.Where(kvp => kvp.Value.Count > 1))
        {
            foreach (var request in dateGroup.Value)
            {
                conflicts.Add(new LeaveConflictDto
                {
                    EmployeeId = request.EmployeeId,
                    EmployeeName = request.Employee.FullName,
                    Department = request.Employee.Department,
                    ConflictDate = dateGroup.Key,
                    ConflictReason = $"Multiple team members on leave ({dateGroup.Value.Count} people)",
                    ConflictingRequestId = request.Id
                });
            }
        }

        return conflicts;
    }

    #endregion

    #region Utility Methods

    public async Task<decimal> CalculateLeaveDaysAsync(DateTime startDate, DateTime endDate, int branchId)
    {
        decimal totalDays = 0;
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (await IsWorkingDayAsync(date, branchId))
            {
                totalDays++;
            }
        }

        return totalDays;
    }

    public async Task<bool> IsWorkingDayAsync(DateTime date, int branchId)
    {
        // Skip weekends (Saturday and Sunday)
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Check if it's a holiday
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
        if (branch != null)
        {
            // Parse local holidays from JSON and check if the date is a holiday
            // This is a simplified implementation - in reality, you'd parse the JSON
            // and check against the holiday dates
        }

        return true;
    }

    #endregion

    #region Private Helper Methods

    private async Task<LeaveRequestDto> MapToLeaveRequestDto(LeaveRequest leaveRequest)
    {
        var approvalHistory = await _unitOfWork.LeaveApprovalHistory.GetByLeaveRequestIdAsync(leaveRequest.Id);
        
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = leaveRequest.Employee?.FullName ?? "",
            LeavePolicyId = leaveRequest.LeavePolicyId,
            LeaveType = leaveRequest.LeavePolicy?.LeaveType ?? LeaveType.Annual,
            LeaveTypeName = leaveRequest.LeavePolicy?.Name ?? "",
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            RequestedDays = leaveRequest.RequestedDays,
            ApprovedDays = leaveRequest.ApprovedDays,
            Reason = leaveRequest.Reason,
            Comments = leaveRequest.Comments,
            Status = leaveRequest.Status,
            ApprovedAt = leaveRequest.ApprovedAt,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedByName = leaveRequest.ApprovedByEmployee?.FullName,
            RejectionReason = leaveRequest.RejectionReason,
            IsEmergency = leaveRequest.IsEmergency,
            AttachmentPath = leaveRequest.AttachmentPath,
            CreatedAt = leaveRequest.CreatedAt,
            ApprovalHistory = approvalHistory.Select(ah => new LeaveApprovalHistoryDto
            {
                Id = ah.Id,
                LeaveRequestId = ah.LeaveRequestId,
                ApproverId = ah.ApproverId,
                ApproverName = ah.Approver?.FullName ?? "",
                Level = ah.Level,
                Action = ah.Action,
                Comments = ah.Comments,
                ActionDate = ah.ActionDate,
                EscalatedToId = ah.EscalatedToId,
                EscalatedToName = ah.EscalatedTo?.FullName
            }).ToList()
        };
    }

    private async Task UpdateLeaveBalanceAsync(int employeeId, int leavePolicyId, decimal usedDays, int year)
    {
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(employeeId, leavePolicyId, year);
        
        if (balance == null)
        {
            var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(leavePolicyId);
            if (leavePolicy != null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeavePolicyId = leavePolicyId,
                    Year = year,
                    AllocatedDays = leavePolicy.AnnualAllocation,
                    UsedDays = usedDays,
                    CarriedForwardDays = 0,
                    EncashedDays = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.LeaveBalances.AddAsync(balance);
            }
        }
        else
        {
            balance.UsedDays += usedDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveBalances.UpdateAsync(balance);
        }
    }

    private async Task CreateLeaveCalendarEntriesAsync(LeaveRequest leaveRequest)
    {
        var calendarEntries = new List<LeaveCalendar>();

        for (var date = leaveRequest.StartDate; date <= leaveRequest.EndDate; date = date.AddDays(1))
        {
            if (await IsWorkingDayAsync(date, leaveRequest.Employee.BranchId))
            {
                calendarEntries.Add(new LeaveCalendar
                {
                    EmployeeId = leaveRequest.EmployeeId,
                    LeaveRequestId = leaveRequest.Id,
                    Date = date,
                    IsFullDay = true,
                    LeaveType = leaveRequest.LeavePolicy.LeaveType,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (calendarEntries.Any())
        {
            await _unitOfWork.LeaveCalendar.AddRangeAsync(calendarEntries);
        }
    }

    #endregion
}