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
    private readonly IMapper _mapper;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        IShiftRepository shiftRepository,
        IShiftAssignmentRepository shiftAssignmentRepository,
        IMapper mapper,
        ILogger<ShiftService> logger)
    {
        _shiftRepository = shiftRepository;
        _shiftAssignmentRepository = shiftAssignmentRepository;
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

    #endregion
}