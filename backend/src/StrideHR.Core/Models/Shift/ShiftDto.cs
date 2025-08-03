using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Shift;

public class ShiftDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftType Type { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? GracePeriod { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public bool IsFlexible { get; set; }
    public TimeSpan? FlexibilityWindow { get; set; }
    public string? TimeZone { get; set; }
    public List<int> WorkingDays { get; set; } = new();
    public decimal OvertimeMultiplier { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public string? BranchName { get; set; }
    public int AssignedEmployeesCount { get; set; }
}

public class CreateShiftDto
{
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftType Type { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? GracePeriod { get; set; } = TimeSpan.FromMinutes(15);
    public bool IsFlexible { get; set; } = false;
    public TimeSpan? FlexibilityWindow { get; set; }
    public string? TimeZone { get; set; }
    public List<int> WorkingDays { get; set; } = new() { 1, 2, 3, 4, 5 }; // Monday to Friday
    public decimal OvertimeMultiplier { get; set; } = 1.5m;
}

public class UpdateShiftDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftType Type { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? GracePeriod { get; set; }
    public bool IsFlexible { get; set; }
    public TimeSpan? FlexibilityWindow { get; set; }
    public string? TimeZone { get; set; }
    public List<int> WorkingDays { get; set; } = new();
    public decimal OvertimeMultiplier { get; set; }
    public bool IsActive { get; set; }
}

public class ShiftTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ShiftType Type { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public List<int> WorkingDays { get; set; } = new();
    public bool IsTemplate { get; set; } = true;
}

public class ShiftSearchCriteria
{
    public int? OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public ShiftType? Type { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}