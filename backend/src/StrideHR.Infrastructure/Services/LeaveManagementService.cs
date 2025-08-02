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

    #region Leave Balance Tracking and Calculation

    public async Task<LeaveBalanceDto> UpdateEmployeeLeaveBalanceAsync(int employeeId, int leavePolicyId, decimal usedDays, int year)
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
                UsedDays = usedDays,
                CarriedForwardDays = 0,
                EncashedDays = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveBalances.AddAsync(balance);
        }
        else
        {
            balance.UsedDays += usedDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveBalances.UpdateAsync(balance);
        }

        await _unitOfWork.SaveChangesAsync();

        return new LeaveBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            LeavePolicyId = balance.LeavePolicyId,
            LeaveType = balance.LeavePolicy?.LeaveType ?? LeaveType.Annual,
            LeaveTypeName = balance.LeavePolicy?.Name ?? "",
            Year = balance.Year,
            AllocatedDays = balance.AllocatedDays,
            UsedDays = balance.UsedDays,
            CarriedForwardDays = balance.CarriedForwardDays,
            EncashedDays = balance.EncashedDays,
            RemainingDays = balance.RemainingDays
        };
    }

    public async Task<IEnumerable<LeaveBalanceDto>> RecalculateLeaveBalancesAsync(int employeeId, int year)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var leavePolicies = await _unitOfWork.LeavePolicies.GetActiveByBranchIdAsync(employee.BranchId);
        var result = new List<LeaveBalanceDto>();

        foreach (var policy in leavePolicies)
        {
            // Get or create balance
            var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(employeeId, policy.Id, year);
            if (balance == null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeavePolicyId = policy.Id,
                    Year = year,
                    AllocatedDays = policy.AnnualAllocation,
                    UsedDays = 0,
                    CarriedForwardDays = 0,
                    EncashedDays = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LeaveBalances.AddAsync(balance);
            }

            // Recalculate accrued days
            var totalAccrued = await _unitOfWork.LeaveAccruals.GetTotalAccruedDaysAsync(employeeId, policy.Id, year);
            balance.AllocatedDays = policy.AnnualAllocation + totalAccrued;

            // Recalculate used days from approved leave requests
            var approvedRequests = await _unitOfWork.LeaveRequests.GetApprovedRequestsByEmployeeAndPolicyAsync(employeeId, policy.Id, year);
            balance.UsedDays = approvedRequests.Sum(r => r.ApprovedDays);

            // Recalculate encashed days
            balance.EncashedDays = await _unitOfWork.LeaveEncashments.GetTotalEncashedDaysAsync(employeeId, policy.Id, year);

            balance.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveBalances.UpdateAsync(balance);

            result.Add(new LeaveBalanceDto
            {
                Id = balance.Id,
                EmployeeId = balance.EmployeeId,
                LeavePolicyId = balance.LeavePolicyId,
                LeaveType = policy.LeaveType,
                LeaveTypeName = policy.Name,
                Year = balance.Year,
                AllocatedDays = balance.AllocatedDays,
                UsedDays = balance.UsedDays,
                CarriedForwardDays = balance.CarriedForwardDays,
                EncashedDays = balance.EncashedDays,
                RemainingDays = balance.RemainingDays
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    #endregion

    #region Leave Accrual Management

    public async Task<IEnumerable<LeaveAccrualDto>> GetEmployeeAccrualsAsync(int employeeId, int year)
    {
        var accruals = await _unitOfWork.LeaveAccruals.GetByEmployeeAndYearAsync(employeeId, year);
        var result = new List<LeaveAccrualDto>();

        foreach (var accrual in accruals)
        {
            result.Add(new LeaveAccrualDto
            {
                Id = accrual.Id,
                EmployeeId = accrual.EmployeeId,
                EmployeeName = accrual.Employee?.FullName ?? "",
                LeavePolicyId = accrual.LeavePolicyId,
                LeaveType = accrual.LeavePolicy?.LeaveType ?? LeaveType.Annual,
                LeaveTypeName = accrual.LeavePolicy?.Name ?? "",
                Year = accrual.Year,
                Month = accrual.Month,
                AccruedDays = accrual.AccruedDays,
                AccrualRate = accrual.AccrualRate,
                AccrualType = accrual.AccrualType,
                AccrualDate = accrual.AccrualDate,
                Notes = accrual.Notes,
                IsProcessed = accrual.IsProcessed
            });
        }

        return result;
    }

    public async Task<LeaveAccrualDto> CreateAccrualAsync(CreateLeaveAccrualDto accrualDto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(accrualDto.EmployeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(accrualDto.LeavePolicyId);
        if (leavePolicy == null)
            throw new ArgumentException("Leave policy not found");

        // Check if accrual already exists for this period
        var existingAccrual = await _unitOfWork.LeaveAccruals.HasAccrualForPeriodAsync(
            accrualDto.EmployeeId, accrualDto.LeavePolicyId, accrualDto.Year, accrualDto.Month);
        
        if (existingAccrual)
            throw new InvalidOperationException("Accrual already exists for this period");

        var accrual = new LeaveAccrual
        {
            EmployeeId = accrualDto.EmployeeId,
            LeavePolicyId = accrualDto.LeavePolicyId,
            Year = accrualDto.Year,
            Month = accrualDto.Month,
            AccruedDays = accrualDto.AccruedDays,
            AccrualRate = accrualDto.AccrualRate,
            AccrualType = accrualDto.AccrualType,
            AccrualDate = accrualDto.AccrualDate,
            Notes = accrualDto.Notes,
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveAccruals.AddAsync(accrual);
        await _unitOfWork.SaveChangesAsync();

        // Update leave balance
        await UpdateLeaveBalanceWithAccrualAsync(accrual);

        return new LeaveAccrualDto
        {
            Id = accrual.Id,
            EmployeeId = accrual.EmployeeId,
            EmployeeName = employee.FullName,
            LeavePolicyId = accrual.LeavePolicyId,
            LeaveType = leavePolicy.LeaveType,
            LeaveTypeName = leavePolicy.Name,
            Year = accrual.Year,
            Month = accrual.Month,
            AccruedDays = accrual.AccruedDays,
            AccrualRate = accrual.AccrualRate,
            AccrualType = accrual.AccrualType,
            AccrualDate = accrual.AccrualDate,
            Notes = accrual.Notes,
            IsProcessed = accrual.IsProcessed
        };
    }

    public async Task<IEnumerable<LeaveAccrualDto>> ProcessMonthlyAccrualsAsync(int year, int month)
    {
        var activeRules = await _unitOfWork.LeaveAccrualRules.GetByFrequencyAsync(AccrualFrequency.Monthly);
        var result = new List<LeaveAccrualDto>();

        foreach (var rule in activeRules)
        {
            var employees = await _unitOfWork.Employees.FindAsync(e => 
                e.Branch.LeavePolicies.Any(lp => lp.Id == rule.LeavePolicyId) &&
                e.JoiningDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month)));

            foreach (var employee in employees)
            {
                // Check if employee meets minimum service requirement
                var serviceMonths = GetServiceMonths(employee.JoiningDate, new DateTime(year, month, 1));
                if (serviceMonths < rule.MinServiceMonths)
                    continue;

                // Check if accrual already exists
                var hasAccrual = await _unitOfWork.LeaveAccruals.HasAccrualForPeriodAsync(
                    employee.Id, rule.LeavePolicyId, year, month);
                
                if (hasAccrual)
                    continue;

                var accruedDays = rule.AccrualRate;
                
                // Apply pro-rating if needed
                if (rule.IsProRated && employee.JoiningDate.Year == year && employee.JoiningDate.Month == month)
                {
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    var workingDaysFromJoining = daysInMonth - employee.JoiningDate.Day + 1;
                    accruedDays = (accruedDays * workingDaysFromJoining) / daysInMonth;
                }

                var accrual = new LeaveAccrual
                {
                    EmployeeId = employee.Id,
                    LeavePolicyId = rule.LeavePolicyId,
                    Year = year,
                    Month = month,
                    AccruedDays = accruedDays,
                    AccrualRate = rule.AccrualRate,
                    AccrualType = AccrualType.Monthly,
                    AccrualDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                    Notes = "Monthly accrual",
                    IsProcessed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.LeaveAccruals.AddAsync(accrual);
                
                var accrualDto = new LeaveAccrualDto
                {
                    Id = accrual.Id,
                    EmployeeId = accrual.EmployeeId,
                    EmployeeName = employee.FullName,
                    LeavePolicyId = accrual.LeavePolicyId,
                    LeaveType = rule.LeavePolicy.LeaveType,
                    LeaveTypeName = rule.LeavePolicy.Name,
                    Year = accrual.Year,
                    Month = accrual.Month,
                    AccruedDays = accrual.AccruedDays,
                    AccrualRate = accrual.AccrualRate,
                    AccrualType = accrual.AccrualType,
                    AccrualDate = accrual.AccrualDate,
                    Notes = accrual.Notes,
                    IsProcessed = accrual.IsProcessed
                };

                result.Add(accrualDto);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        // Process all accruals to update balances
        foreach (var accrualDto in result)
        {
            var accrual = await _unitOfWork.LeaveAccruals.GetByIdAsync(accrualDto.Id);
            if (accrual != null)
            {
                await UpdateLeaveBalanceWithAccrualAsync(accrual);
            }
        }

        return result;
    }

    public async Task<IEnumerable<LeaveAccrualDto>> ProcessEmployeeAccrualsAsync(int employeeId, int year)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var leavePolicies = await _unitOfWork.LeavePolicies.GetActiveByBranchIdAsync(employee.BranchId);
        var result = new List<LeaveAccrualDto>();

        foreach (var policy in leavePolicies)
        {
            var rule = await _unitOfWork.LeaveAccrualRules.GetActiveRuleAsync(policy.Id, DateTime.Now);
            if (rule == null)
                continue;

            // Process accruals based on frequency
            var accruals = await ProcessAccrualsByFrequencyAsync(employee, policy, rule, year);
            result.AddRange(accruals);
        }

        return result;
    }

    #endregion

    #region Leave Accrual Rules Management

    public async Task<IEnumerable<LeaveAccrualRuleDto>> GetAccrualRulesAsync(int leavePolicyId)
    {
        var rules = await _unitOfWork.LeaveAccrualRules.GetByLeavePolicyIdAsync(leavePolicyId);
        var result = new List<LeaveAccrualRuleDto>();

        foreach (var rule in rules)
        {
            result.Add(new LeaveAccrualRuleDto
            {
                Id = rule.Id,
                LeavePolicyId = rule.LeavePolicyId,
                LeaveType = rule.LeavePolicy?.LeaveType ?? LeaveType.Annual,
                LeaveTypeName = rule.LeavePolicy?.Name ?? "",
                AccrualFrequency = rule.AccrualFrequency,
                AccrualRate = rule.AccrualRate,
                MaxAccrualDays = rule.MaxAccrualDays,
                IsProRated = rule.IsProRated,
                MinServiceMonths = rule.MinServiceMonths,
                EffectiveFrom = rule.EffectiveFrom,
                EffectiveTo = rule.EffectiveTo,
                IsActive = rule.IsActive
            });
        }

        return result;
    }

    public async Task<LeaveAccrualRuleDto> CreateAccrualRuleAsync(CreateLeaveAccrualRuleDto ruleDto)
    {
        var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(ruleDto.LeavePolicyId);
        if (leavePolicy == null)
            throw new ArgumentException("Leave policy not found");

        var rule = new LeaveAccrualRule
        {
            LeavePolicyId = ruleDto.LeavePolicyId,
            AccrualFrequency = ruleDto.AccrualFrequency,
            AccrualRate = ruleDto.AccrualRate,
            MaxAccrualDays = ruleDto.MaxAccrualDays,
            IsProRated = ruleDto.IsProRated,
            MinServiceMonths = ruleDto.MinServiceMonths,
            EffectiveFrom = ruleDto.EffectiveFrom,
            EffectiveTo = ruleDto.EffectiveTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveAccrualRules.AddAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        return new LeaveAccrualRuleDto
        {
            Id = rule.Id,
            LeavePolicyId = rule.LeavePolicyId,
            LeaveType = leavePolicy.LeaveType,
            LeaveTypeName = leavePolicy.Name,
            AccrualFrequency = rule.AccrualFrequency,
            AccrualRate = rule.AccrualRate,
            MaxAccrualDays = rule.MaxAccrualDays,
            IsProRated = rule.IsProRated,
            MinServiceMonths = rule.MinServiceMonths,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            IsActive = rule.IsActive
        };
    }

    public async Task<LeaveAccrualRuleDto> UpdateAccrualRuleAsync(int id, CreateLeaveAccrualRuleDto ruleDto)
    {
        var rule = await _unitOfWork.LeaveAccrualRules.GetByIdAsync(id);
        if (rule == null)
            throw new ArgumentException("Accrual rule not found");

        rule.AccrualFrequency = ruleDto.AccrualFrequency;
        rule.AccrualRate = ruleDto.AccrualRate;
        rule.MaxAccrualDays = ruleDto.MaxAccrualDays;
        rule.IsProRated = ruleDto.IsProRated;
        rule.MinServiceMonths = ruleDto.MinServiceMonths;
        rule.EffectiveFrom = ruleDto.EffectiveFrom;
        rule.EffectiveTo = ruleDto.EffectiveTo;
        rule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveAccrualRules.UpdateAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        return new LeaveAccrualRuleDto
        {
            Id = rule.Id,
            LeavePolicyId = rule.LeavePolicyId,
            LeaveType = rule.LeavePolicy?.LeaveType ?? LeaveType.Annual,
            LeaveTypeName = rule.LeavePolicy?.Name ?? "",
            AccrualFrequency = rule.AccrualFrequency,
            AccrualRate = rule.AccrualRate,
            MaxAccrualDays = rule.MaxAccrualDays,
            IsProRated = rule.IsProRated,
            MinServiceMonths = rule.MinServiceMonths,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            IsActive = rule.IsActive
        };
    }

    public async Task<bool> DeleteAccrualRuleAsync(int id)
    {
        var rule = await _unitOfWork.LeaveAccrualRules.GetByIdAsync(id);
        if (rule == null)
            return false;

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveAccrualRules.UpdateAsync(rule);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Leave Encashment Management

    public async Task<IEnumerable<LeaveEncashmentDto>> GetEmployeeEncashmentsAsync(int employeeId, int year)
    {
        var encashments = await _unitOfWork.LeaveEncashments.GetByEmployeeAndYearAsync(employeeId, year);
        var result = new List<LeaveEncashmentDto>();

        foreach (var encashment in encashments)
        {
            result.Add(new LeaveEncashmentDto
            {
                Id = encashment.Id,
                EmployeeId = encashment.EmployeeId,
                EmployeeName = encashment.Employee?.FullName ?? "",
                LeavePolicyId = encashment.LeavePolicyId,
                LeaveType = encashment.LeavePolicy?.LeaveType ?? LeaveType.Annual,
                LeaveTypeName = encashment.LeavePolicy?.Name ?? "",
                Year = encashment.Year,
                EncashedDays = encashment.EncashedDays,
                EncashmentRate = encashment.EncashmentRate,
                EncashmentAmount = encashment.EncashmentAmount,
                EncashmentDate = encashment.EncashmentDate,
                Status = encashment.Status,
                ApprovedBy = encashment.ApprovedBy,
                ApprovedByName = encashment.ApprovedByEmployee?.FullName,
                ApprovedAt = encashment.ApprovedAt,
                Reason = encashment.Reason,
                Comments = encashment.Comments
            });
        }

        return result;
    }

    public async Task<LeaveEncashmentDto> CreateEncashmentRequestAsync(CreateLeaveEncashmentDto requestDto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(requestDto.EmployeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(requestDto.LeavePolicyId);
        if (leavePolicy == null)
            throw new ArgumentException("Leave policy not found");

        if (!leavePolicy.IsEncashmentAllowed)
            throw new InvalidOperationException("Encashment is not allowed for this leave type");

        // Check if there's already a pending encashment
        var hasPending = await _unitOfWork.LeaveEncashments.HasPendingEncashmentAsync(
            requestDto.EmployeeId, requestDto.LeavePolicyId, requestDto.Year);
        
        if (hasPending)
            throw new InvalidOperationException("You already have a pending encashment request for this leave type");

        // Validate available balance
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(
            requestDto.EmployeeId, requestDto.LeavePolicyId, requestDto.Year);
        
        if (balance == null || balance.RemainingDays < requestDto.EncashedDays)
            throw new InvalidOperationException("Insufficient leave balance for encashment");

        // Calculate encashment amount
        var encashmentAmount = await CalculateEncashmentAmountAsync(
            requestDto.EmployeeId, requestDto.LeavePolicyId, requestDto.EncashedDays);

        var encashment = new LeaveEncashment
        {
            EmployeeId = requestDto.EmployeeId,
            LeavePolicyId = requestDto.LeavePolicyId,
            Year = requestDto.Year,
            EncashedDays = requestDto.EncashedDays,
            EncashmentRate = leavePolicy.EncashmentRate,
            EncashmentAmount = encashmentAmount,
            EncashmentDate = DateTime.Now,
            Status = EncashmentStatus.Pending,
            Reason = requestDto.Reason,
            Comments = requestDto.Comments,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveEncashments.AddAsync(encashment);
        await _unitOfWork.SaveChangesAsync();

        return new LeaveEncashmentDto
        {
            Id = encashment.Id,
            EmployeeId = encashment.EmployeeId,
            EmployeeName = employee.FullName,
            LeavePolicyId = encashment.LeavePolicyId,
            LeaveType = leavePolicy.LeaveType,
            LeaveTypeName = leavePolicy.Name,
            Year = encashment.Year,
            EncashedDays = encashment.EncashedDays,
            EncashmentRate = encashment.EncashmentRate,
            EncashmentAmount = encashment.EncashmentAmount,
            EncashmentDate = encashment.EncashmentDate,
            Status = encashment.Status,
            Reason = encashment.Reason,
            Comments = encashment.Comments
        };
    }

    public async Task<LeaveEncashmentDto> ApproveEncashmentAsync(int id, ApproveLeaveEncashmentDto approval, int approverId)
    {
        var encashment = await _unitOfWork.LeaveEncashments.GetByIdAsync(id);
        if (encashment == null)
            throw new ArgumentException("Encashment request not found");

        if (encashment.Status != EncashmentStatus.Pending)
            throw new InvalidOperationException("Encashment request is not pending approval");

        var approvedDays = approval.ApprovedDays ?? encashment.EncashedDays;
        
        // Recalculate amount if days changed
        if (approvedDays != encashment.EncashedDays)
        {
            encashment.EncashmentAmount = await CalculateEncashmentAmountAsync(
                encashment.EmployeeId, encashment.LeavePolicyId, approvedDays);
        }

        encashment.EncashedDays = approvedDays;
        encashment.Status = EncashmentStatus.Approved;
        encashment.ApprovedBy = approverId;
        encashment.ApprovedAt = DateTime.UtcNow;
        encashment.Comments = approval.Comments;
        encashment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveEncashments.UpdateAsync(encashment);

        // Update leave balance
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(
            encashment.EmployeeId, encashment.LeavePolicyId, encashment.Year);
        
        if (balance != null)
        {
            balance.EncashedDays += approvedDays;
            balance.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveBalances.UpdateAsync(balance);
        }

        await _unitOfWork.SaveChangesAsync();

        var approver = await _unitOfWork.Employees.GetByIdAsync(approverId);
        
        return new LeaveEncashmentDto
        {
            Id = encashment.Id,
            EmployeeId = encashment.EmployeeId,
            EmployeeName = encashment.Employee?.FullName ?? "",
            LeavePolicyId = encashment.LeavePolicyId,
            LeaveType = encashment.LeavePolicy?.LeaveType ?? LeaveType.Annual,
            LeaveTypeName = encashment.LeavePolicy?.Name ?? "",
            Year = encashment.Year,
            EncashedDays = encashment.EncashedDays,
            EncashmentRate = encashment.EncashmentRate,
            EncashmentAmount = encashment.EncashmentAmount,
            EncashmentDate = encashment.EncashmentDate,
            Status = encashment.Status,
            ApprovedBy = encashment.ApprovedBy,
            ApprovedByName = approver?.FullName,
            ApprovedAt = encashment.ApprovedAt,
            Reason = encashment.Reason,
            Comments = encashment.Comments
        };
    }

    public async Task<LeaveEncashmentDto> RejectEncashmentAsync(int id, string reason, int approverId)
    {
        var encashment = await _unitOfWork.LeaveEncashments.GetByIdAsync(id);
        if (encashment == null)
            throw new ArgumentException("Encashment request not found");

        if (encashment.Status != EncashmentStatus.Pending)
            throw new InvalidOperationException("Encashment request is not pending approval");

        encashment.Status = EncashmentStatus.Rejected;
        encashment.ApprovedBy = approverId;
        encashment.ApprovedAt = DateTime.UtcNow;
        encashment.Comments = reason;
        encashment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveEncashments.UpdateAsync(encashment);
        await _unitOfWork.SaveChangesAsync();

        var approver = await _unitOfWork.Employees.GetByIdAsync(approverId);

        return new LeaveEncashmentDto
        {
            Id = encashment.Id,
            EmployeeId = encashment.EmployeeId,
            EmployeeName = encashment.Employee?.FullName ?? "",
            LeavePolicyId = encashment.LeavePolicyId,
            LeaveType = encashment.LeavePolicy?.LeaveType ?? LeaveType.Annual,
            LeaveTypeName = encashment.LeavePolicy?.Name ?? "",
            Year = encashment.Year,
            EncashedDays = encashment.EncashedDays,
            EncashmentRate = encashment.EncashmentRate,
            EncashmentAmount = encashment.EncashmentAmount,
            EncashmentDate = encashment.EncashmentDate,
            Status = encashment.Status,
            ApprovedBy = encashment.ApprovedBy,
            ApprovedByName = approver?.FullName,
            ApprovedAt = encashment.ApprovedAt,
            Reason = encashment.Reason,
            Comments = encashment.Comments
        };
    }

    public async Task<IEnumerable<LeaveEncashmentDto>> GetPendingEncashmentsAsync(int branchId)
    {
        var encashments = await _unitOfWork.LeaveEncashments.GetPendingEncashmentsAsync(branchId);
        var result = new List<LeaveEncashmentDto>();

        foreach (var encashment in encashments)
        {
            result.Add(new LeaveEncashmentDto
            {
                Id = encashment.Id,
                EmployeeId = encashment.EmployeeId,
                EmployeeName = encashment.Employee?.FullName ?? "",
                LeavePolicyId = encashment.LeavePolicyId,
                LeaveType = encashment.LeavePolicy?.LeaveType ?? LeaveType.Annual,
                LeaveTypeName = encashment.LeavePolicy?.Name ?? "",
                Year = encashment.Year,
                EncashedDays = encashment.EncashedDays,
                EncashmentRate = encashment.EncashmentRate,
                EncashmentAmount = encashment.EncashmentAmount,
                EncashmentDate = encashment.EncashmentDate,
                Status = encashment.Status,
                Reason = encashment.Reason,
                Comments = encashment.Comments
            });
        }

        return result;
    }

    public async Task<decimal> CalculateEncashmentAmountAsync(int employeeId, int leavePolicyId, decimal days)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(leavePolicyId);
        if (leavePolicy == null)
            throw new ArgumentException("Leave policy not found");

        // Calculate daily salary
        var dailySalary = employee.BasicSalary / 30; // Assuming 30 days per month

        // Apply encashment rate
        var encashmentAmount = dailySalary * days * leavePolicy.EncashmentRate;

        return Math.Round(encashmentAmount, 2);
    }

    #endregion

    #region Leave History and Analytics

    public async Task<LeaveHistoryDto> GetEmployeeLeaveHistoryAsync(int employeeId, int year)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Employee not found");

        var balances = await _unitOfWork.LeaveBalances.GetByEmployeeAndYearAsync(employeeId, year);
        var history = new LeaveHistoryDto
        {
            EmployeeId = employeeId,
            EmployeeName = employee.FullName,
            Year = year,
            LeaveDetails = new List<LeaveHistoryDetailDto>()
        };

        foreach (var balance in balances)
        {
            var transactions = await GetLeaveTransactionsAsync(employeeId, balance.LeavePolicyId, year);
            
            var detail = new LeaveHistoryDetailDto
            {
                LeaveType = balance.LeavePolicy.LeaveType,
                LeaveTypeName = balance.LeavePolicy.Name,
                AllocatedDays = balance.AllocatedDays,
                UsedDays = balance.UsedDays,
                CarriedForwardDays = balance.CarriedForwardDays,
                EncashedDays = balance.EncashedDays,
                RemainingDays = balance.RemainingDays,
                Transactions = transactions.ToList()
            };

            history.LeaveDetails.Add(detail);
        }

        history.TotalAllocated = history.LeaveDetails.Sum(d => d.AllocatedDays);
        history.TotalUsed = history.LeaveDetails.Sum(d => d.UsedDays);
        history.TotalCarriedForward = history.LeaveDetails.Sum(d => d.CarriedForwardDays);
        history.TotalEncashed = history.LeaveDetails.Sum(d => d.EncashedDays);
        history.TotalRemaining = history.LeaveDetails.Sum(d => d.RemainingDays);

        return history;
    }

    public async Task<IEnumerable<LeaveHistoryDto>> GetBranchLeaveHistoryAsync(int branchId, int year)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.BranchId == branchId);
        var result = new List<LeaveHistoryDto>();

        foreach (var employee in employees)
        {
            var history = await GetEmployeeLeaveHistoryAsync(employee.Id, year);
            result.Add(history);
        }

        return result;
    }

    public async Task<LeaveAnalyticsDto> GetLeaveAnalyticsAsync(int branchId, int year)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
        if (branch == null)
            throw new ArgumentException("Branch not found");

        var employees = await _unitOfWork.Employees.FindAsync(e => e.BranchId == branchId);
        var totalEmployees = employees.Count();

        var analytics = new LeaveAnalyticsDto
        {
            BranchId = branchId,
            BranchName = branch.Name,
            Year = year,
            TotalEmployees = totalEmployees,
            LeaveTypeAnalytics = new List<LeaveTypeAnalyticsDto>(),
            MonthlyAnalytics = new List<MonthlyLeaveAnalyticsDto>()
        };

        // Get leave type analytics
        var leavePolicies = await _unitOfWork.LeavePolicies.GetActiveByBranchIdAsync(branchId);
        foreach (var policy in leavePolicies)
        {
            var balances = await _unitOfWork.LeaveBalances.FindAsync(b => 
                b.LeavePolicyId == policy.Id && 
                b.Year == year &&
                employees.Any(e => e.Id == b.EmployeeId));

            var totalAllocated = balances.Sum(b => b.AllocatedDays);
            var totalUsed = balances.Sum(b => b.UsedDays);
            var totalEncashed = balances.Sum(b => b.EncashedDays);
            var totalCarriedForward = balances.Sum(b => b.CarriedForwardDays);

            analytics.LeaveTypeAnalytics.Add(new LeaveTypeAnalyticsDto
            {
                LeaveType = policy.LeaveType,
                LeaveTypeName = policy.Name,
                TotalAllocated = totalAllocated,
                TotalUsed = totalUsed,
                UtilizationPercentage = totalAllocated > 0 ? (totalUsed / totalAllocated) * 100 : 0,
                TotalEncashed = totalEncashed,
                TotalCarriedForward = totalCarriedForward
            });
        }

        // Calculate average utilization
        analytics.AverageLeaveUtilization = analytics.LeaveTypeAnalytics.Any() 
            ? analytics.LeaveTypeAnalytics.Average(lt => lt.UtilizationPercentage) 
            : 0;

        // Get monthly analytics
        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthlyRequests = await _unitOfWork.LeaveRequests.FindAsync(lr =>
                employees.Any(e => e.Id == lr.EmployeeId) &&
                lr.StartDate >= monthStart && lr.StartDate <= monthEnd &&
                lr.Status == LeaveStatus.Approved);

            var totalLeavesTaken = monthlyRequests.Sum(lr => lr.ApprovedDays);
            var totalRequests = monthlyRequests.Count();
            var averageRequestDuration = totalRequests > 0 ? totalLeavesTaken / totalRequests : 0;

            analytics.MonthlyAnalytics.Add(new MonthlyLeaveAnalyticsDto
            {
                Month = month,
                MonthName = monthStart.ToString("MMMM"),
                TotalLeavesTaken = totalLeavesTaken,
                TotalRequests = totalRequests,
                AverageRequestDuration = averageRequestDuration
            });
        }

        return analytics;
    }

    public async Task<IEnumerable<LeaveTransactionDto>> GetLeaveTransactionsAsync(int employeeId, int leavePolicyId, int year)
    {
        var transactions = new List<LeaveTransactionDto>();

        // Get initial allocation
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(employeeId, leavePolicyId, year);
        if (balance != null)
        {
            transactions.Add(new LeaveTransactionDto
            {
                Date = new DateTime(year, 1, 1),
                TransactionType = "Allocated",
                Days = balance.AllocatedDays,
                Description = "Annual allocation"
            });

            // Add carried forward if any
            if (balance.CarriedForwardDays > 0)
            {
                transactions.Add(new LeaveTransactionDto
                {
                    Date = new DateTime(year, 1, 1),
                    TransactionType = "Carried Forward",
                    Days = balance.CarriedForwardDays,
                    Description = "Carried forward from previous year"
                });
            }
        }

        // Get accruals
        var accruals = await _unitOfWork.LeaveAccruals.GetByEmployeeAndPolicyAsync(employeeId, leavePolicyId, year);
        foreach (var accrual in accruals)
        {
            transactions.Add(new LeaveTransactionDto
            {
                Date = accrual.AccrualDate,
                TransactionType = "Accrued",
                Days = accrual.AccruedDays,
                Description = $"Monthly accrual - {accrual.AccrualDate:MMM yyyy}",
                ReferenceId = accrual.Id
            });
        }

        // Get leave requests
        var leaveRequests = await _unitOfWork.LeaveRequests.FindAsync(lr =>
            lr.EmployeeId == employeeId &&
            lr.LeavePolicyId == leavePolicyId &&
            lr.StartDate.Year == year &&
            lr.Status == LeaveStatus.Approved);

        foreach (var request in leaveRequests)
        {
            transactions.Add(new LeaveTransactionDto
            {
                Date = request.StartDate,
                TransactionType = "Used",
                Days = -request.ApprovedDays,
                Description = $"Leave taken: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}",
                ReferenceId = request.Id
            });
        }

        // Get encashments
        var encashments = await _unitOfWork.LeaveEncashments.GetByEmployeeAndYearAsync(employeeId, year);
        foreach (var encashment in encashments.Where(e => e.LeavePolicyId == leavePolicyId && e.Status == EncashmentStatus.Approved))
        {
            transactions.Add(new LeaveTransactionDto
            {
                Date = encashment.EncashmentDate,
                TransactionType = "Encashed",
                Days = -encashment.EncashedDays,
                Description = $"Leave encashed - Amount: {encashment.EncashmentAmount:C}",
                ReferenceId = encashment.Id
            });
        }

        return transactions.OrderBy(t => t.Date).ToList();
    }

    #endregion

    #region Private Helper Methods

    private async Task UpdateLeaveBalanceWithAccrualAsync(LeaveAccrual accrual)
    {
        var balance = await _unitOfWork.LeaveBalances.GetByEmployeeAndPolicyAsync(
            accrual.EmployeeId, accrual.LeavePolicyId, accrual.Year);

        if (balance == null)
        {
            var leavePolicy = await _unitOfWork.LeavePolicies.GetByIdAsync(accrual.LeavePolicyId);
            balance = new LeaveBalance
            {
                EmployeeId = accrual.EmployeeId,
                LeavePolicyId = accrual.LeavePolicyId,
                Year = accrual.Year,
                AllocatedDays = leavePolicy?.AnnualAllocation ?? 0,
                UsedDays = 0,
                CarriedForwardDays = 0,
                EncashedDays = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.LeaveBalances.AddAsync(balance);
        }

        balance.AllocatedDays += accrual.AccruedDays;
        balance.UpdatedAt = DateTime.UtcNow;
        
        accrual.IsProcessed = true;
        accrual.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.LeaveBalances.UpdateAsync(balance);
        await _unitOfWork.LeaveAccruals.UpdateAsync(accrual);
        await _unitOfWork.SaveChangesAsync();
    }

    private int GetServiceMonths(DateTime joiningDate, DateTime currentDate)
    {
        var months = (currentDate.Year - joiningDate.Year) * 12 + currentDate.Month - joiningDate.Month;
        if (currentDate.Day < joiningDate.Day)
            months--;
        return Math.Max(0, months);
    }

    private async Task<IEnumerable<LeaveAccrualDto>> ProcessAccrualsByFrequencyAsync(Employee employee, LeavePolicy policy, LeaveAccrualRule rule, int year)
    {
        var result = new List<LeaveAccrualDto>();

        switch (rule.AccrualFrequency)
        {
            case AccrualFrequency.Monthly:
                for (int month = 1; month <= 12; month++)
                {
                    var hasAccrual = await _unitOfWork.LeaveAccruals.HasAccrualForPeriodAsync(
                        employee.Id, policy.Id, year, month);
                    
                    if (!hasAccrual)
                    {
                        var accrualDto = await CreateMonthlyAccrualAsync(employee, policy, rule, year, month);
                        if (accrualDto != null)
                            result.Add(accrualDto);
                    }
                }
                break;

            case AccrualFrequency.Quarterly:
                for (int quarter = 1; quarter <= 4; quarter++)
                {
                    var quarterMonth = quarter * 3;
                    var hasAccrual = await _unitOfWork.LeaveAccruals.HasAccrualForPeriodAsync(
                        employee.Id, policy.Id, year, quarterMonth);
                    
                    if (!hasAccrual)
                    {
                        var accrualDto = await CreateQuarterlyAccrualAsync(employee, policy, rule, year, quarter);
                        if (accrualDto != null)
                            result.Add(accrualDto);
                    }
                }
                break;

            case AccrualFrequency.Yearly:
                var hasYearlyAccrual = await _unitOfWork.LeaveAccruals.HasAccrualForPeriodAsync(
                    employee.Id, policy.Id, year, 12);
                
                if (!hasYearlyAccrual)
                {
                    var accrualDto = await CreateYearlyAccrualAsync(employee, policy, rule, year);
                    if (accrualDto != null)
                        result.Add(accrualDto);
                }
                break;
        }

        return result;
    }

    private async Task<LeaveAccrualDto?> CreateMonthlyAccrualAsync(Employee employee, LeavePolicy policy, LeaveAccrualRule rule, int year, int month)
    {
        var serviceMonths = GetServiceMonths(employee.JoiningDate, new DateTime(year, month, 1));
        if (serviceMonths < rule.MinServiceMonths)
            return null;

        var accruedDays = rule.AccrualRate;
        
        if (rule.IsProRated && employee.JoiningDate.Year == year && employee.JoiningDate.Month == month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var workingDaysFromJoining = daysInMonth - employee.JoiningDate.Day + 1;
            accruedDays = (accruedDays * workingDaysFromJoining) / daysInMonth;
        }

        var accrual = new LeaveAccrual
        {
            EmployeeId = employee.Id,
            LeavePolicyId = policy.Id,
            Year = year,
            Month = month,
            AccruedDays = accruedDays,
            AccrualRate = rule.AccrualRate,
            AccrualType = AccrualType.Monthly,
            AccrualDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
            Notes = "Monthly accrual",
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveAccruals.AddAsync(accrual);
        await _unitOfWork.SaveChangesAsync();

        await UpdateLeaveBalanceWithAccrualAsync(accrual);

        return new LeaveAccrualDto
        {
            Id = accrual.Id,
            EmployeeId = accrual.EmployeeId,
            EmployeeName = employee.FullName,
            LeavePolicyId = accrual.LeavePolicyId,
            LeaveType = policy.LeaveType,
            LeaveTypeName = policy.Name,
            Year = accrual.Year,
            Month = accrual.Month,
            AccruedDays = accrual.AccruedDays,
            AccrualRate = accrual.AccrualRate,
            AccrualType = accrual.AccrualType,
            AccrualDate = accrual.AccrualDate,
            Notes = accrual.Notes,
            IsProcessed = accrual.IsProcessed
        };
    }

    private async Task<LeaveAccrualDto?> CreateQuarterlyAccrualAsync(Employee employee, LeavePolicy policy, LeaveAccrualRule rule, int year, int quarter)
    {
        var quarterMonth = quarter * 3;
        var serviceMonths = GetServiceMonths(employee.JoiningDate, new DateTime(year, quarterMonth, 1));
        if (serviceMonths < rule.MinServiceMonths)
            return null;

        var accrual = new LeaveAccrual
        {
            EmployeeId = employee.Id,
            LeavePolicyId = policy.Id,
            Year = year,
            Month = quarterMonth,
            AccruedDays = rule.AccrualRate,
            AccrualRate = rule.AccrualRate,
            AccrualType = AccrualType.Quarterly,
            AccrualDate = new DateTime(year, quarterMonth, DateTime.DaysInMonth(year, quarterMonth)),
            Notes = $"Q{quarter} accrual",
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveAccruals.AddAsync(accrual);
        await _unitOfWork.SaveChangesAsync();

        await UpdateLeaveBalanceWithAccrualAsync(accrual);

        return new LeaveAccrualDto
        {
            Id = accrual.Id,
            EmployeeId = accrual.EmployeeId,
            EmployeeName = employee.FullName,
            LeavePolicyId = accrual.LeavePolicyId,
            LeaveType = policy.LeaveType,
            LeaveTypeName = policy.Name,
            Year = accrual.Year,
            Month = accrual.Month,
            AccruedDays = accrual.AccruedDays,
            AccrualRate = accrual.AccrualRate,
            AccrualType = accrual.AccrualType,
            AccrualDate = accrual.AccrualDate,
            Notes = accrual.Notes,
            IsProcessed = accrual.IsProcessed
        };
    }

    private async Task<LeaveAccrualDto?> CreateYearlyAccrualAsync(Employee employee, LeavePolicy policy, LeaveAccrualRule rule, int year)
    {
        var serviceMonths = GetServiceMonths(employee.JoiningDate, new DateTime(year, 12, 31));
        if (serviceMonths < rule.MinServiceMonths)
            return null;

        var accrual = new LeaveAccrual
        {
            EmployeeId = employee.Id,
            LeavePolicyId = policy.Id,
            Year = year,
            Month = 12,
            AccruedDays = rule.AccrualRate,
            AccrualRate = rule.AccrualRate,
            AccrualType = AccrualType.Yearly,
            AccrualDate = new DateTime(year, 12, 31),
            Notes = "Annual accrual",
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveAccruals.AddAsync(accrual);
        await _unitOfWork.SaveChangesAsync();

        await UpdateLeaveBalanceWithAccrualAsync(accrual);

        return new LeaveAccrualDto
        {
            Id = accrual.Id,
            EmployeeId = accrual.EmployeeId,
            EmployeeName = employee.FullName,
            LeavePolicyId = accrual.LeavePolicyId,
            LeaveType = policy.LeaveType,
            LeaveTypeName = policy.Name,
            Year = accrual.Year,
            Month = accrual.Month,
            AccruedDays = accrual.AccruedDays,
            AccrualRate = accrual.AccrualRate,
            AccrualType = accrual.AccrualType,
            AccrualDate = accrual.AccrualDate,
            Notes = accrual.Notes,
            IsProcessed = accrual.IsProcessed
        };
    }

    #endregion
}