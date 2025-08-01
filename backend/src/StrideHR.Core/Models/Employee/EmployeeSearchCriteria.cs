using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Employee;

public class EmployeeSearchCriteria
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public EmployeeStatus? Status { get; set; }
    public int? ReportingManagerId { get; set; }
    public DateTime? JoiningDateFrom { get; set; }
    public DateTime? JoiningDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "FirstName";
    public bool SortDescending { get; set; } = false;
}