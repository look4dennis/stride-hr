using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpensePolicyRule : BaseEntity
{
    public int ExpenseCategoryId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleDescription { get; set; } = string.Empty;
    public ExpensePolicyRuleType RuleType { get; set; }
    public string RuleCondition { get; set; } = string.Empty; // JSON condition
    public decimal? MaxAmount { get; set; }
    public int? MaxQuantity { get; set; }
    public bool RequiresJustification { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }

    // Navigation Properties
    public virtual ExpenseCategory ExpenseCategory { get; set; } = null!;
}