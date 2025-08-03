using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Shift;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IShiftAssignmentRepository _shiftAssignmentRepository;
    private readonly IShiftSwapRequestRepository _shiftSwapRequestRepository;
    private readonly IShiftSwapResponseRepository _shiftSwapResponseRepository;
    private readonly IShiftCoverageRequestRepository _shiftCoverageRequestRepository;
    private readonly IShiftCoverageResponseRepository _shiftCoverageResponseRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        IShiftRepository shiftRepository,
        IShiftAssignmentRepository shiftAssignmentRepository,
        IShiftSwapRequestRepository shiftSwapRequestRepository,
        IShiftSwapResponseRepository shiftSwapResponseRepository,
        IShiftCoverageRequestRepository shiftCoverageRequestRepository,
        IShiftCoverageResponseRepository shiftCoverageResponseRepository,
        IEmployeeRepository employeeRepository,
        IMapper mapper,
        ILogger<ShiftService> logger)
    {
        _shiftRepository = shiftRepository;
        _shiftAssignmentRepository = shiftAssignmentRepository;
        _shiftSwapRequestRepository = shiftSwapRequestRepository;
        _shiftSwapResponseRepository = shiftSwapResponseRepository;
        _shiftCoverageRequestRepository = shiftCoverageRequestRepository;
        _shiftCoverageResponseRepository = shiftCoverageResponseRepository;
        _employeeRepository = employeeRepository;
        _mapper = mapper;
        _logger = logger;
    }

    #region Shift Management

    public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto)
    {
        _logger.LogInformation("Creating new shift: {ShiftName}", createShiftDto.Name);

        // Validate shift name uniqueness
        var isUnique = await _shiftRepository.IsShiftNameUniqueAsync(createShiftDto.OrganizationId, createShiftDto.Name);
        if (!isUnique)
        {
            throw new InvalidOperationException($"Shift name '{createShiftDto.Name}' already exists in this organization.");
        }

        // Validate time ranges
        ValidateShiftTimes(createShiftDto.StartTime, createShiftDto.EndTime);

        var shift = _mapper.Map<Shift>(createShiftDto);
        await _shiftRepository.AddAsync(shift);
        await _shiftRepository.SaveChangesAsync();

        _logger.LogInformation("Shift created successfully with ID: {ShiftId}", shift.Id);
        return _mapper.Map<ShiftDto>(shift);
    }

    public async Task<ShiftDto> UpdateShiftAsync(int shiftId, UpdateShiftDto updateShiftDto)
    {
        _logger.LogInformation("Updating shift: {ShiftId}", shiftId);

        var shift = await _shiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
        {
            throw new ArgumentException($"Shift with ID {shiftId} not found.");
        }

        // Validate shift name uniqueness if name is being changed
        if (!string.IsNullOrEmpty(updateShiftDto.Name) && shift.Name != updateShiftDto.Name)
        {
            var isUnique = await _shiftRepository.IsShiftNameUniqueAsync(shift.OrganizationId, updateShiftDto.Name, shiftId);
            if (!isUnique)
            {
                throw new InvalidOperationException($"Shift name '{updateShiftDto.Name}' already exists in this organization.");
            }
        }

        // Validate time ranges
        ValidateShiftTimes(updateShiftDto.StartTime, updateShiftDto.EndTime);

        _mapper.Map(updateShiftDto, shift);
        await _shiftRepository.UpdateAsync(shift);
        await _shiftRepository.SaveChangesAsync();

        _logger.LogInformation("Shift updated successfully: {ShiftId}", shiftId);
        return _mapper.Map<ShiftDto>(shift);
    }

    public async Task<bool> DeleteShiftAsync(int shiftId)
    {
        _logger.LogInformation("Deleting shift: {ShiftId}", shiftId);

        var shift = await _shiftRepository.GetShiftWithAssignmentsAsync(shiftId);
        if (shift == null)
        {
            return false;
        }

        // Check if shift has active assignments
        var hasActiveAssignments = shift.ShiftAssignments.Any(sa => sa.IsActive);
        if (hasActiveAssignments)
        {
            throw new InvalidOperationException("Cannot delete shift with active assignments. Please remove all assignments first.");
        }

        await _shiftRepository.DeleteAsync(shift);
        await _shiftRepository.SaveChangesAsync();

        _logger.LogInformation("Shift deleted successfully: {ShiftId}", shiftId);
        return true;
    }

    public async Task<ShiftDto?> GetShiftByIdAsync(int shiftId)
    {
        var shift = await _shiftRepository.GetShiftWithAssignmentsAsync(shiftId);
        return shift != null ? _mapper.Map<ShiftDto>(shift) : null;
    }

    public async Task<IEnumerable<ShiftDto>> GetShiftsByOrganizationAsync(int organizationId)
    {
        var shifts = await _shiftRepository.GetByOrganizationIdAsync(organizationId);
        return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
    }

    public async Task<IEnumerable<ShiftDto>> GetShiftsByBranchAsync(int branchId)
    {
        var shifts = await _shiftRepository.GetByBranchIdAsync(branchId);
        return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
    }

    public async Task<IEnumerable<ShiftDto>> GetActiveShiftsAsync(int organizationId)
    {
        var shifts = await _shiftRepository.GetActiveShiftsAsync(organizationId);
        return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
    }

    public async Task<(IEnumerable<ShiftDto> Shifts, int TotalCount)> SearchShiftsAsync(ShiftSearchCriteria criteria)
    {
        var shifts = await _shiftRepository.SearchShiftsAsync(criteria);
        var totalCount = await _shiftRepository.GetTotalCountAsync(criteria);
        
        return (_mapper.Map<IEnumerable<ShiftDto>>(shifts), totalCount);
    }

    #endregion

    #region Shift Templates

    public async Task<IEnumerable<ShiftTemplateDto>> GetShiftTemplatesAsync(int organizationId)
    {
        var shifts = await _shiftRepository.GetShiftTemplatesAsync(organizationId);
        return _mapper.Map<IEnumerable<ShiftTemplateDto>>(shifts);
    }

    public async Task<ShiftDto> CreateShiftFromTemplateAsync(int templateId, CreateShiftDto createShiftDto)
    {
        var template = await _shiftRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new ArgumentException($"Template with ID {templateId} not found.");
        }

        // Override template values with provided values
        createShiftDto.Type = template.Type;
        createShiftDto.StartTime = template.StartTime;
        createShiftDto.EndTime = template.EndTime;
        createShiftDto.BreakDuration = template.BreakDuration;
        createShiftDto.GracePeriod = template.GracePeriod;
        createShiftDto.IsFlexible = template.IsFlexible;
        createShiftDto.FlexibilityWindow = template.FlexibilityWindow;
        createShiftDto.OvertimeMultiplier = template.OvertimeMultiplier;

        if (!string.IsNullOrEmpty(template.WorkingDays))
        {
            createShiftDto.WorkingDays = JsonSerializer.Deserialize<List<int>>(template.WorkingDays) ?? new List<int>();
        }

        return await CreateShiftAsync(createShiftDto);
    }

    #endregion

    #region Shift Assignment Management

    public async Task<ShiftAssignmentDto> AssignEmployeeToShiftAsync(CreateShiftAssignmentDto assignmentDto)
    {
        _logger.LogInformation("Assigning employee {EmployeeId} to shift {ShiftId}", assignmentDto.EmployeeId, assignmentDto.ShiftId);

        // Validate assignment
        var isValid = await ValidateShiftAssignmentAsync(assignmentDto.EmployeeId, assignmentDto.ShiftId, assignmentDto.StartDate, assignmentDto.EndDate);
        if (!isValid)
        {
            throw new InvalidOperationException("Shift assignment validation failed. Please check for conflicts.");
        }

        var assignment = _mapper.Map<ShiftAssignment>(assignmentDto);
        await _shiftAssignmentRepository.AddAsync(assignment);
        await _shiftAssignmentRepository.SaveChangesAsync();

        _logger.LogInformation("Employee assigned to shift successfully: {AssignmentId}", assignment.Id);
        
        var result = await _shiftAssignmentRepository.GetByIdAsync(assignment.Id, 
            sa => sa.Employee, sa => sa.Shift, sa => sa.Shift.Branch);
        return _mapper.Map<ShiftAssignmentDto>(result);
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> BulkAssignEmployeesToShiftAsync(BulkShiftAssignmentDto bulkAssignmentDto)
    {
        _logger.LogInformation("Bulk assigning {EmployeeCount} employees to shift {ShiftId}", 
            bulkAssignmentDto.EmployeeIds.Count, bulkAssignmentDto.ShiftId);

        var assignments = new List<ShiftAssignment>();

        foreach (var employeeId in bulkAssignmentDto.EmployeeIds)
        {
            // Validate each assignment
            var isValid = await ValidateShiftAssignmentAsync(employeeId, bulkAssignmentDto.ShiftId, 
                bulkAssignmentDto.StartDate, bulkAssignmentDto.EndDate);
            
            if (isValid)
            {
                var assignmentDto = new CreateShiftAssignmentDto
                {
                    EmployeeId = employeeId,
                    ShiftId = bulkAssignmentDto.ShiftId,
                    StartDate = bulkAssignmentDto.StartDate,
                    EndDate = bulkAssignmentDto.EndDate,
                    Notes = bulkAssignmentDto.Notes
                };

                var assignment = _mapper.Map<ShiftAssignment>(assignmentDto);
                assignments.Add(assignment);
            }
            else
            {
                _logger.LogWarning("Skipping assignment for employee {EmployeeId} due to validation failure", employeeId);
            }
        }

        if (assignments.Any())
        {
            await _shiftAssignmentRepository.AddRangeAsync(assignments);
            await _shiftAssignmentRepository.SaveChangesAsync();
        }

        _logger.LogInformation("Bulk assignment completed. {AssignmentCount} assignments created", assignments.Count);

        // Return the created assignments with full details
        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var createdAssignments = await _shiftAssignmentRepository.FindAsync(
            sa => assignmentIds.Contains(sa.Id),
            sa => sa.Employee, sa => sa.Shift, sa => sa.Shift.Branch);

        return _mapper.Map<IEnumerable<ShiftAssignmentDto>>(createdAssignments);
    }

    public async Task<ShiftAssignmentDto> UpdateShiftAssignmentAsync(int assignmentId, UpdateShiftAssignmentDto updateDto)
    {
        _logger.LogInformation("Updating shift assignment: {AssignmentId}", assignmentId);

        var assignment = await _shiftAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new ArgumentException($"Shift assignment with ID {assignmentId} not found.");
        }

        // Validate updated assignment
        var isValid = await ValidateShiftAssignmentAsync(assignment.EmployeeId, updateDto.ShiftId, 
            updateDto.StartDate, updateDto.EndDate);
        if (!isValid)
        {
            throw new InvalidOperationException("Shift assignment validation failed. Please check for conflicts.");
        }

        _mapper.Map(updateDto, assignment);
        await _shiftAssignmentRepository.UpdateAsync(assignment);
        await _shiftAssignmentRepository.SaveChangesAsync();

        _logger.LogInformation("Shift assignment updated successfully: {AssignmentId}", assignmentId);

        var result = await _shiftAssignmentRepository.GetByIdAsync(assignmentId, 
            sa => sa.Employee, sa => sa.Shift, sa => sa.Shift.Branch);
        return _mapper.Map<ShiftAssignmentDto>(result);
    }

    public async Task<bool> RemoveEmployeeFromShiftAsync(int assignmentId)
    {
        _logger.LogInformation("Removing shift assignment: {AssignmentId}", assignmentId);

        var assignment = await _shiftAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            return false;
        }

        assignment.IsActive = false;
        assignment.EndDate = DateTime.Today.AddDays(-1); // End yesterday
        assignment.UpdatedAt = DateTime.UtcNow;

        await _shiftAssignmentRepository.UpdateAsync(assignment);
        await _shiftAssignmentRepository.SaveChangesAsync();

        _logger.LogInformation("Shift assignment removed successfully: {AssignmentId}", assignmentId);
        return true;
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> GetEmployeeShiftAssignmentsAsync(int employeeId)
    {
        var assignments = await _shiftAssignmentRepository.GetByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<ShiftAssignmentDto>>(assignments);
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> GetShiftAssignmentsAsync(int shiftId)
    {
        var assignments = await _shiftAssignmentRepository.GetByShiftIdAsync(shiftId);
        return _mapper.Map<IEnumerable<ShiftAssignmentDto>>(assignments);
    }

    public async Task<ShiftAssignmentDto?> GetCurrentEmployeeShiftAsync(int employeeId, DateTime date)
    {
        var assignment = await _shiftAssignmentRepository.GetCurrentAssignmentAsync(employeeId, date);
        return assignment != null ? _mapper.Map<ShiftAssignmentDto>(assignment) : null;
    }

    #endregion

    #region Shift Coverage and Conflict Detection

    public async Task<IEnumerable<ShiftCoverageDto>> GetShiftCoverageAsync(int branchId, DateTime date)
    {
        var shifts = await _shiftRepository.GetByBranchIdAsync(branchId);
        var assignments = await _shiftAssignmentRepository.GetAssignmentsByBranchAsync(branchId, date);

        var coverageList = new List<ShiftCoverageDto>();

        foreach (var shift in shifts.Where(s => s.IsActive))
        {
            var shiftAssignments = assignments.Where(a => a.ShiftId == shift.Id).ToList();
            
            var coverage = new ShiftCoverageDto
            {
                ShiftId = shift.Id,
                ShiftName = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                AssignedEmployees = shiftAssignments.Count,
                Assignments = _mapper.Map<List<ShiftAssignmentDto>>(shiftAssignments),
                HasConflict = false // Will be determined by conflict detection
            };

            coverageList.Add(coverage);
        }

        return coverageList;
    }

    public async Task<IEnumerable<ShiftConflictDto>> DetectShiftConflictsAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate)
    {
        var conflictingAssignments = await _shiftAssignmentRepository.GetConflictingAssignmentsAsync(employeeId, shiftId, startDate, endDate);
        var conflicts = new List<ShiftConflictDto>();

        foreach (var assignment in conflictingAssignments)
        {
            var conflict = new ShiftConflictDto
            {
                EmployeeId = employeeId,
                EmployeeName = $"{assignment.Employee.FirstName} {assignment.Employee.LastName}",
                ConflictType = "Overlap",
                Description = $"Employee is already assigned to shift '{assignment.Shift.Name}' during this period",
                ConflictDate = assignment.StartDate,
                ConflictingAssignments = new List<ShiftAssignmentDto> { _mapper.Map<ShiftAssignmentDto>(assignment) }
            };

            conflicts.Add(conflict);
        }

        return conflicts;
    }

    public async Task<IEnumerable<ShiftConflictDto>> GetAllShiftConflictsAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var assignments = await _shiftAssignmentRepository.GetAssignmentsByBranchAsync(branchId);
        var conflicts = new List<ShiftConflictDto>();

        // Group assignments by employee
        var employeeAssignments = assignments
            .Where(a => a.StartDate <= endDate && (a.EndDate == null || a.EndDate >= startDate))
            .GroupBy(a => a.EmployeeId);

        foreach (var group in employeeAssignments)
        {
            var employeeShifts = group.OrderBy(a => a.StartDate).ToList();
            
            for (int i = 0; i < employeeShifts.Count - 1; i++)
            {
                var current = employeeShifts[i];
                var next = employeeShifts[i + 1];

                // Check for overlapping assignments
                if (current.EndDate == null || current.EndDate >= next.StartDate)
                {
                    var conflict = new ShiftConflictDto
                    {
                        EmployeeId = current.EmployeeId,
                        EmployeeName = $"{current.Employee.FirstName} {current.Employee.LastName}",
                        ConflictType = "Overlap",
                        Description = $"Overlapping shift assignments: '{current.Shift.Name}' and '{next.Shift.Name}'",
                        ConflictDate = next.StartDate,
                        ConflictingAssignments = new List<ShiftAssignmentDto> 
                        { 
                            _mapper.Map<ShiftAssignmentDto>(current),
                            _mapper.Map<ShiftAssignmentDto>(next)
                        }
                    };

                    conflicts.Add(conflict);
                }
            }
        }

        return conflicts;
    }

    public async Task<bool> ValidateShiftAssignmentAsync(int employeeId, int shiftId, DateTime startDate, DateTime? endDate)
    {
        // Check for conflicting assignments
        var conflicts = await DetectShiftConflictsAsync(employeeId, shiftId, startDate, endDate);
        return !conflicts.Any();
    }

    #endregion

    #region Shift Pattern Management

    public async Task<IEnumerable<ShiftDto>> GetShiftsByPatternAsync(int organizationId, ShiftType shiftType)
    {
        var shifts = await _shiftRepository.GetShiftsByTypeAsync(organizationId, shiftType);
        return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
    }

    public async Task<IEnumerable<ShiftAssignmentDto>> GenerateRotatingShiftScheduleAsync(int branchId, List<int> employeeIds, List<int> shiftIds, DateTime startDate, int weeks)
    {
        _logger.LogInformation("Generating rotating shift schedule for {EmployeeCount} employees over {Weeks} weeks", 
            employeeIds.Count, weeks);

        var assignments = new List<ShiftAssignment>();
        var currentDate = startDate;

        for (int week = 0; week < weeks; week++)
        {
            for (int day = 0; day < 7; day++)
            {
                for (int i = 0; i < employeeIds.Count; i++)
                {
                    var employeeId = employeeIds[i];
                    var shiftIndex = (week + i) % shiftIds.Count; // Rotate shifts
                    var shiftId = shiftIds[shiftIndex];

                    // Validate assignment before creating
                    var isValid = await ValidateShiftAssignmentAsync(employeeId, shiftId, currentDate, currentDate);
                    if (isValid)
                    {
                        var assignment = new ShiftAssignment
                        {
                            EmployeeId = employeeId,
                            ShiftId = shiftId,
                            StartDate = currentDate,
                            EndDate = currentDate,
                            IsActive = true,
                            Notes = $"Auto-generated rotating schedule - Week {week + 1}",
                            CreatedAt = DateTime.UtcNow
                        };

                        assignments.Add(assignment);
                    }
                }

                currentDate = currentDate.AddDays(1);
            }
        }

        if (assignments.Any())
        {
            await _shiftAssignmentRepository.AddRangeAsync(assignments);
            await _shiftAssignmentRepository.SaveChangesAsync();
        }

        _logger.LogInformation("Generated {AssignmentCount} rotating shift assignments", assignments.Count);

        // Return the created assignments with full details
        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var createdAssignments = await _shiftAssignmentRepository.FindAsync(
            sa => assignmentIds.Contains(sa.Id),
            sa => sa.Employee, sa => sa.Shift, sa => sa.Shift.Branch);

        return _mapper.Map<IEnumerable<ShiftAssignmentDto>>(createdAssignments);
    }

    #endregion

    #region Reporting and Analytics

    public async Task<IEnumerable<ShiftAssignmentDto>> GetUpcomingShiftAssignmentsAsync(int employeeId, int days = 7)
    {
        var assignments = await _shiftAssignmentRepository.GetUpcomingAssignmentsAsync(employeeId, days);
        return _mapper.Map<IEnumerable<ShiftAssignmentDto>>(assignments);
    }

    public async Task<Dictionary<string, object>> GetShiftAnalyticsAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var assignments = await _shiftAssignmentRepository.GetAssignmentsByBranchAsync(branchId);
        var filteredAssignments = assignments.Where(a => 
            a.StartDate <= endDate && (a.EndDate == null || a.EndDate >= startDate)).ToList();

        var analytics = new Dictionary<string, object>
        {
            ["TotalAssignments"] = filteredAssignments.Count,
            ["ActiveAssignments"] = filteredAssignments.Count(a => a.IsActive),
            ["UniqueEmployees"] = filteredAssignments.Select(a => a.EmployeeId).Distinct().Count(),
            ["ShiftTypeDistribution"] = filteredAssignments
                .GroupBy(a => a.Shift.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ["AssignmentsByShift"] = filteredAssignments
                .GroupBy(a => a.Shift.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ["AverageAssignmentsPerEmployee"] = filteredAssignments.Count > 0 ? 
                (double)filteredAssignments.Count / filteredAssignments.Select(a => a.EmployeeId).Distinct().Count() : 0
        };

        return analytics;
    }

    #endregion

    #region Shift Swapping

    public async Task<ShiftSwapRequestDto> CreateShiftSwapRequestAsync(int requesterId, CreateShiftSwapRequestDto createDto)
    {
        _logger.LogInformation("Creating shift swap request for employee {RequesterId}", requesterId);

        // Validate requester shift assignment
        var requesterAssignment = await _shiftAssignmentRepository.GetByIdAsync(createDto.RequesterShiftAssignmentId);
        if (requesterAssignment == null || requesterAssignment.EmployeeId != requesterId)
        {
            throw new ArgumentException("Invalid shift assignment for requester.");
        }

        // Validate target shift assignment if specified
        if (createDto.TargetShiftAssignmentId.HasValue)
        {
            var targetAssignment = await _shiftAssignmentRepository.GetByIdAsync(createDto.TargetShiftAssignmentId.Value);
            if (targetAssignment == null)
            {
                throw new ArgumentException("Invalid target shift assignment.");
            }
            createDto.TargetEmployeeId = targetAssignment.EmployeeId;
        }

        var swapRequest = _mapper.Map<ShiftSwapRequest>(createDto);
        swapRequest.RequesterId = requesterId;
        swapRequest.Status = ShiftSwapStatus.Pending;
        
        // Set expiration if not provided
        if (!swapRequest.ExpiresAt.HasValue)
        {
            swapRequest.ExpiresAt = DateTime.UtcNow.AddDays(swapRequest.IsEmergency ? 1 : 7);
        }

        await _shiftSwapRequestRepository.AddAsync(swapRequest);
        await _shiftSwapRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift swap request created with ID: {RequestId}", swapRequest.Id);

        var result = await _shiftSwapRequestRepository.GetWithDetailsAsync(swapRequest.Id);
        return _mapper.Map<ShiftSwapRequestDto>(result);
    }

    public async Task<ShiftSwapRequestDto> RespondToShiftSwapRequestAsync(int responderId, CreateShiftSwapResponseDto responseDto)
    {
        _logger.LogInformation("Processing shift swap response from employee {ResponderId}", responderId);

        var swapRequest = await _shiftSwapRequestRepository.GetWithDetailsAsync(responseDto.ShiftSwapRequestId);
        if (swapRequest == null)
        {
            throw new ArgumentException("Shift swap request not found.");
        }

        if (swapRequest.Status != ShiftSwapStatus.Pending)
        {
            throw new InvalidOperationException("Shift swap request is not in pending status.");
        }

        // Validate responder shift assignment
        var responderAssignment = await _shiftAssignmentRepository.GetByIdAsync(responseDto.ResponderShiftAssignmentId);
        if (responderAssignment == null || responderAssignment.EmployeeId != responderId)
        {
            throw new ArgumentException("Invalid shift assignment for responder.");
        }

        var response = _mapper.Map<ShiftSwapResponse>(responseDto);
        response.ResponderId = responderId;
        response.RespondedAt = DateTime.UtcNow;

        await _shiftSwapResponseRepository.AddAsync(response);

        // If accepted, update swap request status to require manager approval
        if (response.IsAccepted)
        {
            swapRequest.Status = ShiftSwapStatus.ManagerApprovalRequired;
            swapRequest.TargetEmployeeId = responderId;
            swapRequest.TargetShiftAssignmentId = responseDto.ResponderShiftAssignmentId;
            await _shiftSwapRequestRepository.UpdateAsync(swapRequest);
        }

        await _shiftSwapRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift swap response processed for request {RequestId}", responseDto.ShiftSwapRequestId);

        var result = await _shiftSwapRequestRepository.GetWithDetailsAsync(swapRequest.Id);
        return _mapper.Map<ShiftSwapRequestDto>(result);
    }

    public async Task<ShiftSwapRequestDto> ApproveShiftSwapRequestAsync(int requestId, int approverId, ApproveShiftSwapDto approvalDto)
    {
        _logger.LogInformation("Processing shift swap approval for request {RequestId} by manager {ApproverId}", requestId, approverId);

        var swapRequest = await _shiftSwapRequestRepository.GetWithDetailsAsync(requestId);
        if (swapRequest == null)
        {
            throw new ArgumentException("Shift swap request not found.");
        }

        if (swapRequest.Status != ShiftSwapStatus.ManagerApprovalRequired)
        {
            throw new InvalidOperationException("Shift swap request is not awaiting manager approval.");
        }

        if (approvalDto.IsApproved)
        {
            // Perform the actual shift swap
            await PerformShiftSwapAsync(swapRequest);
            
            swapRequest.Status = ShiftSwapStatus.Approved;
            swapRequest.ApprovedBy = approverId;
            swapRequest.ApprovedAt = DateTime.UtcNow;
            swapRequest.ApprovalNotes = approvalDto.Notes;
            swapRequest.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            swapRequest.Status = ShiftSwapStatus.Rejected;
            swapRequest.RejectedAt = DateTime.UtcNow;
            swapRequest.RejectionReason = approvalDto.Notes;
        }

        await _shiftSwapRequestRepository.UpdateAsync(swapRequest);
        await _shiftSwapRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift swap request {RequestId} {Status}", requestId, 
            approvalDto.IsApproved ? "approved" : "rejected");

        var result = await _shiftSwapRequestRepository.GetWithDetailsAsync(requestId);
        return _mapper.Map<ShiftSwapRequestDto>(result);
    }

    public async Task<bool> CancelShiftSwapRequestAsync(int requestId, int userId)
    {
        var swapRequest = await _shiftSwapRequestRepository.GetByIdAsync(requestId);
        if (swapRequest == null)
        {
            return false;
        }

        if (swapRequest.RequesterId != userId)
        {
            throw new UnauthorizedAccessException("Only the requester can cancel the swap request.");
        }

        if (swapRequest.Status == ShiftSwapStatus.Completed || swapRequest.Status == ShiftSwapStatus.Approved)
        {
            throw new InvalidOperationException("Cannot cancel a completed or approved swap request.");
        }

        swapRequest.Status = ShiftSwapStatus.Cancelled;
        await _shiftSwapRequestRepository.UpdateAsync(swapRequest);
        await _shiftSwapRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift swap request {RequestId} cancelled by user {UserId}", requestId, userId);
        return true;
    }

    public async Task<IEnumerable<ShiftSwapRequestDto>> GetShiftSwapRequestsAsync(int employeeId)
    {
        var requests = await _shiftSwapRequestRepository.GetByRequesterIdAsync(employeeId);
        var targetRequests = await _shiftSwapRequestRepository.GetByTargetEmployeeIdAsync(employeeId);
        
        var allRequests = requests.Union(targetRequests).Distinct().OrderByDescending(r => r.CreatedAt);
        return _mapper.Map<IEnumerable<ShiftSwapRequestDto>>(allRequests);
    }

    public async Task<IEnumerable<ShiftSwapRequestDto>> GetPendingShiftSwapApprovalsAsync(int managerId)
    {
        var requests = await _shiftSwapRequestRepository.GetPendingApprovalsAsync(managerId);
        return _mapper.Map<IEnumerable<ShiftSwapRequestDto>>(requests);
    }

    public async Task<(IEnumerable<ShiftSwapRequestDto> Requests, int TotalCount)> SearchShiftSwapRequestsAsync(ShiftSwapSearchCriteria criteria)
    {
        var (requests, totalCount) = await _shiftSwapRequestRepository.SearchAsync(criteria);
        return (_mapper.Map<IEnumerable<ShiftSwapRequestDto>>(requests), totalCount);
    }

    #endregion

    #region Shift Coverage

    public async Task<ShiftCoverageRequestDto> CreateShiftCoverageRequestAsync(int requesterId, CreateShiftCoverageRequestDto createDto)
    {
        _logger.LogInformation("Creating shift coverage request for employee {RequesterId}", requesterId);

        // Validate shift assignment
        var shiftAssignment = await _shiftAssignmentRepository.GetByIdAsync(createDto.ShiftAssignmentId);
        if (shiftAssignment == null || shiftAssignment.EmployeeId != requesterId)
        {
            throw new ArgumentException("Invalid shift assignment for requester.");
        }

        var coverageRequest = _mapper.Map<ShiftCoverageRequest>(createDto);
        coverageRequest.RequesterId = requesterId;
        coverageRequest.Status = ShiftCoverageRequestStatus.Open;
        
        // Set expiration if not provided
        if (!coverageRequest.ExpiresAt.HasValue)
        {
            coverageRequest.ExpiresAt = DateTime.UtcNow.AddDays(coverageRequest.IsEmergency ? 1 : 3);
        }

        await _shiftCoverageRequestRepository.AddAsync(coverageRequest);
        await _shiftCoverageRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift coverage request created with ID: {RequestId}", coverageRequest.Id);

        var result = await _shiftCoverageRequestRepository.GetWithDetailsAsync(coverageRequest.Id);
        return _mapper.Map<ShiftCoverageRequestDto>(result);
    }

    public async Task<ShiftCoverageRequestDto> RespondToShiftCoverageRequestAsync(int responderId, CreateShiftCoverageResponseDto responseDto)
    {
        _logger.LogInformation("Processing shift coverage response from employee {ResponderId}", responderId);

        var coverageRequest = await _shiftCoverageRequestRepository.GetWithDetailsAsync(responseDto.ShiftCoverageRequestId);
        if (coverageRequest == null)
        {
            throw new ArgumentException("Shift coverage request not found.");
        }

        if (coverageRequest.Status != ShiftCoverageRequestStatus.Open)
        {
            throw new InvalidOperationException("Shift coverage request is not open for responses.");
        }

        var response = _mapper.Map<ShiftCoverageResponse>(responseDto);
        response.ResponderId = responderId;
        response.RespondedAt = DateTime.UtcNow;

        await _shiftCoverageResponseRepository.AddAsync(response);

        // If accepted, update coverage request status
        if (response.IsAccepted)
        {
            coverageRequest.Status = ShiftCoverageRequestStatus.Accepted;
            coverageRequest.AcceptedBy = responderId;
            coverageRequest.AcceptedAt = DateTime.UtcNow;
            coverageRequest.AcceptanceNotes = responseDto.Notes;
            await _shiftCoverageRequestRepository.UpdateAsync(coverageRequest);
        }

        await _shiftCoverageRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift coverage response processed for request {RequestId}", responseDto.ShiftCoverageRequestId);

        var result = await _shiftCoverageRequestRepository.GetWithDetailsAsync(coverageRequest.Id);
        return _mapper.Map<ShiftCoverageRequestDto>(result);
    }

    public async Task<ShiftCoverageRequestDto> ApproveShiftCoverageRequestAsync(int requestId, int approverId, ApproveShiftCoverageDto approvalDto)
    {
        _logger.LogInformation("Processing shift coverage approval for request {RequestId} by manager {ApproverId}", requestId, approverId);

        var coverageRequest = await _shiftCoverageRequestRepository.GetWithDetailsAsync(requestId);
        if (coverageRequest == null)
        {
            throw new ArgumentException("Shift coverage request not found.");
        }

        if (coverageRequest.Status != ShiftCoverageRequestStatus.Accepted)
        {
            throw new InvalidOperationException("Shift coverage request must be accepted before approval.");
        }

        if (approvalDto.IsApproved && coverageRequest.AcceptedBy.HasValue)
        {
            // Create new shift assignment for the covering employee
            await CreateCoverageShiftAssignmentAsync(coverageRequest);
            
            coverageRequest.ApprovedBy = approverId;
            coverageRequest.ApprovedAt = DateTime.UtcNow;
            coverageRequest.ApprovalNotes = approvalDto.Notes;
        }
        else
        {
            coverageRequest.Status = ShiftCoverageRequestStatus.Rejected;
        }

        await _shiftCoverageRequestRepository.UpdateAsync(coverageRequest);
        await _shiftCoverageRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift coverage request {RequestId} {Status}", requestId, 
            approvalDto.IsApproved ? "approved" : "rejected");

        var result = await _shiftCoverageRequestRepository.GetWithDetailsAsync(requestId);
        return _mapper.Map<ShiftCoverageRequestDto>(result);
    }

    public async Task<bool> CancelShiftCoverageRequestAsync(int requestId, int userId)
    {
        var coverageRequest = await _shiftCoverageRequestRepository.GetByIdAsync(requestId);
        if (coverageRequest == null)
        {
            return false;
        }

        if (coverageRequest.RequesterId != userId)
        {
            throw new UnauthorizedAccessException("Only the requester can cancel the coverage request.");
        }

        if (coverageRequest.ApprovedBy.HasValue)
        {
            throw new InvalidOperationException("Cannot cancel an approved coverage request.");
        }

        coverageRequest.Status = ShiftCoverageRequestStatus.Cancelled;
        await _shiftCoverageRequestRepository.UpdateAsync(coverageRequest);
        await _shiftCoverageRequestRepository.SaveChangesAsync();

        _logger.LogInformation("Shift coverage request {RequestId} cancelled by user {UserId}", requestId, userId);
        return true;
    }

    public async Task<IEnumerable<ShiftCoverageRequestDto>> GetShiftCoverageRequestsAsync(int employeeId)
    {
        var requests = await _shiftCoverageRequestRepository.GetByRequesterIdAsync(employeeId);
        return _mapper.Map<IEnumerable<ShiftCoverageRequestDto>>(requests);
    }

    public async Task<IEnumerable<ShiftCoverageRequestDto>> GetPendingShiftCoverageApprovalsAsync(int managerId)
    {
        var requests = await _shiftCoverageRequestRepository.GetPendingApprovalsAsync(managerId);
        return _mapper.Map<IEnumerable<ShiftCoverageRequestDto>>(requests);
    }

    public async Task<(IEnumerable<ShiftCoverageRequestDto> Requests, int TotalCount)> SearchShiftCoverageRequestsAsync(ShiftCoverageSearchCriteria criteria)
    {
        var (requests, totalCount) = await _shiftCoverageRequestRepository.SearchAsync(criteria);
        return (_mapper.Map<IEnumerable<ShiftCoverageRequestDto>>(requests), totalCount);
    }

    #endregion

    #region Emergency Coverage Broadcasting

    public async Task<List<ShiftCoverageRequestDto>> BroadcastEmergencyShiftCoverageAsync(int broadcasterId, EmergencyShiftCoverageBroadcastDto broadcastDto)
    {
        _logger.LogInformation("Broadcasting emergency shift coverage for shift {ShiftId} on {ShiftDate}", 
            broadcastDto.ShiftId, broadcastDto.ShiftDate);

        // Get eligible employees for the shift
        var eligibleEmployees = await GetEligibleEmployeesForShiftAsync(broadcastDto.BranchId, broadcastDto.ShiftId, broadcastDto.ShiftDate);
        
        if (broadcastDto.TargetEmployeeIds != null && broadcastDto.TargetEmployeeIds.Any())
        {
            eligibleEmployees = eligibleEmployees.Where(e => broadcastDto.TargetEmployeeIds.Contains(e.Id)).ToList();
        }

        var coverageRequests = new List<ShiftCoverageRequest>();

        foreach (var employee in eligibleEmployees)
        {
            // Create a temporary shift assignment for the broadcast
            var tempAssignment = new ShiftAssignment
            {
                EmployeeId = employee.Id,
                ShiftId = broadcastDto.ShiftId,
                StartDate = broadcastDto.ShiftDate,
                EndDate = broadcastDto.ShiftDate,
                IsActive = false, // Temporary assignment
                Notes = "Emergency coverage broadcast"
            };

            await _shiftAssignmentRepository.AddAsync(tempAssignment);
            await _shiftAssignmentRepository.SaveChangesAsync();

            var coverageRequest = new ShiftCoverageRequest
            {
                RequesterId = broadcasterId,
                ShiftAssignmentId = tempAssignment.Id,
                ShiftDate = broadcastDto.ShiftDate,
                Reason = broadcastDto.Reason,
                IsEmergency = true,
                ExpiresAt = broadcastDto.ExpiresAt,
                Status = ShiftCoverageRequestStatus.Open
            };

            coverageRequests.Add(coverageRequest);
        }

        if (coverageRequests.Any())
        {
            await _shiftCoverageRequestRepository.AddRangeAsync(coverageRequests);
            await _shiftCoverageRequestRepository.SaveChangesAsync();
        }

        _logger.LogInformation("Emergency coverage broadcast created {RequestCount} requests", coverageRequests.Count);

        var requestIds = coverageRequests.Select(r => r.Id).ToList();
        var createdRequests = await _shiftCoverageRequestRepository.FindAsync(
            r => requestIds.Contains(r.Id),
            r => r.Requester, r => r.ShiftAssignment, r => r.ShiftAssignment.Shift);

        return _mapper.Map<List<ShiftCoverageRequestDto>>(createdRequests);
    }

    public async Task<IEnumerable<ShiftCoverageRequestDto>> GetEmergencyShiftCoverageRequestsAsync(int branchId)
    {
        var requests = await _shiftCoverageRequestRepository.GetEmergencyRequestsAsync(branchId);
        return _mapper.Map<IEnumerable<ShiftCoverageRequestDto>>(requests);
    }

    #endregion

    #region Detailed Analytics

    public async Task<ShiftAnalyticsDto> GetDetailedShiftAnalyticsAsync(ShiftAnalyticsSearchCriteria criteria)
    {
        _logger.LogInformation("Generating detailed shift analytics for branch {BranchId}", criteria.BranchId);

        var analytics = new ShiftAnalyticsDto
        {
            BranchId = criteria.BranchId ?? 0,
            StartDate = criteria.StartDate,
            EndDate = criteria.EndDate
        };

        if (criteria.IncludeSwapAnalytics)
        {
            await PopulateSwapAnalyticsAsync(analytics, criteria);
        }

        if (criteria.IncludeCoverageAnalytics)
        {
            await PopulateCoverageAnalyticsAsync(analytics, criteria);
        }

        if (criteria.IncludeEmployeeActivity)
        {
            await PopulateEmployeeActivityAsync(analytics, criteria);
        }

        if (criteria.IncludeShiftPatterns)
        {
            await PopulateShiftPatternAnalyticsAsync(analytics, criteria);
        }

        if (criteria.IncludeTrends)
        {
            await PopulateTrendAnalyticsAsync(analytics, criteria);
        }

        return analytics;
    }

    #endregion

    #region Private Helper Methods

    private static void ValidateShiftTimes(TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime >= endTime && endTime != TimeSpan.Zero)
        {
            // Allow overnight shifts where end time is next day (e.g., 22:00 to 06:00)
            if (!(startTime > endTime && endTime < TimeSpan.FromHours(12)))
            {
                throw new ArgumentException("End time must be after start time, or represent an overnight shift.");
            }
        }
    }

    private async Task PerformShiftSwapAsync(ShiftSwapRequest swapRequest)
    {
        // Get the shift assignments
        var requesterAssignment = await _shiftAssignmentRepository.GetByIdAsync(swapRequest.RequesterShiftAssignmentId);
        var targetAssignment = await _shiftAssignmentRepository.GetByIdAsync(swapRequest.TargetShiftAssignmentId!.Value);

        if (requesterAssignment == null || targetAssignment == null)
        {
            throw new InvalidOperationException("Invalid shift assignments for swap.");
        }

        // Swap the employee assignments
        var tempEmployeeId = requesterAssignment.EmployeeId;
        requesterAssignment.EmployeeId = targetAssignment.EmployeeId;
        targetAssignment.EmployeeId = tempEmployeeId;

        // Update assignments
        await _shiftAssignmentRepository.UpdateAsync(requesterAssignment);
        await _shiftAssignmentRepository.UpdateAsync(targetAssignment);
    }

    private async Task CreateCoverageShiftAssignmentAsync(ShiftCoverageRequest coverageRequest)
    {
        var originalAssignment = await _shiftAssignmentRepository.GetByIdAsync(coverageRequest.ShiftAssignmentId);
        if (originalAssignment == null || !coverageRequest.AcceptedBy.HasValue)
        {
            throw new InvalidOperationException("Invalid coverage request data.");
        }

        // Create new assignment for covering employee
        var coverageAssignment = new ShiftAssignment
        {
            EmployeeId = coverageRequest.AcceptedBy.Value,
            ShiftId = originalAssignment.ShiftId,
            StartDate = coverageRequest.ShiftDate,
            EndDate = coverageRequest.ShiftDate,
            IsActive = true,
            Notes = $"Coverage for {originalAssignment.Employee.FirstName} {originalAssignment.Employee.LastName}",
            AssignedBy = coverageRequest.ApprovedBy?.ToString()
        };

        await _shiftAssignmentRepository.AddAsync(coverageAssignment);

        // Deactivate original assignment for the coverage date
        originalAssignment.Notes += $" - Covered by employee {coverageRequest.AcceptedBy} on {coverageRequest.ShiftDate:yyyy-MM-dd}";
        await _shiftAssignmentRepository.UpdateAsync(originalAssignment);
    }

    private async Task<List<Employee>> GetEligibleEmployeesForShiftAsync(int branchId, int shiftId, DateTime shiftDate)
    {
        // This would typically involve complex business logic to determine eligible employees
        // For now, return all active employees in the branch who don't have conflicting assignments
        var allEmployees = await _employeeRepository.GetByBranchIdAsync(branchId);
        var eligibleEmployees = new List<Employee>();

        foreach (var employee in allEmployees.Where(e => e.Status == EmployeeStatus.Active))
        {
            var hasConflict = await _shiftAssignmentRepository.HasConflictingAssignmentAsync(employee.Id, shiftDate);
            if (!hasConflict)
            {
                eligibleEmployees.Add(employee);
            }
        }

        return eligibleEmployees;
    }

    private async Task PopulateSwapAnalyticsAsync(ShiftAnalyticsDto analytics, ShiftAnalyticsSearchCriteria criteria)
    {
        var swapRequests = await _shiftSwapRequestRepository.GetByBranchIdAsync(criteria.BranchId ?? 0, criteria.StartDate, criteria.EndDate);
        
        analytics.TotalShiftSwapRequests = swapRequests.Count();
        analytics.ApprovedShiftSwaps = swapRequests.Count(r => r.Status == ShiftSwapStatus.Approved);
        analytics.RejectedShiftSwaps = swapRequests.Count(r => r.Status == ShiftSwapStatus.Rejected);
        analytics.PendingShiftSwaps = swapRequests.Count(r => r.Status == ShiftSwapStatus.Pending || r.Status == ShiftSwapStatus.ManagerApprovalRequired);
        analytics.EmergencySwapRequests = swapRequests.Count(r => r.IsEmergency);
        
        analytics.ShiftSwapApprovalRate = analytics.TotalShiftSwapRequests > 0 ? 
            (double)analytics.ApprovedShiftSwaps / analytics.TotalShiftSwapRequests * 100 : 0;

        var completedSwaps = swapRequests.Where(r => r.CompletedAt.HasValue && r.CreatedAt != default);
        analytics.AverageSwapProcessingTimeHours = completedSwaps.Any() ? 
            completedSwaps.Average(r => (r.CompletedAt!.Value - r.CreatedAt).TotalHours) : 0;
    }

    private async Task PopulateCoverageAnalyticsAsync(ShiftAnalyticsDto analytics, ShiftAnalyticsSearchCriteria criteria)
    {
        var coverageRequests = await _shiftCoverageRequestRepository.GetByBranchIdAsync(criteria.BranchId ?? 0, criteria.StartDate, criteria.EndDate);
        
        analytics.TotalCoverageRequests = coverageRequests.Count();
        analytics.AcceptedCoverageRequests = coverageRequests.Count(r => r.Status == ShiftCoverageRequestStatus.Accepted);
        analytics.RejectedCoverageRequests = coverageRequests.Count(r => r.Status == ShiftCoverageRequestStatus.Rejected);
        analytics.PendingCoverageRequests = coverageRequests.Count(r => r.Status == ShiftCoverageRequestStatus.Open);
        analytics.EmergencyCoverageRequests = coverageRequests.Count(r => r.IsEmergency);
        
        analytics.CoverageRequestFulfillmentRate = analytics.TotalCoverageRequests > 0 ? 
            (double)analytics.AcceptedCoverageRequests / analytics.TotalCoverageRequests * 100 : 0;

        var respondedRequests = coverageRequests.Where(r => r.AcceptedAt.HasValue && r.CreatedAt != default);
        analytics.AverageCoverageResponseTimeHours = respondedRequests.Any() ? 
            respondedRequests.Average(r => (r.AcceptedAt!.Value - r.CreatedAt).TotalHours) : 0;
    }

    private async Task PopulateEmployeeActivityAsync(ShiftAnalyticsDto analytics, ShiftAnalyticsSearchCriteria criteria)
    {
        // Implementation would involve complex queries to get employee participation data
        // This is a simplified version
        analytics.TopSwapRequesters = new List<EmployeeSwapActivityDto>();
        analytics.TopCoverageProviders = new List<EmployeeSwapActivityDto>();
    }

    private async Task PopulateShiftPatternAnalyticsAsync(ShiftAnalyticsDto analytics, ShiftAnalyticsSearchCriteria criteria)
    {
        // Implementation would analyze shift patterns and their stability
        analytics.ShiftPatternAnalytics = new List<ShiftPatternAnalyticsDto>();
    }

    private async Task PopulateTrendAnalyticsAsync(ShiftAnalyticsDto analytics, ShiftAnalyticsSearchCriteria criteria)
    {
        // Implementation would generate daily and weekly trend data
        analytics.DailyActivity = new List<DailyShiftActivityDto>();
        analytics.WeeklyTrends = new List<WeeklyShiftTrendDto>();
    }

    #endregion
}