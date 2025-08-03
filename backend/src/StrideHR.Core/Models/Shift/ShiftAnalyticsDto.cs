namespace StrideHR.Core.Models.Shift;

public class ShiftAnalyticsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Shift Swap Analytics
    public int TotalShiftSwapRequests { get; set; }
    public int ApprovedShiftSwaps { get; set; }
    public int RejectedShiftSwaps { get; set; }
    public int PendingShiftSwaps { get; set; }
    public double ShiftSwapApprovalRate { get; set; }
    public double AverageSwapProcessingTimeHours { get; set; }
    public int EmergencySwapRequests { get; set; }
    
    // Shift Coverage Analytics
    public int TotalCoverageRequests { get; set; }
    public int AcceptedCoverageRequests { get; set; }
    public int RejectedCoverageRequests { get; set; }
    public int PendingCoverageRequests { get; set; }
    public double CoverageRequestFulfillmentRate { get; set; }
    public double AverageCoverageResponseTimeHours { get; set; }
    public int EmergencyCoverageRequests { get; set; }
    
    // Employee Participation
    public int ActiveSwapParticipants { get; set; }
    public int ActiveCoverageProviders { get; set; }
    public List<EmployeeSwapActivityDto> TopSwapRequesters { get; set; } = new();
    public List<EmployeeSwapActivityDto> TopCoverageProviders { get; set; } = new();
    
    // Shift Pattern Analytics
    public List<ShiftPatternAnalyticsDto> ShiftPatternAnalytics { get; set; } = new();
    
    // Time-based Analytics
    public List<DailyShiftActivityDto> DailyActivity { get; set; } = new();
    public List<WeeklyShiftTrendDto> WeeklyTrends { get; set; } = new();
}

public class EmployeeSwapActivityDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeId_Display { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int SwapRequestsCount { get; set; }
    public int SwapResponsesCount { get; set; }
    public int CoverageRequestsCount { get; set; }
    public int CoverageResponsesCount { get; set; }
    public double ResponseRate { get; set; }
    public double ApprovalRate { get; set; }
}

public class ShiftPatternAnalyticsDto
{
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string ShiftType { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int TotalAssignments { get; set; }
    public int SwapRequests { get; set; }
    public int CoverageRequests { get; set; }
    public double SwapRequestRate { get; set; }
    public double CoverageRequestRate { get; set; }
    public double StabilityScore { get; set; } // Lower score means more swaps/coverage needed
}

public class DailyShiftActivityDto
{
    public DateTime Date { get; set; }
    public int SwapRequests { get; set; }
    public int CoverageRequests { get; set; }
    public int SwapApprovals { get; set; }
    public int CoverageAcceptances { get; set; }
    public int EmergencyRequests { get; set; }
}

public class WeeklyShiftTrendDto
{
    public int WeekNumber { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int TotalSwapRequests { get; set; }
    public int TotalCoverageRequests { get; set; }
    public double SwapApprovalRate { get; set; }
    public double CoverageFulfillmentRate { get; set; }
    public double AverageResponseTimeHours { get; set; }
}

public class ShiftAnalyticsSearchCriteria
{
    public int? BranchId { get; set; }
    public int? ShiftId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeSwapAnalytics { get; set; } = true;
    public bool IncludeCoverageAnalytics { get; set; } = true;
    public bool IncludeEmployeeActivity { get; set; } = true;
    public bool IncludeShiftPatterns { get; set; } = true;
    public bool IncludeTrends { get; set; } = true;
}