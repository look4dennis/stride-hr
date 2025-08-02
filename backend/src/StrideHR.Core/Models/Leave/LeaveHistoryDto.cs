using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Leave;

public class LeaveHistoryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<LeaveHistoryDetailDto> LeaveDetails { get; set; } = new();
    public decimal TotalAllocated { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal TotalCarriedForward { get; set; }
    public decimal TotalEncashed { get; set; }
    public decimal TotalRemaining { get; set; }
}

public class LeaveHistoryDetailDto
{
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
    public decimal EncashedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public List<LeaveTransactionDto> Transactions { get; set; } = new();
}

public class LeaveTransactionDto
{
    public DateTime Date { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Allocated, Used, Carried Forward, Encashed
    public decimal Days { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ReferenceId { get; set; } // LeaveRequest ID, Encashment ID, etc.
}

public class LeaveAnalyticsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TotalEmployees { get; set; }
    public decimal AverageLeaveUtilization { get; set; }
    public List<LeaveTypeAnalyticsDto> LeaveTypeAnalytics { get; set; } = new();
    public List<MonthlyLeaveAnalyticsDto> MonthlyAnalytics { get; set; } = new();
}

public class LeaveTypeAnalyticsDto
{
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal TotalAllocated { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public decimal TotalEncashed { get; set; }
    public decimal TotalCarriedForward { get; set; }
}

public class MonthlyLeaveAnalyticsDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalLeavesTaken { get; set; }
    public int TotalRequests { get; set; }
    public decimal AverageRequestDuration { get; set; }
}