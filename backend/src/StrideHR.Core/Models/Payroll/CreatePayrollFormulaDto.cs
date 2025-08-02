using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Payroll;

public class CreatePayrollFormulaDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PayrollFormulaType Type { get; set; }
    public string Formula { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public int Priority { get; set; } = 0;
    public string? Conditions { get; set; }
    public int? OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
}