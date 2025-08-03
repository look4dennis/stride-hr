namespace StrideHR.Core.Models.Shift;

public class ShiftAssignmentDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeId_Display { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan ShiftStartTime { get; set; }
    public TimeSpan ShiftEndTime { get; set; }
}

public class CreateShiftAssignmentDto
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateShiftAssignmentDto
{
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class BulkShiftAssignmentDto
{
    public List<int> EmployeeIds { get; set; } = new();
    public int ShiftId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class ShiftAssignmentSearchCriteria
{
    public int? EmployeeId { get; set; }
    public int? ShiftId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ShiftCoverageDto
{
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int RequiredEmployees { get; set; }
    public int AssignedEmployees { get; set; }
    public int AvailableEmployees { get; set; }
    public bool HasConflict { get; set; }
    public List<ShiftAssignmentDto> Assignments { get; set; } = new();
}

public class ShiftConflictDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty; // "Overlap", "DoubleBooking", "RestPeriodViolation"
    public string Description { get; set; } = string.Empty;
    public DateTime ConflictDate { get; set; }
    public List<ShiftAssignmentDto> ConflictingAssignments { get; set; } = new();
}