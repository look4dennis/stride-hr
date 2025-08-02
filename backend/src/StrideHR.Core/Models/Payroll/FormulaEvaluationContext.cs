using StrideHR.Core.Entities;

namespace StrideHR.Core.Models.Payroll;

public class FormulaEvaluationContext
{
    public Entities.Employee Employee { get; set; } = null!;
    public Entities.Branch Branch { get; set; } = null!;
    public Entities.Organization Organization { get; set; } = null!;
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal OvertimeHours { get; set; }
    public int WorkingDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public Dictionary<string, decimal> Variables { get; set; } = new();
    public Dictionary<string, decimal> CustomValues { get; set; } = new();
}