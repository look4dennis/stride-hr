namespace StrideHR.Core.Models.Payroll;

public class FormulaVariable
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string DataType { get; set; } = "decimal";
    public string Description { get; set; } = string.Empty;
}