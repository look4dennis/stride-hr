using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Expense;

public class ExpenseAnalyticsDto
{
    public ExpenseAnalyticsPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalTravelExpenses { get; set; }
    public decimal TotalMileageExpenses { get; set; }
    public int TotalClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int PendingClaims { get; set; }
    public int RejectedClaims { get; set; }
    public decimal AverageClaimAmount { get; set; }
    public List<ExpenseCategoryAnalyticsDto> CategoryBreakdown { get; set; } = new();
    public List<EmployeeExpenseAnalyticsDto> EmployeeBreakdown { get; set; } = new();
    public List<MonthlyExpenseTrendDto> MonthlyTrends { get; set; } = new();
    public List<TravelExpenseAnalyticsDto> TravelAnalytics { get; set; } = new();
}

public class ExpenseCategoryAnalyticsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ClaimCount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal BudgetUtilization { get; set; }
    public bool IsOverBudget { get; set; }
}

public class EmployeeExpenseAnalyticsDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal TotalExpenses { get; set; }
    public int ClaimCount { get; set; }
    public decimal AverageClaimAmount { get; set; }
    public decimal TravelExpenses { get; set; }
    public decimal MileageExpenses { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal BudgetUtilization { get; set; }
    public bool IsOverBudget { get; set; }
}

public class MonthlyExpenseTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ClaimCount { get; set; }
    public decimal TravelAmount { get; set; }
    public decimal MileageAmount { get; set; }
    public decimal AverageClaimAmount { get; set; }
}

public class TravelExpenseAnalyticsDto
{
    public TravelMode TravelMode { get; set; }
    public string TravelModeText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TripCount { get; set; }
    public decimal AverageTripCost { get; set; }
    public decimal TotalMileage { get; set; }
    public decimal AverageMileageRate { get; set; }
    public List<string> PopularRoutes { get; set; } = new();
}

public class ExpenseBudgetTrackingDto
{
    public int OrganizationId { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public ExpenseAnalyticsPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BudgetLimit { get; set; }
    public decimal ActualExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal BudgetUtilization { get; set; }
    public bool IsOverBudget { get; set; }
    public decimal ProjectedExpenses { get; set; }
    public List<ExpenseCategoryBudgetDto> CategoryBudgets { get; set; } = new();
}

public class ExpenseCategoryBudgetDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public decimal ActualExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal BudgetUtilization { get; set; }
    public bool IsOverBudget { get; set; }
    public List<string> Alerts { get; set; } = new();
}

public class ExpenseComplianceReportDto
{
    public DateTime ReportDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalClaims { get; set; }
    public int CompliantClaims { get; set; }
    public int NonCompliantClaims { get; set; }
    public decimal ComplianceRate { get; set; }
    public List<ExpenseComplianceViolationDto> Violations { get; set; } = new();
    public List<ExpensePolicyComplianceDto> PolicyCompliance { get; set; } = new();
}

public class ExpenseComplianceViolationDto
{
    public int ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string ViolationDescription { get; set; } = string.Empty;
    public string PolicyRule { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ViolationDate { get; set; }
    public ExpenseViolationSeverity Severity { get; set; }
    public string SeverityText { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class ExpensePolicyComplianceDto
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyDescription { get; set; } = string.Empty;
    public int TotalApplicableClaims { get; set; }
    public int CompliantClaims { get; set; }
    public int ViolatingClaims { get; set; }
    public decimal ComplianceRate { get; set; }
    public List<string> CommonViolations { get; set; } = new();
}

public class ExpenseReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public ExpenseClaimStatus? Status { get; set; }
    public TravelMode? TravelMode { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool? IsTravelExpense { get; set; }
    public bool? HasPolicyViolations { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "ExpenseDate";
    public bool SortDescending { get; set; } = true;
}